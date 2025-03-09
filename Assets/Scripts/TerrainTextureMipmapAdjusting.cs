using UnityEngine;

public class TerrainTextureMipmapAdjusting : MonoBehaviour
{
    public static Texture2D terrainTex;
    public static Texture2D terrainTransparentTex;
    public static Texture2D terrainTransparentMip0;
    public static Texture2D terrainNonSolidTex;
    public static Texture2D terrainNonSolidMip0;
    public static Texture2D terrainMip0;
    public static Texture2D terrainNormalTex;
    public static Texture2D terrainNormalMip0;

    public static Texture2D waterNormalTex;
    public static Texture2D waterNormalMip0;
    public static Material terrainMat;
    public static Texture2D applyingTerrainNormal;
    public static Texture2D itemTerrainTex;
    public static Texture2D itemTerrainMip0;
    public static void SetTerrainTexMipmap()
        {
       Material terrainMat1= VoxelWorld.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial;

       Material itemTerrainMat1 =VoxelWorld.itemPrefab.GetComponent<MeshRenderer>().sharedMaterial;

        Material nonsolidMat = VoxelWorld.chunkPrefab.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
     
        Material transparentMat = VoxelWorld.chunkPrefab.transform.GetChild(4).GetComponent<MeshRenderer>().sharedMaterial;

        terrainTex =new Texture2D(1024,1024,TextureFormat.RGBA32, 6, false);
        terrainTex.filterMode=FilterMode.Point;
        

        //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");
        terrainMip0 =Resources.Load<Texture2D>("Textures/terrain");
     
        terrainTex.SetPixels32(terrainMip0.GetPixels32(0),0);
 

        terrainTex.Apply(true,true);

        terrainTransparentTex=new Texture2D(1024,1024,TextureFormat.RGBA32, 6, false);
        terrainTransparentTex.filterMode = FilterMode.Point;
        terrainTransparentMip0 = Resources.Load<Texture2D>("Textures/terraintransparent");
        terrainTransparentTex.SetPixels32(terrainTransparentMip0.GetPixels32(0), 0);

        terrainTransparentTex.Apply(true,true);

        terrainNonSolidTex = new Texture2D(1024, 1024, TextureFormat.RGBA32, 6, false);
        terrainNonSolidTex.filterMode = FilterMode.Point;
        terrainNonSolidMip0 = Resources.Load<Texture2D>("Textures/nonsolid");
        terrainNonSolidTex.SetPixels32(terrainNonSolidMip0.GetPixels32(0), 0);

        terrainNonSolidTex.Apply(true, true);


        terrainNormalTex = new Texture2D(1024, 1024, TextureFormat.RGBA32, 6, true);
        terrainNormalTex.filterMode = FilterMode.Point;

        //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");

        terrainNormalMip0 = Resources.Load<Texture2D>("Textures/terrainnormal");

        terrainNormalTex.SetPixels32(terrainNormalMip0.GetPixels32(0), 0);


        terrainNormalTex.Apply(true, true);





        waterNormalTex = new Texture2D(1024, 1024, TextureFormat.RGBA32, 6, true);
        waterNormalTex.filterMode = FilterMode.Point;

        //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");

        waterNormalMip0 = Resources.Load<Texture2D>("Textures/nonsolidnormal");

        waterNormalTex.SetPixels32(waterNormalMip0.GetPixels32(0), 0);


        waterNormalTex.Apply(true, true);


        itemTerrainTex = new Texture2D(1024, 1024, TextureFormat.RGBA32,3, false);
        itemTerrainTex.filterMode = FilterMode.Point;

        //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");

        itemTerrainMip0 = Resources.Load<Texture2D>("Textures/itemterrain");

        itemTerrainTex.SetPixels32(itemTerrainMip0.GetPixels32(0), 0);


        itemTerrainTex.Apply(true, true);
        terrainMat1.SetTexture("_BaseMap",terrainTex);
        nonsolidMat.SetTexture("_BaseMap", terrainNonSolidTex);

        itemTerrainMat1.SetTexture("_BaseMap", itemTerrainTex);
        transparentMat.SetTexture("_BaseMap", terrainTransparentTex);
        Debug.Log("Set Texture");
    }

    public static void SetTerrainNormalMipmap()
    {
        terrainMat = VoxelWorld.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial;
        terrainNormalTex = new Texture2D(1024, 1024, TextureFormat.RGBA32, 6, true);
        terrainNormalTex.filterMode = FilterMode.Point;
        
        //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");
        
        terrainNormalMip0 = Resources.Load<Texture2D>("Textures/terrainnormal");

        terrainNormalTex.SetPixels(terrainNormalMip0.GetPixels(0), 0);


        terrainNormalTex.Apply(true, false);
        terrainMat.SetTexture("_BumpMap", terrainNormalTex);

    }
    public static void SetTerrainNormalMipmap(out Texture2D bumpMapTexIn)
    {
       // terrainMat = ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial;
        bumpMapTexIn = new Texture2D(1024, 1024, TextureFormat.RGBA32, 6, true);
        bumpMapTexIn.filterMode = FilterMode.Point;

        //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");

        terrainNormalMip0 = Resources.Load<Texture2D>("Textures/terrainnormal");
        
        bumpMapTexIn.SetPixels(terrainNormalMip0.GetPixels(0), 0);


        bumpMapTexIn.Apply(true, false);
  //      terrainMat.SetTexture("_BumpMap", terrainNormalTex);

    }
    public static void SetTerrainTexMipmap(Texture2D terrainTexIn,Texture2D bumpMapTexIn,Texture2D nonSolidTextureIn,Texture2D waterTexIn)
        {
        TerrainTextureMipmapAdjusting.applyingTerrainNormal = bumpMapTexIn;
        terrainMat= VoxelWorld.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial;
        terrainTex=new Texture2D(1024,1024,TextureFormat.RGBA32,6, false);
        terrainTex.filterMode=FilterMode.Point;
      //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");
        terrainMip0=terrainTexIn;

        terrainTex.SetPixels(terrainMip0.GetPixels(0),0);
   

        terrainTex.Apply(true,true);
        Material nonsolidMat= VoxelWorld.chunkPrefab.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
        Material waterMat= VoxelWorld.chunkPrefab.transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial;
        Material transparentMat = VoxelWorld.chunkPrefab.transform.GetChild(4).GetComponent<MeshRenderer>().sharedMaterial;
        nonsolidMat.SetTexture("_BaseMap",nonSolidTextureIn);
        waterMat.SetTexture("_BaseMap",waterTexIn);
        terrainMat.SetTexture("_BaseMap",terrainTex);
 //      terrainMat.SetTexture("_BumpMap",bumpMapTexIn);
    }

  
      public static void ResetItemChunkTextures()
    {
    
        TerrainTextureMipmapAdjusting.terrainNormalTex = Resources.Load<Texture2D>("Textures/terrainnormal");
        TerrainTextureMipmapAdjusting.waterNormalTex = Resources.Load<Texture2D>("Textures/nonsolidnormal");
        VoxelWorld.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BumpMap", TerrainTextureMipmapAdjusting.terrainNormalTex);
        VoxelWorld.chunkPrefab.transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BumpMap", TerrainTextureMipmapAdjusting.waterNormalTex);
        VoxelWorld.itemPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BaseMap", Resources.Load<Texture2D>("Textures/itemterrain"));
    }
    
 
}
