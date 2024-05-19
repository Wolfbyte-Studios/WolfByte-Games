using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using layercleanup;
using terrainhelpers;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace LayerOptimizer
{
    [CustomEditor(typeof(TerrainLayerUsageOperations))]
    public class TerrainLayerUsageOperationsEditorScript : Editor
    {
        public override void OnInspectorGUI()
        {
            var controller = (TerrainLayerUsageOperations) target;
            var terrain = controller.GetTerrain();
            var layers = terrain.terrainData.terrainLayers;

            var guiStyle = new GUIStyle
            {
                wordWrap = true,
                padding = new RectOffset(16, 16, 16, 16)
            };

            GUILayout.Label("Please Copy terrain prior to any modification! " +
                            "Scene will contain old one disabled and new one active and ready for modifications.",
                guiStyle,
                GUILayout.ExpandHeight(true));
            if (controller.copyProgress >= 0f)
            {
                var initRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                EditorGUI.ProgressBar(initRect, controller.copyProgress, "Copying...");
            }
            else
            {
                if (GUILayout.Button("Copy terrain (Resources and GameObject)", GUILayout.Height(32f)))
                {
                    EditorCoroutineUtility.StartCoroutine(CopyTerrain(), this);
                }
            }
            
            GUILayout.Label("Layer usage can be non actual after any painting operations",
                guiStyle,
                GUILayout.ExpandHeight(true));
            if (GUILayout.Button("Update Layer Usage Info"))
            {
                controller.CalculateLayerUsage();
            }

            var terrainUsage = controller.GetTerrainUsage();
            List<string> usageLabels; 
            if (terrainUsage == null || terrainUsage.Count != layers.Length)
            {
                GUILayout.Label("Warning: usage was not yet calculated or not actual. Click 'Update usage info'.");
                usageLabels = layers.Select(_ => "---").ToList();
            }
            else
            {
                usageLabels = terrainUsage.Select(usage => $"{usage:0.00} %").ToList();
            }
            
            GUILayout.Space(16f);

            var layerIndex = 0;
            const float gridLabelWidth = 100f;
            
            foreach (var layer in layers)
            {
                var otherLayers = layers.Where((_, index) => index != layerIndex)
                    .Select(layerRecord => new GUIContent(layerRecord.name)).ToArray();
                
                var layerPreview = AssetPreview.GetAssetPreview(layer.diffuseTexture);
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label(layerPreview, GUILayout.MaxWidth(64f));
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Layer name: ", GUILayout.Width(gridLabelWidth));
                GUILayout.Label($"{layer.name}");
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Layer usage: ", GUILayout.Width(gridLabelWidth));
                GUILayout.Label($"{usageLabels[layerIndex]}");
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Remove layer: ", GUILayout.Width(gridLabelWidth));
                if (GUILayout.Button(new GUIContent("Remove", 
                    "Layer will be removed, other layers (if any) will increase their alpha or it'll be completely replaced with base layer")))
                {
                    if (Confirm($"Remove layer {layers[layerIndex]}?"))
                    {
                        controller.RemoveLayerIncreasingOthersOrBase(layerIndex);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Replace with: ", 
                    "Layer will be removed and all its usage will be replaced with another chosen layer"),
                    GUILayout.Width(gridLabelWidth));
                var layerToReplaceWith = EditorGUILayout.Popup(-1, otherLayers);
                if (layerToReplaceWith >= layerIndex)
                {
                    layerToReplaceWith++;
                }
                if (layerToReplaceWith >= 0
                    && Confirm($"Replace layer {layers[layerIndex].name} with {layers[layerToReplaceWith]}?"))
                {
                    controller.RemoveLayerReplacingWithOther(layerIndex, layerToReplaceWith);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Swap with: ", GUILayout.Width(gridLabelWidth));
                var layerToExchangeWith = EditorGUILayout.Popup(-1, otherLayers);
                if (layerToExchangeWith >= layerIndex)
                {
                    layerToExchangeWith++;
                }
                if (layerToExchangeWith >= 0
                    && Confirm($"Swap layers {layers[layerIndex].name} and {layers[layerToExchangeWith]}?"))
                {
                    controller.SwapLayers(layerIndex, layerToExchangeWith);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                layerIndex++;
                
                GUILayout.Space(16f);
            }
        }

        private bool Confirm(string message)
        {
            return EditorUtility.DisplayDialog("Warning", message, "Yes", "No");
        }

        private IEnumerator CopyTerrain()
        {
            var controller = (TerrainLayerUsageOperations) target;
            yield return CopyTerrain(controller, _ => SceneView.RepaintAll());
        }
        
        private IEnumerator CopyTerrain(TerrainLayerUsageOperations controller, 
            Action<SliceProgressInfo> progressListener = null)
        {
            controller.copyProgress = 0f;
            yield return null;
            
            Action<SliceProgressInfo> _progressListener = progressInfo =>
            {
                controller.copyProgress = progressInfo.GetProgress();
                progressListener?.Invoke(progressInfo);
                Selection.activeGameObject = progressInfo.singleResult;
            };
            
            yield return new TerrainCopyHelper().PerformCopy(controller.terrain, _progressListener);

            controller.copyProgress = -1f;
            yield return null;
        }
    }
}