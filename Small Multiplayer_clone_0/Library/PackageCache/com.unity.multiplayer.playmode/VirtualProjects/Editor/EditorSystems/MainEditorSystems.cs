using System;
using System.Diagnostics;
using Unity.Multiplayer.Playmode.Common.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class MainEditorSystems
    {
        int m_UnresponsiveTimeout;
        float m_CloneHeartbeatRequestPeriodTwoSecondsRate;
        float m_CloneResponsivenessCheckPeriodOneSecond;
        int m_NextUnresponsiveTimeout;
        float m_NextCloneHeartbeatRequestPeriodTwoSecondsRate;
        float m_NextCloneResponsivenessCheckPeriodOneSecond;
        public const int k_UnresponsiveTimeoutSeconds = 10;

        internal MainApplicationEvents ApplicationEvents { get; }
        internal AssetImportEvents AssetImportEvents { get; }
        internal SceneEvents SceneEvents { get; }

        internal MainEditorSystems()
        {
            ApplicationEvents = new MainApplicationEvents();
            AssetImportEvents = new AssetImportEvents();
            SceneEvents = new SceneEvents();
        }

        internal void Listen(MainEditorContext vpContext)
        {
            /*
             * These system classes are simply an aggregation of logic and other events
             *
             * Its only purpose is to forward events to the Internal Runtimes, Workflows, and MultiplayerPlaymode (UI)
             */

            const int cloneResponsivenessCheckPeriodOneSecond = 1;
            const int cloneHeartbeatRequestPeriodTwoSeconds = 2;
            const int numberOfRetries = 3;
            var cloneResponsiveCheckStartTime = DateTime.UtcNow;
            var cloneHeartbeatRequestStartTime = DateTime.UtcNow;
            m_NextUnresponsiveTimeout = k_UnresponsiveTimeoutSeconds;
            m_NextCloneHeartbeatRequestPeriodTwoSecondsRate = cloneHeartbeatRequestPeriodTwoSeconds;
            m_NextCloneResponsivenessCheckPeriodOneSecond = cloneResponsivenessCheckPeriodOneSecond;

            EditorApplication.focusChanged += isFocused =>
            {
                m_NextUnresponsiveTimeout = k_UnresponsiveTimeoutSeconds * (isFocused ? 1 : 5);
                m_NextCloneHeartbeatRequestPeriodTwoSecondsRate = (float)cloneHeartbeatRequestPeriodTwoSeconds * (isFocused ? 1 : 5);
                m_NextCloneResponsivenessCheckPeriodOneSecond = (float)cloneResponsivenessCheckPeriodOneSecond * (isFocused ? 1 : 5);
            };
            EditorApplication.update += () =>
            {
                // Send heartbeat requests to the clones. This will prompt the clones to send back a
                // heartbeat response which is what we use to determine liveness.
                var hasExceededLiveness = (DateTime.UtcNow - cloneHeartbeatRequestStartTime).TotalSeconds >= m_CloneHeartbeatRequestPeriodTwoSecondsRate;
                if (hasExceededLiveness)
                {
                    cloneHeartbeatRequestStartTime = DateTime.UtcNow;
                    m_CloneHeartbeatRequestPeriodTwoSecondsRate = m_NextCloneHeartbeatRequestPeriodTwoSecondsRate;
                    /* * * * */

                    var hasAnyRunningProcesses = false;
                    foreach (var project in vpContext.VirtualProjectsApi.GetProjectsFunc(VirtualProjectsApi.k_FilterAll))
                    {
                        if (project.ProcessId != -1)
                        {
                            hasAnyRunningProcesses = true;
                            break;
                        }
                    }

                    if (hasAnyRunningProcesses)
                    {
                        MppmLog.Debug("Sending heartbeat");
                        vpContext.MessagingService.Broadcast(new HeartbeatRequestMessage(), () =>
                        {
                            // empty to ensure replies
                        }, error =>
                        {
                            MppmLog.Debug($"Messaging Service failed our Heartbeat Message [{error}]");
                        });
                    }
                    else
                    {
                        MppmLog.Debug("No processes to send heartbeat to");
                    }
                }

#if UNITY_MP_TOOLS_DEV_HEARTBEAT
                // Check if any of the clones is unresponsive. Note that this check is disabled if
                // the main editor or the clone has a debugger attached. The former because we could
                // have been stopped for a while without processing any heartbeats, and the latter
                // because if a clone is being debugged it's expected that we won't receive anything
                // for it. Note that this approach is not foolproof (it's still possible to get an
                // unresponsive clone with a debugger if the timing is right), but it's a good proxy
                // and worst case we just get an annoying popup about the unresponsive clone.
                var hasExceeded = (DateTime.UtcNow - cloneResponsiveCheckStartTime).TotalSeconds >= m_CloneResponsivenessCheckPeriodOneSecond;
                if (hasExceeded && !Debugger.IsAttached)
                {
                    foreach (var project in vpContext.VirtualProjectsApi.GetProjectsFunc(VirtualProjectsApi.k_FilterAll))
                    {
                        var hasState = vpContext.StateRepository.TryGetValue(project.Identifier, out var state);

                        if (!hasState) continue;
                        if (state.LastHeartbeatReceived == default) continue; // This happens when we first launch or :SuspendHeartbeats
                        if (state.IsDebuggerAttached) continue;
                        if (project.EditorState != EditorState.Launched) continue;
                        if ((DateTime.UtcNow - state.LastHeartbeatReceived).TotalSeconds >= m_UnresponsiveTimeout / 2f)
                        {
                            MppmLog.Debug($"Approaching heartbeat time out! [Now:{DateTime.UtcNow} | LastHeartbeatReceived:{state.LastHeartbeatReceived} => m_UnresponsiveTimeout:{m_UnresponsiveTimeout}] [Retries:{state.Retry}]");
                        }

                        if ((DateTime.UtcNow - state.LastHeartbeatReceived).TotalSeconds <= m_UnresponsiveTimeout) continue;

                        if (state.Retry >= numberOfRetries)
                        {
                            state.Retry = 0;
                            ApplicationEvents.InvokeCloneUnresponsive(project.Identifier);
                        }
                        else
                        {
                            state.Retry++;
                            state.LastHeartbeatReceived = DateTime.UtcNow;
                            MppmLog.Debug($"Failed to keep up with heartbeat. Retrying. [{state.Retry}/{numberOfRetries}]");
                        }
                    }

                    /* * * * */
                    cloneResponsiveCheckStartTime = DateTime.UtcNow;
                    m_CloneResponsivenessCheckPeriodOneSecond = m_NextCloneResponsivenessCheckPeriodOneSecond;
                    m_UnresponsiveTimeout = m_NextUnresponsiveTimeout;
                }
#endif

                vpContext.MessagingService.HandleUpdate();
            };
            EditorApplication.quitting += () =>
            {
                // :ApplicationEvent :Quit
                // The Main Editor fundamentally closes all its child editors
                // since it coordinates Asset Syncing at the API level
                foreach (var project in vpContext.VirtualProjectsApi.GetProjectsFunc(VirtualProjectsApi.k_FilterAll))
                {
                    project.Close(out _);
                }
            };

            AssetDatabaseCallbacks.OnPostprocessAllAssetsCallback += (didDomainReload, numAssetsChanged) =>
            {
                // If the main editor is still compiling, there will be another refresh/domain reload when it’s complete.
                // So we won’t bother getting the clone to do an intermediate refresh that is likely to have out of date assets that will trigger the assert message.
                if (!EditorApplication.isCompiling)
                    AssetImportEvents.InvokeRequestImport(didDomainReload, numAssetsChanged);
            };

            vpContext.MessagingService.Receive<CloneInitializedMessage>(identifier =>
            {
                // Clone being initialized can be seen as receiving our first heartbeat.
                ApplicationEvents.InvokeCloneHeartbeatReceived(identifier.Identifier, false);

                if (!vpContext.RequestCloneInfo.ActiveClones.ContainsKey(identifier.Identifier))
                {
                    var state = new RequestCloneInfo.CloneInfo
                    {
                        IsCommunicative = true,
                    };
                    vpContext.RequestCloneInfo.ActiveClones.Create(identifier.Identifier, state);
                }
                else
                {
                    vpContext.RequestCloneInfo.ActiveClones.Update(identifier.Identifier, state => state.IsCommunicative = true, out _);
                }
            });
            vpContext.MessagingService.Receive<HeartbeatResponseMessage>(message =>
            {
                ApplicationEvents.InvokeCloneHeartbeatReceived(message.Identifier, message.DebuggerAttached);
            });
            vpContext.MessagingService.Receive<CloneEditorStateMessage>(message =>
            {
                if (!vpContext.RequestCloneInfo.ActiveClones.ContainsKey(message.Identifier))
                {
                    var state = new RequestCloneInfo.CloneInfo
                    {
                        IsPlaying = message.IsPlaying,
                    };
                    vpContext.RequestCloneInfo.ActiveClones.Create(message.Identifier, state);
                }
                else
                {
                    vpContext.RequestCloneInfo.ActiveClones.Update(message.Identifier, state => state.IsPlaying = message.IsPlaying, out _);
                }
            });

            EditorSceneManager.activeSceneChangedInEditMode += (_, _) =>
            {
                EditorApplication.delayCall += () =>
                {
                    SceneEvents.InvokeSceneHierarchyChanged(SceneHierarchy.FromCurrentEditorSceneManager());
                };
            };

            EditorSceneManager.sceneClosed += _ =>
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.delayCall += () =>
                    {
                        SceneEvents.InvokeSceneHierarchyChanged(SceneHierarchy.FromCurrentEditorSceneManager());
                    };
                }
            };

            EditorSceneManager.sceneOpened += (_, _) =>
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.delayCall += () =>
                    {
                        SceneEvents.InvokeSceneHierarchyChanged(SceneHierarchy.FromCurrentEditorSceneManager());
                    };
                }
            };

            EditorSceneManager.sceneSaved += scene =>
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.delayCall += () => { SceneEvents.InvokeSceneSaved(scene.path); };
                }
            };
        }
    }
}
