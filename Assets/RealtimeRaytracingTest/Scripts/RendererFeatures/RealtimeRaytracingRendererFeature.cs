using System;
using System.Collections.Generic;
using Unity.Mathematics;
 
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

public class RealtimeRaytracingRendererFeature : ScriptableRendererFeature
{
    class RealtimeRaytracingRenderPass : ScriptableRenderPass
    {
        private RayTracingAccelerationStructure rayTracingAccelerationStructure;
        private RayTracingShader shader;

        private RealtimeRaytracingRendererFeatureSettings settings;
        public RTHandle finalImageTarget;
        public RTHandle prevFinalImageTarget;
        public RenderTextureDescriptor finalImageTargetDescriptor;
        public RenderTextureDescriptor defaultBlackTextureDescriptor;
        private Texture2D noiseTexture;
        private int resultTextureUniformID;
        private int prevResultTextureUniformID;
        private int resultTextureSizeUniformID;
        private int worldSpaceCameraPosUniformID;
        private int sceneAcclStructureUniformID;
        private int cameraToWorldUniformID;
        private int cameraInverseProjectionUniformID;
        private int ambientSHUniformID;
        private int skyboxTextureUniformID;
        private int directionalLightConeThetaUniformID;
        private int motionVectorTextureUniformID;
        private int motionVectorDepthTextureUniformID;
        private int cameraDepthTextureUniformID;
        private int accumFrameIndexUniformID;
        private int globalPointLightRadiusUniformID;
        private int maxBounceCountUniformID;
        private int raytracingBlendingMethodUniformID;
        private int whiteNoiseUniformID;
        private Material blendMaterial;
        private static readonly Cubemap defaultBlackCubemap = new Cubemap(1, TextureFormat.ARGB32, 1);
       
        public RealtimeRaytracingRenderPass(RayTracingShader shader, RealtimeRaytracingRendererFeatureSettings settings1,Material finalBlendMaterial)
        {
            noiseTexture = new Texture2D(1024, 1024, GraphicsFormat.R32G32B32A32_SFloat, 1, TextureCreationFlags.None);
            for (int i = 0; i < 2000; i++)
            {
                for (int j = 0; j < 2000; j++)
                {
                    noiseTexture.SetPixel(i,j,new Color(Mathf.Clamp(UnityEngine.Random.Range(0f, 1f),0f,1f) , Mathf.Clamp(UnityEngine.Random.Range(0f, 1f), 0f, 1f), Mathf.Clamp(UnityEngine.Random.Range(0f, 1f), 0f, 1f), Mathf.Clamp(UnityEngine.Random.Range(0f, 1f), 0f, 1f)));
                }
            }
            noiseTexture.Apply();

            this.shader = shader;
            this.settings = settings1;
            this.blendMaterial=finalBlendMaterial;
            RayTracingAccelerationStructure.Settings settings =
                new RayTracingAccelerationStructure.Settings(
                    RayTracingAccelerationStructure.ManagementMode.Automatic,
                    RayTracingAccelerationStructure.RayTracingModeMask.Everything,-1);
            rayTracingAccelerationStructure = new RayTracingAccelerationStructure(settings);
            finalImageTargetDescriptor = new RenderTextureDescriptor(0, 0, RenderTextureFormat.ARGBFloat, 0, 1);
            finalImageTargetDescriptor.enableRandomWrite = true;
            finalImageTargetDescriptor.sRGB = false;
            defaultBlackTextureDescriptor = new RenderTextureDescriptor(1, 1, RenderTextureFormat.ARGB32);
            cameraToWorldUniformID = Shader.PropertyToID("CameraToWorld");
            cameraInverseProjectionUniformID = Shader.PropertyToID("CameraInverseProjection");
            whiteNoiseUniformID= Shader.PropertyToID("RealtimeRayTracingWhiteNoiseTexture");
            resultTextureUniformID = Shader.PropertyToID("UAV_ResultTex");
            prevResultTextureUniformID = Shader.PropertyToID("UAV_PrevResultTex");
            resultTextureSizeUniformID = Shader.PropertyToID("ResultTexSourceSize");
            worldSpaceCameraPosUniformID = Shader.PropertyToID("RayTracingPassWorldSpaceCameraPos");
            sceneAcclStructureUniformID = Shader.PropertyToID("SceneRaytracingAccelerationStructure");
            ambientSHUniformID = Shader.PropertyToID("AmbientSHArray");
            skyboxTextureUniformID = Shader.PropertyToID("SRV_SkyboxTex");
            directionalLightConeThetaUniformID = Shader.PropertyToID("RealtimeRaytracingDirectionalLightConeTheta");
            globalPointLightRadiusUniformID = Shader.PropertyToID("RealtimeRaytracingGlobalPointLightRadius");
            motionVectorTextureUniformID = Shader.PropertyToID("_MotionVectorTexture");
            cameraDepthTextureUniformID = Shader.PropertyToID("_CameraDepthTexture");
            motionVectorDepthTextureUniformID = Shader.PropertyToID("_MotionVectorDepthTexture");
            accumFrameIndexUniformID = Shader.PropertyToID("AccumFrameIndex");
            maxBounceCountUniformID= Shader.PropertyToID("_RaytracingMaxRecursiveDepth");
            raytracingBlendingMethodUniformID = Shader.PropertyToID("_RaytracingBlendingMethod");
            ConfigureInput(ScriptableRenderPassInput.Motion);
        }
        [Obsolete]
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            finalImageTargetDescriptor.width = (int)(cameraTextureDescriptor.width/1.0f);
            finalImageTargetDescriptor.height = (int)(cameraTextureDescriptor.height / 1.0f); ;
          
            RenderingUtils.ReAllocateIfNeeded(ref finalImageTarget, finalImageTargetDescriptor);
            RenderingUtils.ReAllocateIfNeeded(ref prevFinalImageTarget, finalImageTargetDescriptor);
            if (rayTracingAccelerationStructure != null)
            {
                rayTracingAccelerationStructure.Build();

            }

        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }
        public void GetSHVectorArrays(Vector4[] result, ref SphericalHarmonicsL2 sh)
        {
            for (int i = 0; i < 3; i++)
            {
                result[i].Set(sh[i, 3], sh[i, 1], sh[i, 2], sh[i, 0] - sh[i, 6]);
            }
            for (int i1 = 0; i1 < 3; i1++)
            {
                result[3 + i1].Set(sh[i1, 4], sh[i1, 5], sh[i1, 6] * 3.0f, sh[i1, 7]);
            }
            result[6].Set(sh[0, 8], sh[1, 8], sh[2, 8], 1.0f);
        }

        public void SetUniforms(ref CommandBuffer cmd, ref RenderingData renderingData)
        {
            var cameraDepthHandle = renderingData.cameraData.renderer.cameraDepthTargetHandle;
            cmd.SetGlobalVector(worldSpaceCameraPosUniformID, new Vector4(renderingData.cameraData.camera.transform.position.x, renderingData.cameraData.camera.transform.position.y, renderingData.cameraData.camera.transform.position.z, 1));
            cmd.SetGlobalMatrix(cameraToWorldUniformID, renderingData.cameraData.camera.cameraToWorldMatrix);
            cmd.SetGlobalMatrix(cameraInverseProjectionUniformID, renderingData.cameraData.camera.projectionMatrix.inverse);
         
            cmd.SetGlobalTexture(resultTextureUniformID, finalImageTarget);
            cmd.SetGlobalTexture(prevResultTextureUniformID, prevFinalImageTarget);
            cmd.SetGlobalVector(resultTextureSizeUniformID, new Vector4(finalImageTargetDescriptor.width, finalImageTargetDescriptor.height, 1.0f / finalImageTargetDescriptor.width, 1.0f / finalImageTargetDescriptor.height));
            cmd.SetRayTracingAccelerationStructure(shader, sceneAcclStructureUniformID, rayTracingAccelerationStructure);
          
            cmd.SetGlobalTexture(skyboxTextureUniformID, RenderSettings.customReflectionTexture);
            cmd.SetGlobalInteger("AccumFrameIndex", frameIndex);
            var sh = RenderSettings.ambientProbe;
            Vector4[] SHVals = new Vector4[7];
            GetSHVectorArrays(SHVals, ref sh);

            cmd.SetGlobalVectorArray(ambientSHUniformID, SHVals);
            cmd.SetGlobalFloat(directionalLightConeThetaUniformID, settings.directionalLightConeTheta);
        }

        private Vector3 cameraPos;
        private Vector3 cameraEuler;
        private int frameIndex;
        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
          
            var cmd = CommandBufferPool.Get("Realtime Ray Tracing Shader");
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            SetUniforms(ref cmd, ref renderingData);
            uint threadGrouphsX = (uint)Mathf.CeilToInt(finalImageTargetDescriptor.width);
            uint threadGrouphsY = (uint)Mathf.CeilToInt(finalImageTargetDescriptor.height);
            cmd.SetRayTracingShaderPass(shader, "RealtimeDxrPass");
            cmd.DispatchRays(shader, "RayGeneration", threadGrouphsX, threadGrouphsY, 1, renderingData.cameraData.camera);


            var cameraColorHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            Blit(cmd, finalImageTarget, cameraColorHandle,blendMaterial,0);
            cmd.CopyTexture(finalImageTarget, prevFinalImageTarget);

          
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

 

        }

 
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }


        public class RealtimeRaytracingPassData
        {

            public RayTracingAccelerationStructure rtas;
            public Material blendMaterial;
            public RayTracingShader shader;
            public int accumFrameIndex;
            public int maxAccumFrameCount;
            public TextureHandle finalImageTarget;
            public TextureHandle prevFinalImageTarget;
            public TextureHandle motionVectorHandle;
            public TextureHandle motionVectorDepthHandle;
            public TextureHandle depthTextureHandle;
            public TextureHandle cameraColorHandle;
            public TextureHandle blackTextureHandle;
          
            public bool isSceneView;
            
            public uint2 raytracingDispatchNum;
        }
        private bool IsValid()
        {
            if (!SystemInfo.supportsRayTracing) { Debug.Log("system does not support raytracing"); return false;}
            if (shader == null) { Debug.Log("shader null"); return false;}
            if (settings == null) { Debug.Log("settings null"); return false;}
            if (rayTracingAccelerationStructure == null) {Debug.Log("rtas null");return false;}

            return true;
        }
        public void UpdateRTAS()
        {
            rayTracingAccelerationStructure.Build();
        }

        public void CreateTextures(RenderGraph renderGraph, UniversalResourceData resourceData,
            UniversalCameraData cameraData,out TextureHandle finalImageTarget1, out TextureHandle prevFinalImageTarget1,out TextureHandle blackTextureHandle)
        {

            finalImageTargetDescriptor.width = (int)(cameraData.cameraTargetDescriptor.width / 1.0f);
            finalImageTargetDescriptor.height = (int)(cameraData.cameraTargetDescriptor.height / 1.0f); ;
            TextureDesc finalImageTextureDesc = new TextureDesc(finalImageTargetDescriptor);
            finalImageTarget1 = renderGraph.CreateTexture(in finalImageTextureDesc);
            RenderingUtils.ReAllocateHandleIfNeeded(ref prevFinalImageTarget, finalImageTargetDescriptor);
            prevFinalImageTarget1 = renderGraph.ImportTexture(prevFinalImageTarget);
            TextureDesc blackTextureDesc = new TextureDesc(defaultBlackTextureDescriptor);
            blackTextureDesc.clearBuffer = true;
            blackTextureHandle = renderGraph.CreateTexture(in blackTextureDesc);
            
          
        }

        private Dictionary<int, ValueTuple<Matrix4x4, Matrix4x4>> allCameraMatricesPrev =
            new Dictionary<int, (Matrix4x4, Matrix4x4)>();
        private Matrix4x4 cameraToWorldMatrixPrev;
        private Matrix4x4 cameraProjectionMatrixPrev;
        public void SetUniformsPreRenderFunc(ref RayTracingShader shader, ref UniversalCameraData cameraData)
        {
            shader.SetVector(worldSpaceCameraPosUniformID, new Vector4(cameraData.camera.transform.position.x, cameraData.camera.transform.position.y, cameraData.camera.transform.position.z, 1));
            shader.SetMatrix(cameraToWorldUniformID, cameraData.camera.cameraToWorldMatrix);
            shader.SetMatrix(cameraInverseProjectionUniformID, cameraData.camera.projectionMatrix.inverse);


            shader.SetInt("AccumFrameIndex", frameIndex);
            frameIndex++;
            int cameraHashCode=cameraData.camera.GetHashCode();
            if (!allCameraMatricesPrev.ContainsKey(cameraHashCode))
            {
                allCameraMatricesPrev.Add(cameraHashCode,new ValueTuple<Matrix4x4, Matrix4x4>(cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix()));
            }
            ValueTuple<Matrix4x4, Matrix4x4> item = allCameraMatricesPrev[cameraHashCode];
            if (item.Item1 != cameraData.GetViewMatrix()|| item.Item2 != cameraData.GetProjectionMatrix())
            {
                frameIndex = 0;
            }

            allCameraMatricesPrev[cameraHashCode] =
                new ValueTuple<Matrix4x4, Matrix4x4>(cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix());
        //    cameraToWorldMatrixPrev = cameraData.camera.cameraToWorldMatrix;
       //     cameraProjectionMatrixPrev= cameraData.camera.projectionMatrix;
            if (settings.skyboxTexture != null)
            {
                shader.SetTexture(skyboxTextureUniformID, settings.skyboxTexture);
            }
            else
            {
                shader.SetTexture(skyboxTextureUniformID, defaultBlackCubemap);
            }
            Shader.SetGlobalTexture(whiteNoiseUniformID, noiseTexture);
            Shader.SetGlobalFloat(directionalLightConeThetaUniformID, settings.directionalLightConeTheta);
             Shader.SetGlobalFloat(globalPointLightRadiusUniformID, settings.globalPointLightRadius);
            
        }
        public void SetUniformsRenderGraph(ref RealtimeRaytracingPassData data,ref RayTracingShader shader)
        {

           

            shader.SetTexture(resultTextureUniformID, data.finalImageTarget);
            shader.SetTexture(prevResultTextureUniformID, data.prevFinalImageTarget);
            shader.SetVector(resultTextureSizeUniformID, new Vector4(finalImageTargetDescriptor.width, finalImageTargetDescriptor.height, 1.0f / finalImageTargetDescriptor.width, 1.0f / finalImageTargetDescriptor.height));
            shader.SetAccelerationStructure(sceneAcclStructureUniformID, data.rtas);
            shader.SetTexture(cameraDepthTextureUniformID, data.depthTextureHandle);
        //    shader.SetTexture(whiteNoiseUniformID,data.noiseTextureHandle);
            if (!data.isSceneView)
            {
                shader.SetTexture(motionVectorTextureUniformID, data.motionVectorHandle);
                shader.SetTexture(motionVectorDepthTextureUniformID, data.motionVectorDepthHandle);
            }
            else
            {
                shader.SetTexture(motionVectorTextureUniformID, data.blackTextureHandle);
                shader.SetTexture(motionVectorDepthTextureUniformID, data.blackTextureHandle);
            }
          


            /*     var sh = RenderSettings.ambientProbe;
                 Vector4[] SHVals = new Vector4[7];
                 GetSHVectorArrays(SHVals, ref sh);*/

            //   shader.SetGlobalVectorArray(ambientSHUniformID, SHVals);
         
        }
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (!IsValid())
            {
                Debug.Log("Realtime Raytracing invalid");
                return;
            }
            UpdateRTAS();

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
         
            CreateTextures(renderGraph,resourceData, cameraData,out var finalImageTarget1,out var  prevFinalImageTarget1,out var blackTextureHandle1);
            TextureHandle motionVectorHandle = resourceData.motionVectorColor;
            TextureHandle motionVectorDepthHandle = resourceData.motionVectorDepth;
            TextureHandle cameraColorHandle= resourceData.cameraColor;
            TextureHandle cameraDepthHandle = resourceData.cameraDepthTexture;
            SetUniformsPreRenderFunc(ref shader, ref cameraData);
         
            using (IUnsafeRenderGraphBuilder builder =
                   renderGraph.AddUnsafePass<RealtimeRaytracingPassData>("Realtime Raytracing", out var passData))
            {
                builder.AllowGlobalStateModification(true);
                builder.AllowPassCulling(false);

                passData.shader = shader;
               
                passData.finalImageTarget=finalImageTarget1;
                passData.prevFinalImageTarget=prevFinalImageTarget1;
                passData.motionVectorHandle=motionVectorHandle;
                passData.blendMaterial=blendMaterial;
                passData.rtas = rayTracingAccelerationStructure;
                passData.cameraColorHandle = cameraColorHandle;
                passData.blackTextureHandle=blackTextureHandle1;
                passData.motionVectorDepthHandle = motionVectorDepthHandle;
                passData.depthTextureHandle = cameraDepthHandle;
                passData.accumFrameIndex = frameIndex;
                passData.maxAccumFrameCount = settings.maxAccumFrameCount;
                if (cameraData.cameraType == CameraType.SceneView)
                {
                    passData.isSceneView=true;
                }
                else
                {
                    passData.isSceneView = false;
                }
                    uint threadGrouphsX = (uint)Mathf.CeilToInt(finalImageTargetDescriptor.width);
                uint threadGrouphsY = (uint)Mathf.CeilToInt(finalImageTargetDescriptor.height);
                passData.raytracingDispatchNum = new uint2(threadGrouphsX, threadGrouphsY);
                
                builder.UseTexture(motionVectorHandle,AccessFlags.Read);
                builder.UseTexture(motionVectorDepthHandle, AccessFlags.Read);
                builder.UseTexture(finalImageTarget1,AccessFlags.ReadWrite);
                builder.UseTexture(cameraColorHandle, AccessFlags.ReadWrite);
                builder.UseTexture(prevFinalImageTarget1, AccessFlags.ReadWrite);
                builder.UseTexture(blackTextureHandle1, AccessFlags.ReadWrite);
                builder.SetRenderFunc((RealtimeRaytracingPassData data, UnsafeGraphContext rgContext)  =>
                {
                    SetUniformsRenderGraph(ref data,ref data.shader);
                    CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd);
                    if (data.accumFrameIndex < data.maxAccumFrameCount)
                    {
                        cmd.SetRayTracingShaderPass(data.shader, "RealtimeDxrPass");
                        cmd.DispatchRays(data.shader, "RayGeneration", data.raytracingDispatchNum.x, data.raytracingDispatchNum.y, 1);
                        Blitter.BlitCameraTexture(cmd, data.finalImageTarget, data.cameraColorHandle);

                        cmd.CopyTexture(data.finalImageTarget, data.prevFinalImageTarget);
                    }
                    else
                    {

                        Blitter.BlitCameraTexture(cmd, data.prevFinalImageTarget, data.cameraColorHandle);
                    }

                   
                });
            }

        }
        public void Dispose()
        {
            if(finalImageTarget!=null)finalImageTarget.Release();
            if(prevFinalImageTarget!=null)prevFinalImageTarget.Release();
            if(rayTracingAccelerationStructure!=null)rayTracingAccelerationStructure.Release();
        }
    }

    [SerializeField]
    private RayTracingShader shader;

    [SerializeField]
    private RealtimeRaytracingRendererFeatureSettings settings;

    RealtimeRaytracingRenderPass realtimeRaytracingRenderPass;

    private Material resultBlendingMaterial;
    
    public override void Create()
    {
        if(shader==null){return;}

        Shader blendShader=Shader.Find("Hidden/RealtimeRaytracingBlend");
        if (blendShader == null)
        {
            return;
        }

        resultBlendingMaterial = CoreUtils.CreateEngineMaterial(blendShader);
        if (settings == null)
        {
            settings = new RealtimeRaytracingRendererFeatureSettings();
        }
        realtimeRaytracingRenderPass = new RealtimeRaytracingRenderPass(shader, settings, resultBlendingMaterial);

      
        realtimeRaytracingRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (shader == null) { return; }

        if (renderingData.cameraData.cameraType == CameraType.Game ||
            renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderer.EnqueuePass(realtimeRaytracingRenderPass);
        }
       
    }

    protected override void Dispose(bool disposing)
    {
         CoreUtils.Destroy(resultBlendingMaterial);
        base.Dispose(disposing);
    }
}

public enum RaytracingBlendingMethod
{
    Temporal=0,
    Offline=1
}
[Serializable]
public class RealtimeRaytracingRendererFeatureSettings
{
    [Range(0.0001f,0.3f)]
    public float directionalLightConeTheta = 0.001f;


    [Range(0.01f, 10f)]
    public float globalPointLightRadius = 0.5f;

    public Cubemap skyboxTexture;
    [Range(1,10)]
    public int maxRayRecursiveDepth;
    [Range(1, 32768)]
    public int maxAccumFrameCount=16;
    public RaytracingBlendingMethod blendingMethod;

}


