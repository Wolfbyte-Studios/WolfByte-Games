using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
namespace WaffleWare.Examples
{
    [ExecuteInEditMode]
    public class AlignCam : MonoBehaviour
    {
        public Camera gameCam;
        public bool updateView = false;
        private void OnDrawGizmos()
        {
            if (updateView)
            {
                SceneView sceneCam = SceneView.lastActiveSceneView;
                gameCam.transform.position = sceneCam.camera.transform.position;
                gameCam.transform.rotation = sceneCam.camera.transform.rotation;

                if (sceneCam.camera.orthographic != gameCam.orthographic)
                    gameCam.orthographic = !gameCam.orthographic;

                if (sceneCam.camera.orthographic)
                    gameCam.orthographicSize = sceneCam.camera.orthographicSize;
            }
        }
    }
}
#endif