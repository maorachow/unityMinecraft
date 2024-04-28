using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
public class ChunkLoaderBase:MonoBehaviour
{
  //  public static int chunkStrongLoadingRange=32;
    public Vector2 chunkLoadingCenter;
    public float chunkLoadingRange;
    public bool isChunksNeedLoading=false;
    public Plane[] cameraFrustum;
 /*   public static List<ChunkLoaderBase> allChunkLoaders=new List<ChunkLoaderBase>();
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
    }*/
    public void AddChunkLoaderToList(){
        VoxelWorld.currentWorld.allChunkLoaders.Add(this);
    }
    void OnDestroy(){
        VoxelWorld.currentWorld.allChunkLoaders.Remove(this);
    }
  public void TryUpdateWorldThread(){
            if(VoxelWorld.currentWorld.isGoingToQuitWorld==true){
                return;
            }
            for (float x = chunkLoadingCenter.x - chunkLoadingRange; x < chunkLoadingCenter.x + chunkLoadingRange; x += Chunk.chunkWidth)
            {
                for (float z = chunkLoadingCenter.y - chunkLoadingRange; z <chunkLoadingCenter.y + chunkLoadingRange; z += Chunk.chunkWidth)
                    {
                Vector3 pos = new Vector3(x, 0, z);

                Vector2Int chunkPos=  WorldHelper.instance.Vec3ToChunkPos(pos);
                Vector2 chunkCenterPos = chunkPos + new Vector2(Chunk.chunkWidth / 2, Chunk.chunkWidth / 2);

                Chunk chunk = Chunk.GetChunk(chunkPos);
             //   Debug.Log(chunk);
                if (chunk != null|| VoxelWorld.currentWorld.chunkSpawningQueue.Contains(chunkPos)) {
                                continue;
                                }else{
                     
                    bool isLoadingChunk = false;
                    if ((chunkCenterPos - chunkLoadingCenter).magnitude < Chunk.chunkWidth) {

                        isLoadingChunk = true;
                    }
                   int maxHeight= WorldHelper.PreCalculateChunkMaxHeight(chunkPos);
                    //Debug.Log(maxHeight);
                    Bounds bounds = new Bounds(new Vector3(chunkPos.x, 0, chunkPos.y) + new Vector3((float)Chunk.chunkWidth / 2, (float)maxHeight / 2, (float)Chunk.chunkWidth / 2), new Vector3(Chunk.chunkWidth, maxHeight, Chunk.chunkWidth));
           //         Debug.Log(bounds.ToString());
                    if (BoundingBoxCullingHelper.IsBoundingBoxInOrIntersectsFrustum(bounds, cameraFrustum))
                    {
                        isLoadingChunk = true;
                   
                    }
                    if (isLoadingChunk == true)
                    {
                        VoxelWorld.currentWorld.chunkSpawningQueue.Enqueue(chunkPos,(int)Mathf.Abs(chunkPos.x-chunkLoadingCenter.x)+(int)Mathf.Abs(chunkPos.y-chunkLoadingCenter.y)); 
                    }
                   
                }
            }
        }
        
        isChunksNeedLoading=false;
    }
}
