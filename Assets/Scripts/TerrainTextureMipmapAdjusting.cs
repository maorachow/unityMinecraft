using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTextureMipmapAdjusting : MonoBehaviour
{
    public static Texture2D terrainTex;
    public static Texture2D terrainMip0;
    public static Texture2D terrainMip2;
    public static Texture2D terrainMip4;
    public static Texture2D terrainMip6;
    public static Texture2D terrainMip1;
    public static Texture2D terrainMip3;
    public static Texture2D terrainMip5;

    public static Material terrainMat;

      public static void SetTerrainTexMipmap()
        {
        terrainMat=ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial;
        terrainTex=new Texture2D(1024,1024,TextureFormat.RGBA32,6, false);
        terrainTex.filterMode=FilterMode.Point;
      //  var terrainTex2=Resources.Load<Texture2D>("Textures/terrain2");
        terrainMip0=Resources.Load<Texture2D>("Textures/terrain");

        terrainTex.SetPixels(terrainMip0.GetPixels(0),0);
   

        terrainTex.Apply(true,true);
        terrainMat.SetTexture("_BaseMap",terrainTex);
    }

  
      
    
 
}
