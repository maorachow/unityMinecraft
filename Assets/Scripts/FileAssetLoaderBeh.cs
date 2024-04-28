using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using MessagePack;
public class FileAssetLoaderBeh : MonoBehaviour
{
    public static FileAssetLoaderBeh instance;
    public AssetBundle audioAB;
    public AssetBundle textureAB;
    void Start()
    {
        instance=this;
    }

    public bool LoadBlockNameDic(string path){
        GameUIBeh.blockNameDic.Clear();
        try{
            GameUIBeh.blockNameDic=MessagePackSerializer.Deserialize<Dictionary<int,string>>(File.ReadAllBytes(path));
            return true;
        }catch(Exception e){
            Debug.Log("Loading block name failed: "+e.ToString());
            GameUIBeh.AddBlockNameInfo();
             return false;
        }
       
    }
    public bool LoadChunkBlockInfo(string path){
        Chunk.blockInfo.Clear();
        try{
        Chunk.blockInfo=MessagePackSerializer.Deserialize<Dictionary<int,List<Vector2>>>(File.ReadAllBytes(path));
        return true;
        }catch(Exception e){
            Debug.Log("Loading block info failed: "+e.ToString());
           Chunk.AddBlockInfo();
           return false;
        }
    }
    public bool LoadItemBlockInfo(string path){
        Chunk.itemBlockInfo.Clear();
        try{
        Chunk.itemBlockInfo=MessagePackSerializer.Deserialize<Dictionary<int,List<Vector2>>>(File.ReadAllBytes(path));
        return true;
        }catch(Exception e){
            Debug.Log("Loading block info failed: "+e.ToString());
           Chunk.AddBlockInfo();
           return false;
        }
    }
    public bool  LoadBlockAudio(string path){
        try{
            if(audioAB!=null){
                audioAB.Unload(true);
            }
      audioAB=AssetBundle.LoadFromFile(path);
        Chunk.blockAudioDic.Clear();
        Chunk.blockAudioDic.TryAdd(1,audioAB.LoadAsset<AudioClip>("Stone_dig2"));
         Chunk.blockAudioDic.TryAdd(2,audioAB.LoadAsset<AudioClip>("Grass_dig1"));
         Chunk.blockAudioDic.TryAdd(3,audioAB.LoadAsset<AudioClip>("Gravel_dig1"));
         Chunk.blockAudioDic.TryAdd(4,audioAB.LoadAsset<AudioClip>("Grass_dig1"));
         Chunk.blockAudioDic.TryAdd(5,audioAB.LoadAsset<AudioClip>("Stone_dig2"));
         Chunk.blockAudioDic.TryAdd(6,audioAB.LoadAsset<AudioClip>("Wood_dig1"));
         Chunk.blockAudioDic.TryAdd(7,audioAB.LoadAsset<AudioClip>("Wood_dig1"));
         Chunk.blockAudioDic.TryAdd(8,audioAB.LoadAsset<AudioClip>("Wood_dig1"));
         Chunk.blockAudioDic.TryAdd(9,audioAB.LoadAsset<AudioClip>("Grass_dig1"));
        Chunk. blockAudioDic.TryAdd(10,audioAB.LoadAsset<AudioClip>("Stone_dig2"));
         Chunk.blockAudioDic.TryAdd(11,audioAB.LoadAsset<AudioClip>("Sand_dig1"));
         Chunk.blockAudioDic.TryAdd(100,audioAB.LoadAsset<AudioClip>("Stone_dig2"));
         Chunk.blockAudioDic.TryAdd(101,audioAB.LoadAsset<AudioClip>("Grass_dig1"));
         Chunk.blockAudioDic.TryAdd(102,audioAB.LoadAsset<AudioClip>("Wood_dig1"));
         return true;
        }catch(Exception e){
              Debug.Log("Loading block audio failed: "+e.ToString());
           Chunk.AddBlockInfo();
           return false;
        }
       
    }
    public bool LoadBlockTexture(string path){
         try{
            if(textureAB!=null){
            textureAB.Unload(true);    
            }
            
           textureAB=AssetBundle.LoadFromFile(path);
            TerrainTextureMipmapAdjusting.SetTerrainTexMipmap(textureAB.LoadAsset<Texture2D>("terrain"),textureAB.LoadAsset<Texture2D>("terrainnormal"),textureAB.LoadAsset<Texture2D>("nonsolid"),textureAB.LoadAsset<Texture2D>("nonsolid"));
            VoxelWorld.itemPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BaseMap",textureAB.LoadAsset<Texture2D>("itemterrain"));
            ItemEntityBeh.itemTextureInfo=textureAB.LoadAsset<Texture2D>("itemterrain");
            return true;
         }catch(Exception e){
              Debug.Log("Loading block texture failed: "+e.ToString());
                Chunk.AddBlockInfo();
                 TerrainTextureMipmapAdjusting.SetTerrainTexMipmap(Resources.Load<Texture2D>("Textures/terrain"),Resources.Load<Texture2D>("Textures/terrainnormal"),Resources.Load<Texture2D>("Textures/nonsolid"),Resources.Load<Texture2D>("Textures/nonsolid"));
            VoxelWorld.itemPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>("Textures/itemterrain"));
                ItemEntityBeh.AddFlatItemInfo();
                return false;

         }
       
    }

    public void UnloadAndResetResouces(){
         Chunk.AddBlockInfo();
             TerrainTextureMipmapAdjusting.SetTerrainTexMipmap(Resources.Load<Texture2D>("Textures/terrain"),Resources.Load<Texture2D>("Textures/terrainnormal"),Resources.Load<Texture2D>("Textures/nonsolid"),Resources.Load<Texture2D>("Textures/nonsolid"));
             if(audioAB!=null){
             audioAB.Unload(true);   
             }
            if(textureAB!=null){
            textureAB.Unload(true);    
            }
            
    }
}
