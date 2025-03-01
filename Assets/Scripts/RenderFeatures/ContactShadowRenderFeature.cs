using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
 
using System;
using UnityEditor;
using UnityEngine.Experimental.Rendering;


public class ContactShadowRenderFeature : ScriptableRendererFeature
{
    public ContactShadowRenderPass contactShadowRenderPass;
    private Material material;
    private Shader shader;
    [SerializeField] public ContactShadowSettings settings;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.SceneView ||
            renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(contactShadowRenderPass);
        }
       
    }

    public void OnEnable()
    {


    }

    public void OnDisable()
    {
        Debug.Log("contact shadow rendererfeature OnDisable");

    }

    protected override void Dispose(bool disposing)
    {
        contactShadowRenderPass?.Dispose();
        contactShadowRenderPass = null;
        CoreUtils.Destroy(material);
    }
    public override void Create()
    {
        shader=Shader.Find("Hidden/ContactShadowShader");
        if (shader == null)
        {
            return;
        }
        material = CoreUtils.CreateEngineMaterial(shader);
        contactShadowRenderPass = new ContactShadowRenderPass(material,settings);
        contactShadowRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
        contactShadowRenderPass.SetupPass();
    }
}
[Serializable]
public class ContactShadowSettings
{
    [Range(10, 256)] public  int sampleCount;
    [Range(0f, 1f)] public float edgeWidth;
     
    [Range(0.1f, 8f)] public float stepLength;
    
    [Range(0f,45f)] public float fadeDistance;
    public bool downSample=false;

    public enum CurrentRenderingEntryPoint
    {
        AfterPrepasses,
        AfterGBuffer
    }
    public CurrentRenderingEntryPoint currentEntryPoint;
}
public class ContactShadowRenderPass : ScriptableRenderPass
{
    public Material material;
    public ContactShadowSettings settings;
    private RenderTextureDescriptor shadowTextureDescriptor;
    private RTHandle contactShadowMap;
    private static readonly string contactShadowKeyword = "_CONTACT_SHADOW";
    private static readonly int   
              sampleCountID = Shader.PropertyToID("SampleCount"),
              edgeWidthID = Shader.PropertyToID("EdgeWidth"),
              shadowWeightID = Shader.PropertyToID("ShadowWeight"),
                stepLengthID = Shader.PropertyToID("StepLength"),
               blitScreenParamsID = Shader.PropertyToID("_BlitScreenParams"),
        fadeDistanceID=Shader.PropertyToID("FadeDistance");
    public ContactShadowRenderPass(Material mat,ContactShadowSettings settings)
    {
       
        this.material = mat;
        this.settings = settings;
        shadowTextureDescriptor = new RenderTextureDescriptor(Screen.width,
           Screen.height, RenderTextureFormat.RFloat, 0);
    }

    public void SetupPass()
    {
        if (settings.currentEntryPoint==ContactShadowSettings.CurrentRenderingEntryPoint.AfterPrepasses)
        {
            this.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
        }else if (settings.currentEntryPoint == ContactShadowSettings.CurrentRenderingEntryPoint.AfterGBuffer)
        {
            this.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer;
        }
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureInput(ScriptableRenderPassInput.Normal);
    }
    public override void Configure(CommandBuffer cmd,
       RenderTextureDescriptor cameraTextureDescriptor)
    {
        
        // Set the blur texture size to be the same as the camera target size.
        if (!settings.downSample)
        {
            shadowTextureDescriptor.width = cameraTextureDescriptor.width;
            shadowTextureDescriptor.height = cameraTextureDescriptor.height;
        }
        else
        {
            shadowTextureDescriptor.width = cameraTextureDescriptor.width/2;
            shadowTextureDescriptor.height = cameraTextureDescriptor.height / 2;
        }


        // Check if the descriptor has changed, and reallocate the RTHandle if necessary
        material.SetVector(blitScreenParamsID, new Vector4(
            shadowTextureDescriptor.width,
            shadowTextureDescriptor.height,
            1.0f/shadowTextureDescriptor.width,
            1.0f / shadowTextureDescriptor.height));

        RenderingUtils.ReAllocateIfNeeded(ref contactShadowMap, shadowTextureDescriptor);
       // cmd.SetGlobalTexture("_ContactShadowMap", contactShadowMap);
    }
    public void UpdateSettings()
    {
        material.SetInt(sampleCountID, settings.sampleCount);
        material.SetFloat(edgeWidthID, settings.edgeWidth);
      //  material.SetFloat(shadowWeightID, settings.shadowWeight);
        material.SetFloat(stepLengthID, settings.stepLength);
    //    material.SetFloat(shadowBiasID,settings.shadowBias);
        material.SetFloat(fadeDistanceID, settings.fadeDistance);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
       
        CommandBuffer cmd = CommandBufferPool.Get("Generate Contact Shadowmap");
        CoreUtils.SetKeyword(cmd, contactShadowKeyword, true);
        RTHandle cameraTargetHandle =
            renderingData.cameraData.renderer.cameraColorTargetHandle;
        var camera = renderingData.cameraData.camera;
 
        UpdateSettings();
 
        Blit(cmd, cameraTargetHandle, contactShadowMap, material);

        cmd.SetGlobalTexture("_ContactShadowMap", contactShadowMap);
    
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        CoreUtils.SetKeyword(cmd, contactShadowKeyword, false);
    }

    public void Dispose()
    {

        contactShadowMap?.Release();
    }
}


