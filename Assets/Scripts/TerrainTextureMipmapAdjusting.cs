using MessagePack.Formatters;
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
        terrainTex=new Texture2D(1024,1024,TextureFormat.RGBA32, 6, false);
        terrainTex.filterMode=FilterMode.Point;
 

        //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");
        terrainMip0 =Resources.Load<Texture2D>("Textures/terrain");
     
        terrainTex.SetPixels(terrainMip0.GetPixels(0),0);
 

        terrainTex.Apply(true,true);
     
        terrainMat.SetTexture("_BaseMap",terrainTex);
   
    }

    public static void SetTerrainNormalMipmap()
    {
        terrainMat = ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial;
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
