using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTextureMipmapAdjusting : MonoBehaviour
{
    public static Texture2D terrainTex;
    public static Texture2D terrainMip0;
    public static Texture2D terrainNormalTex;
    public static Texture2D terrainNormalMip0;
    public static Material terrainMat;

      public static void SetTerrainTexMipmap()
        {
        terrainMat=ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial;
        terrainTex=new Texture2D(1024,1024,TextureFormat.RGBA32,6, false);
        terrainTex.filterMode=FilterMode.Point;
        terrainNormalTex=new Texture2D (1024,1024,TextureFormat.RGBA32,6,false);
        terrainNormalTex.filterMode=FilterMode.Point;

        //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");
        terrainMip0 =Resources.Load<Texture2D>("Textures/terrain");
        terrainNormalMip0 = Resources.Load<Texture2D>("Textures/terrainnormal");
        terrainTex.SetPixels(terrainMip0.GetPixels(0),0);
        terrainNormalTex.SetPixels(terrainNormalMip0.GetPixels(0), 0);

        terrainTex.Apply(true,true);
        terrainNormalTex.Apply(true, true);
        terrainMat.SetTexture("_BaseMap",terrainTex);
        terrainMat.SetTexture("_BumpMap", terrainNormalTex);
    }

    public static void SetTerrainNormalMipmap(Texture2D terrainNormalTexIn)
    {

        terrainNormalTexIn = new Texture2D(1024, 1024, TextureFormat.RGBA32, 6, false);
        terrainNormalTexIn.filterMode = FilterMode.Point;

        //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");
        
        terrainNormalMip0 = Resources.Load<Texture2D>("Textures/terrainnormal");

        terrainNormalTexIn.SetPixels(terrainNormalMip0.GetPixels(0), 0);


        terrainNormalTexIn.Apply(true, true);
      
    }
    public static void SetTerrainTexMipmap(Texture2D terrainTexIn,Texture2D bumpMapTexIn,Texture2D nonSolidTextureIn,Texture2D waterTexIn)
        {
          pauseMenuUI.instance.terrainNormal=bumpMapTexIn;
        terrainMat=ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial;
        terrainTex=new Texture2D(1024,1024,TextureFormat.RGBA32,6, false);
        terrainTex.filterMode=FilterMode.Point;
      //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");
        terrainMip0=terrainTexIn;

        terrainTex.SetPixels(terrainMip0.GetPixels(0),0);
   

        terrainTex.Apply(true,true);
        Material nonsolidMat=ObjectPools.chunkPrefab.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
        Material waterMat=ObjectPools.chunkPrefab.transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial;
        nonsolidMat.SetTexture("_BaseMap",nonSolidTextureIn);
        waterMat.SetTexture("_BaseMap",waterTexIn);
        terrainMat.SetTexture("_BaseMap",terrainTex);
 //      terrainMat.SetTexture("_BumpMap",bumpMapTexIn);
    }

  
      
    
 
}
