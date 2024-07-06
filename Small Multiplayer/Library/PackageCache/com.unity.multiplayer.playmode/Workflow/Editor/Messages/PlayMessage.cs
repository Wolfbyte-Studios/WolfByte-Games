using System.IO;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class PlayMessage
    {
        static void Serialize(BinaryWriter writer, object value)
        {
        }

        static object Deserialize(BinaryReader reader)
        {
            return new PlayMessage();
        }

        [SerializeMessageDelegates] // ReSharper disable once UnusedMember.Global
        public static SerializeMessageDelegates SerializeMethods() => new SerializeMessageDelegates { SerializeFunc = Serialize, DeserializeFunc = Deserialize };
    }
}
