using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace UnityEngine.Rendering.TADemo
{
    public class WindRenderFeature : ScriptableRendererFeature
    {
        private WindRenderPass windRenderPass;
        public override void Create()
        {
            windRenderPass = new WindRenderPass();
            windRenderPass.renderPassEvent = RenderPassEvent.BeforeRendering;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if ((cameraData.camera.cameraType == CameraType.Game || cameraData.camera.cameraType == CameraType.SceneView) && cameraData.renderType == CameraRenderType.Base)
            {
                renderer.EnqueuePass(windRenderPass);
            }
        }
    }
}


