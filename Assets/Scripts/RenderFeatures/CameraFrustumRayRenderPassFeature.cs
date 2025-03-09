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
        renderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses;
    }

    public class CameraFrustumRenderPass : ScriptableRenderPass
    {

        public static readonly int CameraViewTopLeftCornerID = Shader.PropertyToID("CameraViewTopLeftCorner"),
            CameraViewXExtentID = Shader.PropertyToID("CameraViewXExtent"),
            CameraViewYExtentID = Shader.PropertyToID("CameraViewYExtent"),
            ProjectionParams2ID = Shader.PropertyToID("ProjectionParams2");
        private int cameraViewMatrixUniformID;
        private int cameraProjectionMatrixUniformID;

        private int cameraViewTopLeftCornerUniformID;
        private int cameraViewXExtentUniformID;
        private int cameraViewYExtentUniformID;
        private int cameraViewZExtentUniformID;
        private int cameraViewProjectionMatrixUniformID;
        private int projectionParams2UniformID;

        private Vector4[] cameraTopLeftCorner = new Vector4[2];
        private Vector4[] cameraXExtent = new Vector4[2];
        private Vector4[] cameraYExtent = new Vector4[2];
        private Vector4[] cameraZExtent = new Vector4[2];
        private Matrix4x4[] cameraViewProjections = new Matrix4x4[2];

        private Matrix4x4[] cameraViews = new Matrix4x4[2];
        private Matrix4x4[] cameraProjections = new Matrix4x4[2];

        public CameraFrustumRenderPass()
        {

            cameraViewTopLeftCornerUniformID = Shader.PropertyToID("_CameraViewTopLeftCorner");
            cameraViewXExtentUniformID = Shader.PropertyToID("_CameraViewXExtent");
            cameraViewYExtentUniformID = Shader.PropertyToID("_CameraViewYExtent");
            cameraViewZExtentUniformID = Shader.PropertyToID("_CameraViewZExtent");
            projectionParams2UniformID = Shader.PropertyToID("ProjectionParams2");
            cameraViewProjectionMatrixUniformID = Shader.PropertyToID("_CameraViewProjections");

            cameraViewMatrixUniformID = Shader.PropertyToID("_CameraViews");
            cameraProjectionMatrixUniformID = Shader.PropertyToID("_CameraProjections");
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

#if ENABLE_VR && ENABLE_XR_MODULE
                int eyeCount = renderingData.cameraData.xr.enabled && renderingData.cameraData.xr.singlePassEnabled ? 2 : 1;
#else
            int eyeCount = 1;
#endif
            for (int eyeIndex = 0; eyeIndex < eyeCount; eyeIndex++)
            {
                Matrix4x4 view = renderingData.cameraData.GetViewMatrix(eyeIndex);
                Matrix4x4 proj = renderingData.cameraData.GetProjectionMatrix(eyeIndex);
                cameraViewProjections[eyeIndex] = proj * view;
                cameraViews[eyeIndex] = view;
                cameraProjections[eyeIndex] = proj;
                // camera view space without translation, used by SSAO.hlsl ReconstructViewPos() to calculate view vector.
                Matrix4x4 cview = view;
                cview.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                Matrix4x4 cviewProj = proj * cview;
                Matrix4x4 cviewProjInv = cviewProj.inverse;

                Vector4 topLeftCorner = cviewProjInv.MultiplyPoint(new Vector4(-1, 1, -1, 1));
                Vector4 topRightCorner = cviewProjInv.MultiplyPoint(new Vector4(1, 1, -1, 1));
                Vector4 bottomLeftCorner = cviewProjInv.MultiplyPoint(new Vector4(-1, -1, -1, 1));
                Vector4 farCentre = cviewProjInv.MultiplyPoint(new Vector4(0, 0, 1, 1));

                Vector4 nearCentre = cviewProjInv.MultiplyPoint(new Vector4(0, 0, -1, 1));
                cameraTopLeftCorner[eyeIndex] = topLeftCorner;
                cameraXExtent[eyeIndex] = topRightCorner - topLeftCorner;
                cameraYExtent[eyeIndex] = bottomLeftCorner - topLeftCorner;
                cameraZExtent[eyeIndex] = farCentre- nearCentre;
            }

            cmd.SetGlobalVector(projectionParams2UniformID, new Vector4(1.0f / renderingData.cameraData.camera.nearClipPlane, renderingData.cameraData.worldSpaceCameraPos.x, renderingData.cameraData.worldSpaceCameraPos.y, renderingData.cameraData.worldSpaceCameraPos.z));
            cmd.SetGlobalMatrixArray(cameraViewProjectionMatrixUniformID, cameraViewProjections);

            cmd.SetGlobalMatrixArray(cameraViewMatrixUniformID, cameraViews);
            cmd.SetGlobalMatrixArray(cameraProjectionMatrixUniformID, cameraProjections);

            cmd.SetGlobalVectorArray(cameraViewTopLeftCornerUniformID, cameraTopLeftCorner);
            cmd.SetGlobalVectorArray(cameraViewXExtentUniformID, cameraXExtent);
            cmd.SetGlobalVectorArray(cameraViewYExtentUniformID, cameraYExtent);
            cmd.SetGlobalVectorArray(cameraViewZExtentUniformID, cameraZExtent);

            CoreUtils.SetKeyword(cmd,"_ORTHOGRAPHIC", renderingData.cameraData.camera.orthographic);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
