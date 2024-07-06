using System;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    static class CloneSyncModal
    {
        public enum Choices
        {
            Restart,
            CloseEditor,
            ContinueAnyways,
        }

        internal static Choices DisplayCloneSyncModal(string whatHappened)
        {
            Debug.Log(whatHappened);
            var option = EditorUtility.DisplayDialogComplex(
                whatHappened,
                "Do you want to restart the clone?",
                "Restart",
                "Close clone",
                "Continue anyways");

            return option switch
            {
                0 => Choices.Restart,
                1 => Choices.CloseEditor,
                2 => Choices.ContinueAnyways,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        internal static void DefaultChoiceHandler(Choices choice, VirtualProject project, VirtualProjectStatePerProcessLifetime statePerProcessLifetime)
        {
            switch (choice)
            {
                case Choices.Restart:
                    var args = statePerProcessLifetime.LaunchArgs;
                    project.Close(out _); // NOTE: This will kill the state per life time
                    project.Launch(out _, out _, args);
                    break;
                case Choices.CloseEditor:
                    project.Close(out _);
                    break;
                case Choices.ContinueAnyways:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
