using UnityEngine;

namespace terrainslicer
{
    [ExecuteInEditMode]
    public class TerrainTreeDetailsCopyController : MonoBehaviour
    {
        public Terrain sourceTerrain;
        public bool copyDetails = true;
        public bool copyTrees = true;

        public void PerformCopy()
        {
            if (!sourceTerrain)
            {
                return;
            }

            var dstTerrainData = gameObject.GetComponent<Terrain>().terrainData; 

            if (copyDetails)
            {
                TreeDetailHelper.copyDetailPrototypes(sourceTerrain.terrainData, dstTerrainData);
            }

            if (copyTrees)
            {
                TreeDetailHelper.copyTreePrototypes(sourceTerrain.terrainData, dstTerrainData);
            }
        }
    }
}