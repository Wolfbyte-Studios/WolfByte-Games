using terrainslicer.context;
using UnityEngine;

namespace terrainslicer
{
    [ExecuteInEditMode]
    public class TerrainSlicerController : MonoBehaviour
    {
        public SliceContext context = new SliceContext();
        public float slicingProgress = -1;

        private void OnEnable()
        {
            slicingProgress = -1f;
        }
    }
}