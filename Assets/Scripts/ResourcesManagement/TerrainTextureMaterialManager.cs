using UnityEngine;

public class TerrainTextureMaterialManager
{
    public Texture2D terrainTex;
    public Texture2D terrainMip0;

    public Texture2D terrainNormalTex;
    public Texture2D terrainNormalMip0;

    public Texture2D terrainTransparentTex;
    public Texture2D terrainTransparentMip0;

    public Texture2D terrainNonSolidTex;
    public Texture2D terrainNonSolidMip0;
    
    


    public Texture2D waterTex;
    public Texture2D waterTexMip0;

    public Texture2D waterNormalTex;
    public Texture2D waterNormalMip0;
   
   
    public Texture2D itemTerrainTex;
    public Texture2D itemTerrainMip0;

    public Material terrainMtrl;
    public Material itemTerrainMtrl;
    public Material nonsolidMtrl;
    public Material transparentMtrl;
    public Material waterMtrl;

    private static void CreateLimitedMipmapTextureCopy(ref Texture2D srcTexture, ref Texture2D dstTexture,
        int limitedMipLevel,bool isNormalMap=false)
    {
        
            dstTexture = new Texture2D(srcTexture.width, srcTexture.height, srcTexture.format, limitedMipLevel,!srcTexture.isDataSRGB);
            dstTexture.filterMode = FilterMode.Point;
        
      
    

 
         

            Graphics.CopyTexture(srcTexture,0,0,dstTexture,0,0);
        
         

            dstTexture.Apply(true, true);
    }
    public void LoadAndSetDefaultTexMipmap()
    {
            terrainMtrl = Resources.Load<Material>("Mtrls/terrain");

            itemTerrainMtrl = Resources.Load<Material>("Mtrls/itemparticle");

            nonsolidMtrl = Resources.Load<Material>("Mtrls/nonsolidblocks");

            transparentMtrl = Resources.Load<Material>("Mtrls/solidtransparentblocks");

            waterMtrl = Resources.Load<Material>("Mtrls/watermtrl");
           
    
        terrainMip0 =Resources.Load<Texture2D>("Textures/terrain");
        CreateLimitedMipmapTextureCopy(ref terrainMip0, ref terrainTex, 7);


      
        terrainTransparentMip0 = Resources.Load<Texture2D>("Textures/terraintransparent");
        CreateLimitedMipmapTextureCopy(ref terrainTransparentMip0, ref terrainTransparentTex, 7);


        
        terrainNonSolidMip0 = Resources.Load<Texture2D>("Textures/nonsolid");
        CreateLimitedMipmapTextureCopy(ref terrainNonSolidMip0, ref terrainNonSolidTex, 7);


        
        terrainNormalMip0 = Resources.Load<Texture2D>("Textures/terrainnormal");
        CreateLimitedMipmapTextureCopy(ref terrainNormalMip0, ref terrainNormalTex, 7);


        
        waterTexMip0 = Resources.Load<Texture2D>("Textures/waterterrain");
        CreateLimitedMipmapTextureCopy(ref waterTexMip0, ref waterTex, 7);

        

        waterNormalMip0 = Resources.Load<Texture2D>("Textures/waterterrainnormal");
        CreateLimitedMipmapTextureCopy(ref waterNormalMip0, ref waterNormalTex, 7);



        itemTerrainMip0 = Resources.Load<Texture2D>("Textures/itemterrain");
        CreateLimitedMipmapTextureCopy(ref itemTerrainMip0, ref itemTerrainTex, 7);


        terrainMtrl.SetTexture("_BaseMap",terrainTex);
        terrainMtrl.SetTexture("_BumpMap", terrainNormalTex);
        nonsolidMtrl.SetTexture("_BaseMap", terrainNonSolidTex);
        waterMtrl.SetTexture("_BumpMap", waterNormalTex);
        waterMtrl.SetTexture("_BaseMap", waterTex);
        itemTerrainMtrl.SetTexture("_BaseMap", itemTerrainTex);
        transparentMtrl.SetTexture("_BaseMap", terrainTransparentTex);
        Debug.Log("Set Texture");
    }

   
  
  

  
      public void ResetTerrainTextures()
    {

        terrainMtrl = Resources.Load<Material>("Mtrls/terrain");

        itemTerrainMtrl = Resources.Load<Material>("Mtrls/itemparticle");

        nonsolidMtrl = Resources.Load<Material>("Mtrls/nonsolidblocks");

        transparentMtrl = Resources.Load<Material>("Mtrls/solidtransparentblocks");

        waterMtrl = Resources.Load<Material>("Mtrls/watermtrl");

        terrainTex = Resources.Load<Texture2D>("Textures/terrain");
        terrainNormalTex = Resources.Load<Texture2D>("Textures/terrainnormal");
        waterNormalTex = Resources.Load<Texture2D>("Textures/waterterrainnormal");
        waterTex = Resources.Load<Texture2D>("Textures/waterterrain");
        terrainTransparentTex = Resources.Load<Texture2D>("Textures/terrain");
        terrainNonSolidTex = Resources.Load<Texture2D>("Textures/nonsolid");
        itemTerrainTex = Resources.Load<Texture2D>("Textures/itemterrain");

        terrainMtrl.SetTexture("_BaseMap", terrainTex);
        terrainMtrl.SetTexture("_BumpMap", terrainNormalTex);
        nonsolidMtrl.SetTexture("_BaseMap", terrainNonSolidTex);
        waterMtrl.SetTexture("_BumpMap", waterNormalTex);
        waterMtrl.SetTexture("_BaseMap", waterTex);
        itemTerrainMtrl.SetTexture("_BaseMap", itemTerrainTex);
        transparentMtrl.SetTexture("_BaseMap", terrainTransparentTex);
    }
    
 
}
