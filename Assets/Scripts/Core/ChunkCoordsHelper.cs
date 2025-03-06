using System;
using UnityEngine;

public static class ChunkCoordsHelper
{
    public static Vector3Int Vec3ToBlockPos(Vector3 pos)
    {
        Vector3Int intPos = Vector3Int.FloorToInt(pos);
        return intPos;
    }
    public static int FloatToInt(float f)
    {
        if (f >= 0)
        {
            return (int)f;
        }
        else
        {
            return (int)f - 1;
        }
    }
    public static int FloorFloat(float n)
    {
        int i = (int)n;
        return n >= i ? i : i - 1;
    }

    public static int CeilFloat(float n)
    {
        int i = (int)(n + 1);
        return n >= i ? i : i - 1;
    }
    public static Vector2Int Vec3ToChunkPos(Vector3 pos)
    {
        Vector3 tmp = pos;
        tmp.x = MathF.Floor(tmp.x / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
        tmp.z = MathF.Floor(tmp.z / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
        Vector2Int value = new Vector2Int((int)tmp.x, (int)tmp.z);
        //  mainForm.LogOnTextbox(value.x+" "+value.y+"\n");
        return value;
    }
}
