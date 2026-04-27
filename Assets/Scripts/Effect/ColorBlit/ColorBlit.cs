using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[VolumeComponentMenu("Custom Post-processing/Color Blit")]
public class ColorBlit : CustomPostProcessing
{
    public ClampedFloatParameter intensity = new(0.0f, 0.0f, 2.0f);

    private Material _material;
    private const string _shaderName = "Hidden/PostProcess/ColorBlit";


    public override bool IsActive() => _material != null && intensity.value > 0;

    public override CustomPostProcessInjectionPoint InjectionPoint =>
        CustomPostProcessInjectionPoint.AfterOpaqueAndSky;

    public override int OrderInInjectionPoint => 0;

    public override void Setup()
    {
        if (_material == null)
        {
            _material = CoreUtils.CreateEngineMaterial(_shaderName);
        }
    }

    public override void Render(CommandBuffer cmd, ref RenderingData renderingData, RTHandle source,
        RTHandle destination)
    {
        if (_material == null) return;

        _material.SetFloat("_Intensity", intensity.value);
        Blitter.BlitCameraTexture(cmd, source, destination, _material, 0);
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        CoreUtils.Destroy(_material);
    }
}