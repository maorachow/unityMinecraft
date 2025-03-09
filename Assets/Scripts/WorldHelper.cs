using System;
using monogameMinecraftShared.World;
using UnityEngine;
 


public class WorldHelper:IWorldHelper{
    
    public static WorldHelper instance=new WorldHelper();
    public Vector3 cameraPos;
   
    public bool CheckIsPosInChunk(Vector3 pos,Chunk c){
        if(c==null){
            return false;
        }
          Vector3Int intPos=Vector3Int.FloorToInt(pos);
        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(new Vector3(c.chunkPos.x,0,c.chunkPos.y));
        if(chunkSpacePos.x>=0&&chunkSpacePos.x<Chunk.chunkWidth&&chunkSpacePos.z>=0&&chunkSpacePos.z< Chunk.chunkWidth){
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
        Chunk chunkNeededUpdate=Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(new Vector3(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y));
         if(chunkSpacePos.y<0||chunkSpacePos.y>= Chunk.chunkHeight){
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z]=blockID;
       
          chunkNeededUpdate.isChunkMapUpdated=true;
 
        if(chunkSpacePos.z>= Chunk.chunkWidth -1){
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
       
        if(chunkSpacePos.x>= Chunk.chunkWidth -1){
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
        Chunk chunkNeededUpdate=Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos=intPos-new Vector3Int(chunkNeededUpdate.chunkPos.x,0,chunkNeededUpdate.chunkPos.y);
         if(chunkSpacePos.y<0||chunkSpacePos.y>= Chunk.chunkHeight){
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z]=blockID;
   
    }
    public void SetBlockWithoutUpdateWithSaving(Vector3 pos, BlockData blockID)
    {
        if (blockID == -1)
        {
            return;
        }
        Vector3Int intPos = Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos = intPos - new Vector3Int(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y);
        if (chunkSpacePos.y < 0 || chunkSpacePos.y >= Chunk.chunkHeight)
        {
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z] = blockID;
        chunkNeededUpdate.isChunkMapUpdated = true;
    }
    [Obsolete]
    public void SetBlockByHand(Vector3 pos,short blockID){
        if(blockID==-1){
            return;
        }
        Vector3Int intPos=Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate=Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));
        if (chunkNeededUpdate == null)
        {
            return;
        }
        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(new Vector3(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y));
        if(chunkSpacePos.y<0||chunkSpacePos.y>= Chunk.chunkHeight){
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z]=blockID;
        chunkNeededUpdate.isChunkMapUpdated=true;
     
        if(chunkSpacePos.z>= Chunk.chunkWidth -1){
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
       
        if(chunkSpacePos.x>= Chunk.chunkWidth -1){
            if(chunkNeededUpdate.rightChunk!=null){
      
            chunkNeededUpdate.rightChunk.isChunkMapUpdated=true;
        
        } 
        }
       
    }

    [Obsolete]
    public void SetBlockDataByHand(Vector3 pos, BlockData blockID)
    {
        if (blockID == -1)
        {
            return;
        }
        Vector3Int intPos = Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));
        if (chunkNeededUpdate == null)
        {
            return;
        }
        Vector3Int chunkSpacePos = intPos - Vector3Int.FloorToInt(new Vector3(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y));
        if (chunkSpacePos.y < 0 || chunkSpacePos.y >= Chunk.chunkHeight)
        {
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z] = blockID;
        chunkNeededUpdate.isChunkMapUpdated = true;

        if (chunkSpacePos.z >= Chunk.chunkWidth - 1)
        {
            if (chunkNeededUpdate.frontChunk != null)
            {
                chunkNeededUpdate.frontChunk.isChunkMapUpdated = true;

            }
        }
        if (chunkSpacePos.z <= 0)
        {
            if (chunkNeededUpdate.backChunk != null)
            {

                chunkNeededUpdate.backChunk.isChunkMapUpdated = true;

            }
        }
        if (chunkSpacePos.x <= 0)
        {
            if (chunkNeededUpdate.leftChunk != null)
            {

                chunkNeededUpdate.leftChunk.isChunkMapUpdated = true;

            }
        }

        if (chunkSpacePos.x >= Chunk.chunkWidth - 1)
        {
            if (chunkNeededUpdate.rightChunk != null)
            {

                chunkNeededUpdate.rightChunk.isChunkMapUpdated = true;

            }
        }
    }
    public void SetBlockOptionalDataWithoutUpdate(Vector3Int pos, byte dataByte)
    {
       
        Vector3Int intPos = Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));
        if (chunkNeededUpdate == null)
        {
            return;
        }
        Vector3Int chunkSpacePos = intPos - Vector3Int.FloorToInt(new Vector3(chunkNeededUpdate.chunkPos.x,0, chunkNeededUpdate.chunkPos.y));
        if (chunkSpacePos.y < 0 || chunkSpacePos.y >= Chunk.chunkHeight)
        {
            return;
        }
        chunkNeededUpdate.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z].optionalDataValue= dataByte;
        chunkNeededUpdate.isChunkMapUpdated = true;

        if (chunkSpacePos.z >= Chunk.chunkWidth - 1)
        {
            if (chunkNeededUpdate.frontChunk != null)
            {
                chunkNeededUpdate.frontChunk.isChunkMapUpdated = true;

            }
        }
        if (chunkSpacePos.z <= 0)
        {
            if (chunkNeededUpdate.backChunk != null)
            {

                chunkNeededUpdate.backChunk.isChunkMapUpdated = true;

            }
        }
        if (chunkSpacePos.x <= 0)
        {
            if (chunkNeededUpdate.leftChunk != null)
            {

                chunkNeededUpdate.leftChunk.isChunkMapUpdated = true;

            }
        }

        if (chunkSpacePos.x >= Chunk.chunkWidth - 1)
        {
            if (chunkNeededUpdate.rightChunk != null)
            {

                chunkNeededUpdate.rightChunk.isChunkMapUpdated = true;

            }
        }
    }
    public int GetChunkLandingPoint(float x, float z){
       Vector2Int intPos=new Vector2Int((int)x,(int)z); 

        Chunk locChunk=Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(new Vector3(x,0f,z)));
        if(locChunk==null){
            return 100;
        }
        Vector2Int chunkSpacePos=intPos-locChunk.chunkPos;
        chunkSpacePos.x=Mathf.Clamp(chunkSpacePos.x,0, Chunk.chunkWidth -1);
        chunkSpacePos.y=Mathf.Clamp(chunkSpacePos.y,0, Chunk.chunkWidth -1);
        int landingPointHeight = 100;
        for(int i= Chunk.chunkHeight -2;i>1;i--){
            if(locChunk.map[chunkSpacePos.x,i-1,chunkSpacePos.y]!=0){
                landingPointHeight = i; break;
            }
        }
   //     Debug.Log("chunk landing point height:" + landingPointHeight);
        return landingPointHeight;
    }
  
    public short GetBlock(Vector3 pos){
        Vector3Int intPos=Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate=Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));
        if(chunkNeededUpdate==null){
            return 0;
        }
        Vector3Int chunkSpacePos=intPos-new Vector3Int(chunkNeededUpdate.chunkPos.x,0,chunkNeededUpdate.chunkPos.y);
       if(chunkSpacePos.y<0||chunkSpacePos.y>= Chunk.chunkHeight){
            return 0;
        }
        return chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z];
    }
    public BlockShape? GetBlockShape(Vector3 pos)
    {
        BlockData blockID = GetBlockData(pos);
        if (Chunk.blockInfosNew.ContainsKey(blockID))
        {
            return Chunk.blockInfosNew[blockID].shape;
        }
        else
        {
            return null;
        }
      
    }
    public BlockShape? GetBlockShape(BlockData blockData)
    {
       
        if (Chunk.blockInfosNew.ContainsKey(blockData.blockID))
        {
            return Chunk.blockInfosNew[blockData.blockID].shape;
        }
        else
        {
            return null;
        }

    }
    public void SendPlaceBlockOperation(Vector3 position, BlockData data)
    {
        Vector3Int positionInt = Vector3Int.FloorToInt(position);
        VoxelWorld.currentWorld.worldUpdater.queuedChunkUpdatePoints.Enqueue(new PlacingBlockOperation(positionInt, VoxelWorld.currentWorld.worldUpdater, data));
        Chunk chunkNeededUpdate = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(new Vector3(position.x, position.y, position.z)));
        if (chunkNeededUpdate == null || chunkNeededUpdate.isMeshBuildCompleted == false)
        {
            return;
        }
     /*   Vector3Int chunkSpacePos = position - new Vector3Int(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y);

        if (chunkSpacePos.z >= Chunk.chunkWidth - 1)
        {
            if (chunkNeededUpdate.frontChunk != null)
            {
                chunkNeededUpdate.frontChunk.isChunkMapUpdated = true;

            }
        }
        if (chunkSpacePos.z <= 0)
        {
            if (chunkNeededUpdate.backChunk != null)
            {

                chunkNeededUpdate.backChunk.isChunkMapUpdated = true;

            }
        }
        if (chunkSpacePos.x <= 0)
        {
            if (chunkNeededUpdate.leftChunk != null)
            {

                chunkNeededUpdate.leftChunk.isChunkMapUpdated = true;

            }
        }

        if (chunkSpacePos.x >= Chunk.chunkWidth - 1)
        {
            if (chunkNeededUpdate.rightChunk != null)
            {

                chunkNeededUpdate.rightChunk.isChunkMapUpdated = true;

            }
        }*/


    }
    public void SendBreakBlockOperation(Vector3Int position)
    {
        BlockData prevData = WorldHelper.instance.GetBlockData(position);
        VoxelWorld.currentWorld.worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position, VoxelWorld.currentWorld.worldUpdater, prevData));
        VoxelWorld.currentWorld.worldUpdater.queuedChunkUpdatePoints.Enqueue(new PlacingBlockOperation(position, VoxelWorld.currentWorld.worldUpdater, 0));





        Chunk chunkNeededUpdate = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(new Vector3(position.x, position.y, position.z)));
      
        /*      Vector3Int chunkSpacePos = position - new Vector3Int(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y);
              if (chunkSpacePos.x == 0)
              {

                  GetChunk(new Vector2Int(chunkNeededUpdate.chunkPos.x - Chunk.chunkWidth, chunkNeededUpdate.chunkPos.y))?.BuildChunkAsync();

              }
              if (chunkSpacePos.x == Chunk.chunkWidth - 1)
              {
                  // if (chunkNeededUpdate.rightChunk != null && chunkNeededUpdate.rightChunk.isMapGenCompleted == true)

                  //  chunkNeededUpdate.rightChunk.BuildChunk();
                  GetChunk(new Vector2Int(chunkNeededUpdate.chunkPos.x + Chunk.chunkWidth, chunkNeededUpdate.chunkPos.y))?.BuildChunkAsync();
              }
              if (chunkSpacePos.z == 0)
              {
                  //  if (chunkNeededUpdate.backChunk != null && chunkNeededUpdate.backChunk.isMapGenCompleted == true)
                  //       {
                  //         chunkNeededUpdate.backChunk.BuildChunk();
                  //     }
                  GetChunk(new Vector2Int(chunkNeededUpdate.chunkPos.x, chunkNeededUpdate.chunkPos.y - Chunk.chunkWidth))?.BuildChunkAsync();
              }
              if (chunkSpacePos.z == Chunk.chunkWidth - 1)
              {
                  //   if (chunkNeededUpdate.frontChunk != null && chunkNeededUpdate.frontChunk.isMapGenCompleted == true)
                  //     {
                  //      chunkNeededUpdate.frontChunk.BuildChunk();
                  //     }

                  GetChunk(new Vector2Int(chunkNeededUpdate.chunkPos.x, chunkNeededUpdate.chunkPos.y + Chunk.chunkWidth))?.BuildChunkAsync();
              }*/
    }


    public BlockData GetBlockData(Vector3 pos)
    {
        Vector3Int intPos = Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));
        if (chunkNeededUpdate == null)
        {
            return 0;
        }
        Vector3Int chunkSpacePos = intPos - new Vector3Int(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y);
        if (chunkSpacePos.y < 0 || chunkSpacePos.y >= Chunk.chunkHeight)
        {
            return 0;
        }
        return chunkNeededUpdate.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z];
    }
    public short GetBlock(Vector3 pos,Chunk chunkNeededUpdate){

        Vector3Int intPos=Vector3Int.FloorToInt(pos);
      
        if(chunkNeededUpdate==null){
            return 0;
        }
        Vector3Int chunkSpacePos=intPos-new Vector3Int(chunkNeededUpdate.chunkPos.x,0,chunkNeededUpdate.chunkPos.y);
       if(chunkSpacePos.y<0||chunkSpacePos.y>= Chunk.chunkHeight){
            return 0;
        }
        if(chunkSpacePos.x<0||chunkSpacePos.x>= Chunk.chunkWidth ||chunkSpacePos.z<0||chunkSpacePos.z>= Chunk.chunkWidth){
            return GetBlock(pos);
        }
        return chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z];
    }

    public short GetBlockInSingleChunk(Vector3 chunkSpacePos,Chunk chunkNeededUpdate){
        Vector3Int intPos=Vector3Int.FloorToInt(chunkSpacePos);
       if(intPos.y<0||intPos.y>= Chunk.chunkHeight){
            return 0;
        }
        return chunkNeededUpdate.map[intPos.x,intPos.y,intPos.z];
    }

    /*   public Vector2Int Vec3ToChunkPos(Vector3 pos){
           Vector3 tmp=pos;
           tmp.x = Mathf.Floor(tmp.x / (float)chunkWidth) * chunkWidth;
           tmp.z = Mathf.Floor(tmp.z / (float)chunkWidth) * chunkWidth;
           Vector2Int value=new Vector2Int((int)tmp.x,(int)tmp.z);
           return value;
       }*/
    [Obsolete]
    public void StartUpdateAtPoint(Vector3 blockPoint)
    {
        Vector3Int intPos = new Vector3Int(WorldHelper.instance.FloatToInt(blockPoint.x),
            WorldHelper.instance.FloatToInt(blockPoint.y), WorldHelper.instance.FloatToInt(blockPoint.z));
        Chunk chunkNeededUpdate = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(blockPoint));
        Vector3Int chunkSpacePos = intPos - Vector3Int.FloorToInt(chunkNeededUpdate.transform.position);
        chunkNeededUpdate.BFSInit(chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z);
    }

    [Obsolete]
    public void StartUpdateAtPoint(Vector3 blockPoint, ChunkUpdateTypes updateType,
        BlockData? optionalPrevBlockData = null)
    {
        Vector3Int intPos = new Vector3Int(WorldHelper.instance.FloatToInt(blockPoint.x),
            WorldHelper.instance.FloatToInt(blockPoint.y), WorldHelper.instance.FloatToInt(blockPoint.z));
        Chunk chunkNeededUpdate = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(blockPoint));
        Vector3Int chunkSpacePos = intPos - Vector3Int.FloorToInt(chunkNeededUpdate.transform.position);
        chunkNeededUpdate.BFSInit(chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z, updateType, optionalPrevBlockData);
    }

    [Obsolete]
    public void BreakBlockAtPoint(Vector3 blockPoint)
    {
        ParticleEffectManagerBeh.instance.EmitBreakBlockParticleAtPosition(
            new Vector3(Vector3Int.FloorToInt(blockPoint).x + 0.5f, Vector3Int.FloorToInt(blockPoint).y + 0.5f,
                Vector3Int.FloorToInt(blockPoint).z + 0.5f), WorldHelper.instance.GetBlock(blockPoint));
        ItemEntityBeh.SpawnNewItem(Vector3Int.FloorToInt(blockPoint).x + 0.5f,
            Vector3Int.FloorToInt(blockPoint).y + 0.5f, Vector3Int.FloorToInt(blockPoint).z + 0.5f,
            ItemIDToBlockID.blockIDToItemIDDic[WorldHelper.instance.GetBlock(blockPoint)],
            new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f),
                UnityEngine.Random.Range(-3f, 3f)));
        WorldHelper.instance.SetBlockByHand(blockPoint, 0);
        //  UpdateChunkMeshCollider(blockPoint);
    }

    public void BreakBlockInArea(Vector3 centerPoint, Vector3 minPoint, Vector3 maxPoint)
    {
        for (float x = minPoint.x; x <= maxPoint.x; x++)
        {
            for (float y = minPoint.y; y <= maxPoint.y; y++)
            {
                for (float z = minPoint.z; z <= maxPoint.z; z++)
                {
                    Vector3 blockPointArea = centerPoint + new Vector3(x, y, z);
                    int tmpID2 = WorldHelper.instance.GetBlock(blockPointArea);
                    if (tmpID2 == 0)
                    {
                        continue;
                    }

                    WorldHelper.instance.SendBreakBlockOperation(ChunkCoordsHelper.Vec3ToBlockPos(blockPointArea));
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
        float[,] chunkRawHeight =Chunk. GenerateChunkHeightmap(chunkPos);
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