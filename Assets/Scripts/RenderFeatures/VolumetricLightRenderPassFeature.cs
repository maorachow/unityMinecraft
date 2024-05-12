using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightRenderFeature : ScriptableRendererFeature
{
    
    [SerializeField] private Shader shader;
    private Material material;
    private VolumetricLightRenderPass volumetricLightPass;
    [SerializeField] public VolumetricLightSettings settings;
    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        material = new Material(shader);
        volumetricLightPass = new VolumetricLightRenderPass(material, settings);

        volumetricLightPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
         
          renderer.EnqueuePass(volumetricLightPass);
         
    }

    protected override void Dispose(bool disposing)
    {
        volumetricLightPass.Dispose();
        #if UNITY_EDITOR
                    if (EditorApplication.isPlaying)
                    {
                        UnityEngine.Object.Destroy(material);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(material);
                    }
                #else
                UnityEngine.Object.Destroy(material);
        #endif
    }
}
[Serializable]
public class VolumetricLightSettings
{
    [Range(16, 256)] public int stepCount;
    [Range(0f, 1f)] public float intensity;
    [ColorUsage(true, true)] public Color fogColor;
    public bool isUsingHalfRes=true;
 }
public class VolumetricLightRenderPass : ScriptableRenderPass
{

    public Material material;
    public VolumetricLightSettings defaultSettings;

    public RTHandle volumetricLightHandle;
    public RenderTextureDescriptor volumetricLightDescriptor;
    private static readonly int stepCountID =
        Shader.PropertyToID("StepCount"),
        intensityID =
        Shader.PropertyToID("Intensity"),
        fogColorID =
        Shader.PropertyToID("FogColor");
   


    public VolumetricLightRenderPass(Material material, VolumetricLightSettings defaultSettings)
    {
        this.material = material;
        this.defaultSettings = defaultSettings;

        volumetricLightDescriptor = new RenderTextureDescriptor(Screen.width,
            Screen.height, RenderTextureFormat.Default, 0);
    }

    public override void Configure(CommandBuffer cmd,
       RenderTextureDescriptor cameraTextureDescriptor)
    {
        // Set the blur texture size to be the same as the camera target size.
        if(defaultSettings.isUsingHalfRes==true)
        {
        volumetricLightDescriptor.width = cameraTextureDescriptor.width/2;
        volumetricLightDescriptor.height = cameraTextureDescriptor.height/2;
        }
        else
        {
            volumetricLightDescriptor.width = cameraTextureDescriptor.width;
            volumetricLightDescriptor.height = cameraTextureDescriptor.height;
        }
       

        // Check if the descriptor has changed, and reallocate the RTHandle if necessary
        RenderingUtils.ReAllocateIfNeeded(ref volumetricLightHandle, volumetricLightDescriptor);
    }
    public void UpdateSettings()
    {
        var volumeComponent =
      VolumeManager.instance.stack.GetComponent<VolumetricLightingVolumeComponent>();
        int stepCount = volumeComponent.stepCount.overrideState ? volumeComponent.stepCount.value : defaultSettings.stepCount;
        float intensity = volumeComponent.intensity.overrideState ? volumeComponent.intensity.value : defaultSettings.intensity;
        Color fogColor = volumeComponent.fogColor.overrideState ? volumeComponent.fogColor.value : defaultSettings.fogColor;
        material.SetInt(stepCountID, stepCount);
        material.SetFloat(intensityID, intensity);
        material.SetColor(fogColorID, fogColor);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
       
        CommandBuffer cmd = CommandBufferPool.Get();

        RTHandle cameraTargetHandle =
            renderingData.cameraData.renderer.cameraColorTargetHandle;
            var camera = renderingData.cameraData.camera;
        //   UpdateBlurSettings();

        UpdateSettings();
           // Blit from the camera target to the temporary render texture,
           // using the first shader pass.
           Blitter.BlitCameraTexture(cmd, cameraTargetHandle, volumetricLightHandle, material, 0);
        // Blit from the temporary render texture to the camera target,
        // using the second shader pass.
        Blitter.BlitCameraTexture(cmd, volumetricLightHandle, cameraTargetHandle, material,1);

 

        //Execute the command buffer and release it back to the pool.
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
    #if UNITY_EDITOR
                    if (EditorApplication.isPlaying)
                    {
                        UnityEngine.Object.Destroy(material);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(material);
                    }
    #else
            UnityEngine.Object.Destroy(material);
            #endif

                if (volumetricLightHandle != null) volumetricLightHandle.Release();
    }
}