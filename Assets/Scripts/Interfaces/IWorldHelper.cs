using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWorldHelper
{
    

    public const int chunkWidth=16;
    public const int chunkHeight=256;
    public Vector2Int Vec3ToChunkPos(Vector3 pos);
    public int FloatToInt(float f);
    
    
    public Vector3Int Vec3ToBlockPos(Vector3 pos);

     public void SetBlockByHand(Vector3 pos,int blockID);
         public void SetBlock(Vector3 pos,int blockID);
         public void SetBlockWithoutUpdate(Vector3 pos,int blockID);
    public int GetChunkLandingPoint(float x, float z);
  
    public int GetBlock(Vector3 pos);
    public int GetBlock(Vector3 pos,Chunk chunkNeededUpdate);

    public int GetBlockInSingleChunk(Vector3 chunkSpacePos,Chunk chunkNeededUpdate);
}

