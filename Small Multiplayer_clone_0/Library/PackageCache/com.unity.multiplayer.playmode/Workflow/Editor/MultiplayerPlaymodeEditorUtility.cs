using Unity.Multiplayer.Playmode.Common.Editor;
using Unity.Multiplayer.Playmode.Common.Runtime;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    static class MultiplayerPlaymodeEditorUtility
    {
        public static bool IsPlayerActivateProhibited => EditorUtility.scriptCompilationFailed;

        public static void RevealInFinder(UnityPlayer player)
        {
            var vpContext = EditorContexts.MainEditorContext;
            var directory = vpContext.VirtualProjectsApi.TryGetFunc(player.TypeDependentPlayerInfo.VirtualProjectIdentifier, out var project, out _)
                ? project.Directory
                : Paths.CurrentProjectVirtualProjectsFolder;
            EditorUtility.RevealInFinder(directory);
        }

        public enum FocusPlayerStatus
        {
            None,
            IsNotReady,
            PlayerTypeCannotBeFocused,
            PlayerNotFound,
        }

        public static FocusPlayerStatus FocusPlayerView(PlayerIndex playerIndex)
        {
            if (VirtualProjectsEditor.IsClone)
            {
                EditorContexts.CloneContext.MessagingService.Broadcast(new BroadcastOpenPlayerWindowMessage(playerIndex), null, MppmLog.Warning);
            }
            else
            {
                var player = playerIndex switch
                {
                    PlayerIndex.Player1 => MultiplayerPlaymode.PlayerOne,
                    PlayerIndex.Player2 => MultiplayerPlaymode.PlayerTwo,
                    PlayerIndex.Player3 => MultiplayerPlaymode.PlayerThree,
                    PlayerIndex.Player4 => MultiplayerPlaymode.PlayerFour,
                    _ => null,
                };

                if (player == null) return FocusPlayerStatus.PlayerNotFound;
                if (player.PlayerState is not (PlayerState.Launched or PlayerState.Launching)) return FocusPlayerStatus.IsNotReady;
                if (player.Type == PlayerType.Main) return FocusPlayerStatus.PlayerTypeCannotBeFocused;
                if (player.TypeDependentPlayerInfo.VirtualProjectIdentifier == null) return FocusPlayerStatus.PlayerTypeCannotBeFocused;

                EditorContexts.MainEditorContext.MessagingService.Send(new OpenPlayerWindowMessage(), player.TypeDependentPlayerInfo.VirtualProjectIdentifier,null, MppmLog.Warning);
            }

            return FocusPlayerStatus.None;
        }

        internal static void ErrorFirstTestFailure()
        {
            try {
                var message = VirtualProjectWorkflow.WorkflowMainEditorContext.TestFailure;
                if (message != null)
                {
                    var hasPlayer = Filters.FindFirstPlayerWithVirtualProjectsIdentifier(VirtualProjectWorkflow.WorkflowMainEditorContext.SystemDataStore.LoadAllPlayerJson(), message.Identifier, out var player);
                    Debug.Assert(hasPlayer, $"Unknown Player with VP ID {message.Identifier}");
                    Debug.LogError($"'{message.ResultMessage}'\n" +
                                   $"File[{message.CallingFilePath}]\n" +
                                   $"Line[{message.LineNumber}]\n" +
                                   $"Player[{player.Name}/{message.Identifier}]");
                }
            }  finally {
                VirtualProjectWorkflow.WorkflowMainEditorContext.TestFailure = null;
            }
        }
    }
}
