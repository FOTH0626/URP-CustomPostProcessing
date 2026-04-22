using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostProcessingPass : ScriptableRenderPass, IDisposable
{
    private readonly List<CustomPostProcessing> _customPostProcessings;
    private readonly List<int> _activeCustomPostProcessingIndex;
    private readonly string _profilerTag;
    private readonly List<ProfilingSampler> _profilingSamplers;

    private RTHandle _sourceRT;
    private RTHandle _destinationRT;
    private RTHandle _tempRT0;
    private RTHandle _tempRT1;

    private string _tempRT0Name => "_TemporaryRenderTexture0";
    private string _tempRT1Name => "_TemporaryRenderTexture1";

    public CustomPostProcessingPass(string profilerTag, List<CustomPostProcessing> customPostProcessings)
    {
        _profilerTag = profilerTag;
        _customPostProcessings = customPostProcessings;
        _activeCustomPostProcessingIndex = new List<int>(customPostProcessings.Count);
        _profilingSamplers = customPostProcessings.Select(c => new ProfilingSampler(c.ToString())).ToList();
    }

    public bool SetupCustomPostProcessing()
    {
        _activeCustomPostProcessingIndex.Clear();
        for (int i = 0; i < _customPostProcessings.Count; i++)
        {
            _customPostProcessings[i].Setup();
            if (_customPostProcessings[i].IsActive())
            {
                _activeCustomPostProcessingIndex.Add(i);
            }
        }

        return _activeCustomPostProcessingIndex.Count != 0;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get(_profilerTag);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.msaaSamples = 1;
        descriptor.depthBufferBits = 0;

        _destinationRT = renderingData.cameraData.renderer.cameraColorTargetHandle;
        _sourceRT = renderingData.cameraData.renderer.cameraColorTargetHandle;

        RenderingUtils.ReAllocateIfNeeded(ref _tempRT0, descriptor, name: _tempRT0Name);

        if (_activeCustomPostProcessingIndex.Count == 1)
        {
            int index = _activeCustomPostProcessingIndex[0];
            using (new ProfilingScope(cmd, _profilingSamplers[index]))
            {
                _customPostProcessings[index].Render(cmd, ref renderingData, _sourceRT, _tempRT0);
            }
        }
        else
        {
            RenderingUtils.ReAllocateIfNeeded(ref _tempRT1, descriptor, name: _tempRT1Name);
            Blitter.BlitCameraTexture(cmd, _sourceRT, _tempRT0);

            for (int i = 0; i < _activeCustomPostProcessingIndex.Count; i++)
            {
                int index = _activeCustomPostProcessingIndex[i];
                var customProcessing = _customPostProcessings[index];
                using (new ProfilingScope(cmd, _profilingSamplers[index]))
                {
                    customProcessing.Render(cmd, ref renderingData, _tempRT0, _tempRT1);
                }

                CoreUtils.Swap(ref _tempRT0, ref _tempRT1);
            }
        }

        Blitter.BlitCameraTexture(cmd, _tempRT0, _destinationRT);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        _sourceRT = null;
        _destinationRT = null;
    }

    public void Dispose()
    {
        _tempRT0?.Release();
        _tempRT0 = null;

        _tempRT1?.Release();
        _tempRT1 = null;

        _sourceRT = null;
        _destinationRT = null;
    }
}
