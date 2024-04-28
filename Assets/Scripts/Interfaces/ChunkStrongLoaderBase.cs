using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkStrongLoaderBase : MonoBehaviour
{
  /*      public static int chunkStrongLoadingRange=32;
    public bool isChunksNeededStrongLoading=false;
    public Vector2 chunkLoadingCenter;


  void FixedUpdate(){
        chunkLoadingCenter=new Vector2(transform.position.x,transform.position.z);
        if(isChunksNeededStrongLoading==true){
            StrongLoadChunksAround();
        }
    }
    void StrongLoadChunksAround(){
       for (float x = chunkLoadingCenter.x - chunkStrongLoadingRange; x < chunkLoadingCenter.x + chunkStrongLoadingRange; x += Chunk.chunkWidth)
            {
                for (float z = chunkLoadingCenter.y - chunkStrongLoadingRange; z <chunkLoadingCenter.y + chunkStrongLoadingRange; z += Chunk.chunkWidth)
                    {
                Vector3 pos = new Vector3(x, 0, z);

                Vector2Int chunkPos=  WorldHelper.instance.Vec3ToChunkPos(pos);
               
                Chunk chunk = Chunk.GetChunk(chunkPos);
                if (chunk != null&&chunk.isStrongLoaded==false) {
                    chunk.StrongLoadChunk();
             //        Debug.Log("strongload");
                }
            }
        }
        isChunksNeededStrongLoading=false;
  }*/
}
