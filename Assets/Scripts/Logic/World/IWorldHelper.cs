using UnityEngine;

public interface IWorldHelper
{
    

    public const int chunkWidth=16;
    public const int chunkHeight=256;
 
    public int FloatToInt(float f);
    
    
 

     public void SetBlockByHand(Vector3 pos,short blockID);
         public void SetBlock(Vector3 pos,short blockID);
         public void SetBlockWithoutUpdate(Vector3 pos,short blockID);
    public int GetChunkLandingPoint(float x, float z);
  
    public short GetBlock(Vector3 pos);
    public short GetBlock(Vector3 pos,Chunk chunkNeededUpdate);

    public short GetBlockInSingleChunk(Vector3 chunkSpacePos,Chunk chunkNeededUpdate);
}

