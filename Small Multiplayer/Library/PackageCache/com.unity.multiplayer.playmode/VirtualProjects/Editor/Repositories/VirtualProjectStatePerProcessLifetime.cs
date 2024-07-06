using System;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    // State we keep on the main editor that is capable of surviving a domain reload
    [Serializable]
    class VirtualProjectStatePerProcessLifetime
    {
        public string[] LaunchArgs;
        [SerializeField]
        public DateTime FirstHeartbeatReceived;
        [SerializeField]
        public DateTime LastHeartbeatReceived;
        public bool IsDebuggerAttached;
        public int Retry;

        public override string ToString()
        {
            return $"{nameof(FirstHeartbeatReceived)}: {FirstHeartbeatReceived}, {nameof(LastHeartbeatReceived)}: {LastHeartbeatReceived}, {nameof(Retry)}: {Retry}";
        }
    }
}
