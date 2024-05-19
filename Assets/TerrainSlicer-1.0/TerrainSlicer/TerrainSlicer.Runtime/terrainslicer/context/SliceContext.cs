using UnityEngine;

namespace terrainslicer.context
{
    public class SliceContext
    {
        public int targetResolution = 512;
        public bool copyDetails = true;
        public bool optimizeDetails = true;
        public bool copyTrees = true;
        public bool optimizeTrees = true;
        public bool connectSlicedTerrains = true;
        public bool drawTerrainInstanced = false;
        public bool hideOriginalTerrain = true;
        public bool cleanupUnusedLayers = false;
        public bool useSharedLayers = true;
        public float layerUsageToCleanup = 0.01f;
        public int maxLayersPerTerrain = 8;

        public static SliceContext copyContext(Terrain terrain)
        {
            return new SliceContext
            {
                targetResolution = terrain.terrainData.heightmapResolution,
                copyDetails = true,
                optimizeDetails = false,
                copyTrees = true,
                optimizeTrees = false,
                hideOriginalTerrain = false,
                cleanupUnusedLayers = false,
                useSharedLayers = true,
                maxLayersPerTerrain = terrain.terrainData.alphamapLayers
            };
        }
    }
}