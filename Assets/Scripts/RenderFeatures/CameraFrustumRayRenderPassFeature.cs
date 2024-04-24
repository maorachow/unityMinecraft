using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraFrustumRayRenderPassFeature : ScriptableRendererFeature
{
    public CameraFrustumRenderPass renderPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }

    public override void Create()
    {
        renderPass = new CameraFrustumRenderPass();
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer;
    }

    public class CameraFrustumRenderPass : ScriptableRenderPass
    {

        public static readonly int CameraViewTopLeftCornerID = Shader.PropertyToID("CameraViewTopLeftCorner"),
            CameraViewXExtentID = Shader.PropertyToID("CameraViewXExtent"),
            CameraViewYExtentID = Shader.PropertyToID("CameraViewYExtent"),
            ProjectionParams2ID = Shader.PropertyToID("ProjectionParams2");
             
        public CameraFrustumRenderPass() { }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            Matrix4x4 view = renderingData.cameraData.GetViewMatrix();
            Matrix4x4 proj = renderingData.cameraData.GetProjectionMatrix();
            Matrix4x4 vp = proj * view;

            // 将camera view space 的平移置为0，用来计算world space下相对于相机的vector  
            Matrix4x4 cview = view;
            cview.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            Matrix4x4 cviewProj = proj * cview;

            // 计算viewProj逆矩阵，即从裁剪空间变换到世界空间  
            Matrix4x4 cviewProjInv = cviewProj.inverse;
            var near = renderingData.cameraData.camera.nearClipPlane;

          Vector4 topLeftCorner = cviewProjInv.MultiplyPoint(new Vector4(-1.0f, 1.0f, -1.0f, 1.0f));
        Vector4 topRightCorner = cviewProjInv.MultiplyPoint(new Vector4(1.0f, 1.0f, -1.0f, 1.0f));
        Vector4 bottomLeftCorner = cviewProjInv.MultiplyPoint(new Vector4(-1.0f, -1.0f, -1.0f, 1.0f));
            Vector4 cameraXExtent = topRightCorner - topLeftCorner;
            Vector4 cameraYExtent = bottomLeftCorner - topLeftCorner;
            near = renderingData.cameraData.camera.nearClipPlane;

            cmd.SetGlobalVector(CameraViewTopLeftCornerID, topLeftCorner);
            cmd.SetGlobalVector(CameraViewXExtentID, cameraXExtent);
            cmd.SetGlobalVector(CameraViewYExtentID, cameraYExtent);
            cmd.SetGlobalVector(ProjectionParams2ID, new Vector4(1.0f / near, renderingData.cameraData.worldSpaceCameraPos.x, renderingData.cameraData.worldSpaceCameraPos.y, renderingData.cameraData.worldSpaceCameraPos.z));
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
