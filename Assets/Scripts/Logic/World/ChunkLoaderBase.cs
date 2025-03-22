using UnityEngine;
using UnityEngine.UI;

public class ChunkLoaderBase:MonoBehaviour
{
  //  public static int chunkStrongLoadingRange=32;
    public Vector2 chunkLoadingCenter;
    public float chunkLoadingRange;
    public bool isChunksNeedLoading=false;
 //   public Plane[] cameraFrustum;
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

    private Chunk curChunk;

    public void Start()
    {
      
            AddChunkLoaderToList();
    }
    public void Update()
    {
        chunkLoadingCenter = new Vector2(transform.position.x,transform.position.z);
       // cameraFrustum = GeometryUtility.CalculateFrustumPlanes(mainCam);
        chunkLoadingRange = GlobalGameOptions.inGameRenderDistance;
        if (curChunk == null)
        {
            curChunk = WorldUpdateablesMediator.instance.GetChunk(transform.position);
            isChunksNeedLoading = true;
        }

        if (WorldUpdateablesMediator.instance.CheckIsPosInChunk(transform.position, curChunk) == false)
        {
            curChunk = WorldUpdateablesMediator.instance.GetChunk(transform.position);
            isChunksNeedLoading = true;
            //   curChunkStrongLoader.isChunksNeededStrongLoading=true;
        }
    }
  public void TryUpdateWorldThread(VoxelWorld world){
            if(world.isGoingToQuitWorld==true){
                return;
            }
            for (float x = chunkLoadingCenter.x - chunkLoadingRange; x < chunkLoadingCenter.x + chunkLoadingRange; x += Chunk.chunkWidth)
            {
                for (float z = chunkLoadingCenter.y - chunkLoadingRange;
                     z < chunkLoadingCenter.y + chunkLoadingRange;
                     z += Chunk.chunkWidth)
                {
                    Vector3 pos = new Vector3(x, 0, z);

                    Vector2Int chunkPos =ChunkCoordsHelper.Vec3ToChunkPos(pos);
                    Vector2 chunkCenterPos = chunkPos + new Vector2(Chunk.chunkWidth / 2, Chunk.chunkWidth / 2);

                    Chunk chunk = world.GetChunk(chunkPos);
                    //   Debug.Log(chunk);
                    if (chunk != null || world.CheckIsChunkPosSpawning(chunkPos))
                    {
                        continue;
                    }
                    else
                    {
                        bool isLoadingChunk = false;
                        isLoadingChunk = true;
                  /*  if ((chunkCenterPos - chunkLoadingCenter).magnitude < Chunk.chunkWidth)
                        {
                            isLoadingChunk = true;
                        }

                        int maxHeight = WorldHelper.PreCalculateChunkMaxHeight(chunkPos);
                        //Debug.Log(maxHeight);
                        Bounds bounds =
                            new Bounds(
                                new Vector3(chunkPos.x, 0, chunkPos.y) + new Vector3((float)Chunk.chunkWidth / 2,
                                    (float)maxHeight / 2, (float)Chunk.chunkWidth / 2),
                                new Vector3(Chunk.chunkWidth, maxHeight, Chunk.chunkWidth));
                        //         Debug.Log(bounds.ToString());
                        if (BoundingBoxCullingHelper.IsBoundingBoxInOrIntersectsFrustum(bounds, cameraFrustum))
                        {
                            isLoadingChunk = true;
                        }*/

                        if (isLoadingChunk == true)
                        {
                            world.AddSpawningChunkPosition(chunkPos,
                                (int)Mathf.Abs(chunkPos.x - chunkLoadingCenter.x) +
                                (int)Mathf.Abs(chunkPos.y - chunkLoadingCenter.y));
                        }
                    }
                }
            }

            isChunksNeedLoading = false;
  }
}
