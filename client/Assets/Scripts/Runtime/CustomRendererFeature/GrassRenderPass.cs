using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.TADemo
{
    public class GrassRenderPass : ScriptableRenderPass
    {
        private GrassRenderSettings m_settings;
        private PositionTrackObject m_obj;
        private int rtID;
        private RenderTextureDescriptor desc;
        
        public GrassRenderPass(GrassRenderSettings settings, PositionTrackObject obj)
        {
            m_settings = settings;
            m_obj = obj;
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("Grass Render Pass")))
            {
                Render(cmd);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        void Render(CommandBuffer cmd)
        {
            Vector3 actorPos = m_obj.GetTrackActorPosition();
            cmd.SetGlobalVector("_ActorPosition", actorPos);
            cmd.SetGlobalFloat("_Range", m_settings.range);
        }
    }
}
