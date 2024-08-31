using System;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class MainApplicationEvents
    {
        public event Action<VirtualProjectIdentifier, bool> CloneHeartbeatReceived;
        public event Action<VirtualProjectIdentifier> CloneUnresponsive;

        public void InvokeCloneHeartbeatReceived(VirtualProjectIdentifier identifier, bool debuggerAttached)
        {
            CloneHeartbeatReceived?.Invoke(identifier, debuggerAttached);
        }

        public void InvokeCloneUnresponsive(VirtualProjectIdentifier identifier)
        {
            CloneUnresponsive?.Invoke(identifier);
        }
    }
}
