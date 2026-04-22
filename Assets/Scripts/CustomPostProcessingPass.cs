using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

    public class CustomPostProcessingPass : ScriptableRenderPass
    {
        private List<CustomPostProcessing> _customPostProcessings;
        private List<int> _activeCustomPostProcessingIndex;

        private string _profilerTag;
        private List<ProfilingSampler> _profilingSamplers;

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

            _tempRT0 = RTHandles.Alloc(_tempRT0Name, name: _tempRT0Name);
            _tempRT1 = RTHandles.Alloc(_tempRT1Name, name: _tempRT1Name);
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

            // 获取相机Descriptor
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;

            // 初始化临时RT
            bool rt1Used = false;

            // 设置源和目标RT为本次渲染的RT 在Execute里进行 特殊处理后处理后注入点
            _destinationRT = renderingData.cameraData.renderer.cameraColorTargetHandle;
            _sourceRT = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // 声明temp0临时纹理
            // cmd.GetTemporaryRT(Shader.PropertyToID(_tempRT0.name), descriptor);
            // _tempRT0 = RTHandles.Alloc(_tempRT0.name);
            RenderingUtils.ReAllocateIfNeeded(ref _tempRT0, descriptor, name: _tempRT0Name);

            // 执行每个组件的Render方法
            if (_activeCustomPostProcessingIndex.Count == 1) {
                int index = _activeCustomPostProcessingIndex[0];
                using (new ProfilingScope(cmd, _profilingSamplers[index])) {
                    _customPostProcessings[index].Render(cmd, ref renderingData, _sourceRT, _tempRT0);
                }
            }
            else {
                // 如果有多个组件，则在两个RT上来回bilt
                RenderingUtils.ReAllocateIfNeeded(ref _tempRT1, descriptor, name: _tempRT1Name);
                rt1Used = true;
                Blit(cmd, _sourceRT, _tempRT0);
                for (int i = 0; i < _activeCustomPostProcessingIndex.Count; i++) {
                    int index = _activeCustomPostProcessingIndex[i];
                    var customProcessing = _customPostProcessings[index];
                    using (new ProfilingScope(cmd, _profilingSamplers[index])) {
                        customProcessing.Render(cmd, ref renderingData, _tempRT0, _tempRT1);
                    }

                    CoreUtils.Swap(ref _tempRT0, ref _tempRT1);
                }
            }
	
            Blitter.BlitCameraTexture(cmd, _tempRT0, _destinationRT);

            // 释放
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(_tempRT0.name));
            if (rt1Used) cmd.ReleaseTemporaryRT(Shader.PropertyToID(_tempRT1.name));

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            
        }
        
        
        
    }
