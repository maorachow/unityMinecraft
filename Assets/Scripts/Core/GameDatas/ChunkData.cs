using MessagePack;

[MessagePackObject]
public class ChunkData
{
    [Key(0)]
    public BlockData[,,] map;
   

    [Key(1)]
    public int chunkPosX;
    [Key(2)]
    public int chunkPosZ;
    public ChunkData(){}
    public ChunkData(int chunkPosX,int chunkPosZ,BlockData[,,] map)
    {
        this.map = map;
        this.chunkPosX = chunkPosX;
        this.chunkPosZ = chunkPosZ;
    }
    public ChunkData(int chunkPosX, int chunkPosZ)
    {
        //  this.map = map;
        this.chunkPosX = chunkPosX;
        this.chunkPosZ = chunkPosZ;
    }
}