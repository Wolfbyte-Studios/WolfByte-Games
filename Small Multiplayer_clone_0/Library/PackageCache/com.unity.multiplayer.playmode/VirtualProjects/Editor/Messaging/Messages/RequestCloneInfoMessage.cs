using System.IO;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    class RequestCloneInfoMessage
    {
        static void Serialize(BinaryWriter writer, object value)
        {
        }

        static object Deserialize(BinaryReader reader)
        {
            return new RequestCloneInfoMessage();
        }

        [SerializeMessageDelegates] // ReSharper disable once UnusedMember.Global
        public static SerializeMessageDelegates SerializeMethods() => new SerializeMessageDelegates { SerializeFunc = Serialize, DeserializeFunc = Deserialize };
    }
}
