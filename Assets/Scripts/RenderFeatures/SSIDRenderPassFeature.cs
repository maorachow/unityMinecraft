using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SSIDRenderPassFeature : ScriptableRendererFeature
{

    public SSIDRenderPass SSIDRenderPass;
    private Material material;
    [SerializeField] public Texture2D noiseTex;
    [SerializeField] public Shader shader;
    [SerializeField] SSIDSettings settings;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(SSIDRenderPass);
    }

    public override void Create()
    {
       material=new Material(shader);
        SSIDRenderPass = new SSIDRenderPass(material,noiseTex,settings);
        SSIDRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }
}
[Serializable]
public class SSIDSettings
{
    [Range(0.5f, 3f)]public float SSIDIntensity = 1f;
    [Range(8, 128)] public int SSIDStepCount=16;
    [Range (1, 32)]public int SSIDRayCount = 16;
    [Range(0.01f,0.2f)]public float SSIDStepLength= 16;
    [Range(1f, 20.0f)] public float SSIDRadius = 6f;
    [Range(1f, 50f)] public float SSIDFadeDistance = 15f;
    [Range(0.0005f, 0.025f)] public float blurStrength=0.01f;
    public bool isUsingHalfResolusion=false;
}
public class SSIDRenderPass : ScriptableRenderPass
{
    public List<Vector3> SSIDRandomDir = new List<Vector3>();
    public Material material;
    public SSIDSettings settings;
    public Texture2D noiseTex;
    private RenderTextureDescriptor SSIDTextureDescriptor;
    private RenderTextureDescriptor blurTextureDescriptor;
    private RTHandle SSIDHandle;
    System.Random random = new System.Random();
    public static readonly int SSIDNoiseTexID = Shader.PropertyToID("SSIDNoiseTex"),
         SSIDIntensityID = Shader.PropertyToID("SSIDIntensity"),
        SSIDStepCountID = Shader.PropertyToID("SSIDStepCount"),
        SSIDRayCountID = Shader.PropertyToID("SSIDRayCount"),
        SSIDStepLengthID = Shader.PropertyToID("SSIDStepLength"),
         SSIDRadiusID = Shader.PropertyToID("SSIDRadius"),
        SSIDFadeDistanceID = Shader.PropertyToID("SSIDFadeDistance"),
         blurStrengthID = Shader.PropertyToID("BlurStrength");
    public void GenSSIDRandomDir()
    {
        
        for (int i = 0; i < 16; ++i)
        {
            Vector3 sample = new Vector3((float)random.NextDouble() * 2f - 1f, (float)random.NextDouble() * 2f - 1f, (float)random.NextDouble());
            sample.Normalize();
            sample *= (float)random.NextDouble();

            float scale = (float)i / 64f;
            scale = Mathf.Lerp(0.1f, 1f, scale * scale);
            sample *= scale;
            SSIDRandomDir.Add(sample);
        }
    }
    public SSIDRenderPass(Material mat, Texture2D noiseTex, SSIDSettings settings)
    {
        this.material = mat;

        SSIDTextureDescriptor = new RenderTextureDescriptor(Screen.width,
           Screen.height, RenderTextureFormat.Default, 0);
        blurTextureDescriptor =new RenderTextureDescriptor(Screen.width,
           Screen.height, RenderTextureFormat.Default, 0);
        this.noiseTex = noiseTex;
        this.settings = settings;
    }
    public RTHandle blurHandle1;
    public RTHandle blurHandle2;
    public override void Configure(CommandBuffer cmd,
       RenderTextureDescriptor cameraTextureDescriptor)
    {
        if (settings.isUsingHalfResolusion == false)
        {
     // Set the blur texture size to be the same as the camera target size.
        SSIDTextureDescriptor.width = cameraTextureDescriptor.width;
        SSIDTextureDescriptor.height = cameraTextureDescriptor.height;
        blurTextureDescriptor.width = cameraTextureDescriptor.width;
        blurTextureDescriptor.height = cameraTextureDescriptor.height;
        
        // Check if the descriptor has changed, and reallocate the RTHandle if necessary
        RenderingUtils.ReAllocateIfNeeded(ref SSIDHandle, SSIDTextureDescriptor);
        RenderingUtils.ReAllocateIfNeeded(ref blurHandle1, blurTextureDescriptor);  
        RenderingUtils.ReAllocateIfNeeded(ref blurHandle2, blurTextureDescriptor);
        }
        else
        {
            // Set the blur texture size to be the same as the camera target size.
            SSIDTextureDescriptor.width = cameraTextureDescriptor.width/2;
            SSIDTextureDescriptor.height = cameraTextureDescriptor.height / 2;
            blurTextureDescriptor.width = cameraTextureDescriptor.width / 2;
            blurTextureDescriptor.height = cameraTextureDescriptor.height / 2;

            // Check if the descriptor has changed, and reallocate the RTHandle if necessary
            RenderingUtils.ReAllocateIfNeeded(ref SSIDHandle, SSIDTextureDescriptor);
            RenderingUtils.ReAllocateIfNeeded(ref blurHandle1, blurTextureDescriptor);
            RenderingUtils.ReAllocateIfNeeded(ref blurHandle2, blurTextureDescriptor);
        }

       
    }
    public void UpdateSettings()
    {
        material.SetTexture(SSIDNoiseTexID, noiseTex);
        material.SetFloat(SSIDIntensityID, settings.SSIDIntensity);
        material.SetInt(SSIDRayCountID, settings.SSIDRayCount);
        material.SetInt(SSIDStepCountID, settings.SSIDStepCount);
        material.SetFloat(SSIDStepLengthID, settings.SSIDStepLength);
        material.SetFloat(SSIDRadiusID,settings.SSIDRadius);
        material.SetFloat(SSIDFadeDistanceID, settings.SSIDFadeDistance);
        material.SetFloat(blurStrengthID, settings.blurStrength);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        RTHandle cameraTargetHandle =
            renderingData.cameraData.renderer.cameraColorTargetHandle;
        var camera = renderingData.cameraData.camera;


        // Blit from the camera target to the temporary render texture,
        // using the first shader pass.
        //    Blit(cmd, cameraTargetHandle, blurTextureHandle, material);
        // Blit from the temporary render texture to the camera target,
        // using the second shader pass.

        UpdateSettings();

        //  cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
        //   cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);
        //   cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        Blit(cmd, cameraTargetHandle, SSIDHandle, material, 0);
        Blit(cmd, SSIDHandle, blurHandle1, material, 2);
        Blit(cmd, blurHandle1, blurHandle2, material, 3);
        Blit(cmd, blurHandle2, cameraTargetHandle, material, 1);

        //Execute the command buffer and release it back to the pool.
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
