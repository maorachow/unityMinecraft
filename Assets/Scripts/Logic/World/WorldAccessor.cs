using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAccessor
{
    public VoxelWorld world;
    public WorldAccessor(VoxelWorld world)
    {
        this.world = world;
    }

    public Chunk GetChunk(Vector2Int chunkPos) => world.GetChunk(chunkPos);
    public void SetBlockWithoutUpdate(Vector3 pos, BlockData blockID)
    {
        if (blockID == -1)
        {
            return;
        }

        Vector3Int intPos = Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate = world.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos =
            intPos - new Vector3Int(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y);
        if (chunkSpacePos.y < 0 || chunkSpacePos.y >= Chunk.chunkHeight)
        {
            return;
        }

        chunkNeededUpdate.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z] = blockID;
    }

    public void SetBlockOptionalDataWithoutUpdate(Vector3Int pos, byte dataByte)
    {
        Vector3Int intPos = Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate = world.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));
        if (chunkNeededUpdate == null)
        {
            return;
        }

        Vector3Int chunkSpacePos = intPos -
                                   Vector3Int.FloorToInt(new Vector3(chunkNeededUpdate.chunkPos.x, 0,
                                       chunkNeededUpdate.chunkPos.y));
        if (chunkSpacePos.y < 0 || chunkSpacePos.y >= Chunk.chunkHeight)
        {
            return;
        }

        BlockData blockData1 = chunkNeededUpdate.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z];
        blockData1.optionalDataValue = dataByte;
        chunkNeededUpdate.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z] = blockData1;
    }

    public BlockData GetBlockData(Vector3 pos)
    {
        Vector3Int intPos = Vector3Int.FloorToInt(pos);
        Chunk chunkNeededUpdate = world.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(pos));
        if (chunkNeededUpdate == null)
        {
            return 0;
        }

        Vector3Int chunkSpacePos =
            intPos - new Vector3Int(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y);
        if (chunkSpacePos.y < 0 || chunkSpacePos.y >= Chunk.chunkHeight)
        {
            return 0;
        }

        return chunkNeededUpdate.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z];
    }
    public BlockShape GetBlockShape(Vector3 pos)
    {
        BlockData blockID = GetBlockData(pos);

        return GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.GetBlockInfo(blockID).shape;

    }

    public BlockShape GetBlockShape(BlockData blockID)
    {
         

        return GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.GetBlockInfo(blockID).shape;

    }
}
