using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
public class ChunkLoaderBase:MonoBehaviour
{
    public Vector2 chunkLoadingCenter;
    public float chunkLoadingRange;
    public bool isChunksNeedLoading=false;
    public static List<ChunkLoaderBase> allChunkLoaders=new List<ChunkLoaderBase>();
    public static void InitChunkLoader(){
        allChunkLoaders.Clear();
    }
    public static void TryUpdateAllChunkLoadersThread(){
        while(true){
            Thread.Sleep(50);
            if(WorldManager.isGoingToQuitGame==true){
                return;
            }
            foreach(var cl in allChunkLoaders){
                if(cl.isChunksNeedLoading==true){
               cl.TryUpdateWorldThread();     
                }
                
            }
        }
    }
    public void AddChunkLoaderToList(){
        allChunkLoaders.Add(this);
    }
    void OnDestroy(){
        allChunkLoaders.Remove(this);
    }
  public void TryUpdateWorldThread(){
            if(WorldManager.isGoingToQuitGame==true){
                return;
            }
            for (float x = chunkLoadingCenter.x - chunkLoadingRange; x < chunkLoadingCenter.x + chunkLoadingRange; x += Chunk.chunkWidth)
            {
                for (float z = chunkLoadingCenter.y - chunkLoadingRange; z <chunkLoadingCenter.y + chunkLoadingRange; z += Chunk.chunkWidth)
                    {
                Vector3 pos = new Vector3(x, 0, z);

                Vector2Int chunkPos=  WorldHelper.instance.Vec3ToChunkPos(pos);
               
                Chunk chunk = Chunk.GetChunk(chunkPos);
                if (chunk != null||Chunk.GetUnloadedChunk(chunkPos)!=null||WorldManager.chunkSpawningQueue.Contains(chunkPos)) {
                                        continue;
                            }else{
               WorldManager.chunkSpawningQueue.Enqueue(chunkPos,(int)Mathf.Abs(chunkPos.x-chunkLoadingCenter.x)+(int)Mathf.Abs(chunkPos.y-chunkLoadingCenter.y)); 
                }
            }
        }
        isChunksNeedLoading=false;
    }
}
