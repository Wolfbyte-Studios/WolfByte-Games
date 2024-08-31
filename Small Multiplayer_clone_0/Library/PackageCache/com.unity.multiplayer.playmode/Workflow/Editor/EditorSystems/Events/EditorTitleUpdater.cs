using System;
using Unity.Multiplayer.Playmode.InternalBridge.Editor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class EditorTitleUpdater
    {
        readonly ApplicationTitleDescriptorProxy m_ApplicationTitle;

        public EditorTitleUpdater(ApplicationTitleDescriptorProxy applicationTitle)
        {
            m_ApplicationTitle = applicationTitle ?? throw new ArgumentNullException(nameof(applicationTitle));
        }


        public string title
        {
            set => m_ApplicationTitle.title = value;
        }
    }
}
