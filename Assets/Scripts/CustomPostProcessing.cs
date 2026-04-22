using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum CustomPostProcessInjectionPoint
{
    AfterOpaqueAndSky,
    BeforePostProcess,
    AfterPostProcess
}

public abstract class CustomPostProcessing : VolumeComponent, IPostProcessComponent, IDisposable
{
    #region IPostProcessComponent

    public abstract bool IsActive();
    
    public virtual bool IsTileCompatible() => false;

    #endregion
    
    #region IDisposable  
    public void Dispose() {  
        Dispose(true);  
        GC.SuppressFinalize(this);  
    }  
	
    public virtual void Dispose(bool disposing) {}
    #endregion

    public virtual CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public virtual int OrderInInjectionPoint => 0;
    
    public abstract void Setup();

    public abstract void Render(CommandBuffer cmd, ref RenderingData renderingData, RTHandle source,
        RTHandle destination);


}