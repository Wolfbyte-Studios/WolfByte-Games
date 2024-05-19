using System;
using System.Collections;
using terrainhelpers;
using terrainslicer;
using terrainslicer.context;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace TerrainSlicer
{
    [CustomEditor(typeof(TerrainSlicerController))]
    public class TerrainSlicerEditorScript : Editor
    {
        private void OnEnable()
        {
            var slicer = (TerrainSlicerController) target;
            var sliceContext = slicer.context;

            var terrain = slicer.gameObject.GetComponent<Terrain>();

            sliceContext.targetResolution =
                Math.Min(sliceContext.targetResolution, terrain.terrainData.heightmapResolution - 1);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var slicer = (TerrainSlicerController) target;
            var terrain = slicer.gameObject.GetComponent<Terrain>();
            var sliceContext = slicer.context;

            if (!terrain)
            {
                return;
            }

            EditorGUILayout.LabelField("Source resolution", $"{terrain.terrainData.heightmapResolution - 1}");
            sliceContext.targetResolution = EditorGUILayout.IntField("Target resolution", sliceContext.targetResolution);
            var splatPartsCount =
                Mathf.CeilToInt((terrain.terrainData.heightmapResolution - 1f) / sliceContext.targetResolution);
            GUILayout.Label($" * Terrain will be copied and splatted to {splatPartsCount * splatPartsCount} part(s)",
                new GUIStyle
                {
                    wordWrap = true
                });
            sliceContext.copyDetails = EditorGUILayout.Toggle("Copy details", sliceContext.copyDetails);
            if (sliceContext.copyDetails)
            {
                sliceContext.optimizeDetails = EditorGUILayout.Toggle("Skip unused details", sliceContext.optimizeDetails);
            }

            sliceContext.copyTrees = EditorGUILayout.Toggle("Copy trees", sliceContext.copyTrees);
            if (sliceContext.copyTrees)
            {
                sliceContext.optimizeTrees = EditorGUILayout.Toggle("Skip unused trees", sliceContext.optimizeTrees);
            }
            sliceContext.connectSlicedTerrains = EditorGUILayout.Toggle("Connect sliced terrains", sliceContext.connectSlicedTerrains);
            sliceContext.drawTerrainInstanced = EditorGUILayout.Toggle("Draw terrains instanced", sliceContext.drawTerrainInstanced);
            sliceContext.hideOriginalTerrain = EditorGUILayout.Toggle("Hide original terrain", sliceContext.hideOriginalTerrain);
            sliceContext.cleanupUnusedLayers = EditorGUILayout.Toggle("Cleanup unused layers", sliceContext.cleanupUnusedLayers);
            if (sliceContext.cleanupUnusedLayers)
            {
                sliceContext.layerUsageToCleanup = EditorGUILayout.Slider("Layer usage to cleanup",
                    sliceContext.layerUsageToCleanup, 0f, 1f);
                sliceContext.maxLayersPerTerrain = EditorGUILayout.IntField("Max layers per terrain", sliceContext.maxLayersPerTerrain);
            }

            sliceContext.useSharedLayers = EditorGUILayout.Toggle("Use shared layers", sliceContext.useSharedLayers);
            EditorGUILayout.LabelField("Next sliced assets path", GetTargetPath(out _));

            if (slicer.slicingProgress >= 0)
            {
                var initRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                EditorGUI.ProgressBar(initRect, slicer.slicingProgress, "Slicing...");
            }
            else
            {
                if (GUILayout.Button("Copy and Slice", GUILayout.Height(32f)))
                {
                    EditorCoroutineUtility.StartCoroutine(SlicingCoroutine(sliceContext), this);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private IEnumerator SlicingCoroutine(SliceContext sliceContext)
        {
            var slicer = (TerrainSlicerController) target;
            yield return PerformSlice(slicer, sliceContext, (_) => SceneView.RepaintAll());
        }
        
        private IEnumerator PerformSlice(TerrainSlicerController slicer, 
            SliceContext context, Action<SliceProgressInfo> progressListener = null)
        {
            slicer.slicingProgress = 0f;
            yield return null;
            
            slicer.context = context;
            Action<SliceProgressInfo> _progressListener = progressInfo =>
            {
                slicer.slicingProgress = progressInfo.GetProgress();
                progressListener?.Invoke(progressInfo);

                if (progressInfo.IsDone())
                {
                    Selection.activeGameObject = progressInfo.singleResult;
                }
            };
            yield return new TerrainCopyHelper().PerformSlice(context, slicer.GetComponent<Terrain>(), _progressListener);

            slicer.slicingProgress = -1;
            yield return null;
        }
        
        private string GetTargetPath(out string scenePath)
        {
            return TerrainCopyHelper.GetTargetPath(out scenePath);
        }
    }
}