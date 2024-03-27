using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.TADemo
{
    // https://zhuanlan.zhihu.com/p/524594823
    public class WindRenderPass : ScriptableRenderPass
    {
        // private RenderTextureDescriptor windChannelDesc = new RenderTextureDescriptor();
        // private RenderTexture m_windBufferChannelR1;
        // private RenderTexture m_windBufferChannelR2;
        // private RenderTexture m_windBufferChannelG1;
        // private RenderTexture m_windBufferChannelG2;
        // private RenderTexture m_windBufferChannelB1;
        // private RenderTexture m_windBufferChannelB2;
        //
        // private RenderTextureDescriptor windVelocityDataDesc = new RenderTextureDescriptor();
        // private RenderTexture m_windVelocityData;
        //
        // private int m_windBrandX = ;
        // private int m_windBrandY = ;
        // private int m_windBrandZ = ;
        //
        // const int SHADER_NUMTHREAD_X = 8; //must match compute shader's [numthread(x)]
        // const int SHADER_NUMTHREAD_Y = 8; //must match compute shader's [numthread(y)]
        // const int SHADER_NUMTHREAD_Z = 8; //must match compute shader's [numthread(z)]
        //
        // private ComputeShader windRenderCS;
        //
        // public WindRenderPass()
        // {
        //     windChannelDesc.enableRandomWrite = true;
        //     windChannelDesc.width = m_windBrandX;
        //     windChannelDesc.height = m_windBrandY;
        //     windChannelDesc.dimension = TextureDimension.Tex3D;
        //     windChannelDesc.volumeDepth = m_windBrandZ;
        //     windChannelDesc.colorFormat = RenderTextureFormat.RInt;
        //     windChannelDesc.graphicsFormat = GraphicsFormat.R32_SInt;
        //     windChannelDesc.msaaSamples = 1;
        //
        //     CreateRenderTexture(m_windBufferChannelR1, windChannelDesc, "WindBufferChannelR1");
        //     CreateRenderTexture(m_windBufferChannelR2, windChannelDesc, "WindBufferChannelR2");
        //     CreateRenderTexture(m_windBufferChannelG1, windChannelDesc, "WindBufferChannelG1");
        //     CreateRenderTexture(m_windBufferChannelG2, windChannelDesc, "WindBufferChannelG2");
        //     CreateRenderTexture(m_windBufferChannelB1, windChannelDesc, "WindBufferChannelB1");
        //     CreateRenderTexture(m_windBufferChannelB2, windChannelDesc, "WindBufferChannelB2");
        //     
        //
        //     windVelocityDataDesc.enableRandomWrite = true;
        //     windVelocityDataDesc.width = m_windBrandX;
        //     windVelocityDataDesc.height = m_windBrandY;
        //     windVelocityDataDesc.dimension = TextureDimension.Tex3D;
        //     windVelocityDataDesc.volumeDepth = m_windBrandZ;
        //     windVelocityDataDesc.colorFormat = RenderTextureFormat.ARGBFloat;
        //     windVelocityDataDesc.graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat;
        //     windVelocityDataDesc.msaaSamples = 1;
        //     CreateRenderTexture(m_windVelocityData, windVelocityDataDesc, "WindVelocityData");
        // }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 调用Mgr的逻辑
            if (WindManager.Instance != null && WindManager.Instance.gameObject.activeSelf)
            {
                WindManager.Instance.DoRenderWindVolume();
            }
            // CommandBuffer cmd = CommandBufferPool.Get();
            // using (new ProfilingScope(cmd, new ProfilingSampler("Wind Render Pass")))
            // {
            //     Render(cmd);
            //     context.ExecuteCommandBuffer(cmd);
            //     CommandBufferPool.Release(cmd);
            // }
        }

        void Render(CommandBuffer cmd)
        {
            
        }

        void CreateRenderTexture(RenderTexture rt, RenderTextureDescriptor desc, string name)
        {
            rt = new RenderTexture(desc);
            rt.name = name;
        }

        // 1. 风力数据的偏移存储，1立方厘米代表1个像素
        // 上一帧的风力数据会留到下一帧并参与模拟计算和效果表现，因为风力中心对象的移动会导致分离信息的记录偏差
        // void DoShiftPos(int form)
        // {
        //     windRenderCS = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Resources/Shaders/ComputeShaders/WindRenderCS.compute");
        //     int kernel = windRenderCS.FindKernel("WindOffsetDataKernel");
        //     
        //     if(windRenderCS != null)
        //     {
        //         RenderTexture formRTR = form == 1 ? m_windBufferChannelR1 : m_windBufferChannelR2;
        //         RenderTexture formRTG = form == 1 ? m_windBufferChannelG1 : m_windBufferChannelG2;
        //         RenderTexture formRTB = form == 1 ? m_windBufferChannelB1 : m_windBufferChannelB2;
        //
        //         RenderTexture toRTR = form == 1 ? m_windBufferChannelR2 : m_windBufferChannelR1;
        //         RenderTexture toRTG = form == 1 ? m_windBufferChannelG2 : m_windBufferChannelG1;
        //         RenderTexture toRTB = form == 1 ? m_windBufferChannelB2 : m_windBufferChannelB1;
        //
        //         Vector3 cellPos = ConvertFloatPointToInt(m_OffsetPos);
        //         Vector3 offsetPos = cellPos - m_LastOffsetPos;
        //         
        //         windRenderCS.SetVector("VolumeSizeMinusOne", m_volumeSizeMinusOne);
        //         windRenderCS.SetInt("OffsetPosX", (int)offsetPos.x);
        //         windRenderCS.SetInt("OffsetPosY", (int)offsetPos.y);
        //         windRenderCS.SetInt("OffsetPosZ", (int)offsetPos.z);
        //         
        //         windRenderCS.SetTexture(kernel, "WindBufferInputX", formRTR);
        //         windRenderCS.SetTexture(kernel, "WindBufferInputY", formRTG);
        //         windRenderCS.SetTexture(kernel, "WindBufferInputZ", formRTB);
        //         
        //         windRenderCS.SetTexture(kernel, "WindBufferOutputX", toRTR);
        //         windRenderCS.SetTexture(kernel, "WindBufferOutputY", toRTG);
        //         windRenderCS.SetTexture(kernel, "WindBufferOutputZ", toRTB);
        //         
        //         windRenderCS.Dispatch(kernel, m_windBrandX / 4, m_windBrandY / 4, m_windBrandZ / 4);
        //         m_LastOffsetPos = cellPos;
        //     }
        // }
        // 2. 风的扩散模拟
        // 风的扩散模拟可以看作一个blur
        // void DoDiffusion(int form)
        // {
        //     int kernel = windRenderCS.FindKernel("WindDiffusionKernel");
        //     RenderTexture formRTR = form == 1 ? m_windBufferChannelR1 : m_windBufferChannelR2;
        //     RenderTexture formRTG = form == 1 ? m_windBufferChannelG1 : m_windBufferChannelG2;
        //     RenderTexture formRTB = form == 1 ? m_windBufferChannelB1 : m_windBufferChannelB2;
        //
        //     RenderTexture toRTR = form == 1 ? m_windBufferChannelR2 : m_windBufferChannelR1;
        //     RenderTexture toRTG = form == 1 ? m_windBufferChannelG2 : m_windBufferChannelG1;
        //     RenderTexture toRTB = form == 1 ? m_windBufferChannelB2 : m_windBufferChannelB1;
        //     
        //     windRenderCS.SetVector("VolumeSizeMinusOne", m_volumeSizeMinusOne);
        //     windRenderCS.SetFloat("", m_diffusionForce);
        //     
        //     windRenderCS.SetTexture(kernel, "WindBufferInputX", formRTR);
        //     windRenderCS.SetTexture(kernel, "WindBufferOutputX", toRTR);
        //     windRenderCS.Dispatch(kernel, m_windBrandX / 4, m_windBrandY / 4, m_windBrandZ / 4);
        //     
        //     windRenderCS.SetTexture(kernel, "WindBufferInputY", formRTG);
        //     windRenderCS.SetTexture(kernel, "WindBufferOutputY", toRTG);
        //     windRenderCS.Dispatch(kernel, m_windBrandX / 4, m_windBrandY / 4, m_windBrandZ / 4);
        //     
        //     windRenderCS.SetTexture(kernel, "WindBufferInputZ", formRTB);
        //     windRenderCS.SetTexture(kernel, "WindBufferOutputZ", toRTB);
        //     windRenderCS.Dispatch(kernel, m_windBrandX / 4, m_windBrandY / 4, m_windBrandZ / 4);
        // }

    }
}