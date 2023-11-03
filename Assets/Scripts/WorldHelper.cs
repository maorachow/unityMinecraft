using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WorldHelper:IWorldHelper{
    public int chunkWidth=16;
    public int chunkHeight=256;
    public static WorldHelper instance=new WorldHelper();
     public Vector3Int Vec3ToBlockPos(Vector3 pos){
        Vector3Int intPos=new Vector3Int(FloatToInt(pos.x),FloatToInt(pos.y),FloatToInt(pos.z));
        return intPos;
    }
   
    public void SetBlock(Vector3 pos,short blockID){

        Vector3Int intPos=new Vector3Int(FloatToInt(pos.x),FloatToInt(pos.y),FloatToInt(pos.z));
        Chunk chunkNeededUpdate=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos=intPos-new Vector3Int(FloatToInt(chunkNeededUpdate.transform.position.x),FloatToInt(chunkNeededUpdate.transform.position.y),FloatToInt(chunkNeededUpdate.transform.position.z));
         if(chunkSpacePos.y<0||chunkSpacePos.y>=chunkHeight){
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z]=blockID;
        chunkNeededUpdate.isChunkColliderUpdated=true;
          chunkNeededUpdate.isChunkMapUpdated=true;
 
        if(chunkSpacePos.z>=chunkWidth-1){
         if(chunkNeededUpdate.frontChunk!=null){
           chunkNeededUpdate.frontChunk.isChunkMapUpdated=true;
              chunkNeededUpdate.frontChunk.isChunkColliderUpdated=true;
        }    
        }
        if(chunkSpacePos.z<=0){
         if(chunkNeededUpdate.backChunk!=null){
          
            chunkNeededUpdate.backChunk.isChunkMapUpdated=true;
            chunkNeededUpdate.backChunk.isChunkColliderUpdated=true;
        }    
        }
        if(chunkSpacePos.x<=0){
          if(chunkNeededUpdate.leftChunk!=null){
       
            chunkNeededUpdate.leftChunk.isChunkMapUpdated=true;
           chunkNeededUpdate.leftChunk.isChunkColliderUpdated=true;
        }   
        }
       
        if(chunkSpacePos.x>=chunkWidth-1){
            if(chunkNeededUpdate.rightChunk!=null){
      
            chunkNeededUpdate.rightChunk.isChunkMapUpdated=true;
            chunkNeededUpdate.rightChunk.isChunkColliderUpdated=true;
        } 
        }
    }

     
    public void SetBlockWithoutUpdate(Vector3 pos,short blockID){

        Vector3Int intPos=new Vector3Int(FloatToInt(pos.x),FloatToInt(pos.y),FloatToInt(pos.z));
        Chunk chunkNeededUpdate=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos=intPos-new Vector3Int(FloatToInt(chunkNeededUpdate.transform.position.x),FloatToInt(chunkNeededUpdate.transform.position.y),FloatToInt(chunkNeededUpdate.transform.position.z));
         if(chunkSpacePos.y<0||chunkSpacePos.y>=chunkHeight){
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z]=blockID;
   
    }
      
    public void SetBlockByHand(Vector3 pos,short blockID){

        Vector3Int intPos=new Vector3Int(FloatToInt(pos.x),FloatToInt(pos.y),FloatToInt(pos.z));
        Chunk chunkNeededUpdate=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(chunkNeededUpdate.transform.position);
        if(chunkSpacePos.y<0||chunkSpacePos.y>=chunkHeight){
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z]=blockID;
        chunkNeededUpdate.isChunkMapUpdated=true;
        chunkNeededUpdate.isChunkColliderUpdated=true;
        if(chunkSpacePos.z>=chunkWidth-1){
         if(chunkNeededUpdate.frontChunk!=null){
           chunkNeededUpdate.frontChunk.isChunkMapUpdated=true;
              chunkNeededUpdate.frontChunk.isChunkColliderUpdated=true;
        }    
        }
        if(chunkSpacePos.z<=0){
         if(chunkNeededUpdate.backChunk!=null){
          
            chunkNeededUpdate.backChunk.isChunkMapUpdated=true;
            chunkNeededUpdate.backChunk.isChunkColliderUpdated=true;
        }    
        }
        if(chunkSpacePos.x<=0){
          if(chunkNeededUpdate.leftChunk!=null){
       
            chunkNeededUpdate.leftChunk.isChunkMapUpdated=true;
           chunkNeededUpdate.leftChunk.isChunkColliderUpdated=true;
        }   
        }
       
        if(chunkSpacePos.x>=chunkWidth-1){
            if(chunkNeededUpdate.rightChunk!=null){
      
            chunkNeededUpdate.rightChunk.isChunkMapUpdated=true;
            chunkNeededUpdate.rightChunk.isChunkColliderUpdated=true;
        } 
        }
       
    }
     
    public int GetChunkLandingPoint(float x, float z){
       Vector2Int intPos=new Vector2Int((int)x,(int)z); 

        Chunk locChunk=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(new Vector3(x,0f,z)));
        if(locChunk==null){
            return 100;
        }
        Vector2Int chunkSpacePos=intPos-locChunk.chunkPos;
        chunkSpacePos.x=Mathf.Clamp(chunkSpacePos.x,0,chunkWidth-1);
        chunkSpacePos.y=Mathf.Clamp(chunkSpacePos.y,0,chunkWidth-1);
  
        for(int i=chunkHeight-2;i>1;i--){
            if(locChunk.map[chunkSpacePos.x,i-1,chunkSpacePos.y]!=0){
                return i;
            }
        }
        return 100;
    }
  
    public short GetBlock(Vector3 pos){
        Vector3Int intPos=Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate=Chunk.GetChunk(Vec3ToChunkPos(pos));
        if(chunkNeededUpdate==null){
            return 0;
        }
        Vector3Int chunkSpacePos=intPos-new Vector3Int(chunkNeededUpdate.chunkPos.x,0,chunkNeededUpdate.chunkPos.y);
       if(chunkSpacePos.y<0||chunkSpacePos.y>=chunkHeight){
            return 0;
        }
        return chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z];
    }
    public short GetBlock(Vector3 pos,Chunk chunkNeededUpdate){

        Vector3Int intPos=Vector3Int.FloorToInt(pos);
      
        if(chunkNeededUpdate==null){
            return 0;
        }
        Vector3Int chunkSpacePos=intPos-new Vector3Int(chunkNeededUpdate.chunkPos.x,0,chunkNeededUpdate.chunkPos.y);
       if(chunkSpacePos.y<0||chunkSpacePos.y>=chunkHeight){
            return 0;
        }
        return chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z];
    }

    public short GetBlockInSingleChunk(Vector3 chunkSpacePos,Chunk chunkNeededUpdate){
        Vector3Int intPos=Vector3Int.FloorToInt(chunkSpacePos);
       if(intPos.y<0||intPos.y>=chunkHeight){
            return 0;
        }
        return chunkNeededUpdate.map[intPos.x,intPos.y,intPos.z];
    }

        public Vector2Int Vec3ToChunkPos(Vector3 pos){
        Vector3 tmp=pos;
        tmp.x = Mathf.Floor(tmp.x / (float)chunkWidth) * chunkWidth;
        tmp.z = Mathf.Floor(tmp.z / (float)chunkWidth) * chunkWidth;
        Vector2Int value=new Vector2Int((int)tmp.x,(int)tmp.z);
        return value;
    }
       
    public int FloatToInt(float f){
        if(f>=0){
            return (int)f;
        }else{
            return (int)f-1;
        }
    }
}