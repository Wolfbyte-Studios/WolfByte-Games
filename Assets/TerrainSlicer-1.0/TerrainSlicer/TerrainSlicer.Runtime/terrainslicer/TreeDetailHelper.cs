using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace terrainslicer
{
    public static class TreeDetailHelper
    {
        private static bool debugLogging = false;

        public static void resetDetails(TerrainData dstTerrainData)
        {
            dstTerrainData.detailPrototypes = new DetailPrototype[0];
        }

        public static void copyDetailPrototypes(TerrainData srcTerrainData, TerrainData dstTerrainData)
        {
            var clonedPrototypes = srcTerrainData.detailPrototypes
                .Select(proto => new DetailPrototype(proto)).ToArray();
            dstTerrainData.detailPrototypes = clonedPrototypes;
        }
        
        public static void copyDetails(int xStep, int yStep, int totalSteps, 
            TerrainData srcTerrainData, 
            TerrainData dstTerrainData, 
            bool optimizeDetails)
        {
            var resolution = Mathf.FloorToInt((float)srcTerrainData.detailResolution / totalSteps);
            var xOffset = xStep * resolution;
            var yOffset = yStep * resolution;

            var existingLayers = getExistingDetailLayers(xOffset, yOffset, resolution, srcTerrainData, optimizeDetails);

            var clonedPrototypes = srcTerrainData.detailPrototypes
                .Where((proto, index) => existingLayers.Contains(index))
                .Select(proto => new DetailPrototype(proto)).ToArray();
            dstTerrainData.detailPrototypes = clonedPrototypes;
            dstTerrainData.SetDetailResolution(resolution, srcTerrainData.detailResolutionPerPatch);

            for (var i = 0; i < existingLayers.Length; i++)
            {
                var srcLayerIndex = existingLayers[i];
                var dstLayerIndex = i;
                
                var layerDetails = srcTerrainData.GetDetailLayer(xOffset, yOffset, resolution, resolution, srcLayerIndex);
                dstTerrainData.SetDetailLayer(0, 0, dstLayerIndex, layerDetails);
            }
        }

        public static void copyTreePrototypes(TerrainData srcTerrainData, TerrainData dstTerrainData)
        {
            dstTerrainData.treePrototypes =
                srcTerrainData.treePrototypes.Select(proto => new TreePrototype(proto)).ToArray();
        }

        public static void resetTrees(TerrainData dstTerrainData)
        {
            dstTerrainData.treeInstances = new TreeInstance[0];
            dstTerrainData.treePrototypes = new TreePrototype[0];
        }

        public static void copyTrees(int xStep, int yStep, int totalSteps, TerrainData srcTerrainData, 
            TerrainData dstTerrainData, bool optimizeTrees)
        {
            var min = new Vector3((float)xStep / totalSteps, 0f, (float)yStep / totalSteps);
            var max = new Vector3((xStep + 1f) / totalSteps, 0f, (yStep + 1f) / totalSteps);
        
            LogUtils.DebugLog($"Copy trees for area {min} - {max}", debugLogging);

            var existingTrees = srcTerrainData.treeInstances
                .Where(treeInstance => treeInstance.position.x >= min.x && treeInstance.position.x < max.x
                                                                        && treeInstance.position.z >= min.z &&
                                                                        treeInstance.position.z < max.z)
                .ToArray();

            var srcTreeInstancesToCopyPrototypes =
                optimizeTrees ? existingTrees : srcTerrainData.treeInstances; 
            calculateTreeProtoIndexRemap(srcTerrainData.treePrototypes,
                srcTreeInstancesToCopyPrototypes,
                out var indexRemap, out var dstPrototypes);
            dstTerrainData.treePrototypes = dstPrototypes.ToArray();
        
            var clonedTrees = existingTrees.Select(tree =>
                {
                    var clonedTree = copyTreeInstance(tree);
                    clonedTree.position = (clonedTree.position - min) * (float)totalSteps; //Ignore wrong Y scale, because will re-align trees
                    clonedTree.prototypeIndex = indexRemap[clonedTree.prototypeIndex];
                    LogUtils.DebugLog($"Re-position tree [{tree.prototypeIndex}] from {tree.position} to {clonedTree.position}", debugLogging);
                    return clonedTree;
                }).ToArray();
        
            LogUtils.DebugLog($"Total trees found: {clonedTrees.Length}", debugLogging);
        
            dstTerrainData.SetTreeInstances(clonedTrees, true);
        }

        private static int[] getExistingDetailLayers(int xOffset, int yOffset, int resolution,
            TerrainData srcTerrainData, bool optimizeDetails)
        {
            if (optimizeDetails)
            {
                return srcTerrainData.GetSupportedLayers(xOffset, yOffset, resolution, resolution);
            }

            return Enumerable.Range(0, srcTerrainData.detailPrototypes.Length).Select(i => i).ToArray();
        }
        
        private static void calculateTreeProtoIndexRemap(TreePrototype[] srcPrototypes, TreeInstance[] existingTrees,
            out Dictionary<int, int> indexRemap,
            out List<TreePrototype> dstPrototypes)
        {
            indexRemap = new Dictionary<int, int>();
            dstPrototypes = new List<TreePrototype>();

            var existingTreeIndices = existingTrees.Select(treeInstance => treeInstance.prototypeIndex).Distinct().ToArray();
            
            for (var i = 0; i < existingTreeIndices.Length; i++)
            {
                var existingTreeIndex = existingTreeIndices[i];
                var srcPrototype = srcPrototypes[existingTreeIndex];
                indexRemap.Add(existingTreeIndex, i);
                dstPrototypes.Add(new TreePrototype(srcPrototype));
            }
        }

        private static TreeInstance copyTreeInstance(TreeInstance source)
        {
            return new TreeInstance
            {
                color = source.color,
                position = source.position,
                rotation = source.rotation,
                heightScale = source.heightScale,
                lightmapColor = source.lightmapColor,
                prototypeIndex = source.prototypeIndex,
                widthScale = source.widthScale
            };
        }
    }
}