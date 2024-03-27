using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.TADemo
{
    public class GrassRendererFeature : ScriptableRendererFeature
    {
        public GrassRenderSettings settings;
        GrassRenderPass m_grassPsss;
        public static PositionTrackObject  trackActor;
        public override void Create()
        {
            if(trackActor == null) return;
            m_grassPsss = new GrassRenderPass(settings, trackActor);
            m_grassPsss.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
           if(trackActor == null)
               return;
           
           renderer.EnqueuePass(m_grassPsss);
        }
        
    }
    [Serializable]
    public class GrassRenderSettings
    {
        public int range = 5;
    }
}