using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk3x3Access
{
    public Chunk frontChunk;
    public Chunk backChunk;
    public Chunk leftChunk;
    public Chunk rightChunk;
    public Chunk frontLeftChunk;
    public Chunk frontRightChunk;
    public Chunk backLeftChunk;
    public Chunk backRightChunk;
    public Chunk centerChunk;
    public void Visualize(){
        Debug.DrawLine(new Vector3(frontChunk.chunkPos.x,0f,frontChunk.chunkPos.y)+new Vector3(8f,0f,8f),new Vector3(frontChunk.chunkPos.x,100f,frontChunk.chunkPos.y)+new Vector3(8f,0f,8f),Color.green);
        Debug.DrawLine(new Vector3(backChunk.chunkPos.x,0f,backChunk.chunkPos.y)+new Vector3(8f,0f,8f),new Vector3(backChunk.chunkPos.x,100f,backChunk.chunkPos.y)+new Vector3(8f,0f,8f),Color.green);
        Debug.DrawLine(new Vector3(leftChunk.chunkPos.x,0f,leftChunk.chunkPos.y)+new Vector3(8f,0f,8f),new Vector3(leftChunk.chunkPos.x,100f,leftChunk.chunkPos.y)+new Vector3(8f,0f,8f),Color.green);
        Debug.DrawLine(new Vector3(rightChunk.chunkPos.x,0f,rightChunk.chunkPos.y)+new Vector3(8f,0f,8f),new Vector3(rightChunk.chunkPos.x,100f,rightChunk.chunkPos.y)+new Vector3(8f,0f,8f),Color.green);
        Debug.DrawLine(new Vector3(frontLeftChunk.chunkPos.x,0f,frontLeftChunk.chunkPos.y)+new Vector3(8f,0f,8f),new Vector3(frontLeftChunk.chunkPos.x,100f,frontLeftChunk.chunkPos.y)+new Vector3(8f,0f,8f),Color.green);
        Debug.DrawLine(new Vector3(frontRightChunk.chunkPos.x,0f,frontRightChunk.chunkPos.y)+new Vector3(8f,0f,8f),new Vector3(frontRightChunk.chunkPos.x,100f,frontRightChunk.chunkPos.y)+new Vector3(8f,0f,8f),Color.green);
        Debug.DrawLine(new Vector3(backLeftChunk.chunkPos.x,0f,backLeftChunk.chunkPos.y)+new Vector3(8f,0f,8f),new Vector3(backLeftChunk.chunkPos.x,100f,backLeftChunk.chunkPos.y)+new Vector3(8f,0f,8f),Color.green);
        Debug.DrawLine(new Vector3(backRightChunk.chunkPos.x,0f,backRightChunk.chunkPos.y)+new Vector3(8f,0f,8f),new Vector3(backRightChunk.chunkPos.x,100f,backRightChunk.chunkPos.y)+new Vector3(8f,0f,8f),Color.green);
        Debug.DrawLine(new Vector3(centerChunk.chunkPos.x,0f,centerChunk.chunkPos.y)+new Vector3(8f,0f,8f),new Vector3(centerChunk.chunkPos.x,100f,centerChunk.chunkPos.y)+new Vector3(8f,0f,8f),Color.green);

    }
}
