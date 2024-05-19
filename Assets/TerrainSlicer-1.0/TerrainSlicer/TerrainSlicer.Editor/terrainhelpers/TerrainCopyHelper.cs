using System;
using System.Collections;
using System.Collections.Generic;
using terrainslicer;
using terrainslicer.context;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace terrainhelpers
{
    public class TerrainCopyHelper
    {
        private static bool debugLogging = false;
        private static string slicedFolder = "TerrainCopy";

        private SliceContext context;
        private GameObject sourceTerrainGameObject;
        private Terrain sourceTerrain;
        private string targetPath;
        private readonly SliceProgressInfo progressInfo = new SliceProgressInfo();

        public IEnumerator PerformCopy(Terrain sourceTerrain, Action<SliceProgressInfo> progressListener = null)
        {
            this.sourceTerrain = sourceTerrain;
            sourceTerrainGameObject = sourceTerrain.gameObject;
            context = SliceContext.copyContext(sourceTerrain);
            yield return performSlice(progressListener);
        }
        
        public IEnumerator PerformSlice(SliceContext context, Terrain sourceTerrain, Action<SliceProgressInfo> progressListener = null)
        {
            this.context = context;
            this.sourceTerrain = sourceTerrain;
            sourceTerrainGameObject = sourceTerrain.gameObject;
            return performSlice(progressListener);
        }
        
        public static string GetTargetPath(out string scenePath)
        {
            const int scanLimit = 1000;
            var sceneFilePath = SceneManager.GetActiveScene().path;
            scenePath = sceneFilePath.Substring(0, sceneFilePath.LastIndexOf('/')) + '/';
            var nextEmptyTargetPath = $"{scenePath}{slicedFolder}";
            var checkCounter = 1;
            while (AssetDatabase.IsValidFolder(nextEmptyTargetPath))
            {
                if (checkCounter > scanLimit)
                {
                    throw new Exception($"Too much folders ({scanLimit}) to scan, plz cleanup some folders with pattern [{nextEmptyTargetPath}]");
                }
                nextEmptyTargetPath = $"{scenePath}{slicedFolder} {checkCounter}";
                checkCounter++;
            }
            return nextEmptyTargetPath;
        }

        private string getAssetPath(string name, string dotExtension)
        {
            return $"{targetPath}/{name}{dotExtension}";
        }

        private void initSlicedFolder()
        {
            var targetPathToCheck = GetTargetPath(out var scenePath);
            if (!AssetDatabase.IsValidFolder(targetPathToCheck))
            {
                var scenePathNoSlash = scenePath.TrimEnd('/');
                var folderNameToCreate = targetPathToCheck.Substring(targetPathToCheck.LastIndexOf('/') + 1);
                AssetDatabase.CreateFolder(scenePathNoSlash, folderNameToCreate);
            }

            targetPath = targetPathToCheck;
        }

        private IEnumerator performSlice(Action<SliceProgressInfo> progressListener = null)
        {
            initSlicedFolder();
            
            var totalSteps = Mathf.FloorToInt(
                (float)sourceTerrain.GetComponent<Terrain>().terrainData.heightmapResolution / context.targetResolution);

            progressInfo.Init(context, totalSteps * totalSteps);
            yield return null;
            
            var sharedLayers = context.useSharedLayers ? new Dictionary<string, TerrainLayer>() : null;
        
            LogUtils.DebugLog($"Total slicing steps: {totalSteps}", debugLogging);
            for (var x = 0; x < totalSteps; x++)
            {
                for (var y = 0; y < totalSteps; y++)
                {
                    var index = x * totalSteps + y;
                    yield return createTerrainPart($"SlicedTerrain{index:000}", x, y, totalSteps, sharedLayers, progressListener);
                }
            }

            if (context.hideOriginalTerrain)
            {
                sourceTerrainGameObject.SetActive(false);
            }
        }
    
        private IEnumerator createTerrainPart(
            String targetName,
            int xStep,
            int yStep,
            int totalSteps,
            Dictionary<string, TerrainLayer> sharedLayers,
            Action<SliceProgressInfo> progressListener = null)
        {
            var srcTerrain = sourceTerrain;
            var srcTerrainData = srcTerrain.terrainData;

            var terrainGameObject = UnityEngine.Object.Instantiate(sourceTerrain).gameObject;
            var dstTerrain = terrainGameObject.GetComponent<Terrain>();
            dstTerrain.allowAutoConnect = context.connectSlicedTerrains;
            dstTerrain.drawInstanced = context.drawTerrainInstanced;
        
            var dstTerrainData = new TerrainData();
            CloneUtils.CopyFrom(dstTerrainData, srcTerrain.terrainData);
            dstTerrainData.name = targetName;
            terrainGameObject.name = targetName;
        
            dstTerrain.terrainData = dstTerrainData;
            terrainGameObject.GetComponent<TerrainCollider>().terrainData = dstTerrainData;
            AssetDatabase.CreateAsset(dstTerrainData, getAssetPath(targetName, ".asset"));
        
            HeightmapHelper.copyHeightMap(xStep, yStep, totalSteps, srcTerrainData, dstTerrainData);
            AssetDatabase.SaveAssets();
            
            progressInfo.OnHeightmapCopied();
            progressListener?.Invoke(progressInfo);
            yield return null;
        
            AlphamapHelper.copyAlphamap(xStep, yStep, totalSteps, 
                srcTerrainData, dstTerrainData, 
                context.cleanupUnusedLayers, context.layerUsageToCleanup, context.maxLayersPerTerrain, 
                sharedLayers, out var newLayers, 
                debugLogging);
        
            foreach (var clonedLayer in newLayers)
            {
                AssetDatabase.CreateAsset(clonedLayer, getAssetPath($"{clonedLayer.name}", ".terrainlayer"));
            }
            AssetDatabase.SaveAssets();
        
            progressInfo.OnAlphamapCopied();
            progressListener?.Invoke(progressInfo);
            yield return null;
            
            if (context.copyTrees)
            {
                TreeDetailHelper.copyTrees(xStep, yStep, totalSteps, srcTerrainData, dstTerrain.terrainData, context.optimizeTrees);
                AssetDatabase.SaveAssets();

                progressInfo.OnTreesCopied();
                progressListener?.Invoke(progressInfo);
                yield return null;
            }
            else
            {
                TreeDetailHelper.resetTrees(dstTerrainData);
                AssetDatabase.SaveAssets();
            }

            if (context.copyDetails)
            {
                TreeDetailHelper.copyDetails(xStep, yStep, totalSteps, srcTerrainData, dstTerrain.terrainData, context.optimizeDetails);
                AssetDatabase.SaveAssets();
                
                progressInfo.OnDetailsCopied();
                progressListener?.Invoke(progressInfo);
                yield return null;
            }
            else
            {
                TreeDetailHelper.resetDetails(dstTerrainData);
                AssetDatabase.SaveAssets();
            }

            dstTerrain.transform.position = calculateTerrainPosition(srcTerrain, xStep, yStep, totalSteps, dstTerrainData);
            AssetDatabase.SaveAssets();
            
            progressInfo.OnPartCopied(terrainGameObject);
            progressListener?.Invoke(progressInfo);
            yield return null;
        }

        private Vector3 calculateTerrainPosition(Terrain srcTerrain, int xStep, int yStep, int totalSteps, TerrainData dstTerrain)
        {
            var srcSizeOXZ = new Vector3(srcTerrain.terrainData.size.x, 0f, srcTerrain.terrainData.size.z);
            var srcPosition = srcTerrain.GetPosition();

            var xOffset = xStep * srcSizeOXZ.x / totalSteps;
            var yOffset = yStep * srcSizeOXZ.z / totalSteps;
        
            return srcPosition + new Vector3(xOffset, 0f, yOffset);
        }
    }

    public class SliceProgressInfo
    {
        public bool copyTrees;
        public bool copyDetails;
        public int totalParts;

        public int partsCopied;
        public bool heightCopied;
        public bool alphaAndLayersCopied;
        public bool treesCopied;
        public bool detailsCopied;

        //Actual only for copy
        public GameObject singleResult;

        public void Init(SliceContext context, int totalSliceSteps)
        {
            totalParts = totalSliceSteps;
            partsCopied = 0;
            copyDetails = context.copyDetails;
            copyTrees = context.copyTrees;
            ResetSubProgress();
        }

        private void ResetSubProgress()
        {
            heightCopied = false;
            alphaAndLayersCopied = false;
            treesCopied = false;
            detailsCopied = false;
        }

        public void OnPartCopied(GameObject partGameObject)
        {
            singleResult = partGameObject;
            partsCopied++;
            ResetSubProgress();
        }

        public void OnHeightmapCopied()
        {
            heightCopied = true;
        }

        public void OnAlphamapCopied()
        {
            alphaAndLayersCopied = true;
        }

        public void OnTreesCopied()
        {
            treesCopied = true;
        }

        public void OnDetailsCopied()
        {
            detailsCopied = true;
        }

        public bool IsDone()
        {
            return partsCopied >= totalParts;
        }

        public float GetProgress()
        {
            if (partsCopied >= totalParts)
            {
                return 1f;
            }
            
            var singleStepLength = 1f / totalParts;

            var stepProgress = 0f;
            
            var defaultSubStepIncrement = 0.25f; // 1f / (height, alpha, trees, details)
            var subStepIncrement = defaultSubStepIncrement;
            if (!copyTrees)
            {
                subStepIncrement += defaultSubStepIncrement;
            }

            if (!copyDetails)
            {
                subStepIncrement += defaultSubStepIncrement;
            }
            
            if (heightCopied)
            {
                stepProgress += subStepIncrement;
            }

            if (alphaAndLayersCopied)
            {
                stepProgress += subStepIncrement;
            }

            if (copyTrees && treesCopied)
            {
                stepProgress += subStepIncrement;
            }

            if (copyDetails && detailsCopied)
            {
                stepProgress += subStepIncrement;
            }

            return singleStepLength * partsCopied + singleStepLength * stepProgress;
        }
    }
}