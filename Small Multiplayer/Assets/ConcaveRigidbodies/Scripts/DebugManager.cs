#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaffleWare.Examples
{
    public class DebugManager : MonoBehaviour
    {
        public bool useSceneView = false;
        private void Awake()
        {
            if (useSceneView)
                UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        }
    }
}

#endif