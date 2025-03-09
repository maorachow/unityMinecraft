using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityScreenSpaceReflections
{
    public class HiZBufferRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private HiZBufferRenderFeatureSettings settings;
        private Shader shader;
        private Material material;
        private HiZBufferRenderPass hiZBufferPass;
        public override void Create()
        {
            //     Debug.Log("validate");
            shader = Shader.Find("Hidden/HiZBufferShader");
            if (shader == null)
            {
                Debug.Log("null shader");
                return;
            }

            material = CoreUtils.CreateEngineMaterial(shader);
            if (settings == null)
            {
                settings = new HiZBufferRenderFeatureSettings();
            }

            if (hiZBufferPass != null)
            {
                Debug.Log("dispose");
                hiZBufferPass.Dispose();
            }
            hiZBufferPass = new HiZBufferRenderPass(material, settings);

            if (settings.currentEntryPoint == HiZBufferRenderFeatureSettings.CurrentRenderingEntryPoint.AfterGBuffer)
            {
                hiZBufferPass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer;
            }
            else if (settings.currentEntryPoint == HiZBufferRenderFeatureSettings.CurrentRenderingEntryPoint.AfterPrepasses)
            {
                hiZBufferPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
            }
            else
            {
                hiZBufferPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
            }
          
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {

            renderer.EnqueuePass(hiZBufferPass);
        }
        protected override void Dispose(bool disposing)
        {
            hiZBufferPass.Dispose();
            CoreUtils.Destroy(material);
        }
    }
    [Serializable]
    public class HiZBufferRenderFeatureSettings
    {
        [Tooltip("Determines is generating the Hi-Z buffer with half resolution or not.\r\nAlso affects screen space reflection render feature since it depends on the Hi-Z buffer to work. Improves performance.")]
        public bool downSample = false;
        [Tooltip("The total depth mipmap chain count of the Hi-Z buffer.\r\nHigher value means further tracing distance in all screen-space ray-marching render features that depends on current Hi-Z buffer render feature.")]
        [Range(2, 16)] public int hiZMipCount = 3;

        public enum CurrentRenderingEntryPoint
        {
            AfterPrepasses,
            AfterGBuffer
        }
        public CurrentRenderingEntryPoint currentEntryPoint;

    }
    [ExecuteAlways]
    public class HiZBufferRenderPass : ScriptableRenderPass
    {
        private HiZBufferRenderFeatureSettings defaultSettings;
        private Material material;
        private RTHandle[] renderTargetMips;
        private RenderTextureDescriptor[] renderTargetMipDescriptors;
        private RenderTextureDescriptor finalHiZTextureDescriptor;
        private RTHandle finalHiZTarget;
        private int sourceSizeUniformID;
        private int hizBufferTextureID;
        private int maxHiZMipLevelID;
        private int actualMaxMipCount;
        public static int maxHiZMipLevel;
        public static Vector4 hiZTargetMip0SourceSize;
        public static bool downSample = false;
        public HiZBufferRenderPass(Material mat, HiZBufferRenderFeatureSettings defaultSettings)
        {
            this.defaultSettings = defaultSettings;
            this.material = mat;
            renderTargetMips = new RTHandle[defaultSettings.hiZMipCount];
            renderTargetMipDescriptors = new RenderTextureDescriptor[defaultSettings.hiZMipCount];
            sourceSizeUniformID = Shader.PropertyToID("SourceSize");
            hizBufferTextureID = Shader.PropertyToID("HiZBufferTexture");
            maxHiZMipLevelID = Shader.PropertyToID("MaxHiZMipLevel");

        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (defaultSettings.hiZMipCount <= 0)
            {
                Debug.Log("hiz mip level is 0");
                return;
            }
            ConfigureInput(ScriptableRenderPassInput.Normal);
            var desc = cameraTextureDescriptor;
            var width = Math.Max((int)Math.Ceiling(Mathf.Log(desc.width, 2) - 1.0f), 1);
            var height = Math.Max((int)Math.Ceiling(Mathf.Log(desc.height, 2) - 1.0f), 1);
            HiZBufferRenderPass.downSample = defaultSettings.downSample;
            if (downSample == true)
            {
                width = 1 << width;
                height = 1 << height;
            }
            else
            {
                width = 2 << width;
                height = 2 << height;
            }

            int minLength = Math.Min(width, height);

            int maxMipLevel = (int)Math.Ceiling(Mathf.Log(minLength*2, 2));
            actualMaxMipCount = Math.Min(maxMipLevel, defaultSettings.hiZMipCount);
            finalHiZTextureDescriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.RFloat, 0,
                actualMaxMipCount);
            finalHiZTextureDescriptor.useMipMap = true;
            finalHiZTextureDescriptor.sRGB = false;
            hiZTargetMip0SourceSize = new Vector4(width, height, 1.0f / width, 1.0f / height);
            RenderingUtils.ReAllocateIfNeeded(ref finalHiZTarget, in finalHiZTextureDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "finalHiZBufferTarget");
            for (int i = 0; i < actualMaxMipCount; i++)
            {
                renderTargetMipDescriptors[i] = new RenderTextureDescriptor(width, height, RenderTextureFormat.RFloat, 0, 1);
                renderTargetMipDescriptors[i].msaaSamples = 1;
                renderTargetMipDescriptors[i].useMipMap = false;
                renderTargetMipDescriptors[i].sRGB = false;
                RenderingUtils.ReAllocateIfNeeded(ref renderTargetMips[i], renderTargetMipDescriptors[i], FilterMode.Point, TextureWrapMode.Clamp, name: "hizBufferTexture" + i);


                width = Math.Max(width / 2, 1);
                height = Math.Max(height / 2, 1);
            }

        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {






            if (material == null)
            {
                Debug.LogErrorFormat("{0}.Execute(): Missing material.", GetType().Name);
                return;
            }

            if (defaultSettings.hiZMipCount <= 0)
            {
                Debug.Log("hiz mip level is 0");
                return;
            }
            var cmd = CommandBufferPool.Get("Generate Hi-Z Buffer");
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            RTHandle cameraDepthTexture;//will be full black when in forward rendering
            
                cameraDepthTexture =renderingData.cameraData.renderer.cameraDepthTargetHandle;
            
           
            if (cameraDepthTexture == null || cameraDepthTexture.rt == null)
            {
                Debug.Log("camera depth target null");
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                return;
            }


            // mip 0
            
            Blitter.BlitCameraTexture(cmd, cameraDepthTexture, renderTargetMips[0], material, 1);
            cmd.CopyTexture(renderTargetMips[0], 0, 0, finalHiZTarget, 0, 0);

            // mip 1~max
            for (int i = 1; i < actualMaxMipCount; i++)
            {
                //       Debug.Log("hiz mip count:"+defaultSettings.hiZMipCount+" final hiz target mip count:"+ finalHiZTarget.rt.mipmapCount+" actual hiz target max mip count:"+ actualMaxMipCount);
                cmd.SetGlobalVector(sourceSizeUniformID, new Vector4(renderTargetMipDescriptors[i - 1].width, renderTargetMipDescriptors[i - 1].height, 1.0f / renderTargetMipDescriptors[i - 1].width, 1.0f / renderTargetMipDescriptors[i - 1].height));
                Blitter.BlitCameraTexture(cmd, renderTargetMips[i - 1], renderTargetMips[i], material, 0);

                cmd.CopyTexture(renderTargetMips[i], 0, 0, finalHiZTarget, 0, i);
            }

            maxHiZMipLevel = actualMaxMipCount;
            // set global hiz texture
            cmd.SetGlobalFloat(maxHiZMipLevelID, actualMaxMipCount);
            cmd.SetGlobalTexture(hizBufferTextureID, finalHiZTarget);
            //   Blitter.BlitCameraTexture(cmd, finalHiZTarget, cameraColorTexture);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {


            if (finalHiZTarget != null) finalHiZTarget.Release();
            if (renderTargetMips != null)
            {
                foreach (var item in renderTargetMips)
                {
                    if (item != null)
                    {
                        item.Release();
                    }
                }
            }
        }
    }
}