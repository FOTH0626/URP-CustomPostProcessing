using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostProcessingFeature : ScriptableRendererFeature
{
     private List<CustomPostProcessing> _customPostProcessings;
     private CustomPostProcessingPass _afterOpaqueAndSkyPasses;
     private CustomPostProcessingPass _beforePostProcessingPasses;
     private CustomPostProcessingPass _afterPostProcessingPasses;
     
    
    
    public override void Create()
    {
        var stack = VolumeManager.instance.stack;

        _customPostProcessings = VolumeManager.instance.baseComponentTypeArray
            .Where(t => t.IsSubclassOf(typeof(CustomPostProcessing)))
            .Select(t => stack.GetComponent(t) as CustomPostProcessing)
            .ToList();
        
        var afterOpaqueAndSkyCPPs = _customPostProcessings
            .Where(c => c.InjectionPoint == CustomPostProcessInjectionPoint.AfterOpaqueAndSky) // 筛选出所有CustomPostProcessing类中注入点为透明物体和天空后的实例
            .OrderBy(c => c.OrderInInjectionPoint) // 按照顺序排序
            .ToList(); // 转换为List
        // 创建CustomPostProcessingPass类
        _afterOpaqueAndSkyPasses = new CustomPostProcessingPass("Custom PostProcess after Skybox", afterOpaqueAndSkyCPPs);
        // 设置Pass执行时间
        _afterOpaqueAndSkyPasses.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

        var beforePostProcessingCPPs = _customPostProcessings
            .Where(c => c.InjectionPoint == CustomPostProcessInjectionPoint.BeforePostProcess)
            .OrderBy(c => c.OrderInInjectionPoint)
            .ToList();
        _beforePostProcessingPasses = new CustomPostProcessingPass("Custom PostProcess before PostProcess", beforePostProcessingCPPs);
        _beforePostProcessingPasses.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        var afterPostProcessCPPs = _customPostProcessings
            .Where(c => c.InjectionPoint == CustomPostProcessInjectionPoint.AfterPostProcess)
            .OrderBy(c => c.OrderInInjectionPoint)
            .ToList();
        _afterPostProcessingPasses = new CustomPostProcessingPass("Custom PostProcess after PostProcessing", afterPostProcessCPPs);
        _afterPostProcessingPasses.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.postProcessEnabled)
        {
            if (_afterOpaqueAndSkyPasses.SetupCustomPostProcessing()) {
                _afterOpaqueAndSkyPasses.ConfigureInput(ScriptableRenderPassInput.Color);
                renderer.EnqueuePass(_afterOpaqueAndSkyPasses);
            }
		
            if (_beforePostProcessingPasses.SetupCustomPostProcessing()) {
                _beforePostProcessingPasses.ConfigureInput(ScriptableRenderPassInput.Color);
                renderer.EnqueuePass(_beforePostProcessingPasses);
            }
		
            if (_afterPostProcessingPasses.SetupCustomPostProcessing()) {
                _afterPostProcessingPasses.ConfigureInput(ScriptableRenderPassInput.Color);
                renderer.EnqueuePass(_afterPostProcessingPasses);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && _customPostProcessings != null)
        {
            foreach (var item in _customPostProcessings)
            {
                item.Dispose();
            }
        }
    }
    
    
}
