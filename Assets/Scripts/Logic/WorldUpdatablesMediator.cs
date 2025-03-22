using monogameMinecraftShared.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using Random = UnityEngine.Random;

public class WorldUpdateablesMediator
{
    public static WorldUpdateablesMediator instance=new WorldUpdateablesMediator();

    private WorldUpdateablesMediator()
    {

    }

    public BlockData GetBlockData(Vector3 position)
    {
        return VoxelWorld.currentWorld.worldAccessor.GetBlockData(position);
    }
    public BlockData GetBlockData(Vector3 position,Chunk curChunk)
    {
        Vector3Int intPos = Vector3Int.FloorToInt(position);

        if (curChunk == null)
        {
            return 0;
        }
        Vector3Int chunkSpacePos =
            intPos - new Vector3Int(curChunk.chunkPos.x, 0, curChunk.chunkPos.y);
        if (chunkSpacePos.y < 0 || chunkSpacePos.y >= Chunk.chunkHeight)
        {
            return 0;
        }

        if (chunkSpacePos.x < 0 || chunkSpacePos.x >= Chunk.chunkWidth || chunkSpacePos.z < 0 ||
            chunkSpacePos.z >= Chunk.chunkWidth)
        {
            return GetBlockData(position);
        }

        return curChunk.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z];
    }
    public BlockShape GetBlockShape(Vector3 position)
    {
        return VoxelWorld.currentWorld.worldAccessor.GetBlockShape(position);
    }

    public BlockShape GetBlockShape(BlockData blockData)
    {
        return VoxelWorld.currentWorld.worldAccessor.GetBlockShape(blockData);
    }
    public BlockShape GetBlockShape(BlockInfo blockInfo)
    {
        return blockInfo.shape;
    }


    public BlockInfo GetBlockInfo(BlockData blockData)
    {
        return GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.GetBlockInfo(blockData);
    }

    public bool IsPositionColliderLoaded(Vector3 position)
    {
        Chunk c = VoxelWorld.currentWorld.worldAccessor.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(position));
        if (c != null)
        {
            if (c.isColliderBuildingCompleted)
            {
                return true;
            }
            
        }
        return false;
    }

    public int GetLandingPoint(Vector2 xzPosition)
    {
        Vector2Int intPos = new Vector2Int((int)xzPosition.x, (int)xzPosition.y);

        Chunk locChunk = VoxelWorld.currentWorld.worldAccessor.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(new Vector3(intPos.x, 0f, intPos.y)));
        if (locChunk == null||locChunk.isColliderBuildingCompleted==false)
        {
            return Chunk.chunkHeight;
        }

        Vector2Int chunkSpacePos = intPos - locChunk.chunkPos;
        chunkSpacePos.x = Mathf.Clamp(chunkSpacePos.x, 0, Chunk.chunkWidth - 1);
        chunkSpacePos.y = Mathf.Clamp(chunkSpacePos.y, 0, Chunk.chunkWidth - 1);
        int landingPointHeight = Chunk.chunkHeight;
        for (int i = Chunk.chunkHeight - 1; i > 1; i--)
        {
            if (GetBlockShape(locChunk.map[chunkSpacePos.x, i - 1, chunkSpacePos.y]) ==BlockShape.Solid)
            {
                landingPointHeight = i;
                break;
            }
        }

        //     Debug.Log("chunk landing point height:" + landingPointHeight);
        return landingPointHeight;
    }

    public Vector3 TryFindRandomLandingPos(Vector3 centerPos,float radius, out bool succeeded)
    {
        Vector2 randomHorizontalPos = new Vector2(Random.Range(centerPos.x - radius, centerPos.x + radius), Random.Range(centerPos.z - radius, centerPos.z + radius));
        Vector3 finalLandingPos = new Vector3(randomHorizontalPos.x,GetLandingPoint(new Vector2(randomHorizontalPos.x, randomHorizontalPos.y)) + 1f, randomHorizontalPos.y);
        if (finalLandingPos.y > Chunk.chunkHeight - 1f)
        {
            succeeded = false;
        }
        else
        {
            succeeded=true;
        }
        return finalLandingPos;

    }
    public void SendBreakBlockOperation(Vector3 position)
    {
        Vector3Int positionInt = Vector3Int.FloorToInt(position);
        BlockData prevData = WorldUpdateablesMediator.instance.GetBlockData(position);
        VoxelWorld.currentWorld.worldUpdater.queuedChunkUpdatePoints.Enqueue(
            new BreakBlockOperation(positionInt, VoxelWorld.currentWorld.worldUpdater, prevData));

    }
    public void SendPlaceBlockOperation(Vector3 position,BlockData data)
    {

        Vector3Int positionInt = Vector3Int.FloorToInt(position);
        VoxelWorld.currentWorld.worldUpdater.queuedChunkUpdatePoints.Enqueue(
            new PlacingBlockOperation(positionInt, VoxelWorld.currentWorld.worldUpdater, data));
     
    }
    public void SendDoorInteractingOperation(Vector3Int position)
    {
        Vector3Int positionInt = Vector3Int.FloorToInt(position);
        VoxelWorld.currentWorld.worldUpdater.queuedChunkUpdatePoints.Enqueue(
            new DoorInteractingOperation(positionInt, VoxelWorld.currentWorld.worldUpdater));

    }
    public SimpleAxisAlignedBB GetSelectableBoundingBoxInWorld(Vector3 position)
    {
        Vector3Int blockPos = ChunkCoordsHelper.Vec3ToBlockPos(position);
        BlockData data= GetBlockData(position);
        return BlockBoundingBoxUtility.GetBoundingBoxSelectable(blockPos.x, blockPos.y, blockPos.z, data);

    }

    public Chunk GetChunk(Vector3 position)
    {
        return VoxelWorld.currentWorld.worldAccessor.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(position));
    }

    public bool CheckIsPosInChunk(Vector3 pos, Chunk c)
    {
        if (c == null)
        {
            return false;
        }

        Vector3Int intPos = Vector3Int.FloorToInt(pos);
        Vector3Int chunkSpacePos = intPos - Vector3Int.FloorToInt(new Vector3(c.chunkPos.x, 0, c.chunkPos.y));
        if (chunkSpacePos.x >= 0 && chunkSpacePos.x < Chunk.chunkWidth && chunkSpacePos.z >= 0 &&
            chunkSpacePos.z < Chunk.chunkWidth)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsChunkColliderLoaded(Chunk c)
    {
        if (c != null && c.isColliderBuildingCompleted == true)
        {
            return true;
        }
        return false;
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
                    BlockData tmpID2 =GetBlockData(blockPointArea);
                    if (GetBlockShape(tmpID2)==BlockShape.Empty)
                    {
                        continue;
                    }

                    SendBreakBlockOperation(ChunkCoordsHelper.Vec3ToBlockPos(blockPointArea));
                }
            }
        }
    }

    public void TrySpawnEntity(Vector3 position, int entityTypeID)
    {
        VoxelWorld.currentWorld.entityManager.SpawnNewEntity(position.x, position.y, position.z, entityTypeID);
    }
    public void TrySpawnEntity(Vector3 position, int entityTypeID,Vector3 initialDirection)
    {
        VoxelWorld.currentWorld.entityManager.SpawnNewEntity(position.x, position.y, position.z, entityTypeID, initialDirection);
    }

    public void TrySpawnItemEntity(Vector3 position, int itemID, Vector3 startingSpeed)
    {
        VoxelWorld.currentWorld.itemEntityManager.SpawnNewItem(position.x, position.y, position.z, itemID, startingSpeed);
    }
    public void TrySpawnItemEntityFromBlockID(Vector3 position, int blockID, Vector3 startingSpeed)
    {
        int itemID =
            GlobalGameResourcesManager.instance.itemIDToBlockIDMapper.ToItemID(blockID);
        if (GlobalGameResourcesManager.instance.itemIDToBlockIDMapper.CanMapToItemID(blockID))
        {
            VoxelWorld.currentWorld.itemEntityManager.SpawnNewItem(position.x, position.y, position.z, itemID, startingSpeed);
        }
    }

    public void TrySpawnBlockBreakingParticle(Vector3 position, int blockID)
    {
        

        if (GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.IsBlockDataValid(blockID))
        {
            ParticleEffectManagerBeh.instance.EmitBreakBlockParticleAtPosition(position, blockID);
        }
    }
   
}
