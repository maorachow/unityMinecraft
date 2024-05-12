
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using UnityEditor;

public class SSRRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private SSRSettings settings;
    [SerializeField] private Shader shader;
 
    private Material hiZBufferMaterial;
 
    private SSRRenderPass ssrRenderPass;
     
    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        hiZBufferMaterial = new Material(shader);
        ssrRenderPass = new SSRRenderPass(hiZBufferMaterial, settings);
         
        ssrRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
       
        renderer.EnqueuePass(ssrRenderPass);


    }

    protected override void Dispose(bool disposing)
    {
        ssrRenderPass.Dispose();
      
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

    }
}

[Serializable]
public class SSRSettings
{
    [Range(16, 256)] public int stepCount;
    [Range(0.05f, 1.5f)] public float strideSize;
    [Range(0.01f, 1f)] public float thickness;
    [Range(0.1f, 200f)] public float fadeDistance;
    [Range(-2f, 2f)] public float SSRBias;
    public bool isUsingHalfResolusion = false;
}

/*public class HiZBufferPass : ScriptableRenderPass
{
    public int hiZMipCount = 6;
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
        this.hiZMipCount = hiZMipCount;
        this.hiZBufferMaterial = hiZMaterial;
        HiZBufferTextures = new RTHandle[hiZMipCount];
        HiZBufferTextureDescriptors = new RenderTextureDescriptor[hiZMipCount];
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {






        Debug.Log("setup");
        // 分配RTHandle
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        // 把高和宽变换为2的整次幂 然后除以2
        var width = Math.Max((int)Math.Ceiling(Mathf.Log(desc.width, 2) - 1.0f), 1);
        var height = Math.Max((int)Math.Ceiling(Mathf.Log(desc.height, 2) - 1.0f), 1);
        width = 1 << width;
        height = 1 << height;

        HiZBufferTextureDescriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.RFloat, 0, hiZMipCount);
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


        base.OnCameraCleanup(cmd);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

        var cmd = CommandBufferPool.Get();

        var mCameraColorTexture = renderingData.cameraData.renderer.cameraColorTargetHandle;
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
        cmd.SetGlobalFloat(MaxHiZBufferTextureMipLevelID, hiZMipCount - 1);
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
}*/
public class SSRRenderPass : ScriptableRenderPass
{

    private UniversalRenderer m_Renderer;

    private static readonly int mProjectionParams2ID = Shader.PropertyToID("_ProjectionParams2"),
               mCameraViewTopLeftCornerID = Shader.PropertyToID("_CameraViewTopLeftCorner"),
               mCameraViewXExtentID = Shader.PropertyToID("_CameraViewXExtent"),
               mCameraViewYExtentID = Shader.PropertyToID("_CameraViewYExtent"),
                stepCountID = Shader.PropertyToID("StepCount"),
                strideSizeID = Shader.PropertyToID("StrideSize"),
                thicknessID = Shader.PropertyToID("SSRThickness"),
         fadeDistanceID = Shader.PropertyToID("FadeDistance"),
        biasID = Shader.PropertyToID("SSRBias");


    private SSRSettings defaultSettings;
    private Material material;

    private RenderTextureDescriptor ssrTextureDescriptor;
    private RTHandle ssrHandle;

    public SSRRenderPass(Material material, SSRSettings settings)
    {
        this.material = material;

        this.defaultSettings = settings;
        ssrTextureDescriptor = new RenderTextureDescriptor(Screen.width,
            Screen.height, RenderTextureFormat.Default, 0);
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        Matrix4x4 view = renderingData.cameraData.GetViewMatrix();
        Matrix4x4 proj = renderingData.cameraData.GetProjectionMatrix();
        Matrix4x4 vp = proj * view;

        // 将camera view space 的平移置为0，用来计算world space下相对于相机的vector
        Matrix4x4 cview = view;
        cview.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        Matrix4x4 cviewProj = proj * cview;

        // 计算viewProj逆矩阵，即从裁剪空间变换到世界空间
        Matrix4x4 cviewProjInv = cviewProj.inverse;

        // 计算世界空间下，近平面四个角的坐标
        var near = renderingData.cameraData.camera.nearClipPlane;
        // Vector4 topLeftCorner = cviewProjInv * new Vector4(-near, near, -near, near);
        // Vector4 topRightCorner = cviewProjInv * new Vector4(near, near, -near, near);
        // Vector4 bottomLeftCorner = cviewProjInv * new Vector4(-near, -near, -near, near);
        Vector4 topLeftCorner = cviewProjInv.MultiplyPoint(new Vector4(-1.0f, 1.0f, -1.0f, 1.0f));
        Vector4 topRightCorner = cviewProjInv.MultiplyPoint(new Vector4(1.0f, 1.0f, -1.0f, 1.0f));
        Vector4 bottomLeftCorner = cviewProjInv.MultiplyPoint(new Vector4(-1.0f, -1.0f, -1.0f, 1.0f));

        // 计算相机近平面上方向向量
        Vector4 cameraXExtent = topRightCorner - topLeftCorner;
        Vector4 cameraYExtent = bottomLeftCorner - topLeftCorner;

        near = renderingData.cameraData.camera.nearClipPlane;

        // 发送ReconstructViewPos参数
        material.SetVector(mCameraViewTopLeftCornerID, topLeftCorner);
        material.SetVector(mCameraViewXExtentID, cameraXExtent);
        material.SetVector(mCameraViewYExtentID, cameraYExtent);
        material.SetVector(mProjectionParams2ID, new Vector4(1.0f / near, renderingData.cameraData.worldSpaceCameraPos.x, renderingData.cameraData.worldSpaceCameraPos.y, renderingData.cameraData.worldSpaceCameraPos.z));

    }
    public override void Configure(CommandBuffer cmd,
        RenderTextureDescriptor cameraTextureDescriptor)
    {

        if (defaultSettings.isUsingHalfResolusion == false)
        {
            // Set the blur texture size to be the same as the camera target size.
            ssrTextureDescriptor.width = cameraTextureDescriptor.width;
            ssrTextureDescriptor.height = cameraTextureDescriptor.height;

        // Check if the descriptor has changed, and reallocate the RTHandle if necessary
        RenderingUtils.ReAllocateIfNeeded(ref ssrHandle, ssrTextureDescriptor);
        }
        else
        {
            // Set the blur texture size to be the same as the camera target size.
            ssrTextureDescriptor.width = cameraTextureDescriptor.width/2;
            ssrTextureDescriptor.height = cameraTextureDescriptor.height/2;

            // Check if the descriptor has changed, and reallocate the RTHandle if necessary
            RenderingUtils.ReAllocateIfNeeded(ref ssrHandle, ssrTextureDescriptor);
        }
       
    }

    private void UpdateSSRSettings()
    {
        if (material == null) return;


        // Use the Volume settings or the default settings if no Volume is set.
        int stepCount = defaultSettings.stepCount;
        float strideSize = defaultSettings.strideSize;
        float thickness = defaultSettings.thickness;
        float fadeDistance = defaultSettings.fadeDistance;
        float bias=defaultSettings.SSRBias;
        material.SetInt(stepCountID, stepCount);
        material.SetFloat(strideSizeID, strideSize);
        material.SetFloat(thicknessID, thickness);
        material.SetFloat(fadeDistanceID, fadeDistance);
        material.SetFloat(biasID, bias);
    }


    public override void Execute(ScriptableRenderContext context,
        ref RenderingData renderingData)
    {
        //Get a CommandBuffer from pool.
        CommandBuffer cmd = CommandBufferPool.Get();

        RTHandle cameraTargetHandle =
            renderingData.cameraData.renderer.cameraColorTargetHandle;
        var camera = renderingData.cameraData.camera;
        UpdateSSRSettings();

        // Blit from the camera target to the temporary render texture,
        // using the first shader pass.
        //    Blit(cmd, cameraTargetHandle, blurTextureHandle, material);
        // Blit from the temporary render texture to the camera target,
        // using the second shader pass.

        material.SetMatrix("matView", camera.worldToCameraMatrix);
        material.SetMatrix("matProjection", camera.projectionMatrix);
        material.SetMatrix("matInverseView", camera.worldToCameraMatrix.inverse);
        material.SetMatrix("matInverseProjection", camera.projectionMatrix.inverse);
        //     material.SetVector("CameraPos", WorldHelper.instance.cameraPos);

        //  cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
        //   cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);
        //   cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        Blit(cmd, cameraTargetHandle, ssrHandle, material, 0);
        Blit(cmd, ssrHandle, cameraTargetHandle, material, 1);

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


        if (ssrHandle != null) ssrHandle.Release();
    }
}