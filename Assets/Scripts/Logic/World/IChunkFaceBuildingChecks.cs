using UnityEngine;

public interface IChunkFaceBuildingChecks
{
    public int GetChunkBlockType(int x, int y, int z);
    public bool CheckNeedBuildFace(int x, int y, int z, BlockData curBlock);
    public bool CheckNeedBuildFace(int x, int y, int z, BlockData curBlock, int LODSkipBlockCount);
    public Vector2Int chunkPos { get; }
    //  public 

    public bool CheckNeedBuildFace(int x, int y, int z, bool isThisNS);
}