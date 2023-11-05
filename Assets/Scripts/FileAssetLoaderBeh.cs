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

    public void LoadBlockNameDic(string path){
        PlayerMove.blockNameDic.Clear();
        try{
        PlayerMove.blockNameDic=MessagePackSerializer.Deserialize<Dictionary<int,string>>(File.ReadAllBytes(path));
        }catch(Exception e){
            Debug.Log("Loading block name failed: "+e.ToString());
            PlayerMove.AddBlockNameInfo();
        }
       
    }
    public void LoadChunkBlockInfo(string path){
        Chunk.blockInfo.Clear();
        try{
        Chunk.blockInfo=MessagePackSerializer.Deserialize<Dictionary<int,List<Vector2>>>(File.ReadAllBytes(path));
        }catch(Exception e){
            Debug.Log("Loading block info failed: "+e.ToString());
           Chunk.AddBlockInfo();
        }
    }
    public void LoadItemBlockInfo(string path){
        Chunk.itemBlockInfo.Clear();
        try{
        Chunk.itemBlockInfo=MessagePackSerializer.Deserialize<Dictionary<int,List<Vector2>>>(File.ReadAllBytes(path));
        }catch(Exception e){
            Debug.Log("Loading block info failed: "+e.ToString());
           Chunk.AddBlockInfo();
        }
    }
    public void LoadBlockAudio(string path){
        try{
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
        }catch(Exception e){
              Debug.Log("Loading block audio failed: "+e.ToString());
           Chunk.AddBlockInfo();
        }
       
    }
    public void LoadBlockTexture(string path){
         try{
           textureAB=AssetBundle.LoadFromFile(path);
            TerrainTextureMipmapAdjusting.SetTerrainTexMipmap(textureAB.LoadAsset<Texture2D>("terrain"),textureAB.LoadAsset<Texture2D>("terrainnormal"),textureAB.LoadAsset<Texture2D>("nonsolid"),textureAB.LoadAsset<Texture2D>("nonsolid"));
         }catch(Exception e){
              Debug.Log("Loading block texture failed: "+e.ToString());
                Chunk.AddBlockInfo();
                 TerrainTextureMipmapAdjusting.SetTerrainTexMipmap(Resources.Load<Texture2D>("Textures/terrain"),Resources.Load<Texture2D>("Textures/terrainnormal"),Resources.Load<Texture2D>("Textures/nonsolid"),Resources.Load<Texture2D>("Textures/nonsolid"));
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
