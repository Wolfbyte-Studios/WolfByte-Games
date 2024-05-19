using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Meister/Outline")]
public sealed class Outline : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter colorIntensity = new ClampedFloatParameter(0, 0, 1);
    public ClampedFloatParameter normalIntensity = new ClampedFloatParameter(0, 0, 1);
    public ClampedFloatParameter depthIntensity = new ClampedFloatParameter(0, 0, 1);
    public ClampedIntParameter delta = new ClampedIntParameter(1, 1, 5);
    public ColorParameter color = new ColorParameter(Color.black, false, false, true);

    Material m_Material;

    public bool IsActive() => m_Material != null && (colorIntensity.value > 0f || normalIntensity.value > 0f || depthIntensity.value > 0f);

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

    const string kShaderName = "Hidden/Shader/Outline";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume Sobel is unable to load.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        m_Material.SetFloat("_ColorIntensity", colorIntensity.value*3);
        m_Material.SetFloat("_NormalIntensity", normalIntensity.value*5);
        m_Material.SetFloat("_DepthIntensity", depthIntensity.value*6);
        m_Material.SetColor("_Color", color.value);
        m_Material.SetInt("_Delta", delta.value);
        cmd.Blit(source, destination, m_Material, 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
