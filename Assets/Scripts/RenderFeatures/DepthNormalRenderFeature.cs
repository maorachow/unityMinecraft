using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

public class DepthNormalsFeature : ScriptableRendererFeature
{
    [SerializeField] public bool NormalTexture = false; // 当关闭SSAO或SSAO使用Depth Only时，开启此选项渲染法线图
    DepthNormalsPass mDepthNormalsPass;
    public override void Create()
    {
        mDepthNormalsPass = new DepthNormalsPass();
    }

    // 当为每个摄像机设置一个渲染器时，调用此方法
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 如果需要渲染法线，则入队
        if (NormalTexture)
        {
            renderer.EnqueuePass(mDepthNormalsPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    // 渲染法线Pass
    private class DepthNormalsPass : ScriptableRenderPass
    {
        // 相机初始化
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // 设置输入为Normal，让Unity RP添加DepthNormalPrepass Pass
            ConfigureInput(ScriptableRenderPassInput.Normal);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 什么都不做，我们只需要在相机初始化时配置DepthNormals即可
        }
    }
}