using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WorldHelper:IWorldHelper{
    public int chunkWidth=16;
    public int chunkHeight=256;
    public static WorldHelper instance=new WorldHelper();
     public Vector3Int Vec3ToBlockPos(Vector3 pos){
        Vector3Int intPos=Vector3Int.FloorToInt(pos);
        return intPos;
    }
   
    public void SetBlock(Vector3 pos,short blockID){
            if(blockID==-1){
            return;
        }
        Vector3Int intPos=Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos=intPos-new Vector3Int(FloatToInt(chunkNeededUpdate.chunkPos.x),FloatToInt(chunkNeededUpdate.transform.position.y),FloatToInt(chunkNeededUpdate.chunkPos.y));
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
            if(blockID==-1){
            return;
        }
        Vector3Int intPos=Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos=intPos-new Vector3Int(chunkNeededUpdate.chunkPos.x,0,chunkNeededUpdate.chunkPos.y);
         if(chunkSpacePos.y<0||chunkSpacePos.y>=chunkHeight){
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z]=blockID;
   
    }
      
    public void SetBlockByHand(Vector3 pos,short blockID){
        if(blockID==-1){
            return;
        }
        Vector3Int intPos=Vector3Int.FloorToInt(pos);
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
    public void StartUpdateAtPoint(Vector3 blockPoint){
           Vector3Int intPos=new Vector3Int(WorldHelper.instance.FloatToInt(blockPoint.x),WorldHelper.instance.FloatToInt(blockPoint.y),WorldHelper.instance.FloatToInt(blockPoint.z));
            Chunk chunkNeededUpdate=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(blockPoint));
            Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(chunkNeededUpdate.transform.position);
            chunkNeededUpdate.BFSInit(chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z);
    }
    public void BreakBlockAtPoint(Vector3 blockPoint){
            GameObject a=ObjectPools.particleEffectPool.Get();
            a.transform.position=new Vector3(Vector3Int.FloorToInt(blockPoint).x+0.5f,Vector3Int.FloorToInt(blockPoint).y+0.5f,Vector3Int.FloorToInt(blockPoint).z+0.5f);
            a.GetComponent<particleAndEffectBeh>().blockID=WorldHelper.instance.GetBlock(blockPoint);
            a.GetComponent<particleAndEffectBeh>().SendMessage("EmitParticle");
            ItemEntityBeh.SpawnNewItem(Vector3Int.FloorToInt(blockPoint).x+0.5f,Vector3Int.FloorToInt(blockPoint).y+0.5f,Vector3Int.FloorToInt(blockPoint).z+0.5f,ItemIDToBlockID.blockIDToItemIDDic[WorldHelper.instance.GetBlock(blockPoint)],new Vector3(UnityEngine.Random.Range(-3f,3f),UnityEngine.Random.Range(-3f,3f),UnityEngine.Random.Range(-3f,3f)));
            WorldHelper.instance.SetBlockByHand(blockPoint,0);
          //  UpdateChunkMeshCollider(blockPoint);
    }
    public void BreakBlockInArea(Vector3 centerPoint,Vector3 minPoint,Vector3 maxPoint){
          for(float x=minPoint.x;x<=maxPoint.x;x++){
                for(float y=minPoint.y;y<=maxPoint.y;y++){
                    for(float z=minPoint.z;z<=maxPoint.z;z++){
                        Vector3 blockPointArea=centerPoint+new Vector3(x,y,z);
                    int tmpID2=WorldHelper.instance.GetBlock(blockPointArea);
                    if(tmpID2==0){
                    continue;
                    }
                    
                    BreakBlockAtPoint(blockPointArea);
               

                    }
                }
             }
    }
    public int FloatToInt(float f){
        if(f>=0){
            return (int)f;
        }else{
            return (int)f-1;
        }
    }
}