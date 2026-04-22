using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RadialBlur : CustomPostProcessing
{
    private static readonly int RadialBlurParams = Shader.PropertyToID("_RadialBlurParams");

    public ClampedFloatParameter BlurRadius = new ClampedFloatParameter(0f,0f,1f);
    public ClampedIntParameter Iteration = new ClampedIntParameter (10,2,30);
    public ClampedFloatParameter RadialCenterX = new ClampedFloatParameter (0.5f,0f,1f);
    public ClampedFloatParameter RadialCenterY = new ClampedFloatParameter(0.5f,0f,1f);
    
    private Material _material;
    private const string shaderName = "Hidden/PostProcess/RadialBlur";

    public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
    public override int OrderInInjectionPoint => 10;

    public override bool IsActive()
     => _material != null && BlurRadius.value > 0f;
    

    public override void Setup()
    {
        if (_material == null)
        {
            _material = CoreUtils.CreateEngineMaterial(shaderName);
        }
    }

    public override void Render(CommandBuffer cmd, ref RenderingData renderingData, RTHandle source, RTHandle destination)
    {
        if (_material == null) return;
        
            var Params = new Vector4(
                BlurRadius.value * 0.02f,
                Iteration.value,
                RadialCenterX.value,
                RadialCenterY.value
            );
            _material.SetVector(RadialBlurParams,Params);
            Blitter.BlitCameraTexture(cmd,source,destination,_material,0);
        
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        CoreUtils.Destroy(_material);
    }
}
