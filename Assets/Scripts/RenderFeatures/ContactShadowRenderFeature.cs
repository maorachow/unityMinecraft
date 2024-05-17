using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
 
using System;
using UnityEditor;

public class ContactShadowRenderFeature : ScriptableRendererFeature
{
    public ContactShadowRenderPass contactShadowRenderPass;
    private Material material;
    [SerializeField]  public Shader shader;
    [SerializeField] public ContactShadowSettings settings;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(contactShadowRenderPass);
    }

    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        material = new Material(shader);
        contactShadowRenderPass = new ContactShadowRenderPass(material,settings);
        contactShadowRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }
}
[Serializable]
public class ContactShadowSettings
{
    [Range(10, 256)] public  int sampleCount;
    [Range(0f, 1f)] public float edgeWidth;
    [Range(0f, 1f)] public float shadowWeight;
    [Range(0.1f, 8f)] public float stepLength;
    [Range(0f, 0.1f)] public float shadowBias;
    [Range(0f,30f)] public float fadeDistance;
}
public class ContactShadowRenderPass : ScriptableRenderPass
{
    public Material material;
    public ContactShadowSettings settings;
    private RenderTextureDescriptor shadowTextureDescriptor;
    private RTHandle contactShadowHandle;

    private static readonly int   
              sampleCountID = Shader.PropertyToID("SampleCount"),
              edgeWidthID = Shader.PropertyToID("EdgeWidth"),
              shadowWeightID = Shader.PropertyToID("ShadowWeight"),
                stepLengthID = Shader.PropertyToID("StepLength"),
                shadowBiasID = Shader.PropertyToID("ContactShadowBias"),
        fadeDistanceID=Shader.PropertyToID("FadeDistance");
    public ContactShadowRenderPass(Material mat,ContactShadowSettings settings)
    {
        this.material = mat;
        this.settings = settings;
        shadowTextureDescriptor = new RenderTextureDescriptor(Screen.width,
           Screen.height, RenderTextureFormat.Default, 0);
    }

    public override void Configure(CommandBuffer cmd,
       RenderTextureDescriptor cameraTextureDescriptor)
    {
        // Set the blur texture size to be the same as the camera target size.
        shadowTextureDescriptor.width = cameraTextureDescriptor.width;
        shadowTextureDescriptor.height = cameraTextureDescriptor.height;

        // Check if the descriptor has changed, and reallocate the RTHandle if necessary
        RenderingUtils.ReAllocateIfNeeded(ref contactShadowHandle, shadowTextureDescriptor);
    }
    public void UpdateSettings()
    {
        material.SetInt(sampleCountID, settings.sampleCount);
        material.SetFloat(edgeWidthID, settings.edgeWidth);
        material.SetFloat(shadowWeightID, settings.shadowWeight);
        material.SetFloat(stepLengthID, settings.stepLength);
        material.SetFloat(shadowBiasID,settings.shadowBias);
        material.SetFloat(fadeDistanceID, settings.fadeDistance);
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
        Blit(cmd, cameraTargetHandle, contactShadowHandle, material, 0);
        Blit(cmd, contactShadowHandle, cameraTargetHandle, material,1);

        //Execute the command buffer and release it back to the pool.
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
