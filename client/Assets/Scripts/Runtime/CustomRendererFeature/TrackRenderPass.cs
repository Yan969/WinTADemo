using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.TADemo
{
    public class TrackRenderPass : ScriptableRenderPass
    {
        private GrassRenderSettings m_settings;
        private List<PositionTrackObject> m_objs;
        private int rtID;
        private RenderTextureDescriptor desc;
        
        public TrackRenderPass(GrassRenderSettings settings, List<PositionTrackObject> objs)
        {
            m_settings = settings;
            m_objs = objs;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            rtID = Shader.PropertyToID("PositionTrackRT");
            //desc = new RenderTextureDescriptor(m_settings.rtSize, m_settings.rtSize, RenderTextureFormat.ARGBHalf, 0);
            cmd.GetTemporaryRT(rtID, desc);
            
            ConfigureInput(ScriptableRenderPassInput.Color);
            ConfigureTarget(rtID);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("Track Render Pass")))
            {
                Render(cmd);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
        void Render(CommandBuffer cmd)
        {
            foreach (var obj in m_objs)
            {
                // if (obj.IsCenter())
                // {
                //     cmd.SetGlobalVector("", obj.GetPositionTrack());
                // }
                //Vector3 pos2uv = pos / 2.0f + new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
    }

}
