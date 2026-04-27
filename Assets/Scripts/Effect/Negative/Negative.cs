using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Custom Post-processing/Negative")]
public class Negative :CustomPostProcessing
{
    #region Parameters

    public BoolParameter EffectActive = new BoolParameter(false);

    #endregion

    private Material _material;
    private readonly string _shaderName = "Hidden/PostProcess/Negative";
    public override CustomPostProcessInjectionPoint InjectionPoint =>
        CustomPostProcessInjectionPoint.AfterPostProcess;

    public override int OrderInInjectionPoint => 1;
    
    public override bool IsActive()
    {
        return EffectActive.value;
    }

    public override void Setup()
    {
        if (_material == null)
        {
            _material = CoreUtils.CreateEngineMaterial(_shaderName);
        }
    }

    public override void Render(CommandBuffer cmd, ref RenderingData renderingData, RTHandle source, RTHandle destination)
    {
        if (_material == null) return;

        Blitter.BlitCameraTexture(cmd, source, destination, _material, 0);
    }
}
