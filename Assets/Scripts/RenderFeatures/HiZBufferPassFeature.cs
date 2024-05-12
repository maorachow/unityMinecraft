using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using UnityEditor;
using System.Linq;
public class HiZBufferPassFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader hiZShader;
    [SerializeField] private int hiZMipCount = 8;
    private Material hiZBufferMaterial;
    public HiZBufferPass hiZBufferPass;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(hiZBufferPass);
    }

    public override void Create()
    {
        hiZBufferMaterial = new Material(hiZShader);
        hiZBufferPass = new HiZBufferPass(hiZMipCount, hiZBufferMaterial);
        hiZBufferPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }
 
    public class HiZBufferPass : ScriptableRenderPass
    {
        public int hiZMipCount = 8;
        public RTHandle[] HiZBufferTextures;
        public RenderTextureDescriptor[] HiZBufferTextureDescriptors;
        public RTHandle HiZBufferTexture;
        public RenderTextureDescriptor HiZBufferTextureDescriptor;
        public Material hiZBufferMaterial;
        public static readonly int HiZBufferFromMiplevelID = Shader.PropertyToID("HiZBufferFromMipLevel"),
            HiZBufferToMiplevelID = Shader.PropertyToID("HiZBufferToMipLevel"),
            SourceSizeID = Shader.PropertyToID("SourceSize"),
            MaxHiZBufferTextureMipLevelID = Shader.PropertyToID("MaxHiZufferTextureMipLevel"),
            HiZBufferTextureID = Shader.PropertyToID("HiZBufferTexture");

        public HiZBufferPass(int hiZMipCount, Material hiZMaterial)
        {
            this.hiZMipCount=hiZMipCount;
            this.hiZBufferMaterial = hiZMaterial;
            HiZBufferTextures = new RTHandle[hiZMipCount];
            HiZBufferTextureDescriptors = new RenderTextureDescriptor[hiZMipCount];
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {



            


            // Debug.Log("setup");
            // 分配RTHandle


            var desc = renderingData.cameraData.cameraTargetDescriptor;
            // 把高和宽变换为2的整次幂 然后除以2
            var width = Math.Max((int)Math.Ceiling(Mathf.Log(desc.width, 2) - 1.0f), 1);
            var height = Math.Max((int)Math.Ceiling(Mathf.Log(desc.height, 2) - 1.0f), 1);
            width = 1 << width;
            height = 1 << height;

            HiZBufferTextureDescriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.RFloat, 0, hiZMipCount+1);
            HiZBufferTextureDescriptor.msaaSamples = 1;
            HiZBufferTextureDescriptor.useMipMap = true;
            HiZBufferTextureDescriptor.sRGB = false;// linear
            RenderingUtils.ReAllocateIfNeeded(ref HiZBufferTexture, HiZBufferTextureDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp);
            for (int i = 0; i < hiZMipCount; i++)
            {
                HiZBufferTextureDescriptors[i] = new RenderTextureDescriptor(width, height, RenderTextureFormat.RFloat, 0, 1);
                HiZBufferTextureDescriptors[i].msaaSamples = 1;
                HiZBufferTextureDescriptors[i].useMipMap = false;
                HiZBufferTextureDescriptors[i].sRGB = false;// linear
                RenderingUtils.ReAllocateIfNeeded(ref HiZBufferTextures[i], HiZBufferTextureDescriptors[i], FilterMode.Bilinear, TextureWrapMode.Clamp);
                width = Math.Max(width / 2, 1);
                height = Math.Max(height / 2, 1);
            }

        }
 
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        /*    if (HiZBufferTexture != null){
                HiZBufferTexture.Release();
            }
            if( HiZBufferTextures != null)
            {
                for (int i = 0;  i < HiZBufferTextures.Length; i++)
                {
                    if (HiZBufferTextures[i] != null) HiZBufferTextures[i].Release();
                }
            }*/
            base.OnCameraCleanup(cmd);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            var cmd = CommandBufferPool.Get();

         
            var mCameraDepthTexture = renderingData.cameraData.renderer.cameraDepthTargetHandle;
            Blit(cmd, mCameraDepthTexture, HiZBufferTextures[0]);
            cmd.CopyTexture(HiZBufferTextures[0], 0, 0, HiZBufferTexture, 0, 0);
            for (int i = 1; i < hiZMipCount; i++)
            {
                cmd.SetGlobalFloat(HiZBufferFromMiplevelID, i - 1);
                cmd.SetGlobalFloat(HiZBufferToMiplevelID, i);
                cmd.SetGlobalVector(SourceSizeID, new Vector4(HiZBufferTextureDescriptors[i - 1].width, HiZBufferTextureDescriptors[i - 1].height, 1.0f / HiZBufferTextureDescriptors[i - 1].width, 1.0f / HiZBufferTextureDescriptors[i - 1].height));
                Blit(cmd, HiZBufferTextures[i - 1], HiZBufferTextures[i], material: hiZBufferMaterial, passIndex: 0);
                cmd.CopyTexture(HiZBufferTextures[i], 0, 0, HiZBufferTexture, 0, i);
            }
            cmd.SetGlobalFloat(MaxHiZBufferTextureMipLevelID, hiZMipCount -1);
            cmd.SetGlobalTexture(HiZBufferTextureID, HiZBufferTexture);
            // cmd.Blit(HiZBufferTextures[4], mCameraColorTexture);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

        }
        public void Dispose()
        {
#if UNITY_EDITOR
                                if (EditorApplication.isPlaying)
                                {
                                    UnityEngine.Object.Destroy(hiZBufferMaterial);
                                }
                                else
                                {
                                    UnityEngine.Object.DestroyImmediate(hiZBufferMaterial);
                                }
#else
            UnityEngine.Object.Destroy(hiZBufferMaterial);
#endif
            if (HiZBufferTextures != null)
            {
                for (int i = 0; i < HiZBufferTextures.Length; i++)
                {
                    if (HiZBufferTextures[i] != null) HiZBufferTextures[i].Release();
                }
            }

            if (HiZBufferTexture != null)
            {
                HiZBufferTexture.Release();
            }

        }
    }
}
