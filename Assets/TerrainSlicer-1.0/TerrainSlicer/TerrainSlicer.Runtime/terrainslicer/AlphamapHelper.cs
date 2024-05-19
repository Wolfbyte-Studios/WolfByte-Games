using System.Collections.Generic;
using terrainslicer.layers;
using UnityEngine;

namespace terrainslicer
{
    public static class AlphamapHelper
    {        
        public static void copyAlphamap(
            int xStep, int yStep, int totalSteps,
            TerrainData srcTerrainData, TerrainData dstTerrainData,
            bool cleanupUnusedLayers,
            float layerUsageToCleanup,
            int maxLayersPerTerrain,
            Dictionary<string, TerrainLayer> sharedLayers,
            out List<TerrainLayer> newLayers,
            bool debugLog = false)
        {
        
            var resolution = Mathf.FloorToInt((float)srcTerrainData.alphamapResolution / totalSteps);
            var xOffset = xStep * resolution;
            var yOffset = yStep * resolution;
        
            var srcAlphamap = srcTerrainData.GetAlphamaps(xOffset, yOffset, resolution, resolution);

            var layerSuffix = $"_{xStep * totalSteps + yStep}_";

            LayersHelper.cloneOrRemapLayers(
                srcTerrainData,
                srcAlphamap,
                out var layerRemap,
                out var dstLayers,
                out newLayers,
                layerSuffix,
                cleanupUnusedLayers,
                layerUsageToCleanup,
                maxLayersPerTerrain,
                sharedLayers,
                debugLog);

            dstTerrainData.terrainLayers = dstLayers.ToArray();

            var alphamap = new float[resolution, resolution, dstLayers.Count];
            
            for (var x = 0; x < resolution; x++)
            {
                for (var y = 0; y < resolution; y++)
                {
                    //TODO: place-a-start
                    var cellAlphaSum = 0f;
                    var containMissedLayers = false;
                    for (var t = 0; t < srcTerrainData.alphamapLayers; t++)
                    {
                        if (!layerRemap.ContainsKey(t))
                        {
                            //Normalize such cells to be in sum = 1f
                            containMissedLayers = true;
                            continue;
                        }

                        var remappedIndex = layerRemap[t];
                        alphamap[x, y, remappedIndex] = srcAlphamap[x, y, t];
                        cellAlphaSum += srcAlphamap[x, y, t];
                    }

                    if (containMissedLayers)
                    {
                        if (cellAlphaSum < 0.3f)
                        {
                            for (var t = 0; t < dstLayers.Count; t++)
                            {
                                //Replace with base layer
                                var value = t == 0 ? 1f : 0f;
                                alphamap[x, y, t] = value;
                            }
                        }
                        else
                        {
                            for (var t = 0; t < dstLayers.Count; t++)
                            {
                                //Normalize cells with missing layers
                                alphamap[x, y, t] /= cellAlphaSum;
                            }
                        }
                    }
                    //TODO: place-a-end
                }
            }

            dstTerrainData.alphamapResolution = resolution;
            dstTerrainData.SetAlphamaps(0, 0, alphamap);
        }

        public static Dictionary<int, int> CalculateLayerRemap(int srcLayersCount, List<int> indexToRemove)
        {
            var layerRemap = new Dictionary<int, int>();
            var dstIndex = 0;

            for (var i = 0; i < srcLayersCount; i++)
            {
                if (indexToRemove.Contains(i))
                {
                    continue;
                }
                layerRemap.Add(i, dstIndex);
                dstIndex++;
            }

            return layerRemap;
        }

        //TODO: replace place-a-start/place-a-end with call to this method
        public static void CleanupAndRemapCellLayers(int x, int y, 
            Dictionary<int, int> layerRemap,
            int srcLayersCount,
            float[,,] srcAlphamap,
            float[,,] dstAlphamap,
            int indexToReplaceWith = -1)
        {
            var cellAlphaSum = 0f;
            var deletedLayersCount = 0;
            for (var t = 0; t < srcLayersCount; t++)
            {
                if (!layerRemap.ContainsKey(t))
                {
                    deletedLayersCount++;
                    continue;
                }

                var remappedIndex = layerRemap[t];
                dstAlphamap[x, y, remappedIndex] = srcAlphamap[x, y, t];
                cellAlphaSum += srcAlphamap[x, y, t];
            }

            if (deletedLayersCount > 0)
            {
                var dstLayersCount = srcLayersCount - deletedLayersCount;
                if (cellAlphaSum < 0.3f)
                {
                    var remappedIndexToReplace =
                        indexToReplaceWith >= 0 ? layerRemap[indexToReplaceWith] : indexToReplaceWith;
                    for (var t = 0; t < dstLayersCount; t++)
                    {
                        //Replace with base layer or with indexToReplaceWith
                        var value = t == remappedIndexToReplace
                                    || remappedIndexToReplace < 0 && t == 0 
                                    ? 1f : 0f;
                        dstAlphamap[x, y, t] = value;
                    }
                }
                else
                {
                    for (var t = 0; t < dstLayersCount; t++)
                    {
                        //Normalize cells with missing layers
                        dstAlphamap[x, y, t] /= cellAlphaSum;
                    }
                }
            }
        }
    }
}