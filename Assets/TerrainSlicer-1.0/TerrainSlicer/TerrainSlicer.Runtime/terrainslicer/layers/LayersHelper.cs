using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace terrainslicer.layers
{
    public static class LayersHelper
    {
        public static void cloneOrRemapLayers(
            TerrainData srcTerrainData,
            float[,,] srcAlphamap,
            out Dictionary<int, int> layerRemap,
            out List<TerrainLayer> clonedLayers,
            out List<TerrainLayer> newLayers,
            string layerSuffix,
            bool cleanupUnusedLayers,
            float layerUsageToCleanup,
            int maxLayersPerTerrain,
            Dictionary<string, TerrainLayer> sharedLayers,
            bool debugLog = false)
        {

            newLayers = new List<TerrainLayer>();
            
            if (cleanupUnusedLayers)
            {
                findRemapOnlyUsedLayers(
                    srcTerrainData, 
                    sharedLayers,
                    newLayers,
                    srcAlphamap,
                    layerUsageToCleanup,
                    maxLayersPerTerrain,
                    out layerRemap, 
                    out clonedLayers,
                    layerSuffix,
                    debugLog);
            }
            else
            {
                cloneSourceLayers(
                    srcTerrainData,
                    sharedLayers,
                    newLayers,
                    out layerRemap, 
                    out clonedLayers, 
                    layerSuffix
                );
            }
        }
        
        private static void cloneSourceLayers(
            TerrainData srcTerrainData,
            Dictionary<string, TerrainLayer> sharedLayers,
            List<TerrainLayer> newLayers,
            out Dictionary<int, int> layerRemap,
            out List<TerrainLayer> clonedLayers,
            string layerSuffix)
        {
            layerRemap = new Dictionary<int, int>();
            clonedLayers = new List<TerrainLayer>();

            var index = 0;
            foreach (var srcLayer in srcTerrainData.terrainLayers)
            {
                layerRemap.Add(index, index);
                clonedLayers.Add(cloneLayerOrChooseFromShared(
                    srcLayer,
                    sharedLayers,
                    newLayers,
                    layerSuffix 
                ));
                index++;
            }
        }

        private static void normalizeLayerUsage(float[] layerUsage)
        {
            var max = layerUsage.Sum();
            for (var i = 0; i < layerUsage.Length; i++)
            {
                layerUsage[i] /= max;
            }
        }

        private static Dictionary<int, float> sortAndIndexLayerUsage(float[] layerUsage, int maxLayers)
        {
            normalizeLayerUsage(layerUsage);
            return layerUsage.Select((usage, index) => new KeyValuePair<int, float>(index, usage))
                .OrderByDescending(pair => pair.Value)
                .ToList()
                .GetRange(0, maxLayers >= layerUsage.Length ? layerUsage.Length : maxLayers)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private static void findRemapOnlyUsedLayers(
            TerrainData srcTerrainData,
            Dictionary<string, TerrainLayer> sharedLayers,
            List<TerrainLayer> newLayers,
            float[,,] srcAlphamap,
            float layerUsageToCleanup,
            int maxLayersPerTerrain,
            out Dictionary<int, int> layerRemap,
            out List<TerrainLayer> actualLayers,
            string layerSuffix,
            bool debugLog = false)
        {
            var layerUsage = new float[srcTerrainData.alphamapLayers];
            
            for (var x = 0; x < srcAlphamap.GetLength(0); x++)
            {
                for (var y = 0; y < srcAlphamap.GetLength(1); y++)
                {
                    for (var t = 0; t < srcTerrainData.alphamapLayers; t++)
                    {
                        layerUsage[t] += srcAlphamap[x, y, t];
                    }
                }
            }

            layerRemap = new Dictionary<int, int>();
            actualLayers = new List<TerrainLayer>();
            var actualIndex = 0;

            var usefulLayersIndexMap = sortAndIndexLayerUsage(layerUsage, maxLayersPerTerrain);
            
            for (var i = 0; i < layerUsage.Length; i++)
            {
                if (!usefulLayersIndexMap.ContainsKey(i) || layerUsage[i] < layerUsageToCleanup)
                {
                    LogUtils.DebugLog($"Cleanup layer {srcTerrainData.terrainLayers[i].name}, usage={layerUsage[i]}", debugLog);
                    continue;
                }
                
                LogUtils.DebugLog($"Leave cloned layer {srcTerrainData.terrainLayers[i].name}, usage={layerUsage[i]}", debugLog);
                
                layerRemap.Add(i, actualIndex);
                
                actualLayers.Add(cloneLayerOrChooseFromShared(
                    srcTerrainData.terrainLayers[i],
                    sharedLayers,
                    newLayers,
                    layerSuffix
                ));
                
                actualIndex++;
            }
        }
        
        private static TerrainLayer cloneLayerOrChooseFromShared(
            TerrainLayer source,
            Dictionary<string, TerrainLayer> sharedLayers,
            List<TerrainLayer> newLayers,
            string newLayerSuffix)
        {
            if (sharedLayers != null && sharedLayers.ContainsKey(source.name))
            {
                return sharedLayers[source.name];
            }

            var clonedLayer = new TerrainLayer();
            CloneUtils.CopyFrom(clonedLayer, source);
            clonedLayer.name += newLayerSuffix;
            sharedLayers?.Add(source.name, clonedLayer);
            newLayers.Add(clonedLayer);
            return clonedLayer;
        }
    }
}