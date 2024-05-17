using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WorldHelper:IWorldHelper{
    public int chunkWidth=16;
    public int chunkHeight=256;
    public static WorldHelper instance=new WorldHelper();
    public Vector3 cameraPos;
     public Vector3Int Vec3ToBlockPos(Vector3 pos){
        Vector3Int intPos=Vector3Int.FloorToInt(pos);
        return intPos;
    }
    public bool CheckIsPosInChunk(Vector3 pos,Chunk c){
        if(c==null){
            return false;
        }
          Vector3Int intPos=Vector3Int.FloorToInt(pos);
        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(new Vector3(c.chunkPos.x,0,c.chunkPos.y));
        if(chunkSpacePos.x>=0&&chunkSpacePos.x<chunkWidth&&chunkSpacePos.z>=0&&chunkSpacePos.z<chunkWidth){
            return true;
        }else{
            return false;
        }
    }
    public void SetBlock(Vector3 pos,short blockID){
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
 
        if(chunkSpacePos.z>=chunkWidth-1){
         if(chunkNeededUpdate.frontChunk!=null){
           chunkNeededUpdate.frontChunk.isChunkMapUpdated=true;
         
        }    
        }
        if(chunkSpacePos.z<=0){
         if(chunkNeededUpdate.backChunk!=null){
          
            chunkNeededUpdate.backChunk.isChunkMapUpdated=true;
         }    
        }
        if(chunkSpacePos.x<=0){
          if(chunkNeededUpdate.leftChunk!=null){
       
            chunkNeededUpdate.leftChunk.isChunkMapUpdated=true;
        
        }   
        }
       
        if(chunkSpacePos.x>=chunkWidth-1){
            if(chunkNeededUpdate.rightChunk!=null){
      
            chunkNeededUpdate.rightChunk.isChunkMapUpdated=true;
      
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
        if (chunkNeededUpdate == null)
        {
            return;
        }
        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(chunkNeededUpdate.transform.position);
        if(chunkSpacePos.y<0||chunkSpacePos.y>=chunkHeight){
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z]=blockID;
        chunkNeededUpdate.isChunkMapUpdated=true;
     
        if(chunkSpacePos.z>=chunkWidth-1){
         if(chunkNeededUpdate.frontChunk!=null){
           chunkNeededUpdate.frontChunk.isChunkMapUpdated=true;
          
        }    
        }
        if(chunkSpacePos.z<=0){
         if(chunkNeededUpdate.backChunk!=null){
          
            chunkNeededUpdate.backChunk.isChunkMapUpdated=true;
        
        }    
        }
        if(chunkSpacePos.x<=0){
          if(chunkNeededUpdate.leftChunk!=null){
       
            chunkNeededUpdate.leftChunk.isChunkMapUpdated=true;
       
        }   
        }
       
        if(chunkSpacePos.x>=chunkWidth-1){
            if(chunkNeededUpdate.rightChunk!=null){
      
            chunkNeededUpdate.rightChunk.isChunkMapUpdated=true;
        
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
        int landingPointHeight = 100;
        for(int i=chunkHeight-2;i>1;i--){
            if(locChunk.map[chunkSpacePos.x,i-1,chunkSpacePos.y]!=0){
                landingPointHeight = i; break;
            }
        }
   //     Debug.Log("chunk landing point height:" + landingPointHeight);
        return landingPointHeight;
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
        if(chunkSpacePos.x<0||chunkSpacePos.x>=chunkWidth||chunkSpacePos.z<0||chunkSpacePos.z>=chunkWidth){
            return GetBlock(pos);
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
        ParticleEffectManagerBeh.instance.EmitBreakBlockParticleAtPosition(new Vector3(Vector3Int.FloorToInt(blockPoint).x + 0.5f, Vector3Int.FloorToInt(blockPoint).y + 0.5f, Vector3Int.FloorToInt(blockPoint).z + 0.5f), WorldHelper.instance.GetBlock(blockPoint));
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

    public static int PreCalculateChunkMaxHeight(Vector2Int chunkPos)
    {
        switch (VoxelWorld.currentWorld.worldGenType)
        {
            case 0:
        float[,] chunkRawHeight =Chunk. GetRawChunkHeightmap(chunkPos);
        float maxValue = 0f;
        foreach(float x in chunkRawHeight)
        {
            if (x > maxValue) { maxValue = x; }
        }
        return (int)maxValue;
                break;
                case 1:

                return 150;
                break;
                default: return 150;
        }
       
    }
}