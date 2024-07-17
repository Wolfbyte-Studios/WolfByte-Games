using System.IO;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class BroadcastOpenPlayerWindowMessage
    {
        public BroadcastOpenPlayerWindowMessage(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }

        public PlayerIndex PlayerIndex { get; }

        static void Serialize(BinaryWriter writer, object message)
        {
            var value = message as BroadcastOpenPlayerWindowMessage;
            writer.Write((int)value.PlayerIndex);
        }

        static object Deserialize(BinaryReader reader)
        {
            return new BroadcastOpenPlayerWindowMessage((PlayerIndex)reader.ReadInt32());
        }

        [SerializeMessageDelegates] // ReSharper disable once UnusedMember.Global
        public static SerializeMessageDelegates SerializeMethods() => new SerializeMessageDelegates { SerializeFunc = Serialize, DeserializeFunc = Deserialize };
    }
}
