using UnityEngine;

namespace terrainslicer
{
    public static class HeightmapHelper
    {
        private static bool debugLogging = false;
        
        public static void copyHeightMap(int xStep, int yStep, int totalSteps,
            TerrainData srcTerrainData, TerrainData dstTerrainData)
        {

            var resolution = Mathf.FloorToInt((srcTerrainData.heightmapResolution - 1f) / totalSteps);
            var xOffset = xStep * resolution;
            var yOffset = yStep * resolution;
            var copyResolution = resolution + 1;
        
            LogUtils.DebugLog($"copyHeightmap, x/y: {xOffset} / {yOffset}, resolution: {resolution}", debugLogging);
            LogUtils.DebugLog($"Trying to read from {xOffset} - {xOffset + resolution}, {yOffset} - {yOffset + resolution}", debugLogging);

            var srcHeights = srcTerrainData.GetHeights(xOffset, yOffset, copyResolution, copyResolution);

            var minHeight = float.MaxValue;
            var maxHeight = float.MinValue;

            var heights = new float[copyResolution, copyResolution];
            for (var x = 0; x < copyResolution; x++)
            {
                for (var y = 0; y < copyResolution; y++)
                {
                    var srcHeight = srcHeights[x, y];
                    heights[x, y] = srcHeight;
                    minHeight = Mathf.Min(minHeight, srcHeight);
                    maxHeight = Mathf.Max(maxHeight, srcHeight);
                }
            }

            dstTerrainData.heightmapResolution = resolution;
            dstTerrainData.SetHeights(0, 0, heights);
        }
    }
}