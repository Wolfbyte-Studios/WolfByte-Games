using System.Collections.Generic;
using System.Linq;
using terrainslicer;
using terrainslicer.utils;
using UnityEngine;

namespace layercleanup
{
    [ExecuteInEditMode]
    public class TerrainLayerUsageOperations : MonoBehaviour
    {
        private List<float> terrainUsage;
        
        public Terrain terrain;
        public float copyProgress = -1;
        
        private void OnEnable()
        {
            terrain = GetComponent<Terrain>();
            copyProgress = -1f;
        }

        public Terrain GetTerrain()
        {
            if (!terrain)
            {
                terrain = GetComponent<Terrain>();
            }
            return terrain;
        }

        public List<float> GetTerrainUsage()
        {
            return terrainUsage;
        }

        public void CalculateLayerUsage()
        {
            var data = GetTerrain().terrainData;
            var terrainLayerUsage = new float[data.alphamapLayers];
            var alphamap = data.GetAlphamaps(0, 0, data.alphamapResolution, data.alphamapResolution);
            CommonUtils.VisitAllGridCells(data.alphamapResolution, context =>
            {
                for (var t = 0; t < data.alphamapLayers; t++)
                {
                    terrainLayerUsage[t] += alphamap[context.x, context.y, t];
                }
            });

            var sum = terrainLayerUsage.Sum();
            if (sum < 0.001f)
            {
                sum = 1f;
            }
            terrainUsage = terrainLayerUsage.Select(usage => 100f * usage / sum).ToList();
        }

        public void RemoveLayerIncreasingOthersOrBase(int index)
        {
            var data = GetTerrain().terrainData;
            var srcAlphamap = data.GetAlphamaps(0, 0, data.alphamapResolution, data.alphamapResolution);
            var dstAlphamap = new float[data.alphamapResolution, data.alphamapResolution, data.alphamapLayers - 1];

            var indicesToRemove = new List<int> {index};
            var layerRemap = AlphamapHelper.CalculateLayerRemap(data.alphamapLayers, indicesToRemove);

            CommonUtils.VisitAllGridCells(data.alphamapResolution, context => 
                AlphamapHelper.CleanupAndRemapCellLayers(context.x, context.y, layerRemap, data.alphamapLayers,
                    srcAlphamap, dstAlphamap)
            );

            var dstTerrainLayers = new List<TerrainLayer>();
            for (var i = 0; i < data.alphamapLayers; i++)
            {
                if (indicesToRemove.Contains(i))
                {
                    continue;
                }
                dstTerrainLayers.Add(data.terrainLayers[i]);
            }

            data.terrainLayers = dstTerrainLayers.ToArray();
            data.SetAlphamaps(0, 0, dstAlphamap);
            
            CalculateLayerUsage();
        }

        public void RemoveLayerReplacingWithOther(int indexToRemove, int indexToReplaceWith)
        {
            var data = GetTerrain().terrainData;
            var srcAlphamap = data.GetAlphamaps(0, 0, data.alphamapResolution, data.alphamapResolution);
            var dstAlphamap = new float[data.alphamapResolution, data.alphamapResolution, data.alphamapLayers - 1];

            var indicesToRemove = new List<int> {indexToRemove};
            var layerRemap = AlphamapHelper.CalculateLayerRemap(data.alphamapLayers, indicesToRemove);
            
            CommonUtils.VisitAllGridCells(data.alphamapResolution, context => 
                AlphamapHelper.CleanupAndRemapCellLayers(context.x, context.y, layerRemap, data.alphamapLayers,
                    srcAlphamap, dstAlphamap, indexToReplaceWith)
            );

            var dstTerrainLayers = new List<TerrainLayer>();
            for (var i = 0; i < data.alphamapLayers; i++)
            {
                if (indicesToRemove.Contains(i))
                {
                    continue;
                }
                dstTerrainLayers.Add(data.terrainLayers[i]);
            }

            data.terrainLayers = dstTerrainLayers.ToArray();
            data.SetAlphamaps(0, 0, dstAlphamap);
            
            CalculateLayerUsage();
        }

        //TODO: calculate totalLayers, remove from args
        public void SwapLayers(int layer1, int layer2)
        {
            var fromToMap = new Dictionary<int, int>();
            for (var i = 0; i < GetTerrain().terrainData.alphamapLayers; i++)
            {
                var targetIndex = i == layer1
                    ? layer2
                    : i == layer2
                        ? layer1
                        : i;
                fromToMap.Add(i, targetIndex);
            }
            RemapLayers(fromToMap);
        }

        private void RemapLayers(Dictionary<int, int> fromToIndexMap)
        {
            var data = GetTerrain().terrainData;
            var dstTerrainLayers = new List<TerrainLayer>();
            for (var i = 0; i < data.alphamapLayers; i++)
            {
                dstTerrainLayers.Add(data.terrainLayers[fromToIndexMap[i]]);
            }

            data.terrainLayers = dstTerrainLayers.ToArray();
        }
    }
}