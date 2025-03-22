using System;
using UnityEngine;
using System.Threading.Tasks;
using Priority_Queue;
 
using UnityEngine.SceneManagement;
using Random=UnityEngine.Random;
public class ChunkLoadingQueueItem{
  public Chunk c;
  
  public ChunkLoadingQueueItem(Chunk c){
    this.c=c;
    
  }
}
public class WorldManager : MonoBehaviour
{




    //  public static UnityAction<Scene,Scene> sceneChangedEvent;
    [Obsolete]
    public static RuntimePlatform platform = Application.platform;
    [Obsolete]
    public static string gameWorldDataPath;
    public bool isChunkFastLoadingEnabled=false;
    public bool doMonstersSpawn=true;
//    public static bool isGoingToQuitGame=false;

  
    public Transform playerPos;
    public Camera playerCam;
    public GameObject lightSource;
  
   
    void Awake(){

    /*   if(platform==RuntimePlatform.WindowsPlayer||platform==RuntimePlatform.WindowsEditor){
        gameWorldDataPath="C:/";
      }else{
        gameWorldDataPath=Application.persistentDataPath;
      }*/
         if(platform==RuntimePlatform.Android||platform==RuntimePlatform.IPhonePlayer){
            Application.targetFrameRate = 60;
        }else{
         Application.targetFrameRate = 1024;   
        }
      
        if (EntityBeh.isEntitiesLoad == false)
        {
            EntityBeh.LoadEntities();
        }

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 1:
                VoxelWorld.currentWorld = VoxelWorld.worlds[0];
                break;
            case 2:
                VoxelWorld.currentWorld = VoxelWorld.worlds[1];
                break;
        }
    } 
    void Start(){
      //    ChunkLoaderBase.InitChunkLoader();
         
          Chunk.AddBlockInfo();  
          ItemEntityBeh.AddFlatItemInfo();
      //   Chunk.ReadJson();
      //    t.Wait();
        //    Chunk.ReadJson();
            lightSource=GameObject.Find("Directional Light");
            playerPos=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            playerCam=GameObject.Find("Main Camera").GetComponent<Camera>();


        //     await Task.Run(()=>EntityBeh.ReadEntityJson());

        //    EntityBeh.SpawnEntityFromFile();
        //   await Task.Run(()=>ItemEntityBeh.ReadItemEntityJson());

        // EntityBeh.playerTransform= GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        // StartCoroutine(ItemEntityBeh.SpawnItemEntityFromFile());
        //        chunkSpawningQueue=new SimplePriorityQueue<Vector2Int>();
        //       chunkLoadingQueue=new SimplePriorityQueue<ChunkLoadingQueueItem>();
        //       chunkUnloadingQueue=new  SimplePriorityQueue<Vector2Int>();
        //       isGoingToQuitGame=false;
        //    UnityAction t2ThreadFunc=new UnityAction(playerPos.GetComponent<PlayerMove>().TryUpdateWorldThread);


        //     sceneChangedEvent=(Scene s,Scene s2)=>{t2.Abort();t3.Abort();t4.Abort();Debug.Log("ChangeScene");};
        // SceneManager.activeSceneChanged-=sceneChangedEvent;  
        //  //SceneManager.activeSceneChanged+=  sceneChangedEvent;
        //     Chunk.Chunks.Clear();

       

        VoxelWorld.currentWorld.InitObjectPools();
        VoxelWorld.currentWorld.InitWorld(); 
 //       TerrainTextureMipmapAdjusting.LoadAndSetDefaultTexMipmap();
         
      

    }

     
    void FixedUpdate(){
        //   if(isChunkFastLoadingEnabled==false){
        VoxelWorld.currentWorld.OnFixedUpdate(Time.fixedDeltaTime);
      

        //   }
        if (VoxelWorld.currentWorld.worldID == 0)
        {
            if(Random.Range(0f,100f)>99.8f&& VoxelWorld.currentWorld.entityManager.worldEntities.Count<70&&doMonstersSpawn)
            {
                Vector3 randomSpawnPos =
                    WorldUpdateablesMediator.instance.TryFindRandomLandingPos(playerPos.position, 40f,
                        out bool succeeded);
                if (succeeded == true)
                {
                    VoxelWorld.currentWorld.entityManager.SpawnNewEntity(randomSpawnPos.x, randomSpawnPos.y, randomSpawnPos.z, 0);
                }

                
            }
            if (Random.Range(0f, 100f) > 99.8f && VoxelWorld.currentWorld.entityManager.worldEntities.Count < 70 && doMonstersSpawn)
            {
                Vector3 randomSpawnPos =
                    WorldUpdateablesMediator.instance.TryFindRandomLandingPos(playerPos.position, 40f,
                        out bool succeeded);
                if (succeeded == true)
                {
                    VoxelWorld.currentWorld.entityManager.SpawnNewEntity(randomSpawnPos.x, randomSpawnPos.y, randomSpawnPos.z, 1);
                }
            }
            if (Random.Range(0f, 100f) > 99.8f && VoxelWorld.currentWorld.entityManager.worldEntities.Count < 70 && doMonstersSpawn)
            {
                Vector3 randomSpawnPos =
                    WorldUpdateablesMediator.instance.TryFindRandomLandingPos(playerPos.position, 40f,
                        out bool succeeded);
                if (succeeded == true)
                {
                    VoxelWorld.currentWorld.entityManager.SpawnNewEntity(randomSpawnPos.x, randomSpawnPos.y, randomSpawnPos.z, 3);
                }
            }
            if (Random.Range(0f, 100f) > 99.85f && VoxelWorld.currentWorld.entityManager.worldEntities.Count < 70 && doMonstersSpawn)
            {
                Vector3 randomSpawnPos =
                    WorldUpdateablesMediator.instance.TryFindRandomLandingPos(playerPos.position, 40f,
                        out bool succeeded);
                if (succeeded == true)
                {
                    VoxelWorld.currentWorld.entityManager.SpawnNewEntity(randomSpawnPos.x, randomSpawnPos.y, randomSpawnPos.z, 5);
                }
            }
        }
        else if (VoxelWorld.currentWorld.worldID == 1)
        {
            if (Random.Range(0f, 100f) > 99.7f && VoxelWorld.currentWorld.entityManager.worldEntities.Count < 70 && doMonstersSpawn)
            {
                Vector3 randomSpawnPos =
                    WorldUpdateablesMediator.instance.TryFindRandomLandingPos(playerPos.position, 40f,
                        out bool succeeded);
                if (succeeded == true)
                {
                    VoxelWorld.currentWorld.entityManager.SpawnNewEntity(randomSpawnPos.x, randomSpawnPos.y, randomSpawnPos.z, 5);
                }
            }
        }
        
    }
   /* int blockedDisablingCount=0;
    void DisableChunks(){
  //  Debug.Log(chunkUnloadingQueue.Count);
     if(chunkUnloadingQueue.Count>0){
     
        Chunk c=Chunk.GetChunk(chunkUnloadingQueue.First);
        
        if(c!=null){
                lock (c.taskLock)
                {
            if(c.isTaskCompleted==true){
            ObjectPools.chunkPool.Remove(c.gameObject);
            chunkUnloadingQueue.Dequeue();
          //  Debug.Log("completed");
            return;
          }else{
            blockedDisablingCount++;
            if(blockedDisablingCount>15){
              blockedDisablingCount=0;
                Destroy(c.gameObject);
            chunkUnloadingQueue.Dequeue();
           Debug.Log("destroy");
            return;
            }
             return;
          }
                }
         
        } 
        else{
         chunkUnloadingQueue.Dequeue();
       //  Debug.Log("none");
         return;
       }
       }
    */


 /* public async void StrongLoadChunkInQueue(){
   // NativeList<int> meshesID=new NativeList<int>(Allocator.TempJob);
   // List<Chunk> meshChunks=new List<Chunk>();
    while(chunkStrongLoadingQueue.Count>0){
     // Debug.Log(chunkStrongLoadingQueue.Count);
      //  Debug.Log(chunkStrongLoadingQueue.Count);
      Chunk c=Chunk.GetChunk(chunkStrongLoadingQueue[0]);
      if(c.isStrongLoaded==true){
          chunkStrongLoadingQueue.RemoveAt(0);
        continue;
      }
      
      if(c==null){
        chunkStrongLoadingQueue.RemoveAt(0);
          continue;
     
      }

      
      if(c.isChunkMapUpdated==true){
       
        
     
        chunkStrongLoadingQueue.RemoveAt(0);
         continue;
        }else{
     
        c.meshCollider.sharedMesh=c.chunkMesh;
       
          c.isStrongLoaded=true;
        chunkStrongLoadingQueue.RemoveAt(0);
         continue;
      }
     if(c.meshCollider.sharedMesh.GetInstanceID()==c.chunkMesh.GetInstanceID()){
        Debug.Log("same");
          chunkStrongLoadingQueue.RemoveAt(0);
        continue;
      }
    }
 
   
  }*/
   /*  void SpawnChunks(){
    //  (var c in chunkSpawningQueue){
 //     await Task.Delay(20);
 if(isChunkFastLoadingEnabled==true){
 for(int i=0;i<6;i++){
  if(chunkSpawningQueue.Count>0){
        
        Vector2Int cPos=chunkSpawningQueue.First;
        if(Chunk.GetChunk(cPos)!=null){
           chunkSpawningQueue.Dequeue();
         continue;
        }
        if(chunkUnloadingQueue.Contains(cPos)){
           chunkSpawningQueue.Dequeue();
         continue;
        }
       
        ObjectPools.chunkPool.Get(cPos);    
         
        
        chunkSpawningQueue.Dequeue();
         continue;
      }

  }
 }else{
   for(int i=0;i<3;i++){
  if(chunkSpawningQueue.Count>0){
        
        Vector2Int cPos=chunkSpawningQueue.First;
        if(Chunk.GetChunk(cPos)!=null){
           chunkSpawningQueue.Dequeue();
         continue;
        }
        if(chunkUnloadingQueue.Contains(cPos)){
           chunkSpawningQueue.Dequeue();
         continue;
        }
        //ObjectPools.chunkPool.Get(cPos);
        
        ObjectPools.chunkPool.Get(cPos);    
       
        chunkSpawningQueue.Dequeue();
         continue;
      }

  }
 }
 
   
        
   //   }



    }  */
   /*   public void BuildAllChunks(){
     
      if(isChunkFastLoadingEnabled==true){
        for(int i=0;i<10;i++){
         if(chunkLoadingQueue.Count>0){
                if(chunkLoadingQueue.First.c==null){
                    chunkLoadingQueue.Dequeue(); 
                    continue;
                }
           
            
                
                     
                       chunkLoadingQueue.First.c.StartLoadChunk();  
                      
                  
                  
                  chunkLoadingQueue.Dequeue(); 
                
                 
              }
       }
      }else{
          for(int i=0;i<2;i++){
         if(chunkLoadingQueue.Count>0){
                if(chunkLoadingQueue.First.c==null){
                    chunkLoadingQueue.Dequeue(); 
                    continue;
                }
           
            
                
                     
                  chunkLoadingQueue.First.c.StartLoadChunk();  
                      
                  
                  
                  chunkLoadingQueue.Dequeue(); 
                
                 
              }
       }
      }
     
             
          
          
        
      

    }*/

public void FastChunkLoadingButtonOnValueChanged(bool b){
  isChunkFastLoadingEnabled=b;
}
 /*public static void DestroyAllChunks(){
  foreach(var cKvp in Chunk.Chunks){
    var c=cKvp.Value;
    c.leftChunk=null;
    c.rightChunk=null;
    c.frontChunk=null;
    c.backChunk=null;
    c.backLeftChunk=null;
    c.backRightChunk=null;
    c.frontLeftChunk=null;
    c.frontRightChunk=null;
    c=null;
  }
  Chunk.Chunks.Clear();
 }*/
void OnApplicationQuit(){
  
 // isGoingToQuitGame=true;
        VoxelWorld.currentWorld.isGoingToQuitWorld = true;
        GlobalGameResourcesManager.instance.terrainTextureMaterialManager.ResetTerrainTextures();
}

 
    void Update(){

        if (PlayerMove.instance.teleportingThroughWorldParams.Item1 == true)
        {

          
            GameDataPersistenceManager.instance.isPlayerTeleportedThroughWorld=true;
            GameDataPersistenceManager.instance.isPlayerTeleportedThroughWorldSelfByPlayer =
                PlayerMove.instance.teleportingThroughWorldParams.Item3;
            if (PlayerMove.instance.teleportingThroughWorldParams.Item3==true)
            {
                PlayerMove.instance.SavePlayerDataToPersistenceManager();
            }
            switch (PlayerMove.instance.teleportingThroughWorldParams.Item2)
            {
                case 0:
                    
                    SceneManagementHelper.SwitchToWorldWithSceneChanged(0, 1);

                    break;
                case 1:
                    SceneManagementHelper.SwitchToWorldWithSceneChanged(1, 2);
                    break;
            }
        }
      Chunk.playerPosVec=playerPos.position;
      lightSource.transform.Rotate(new Vector3(Time.deltaTime,0f,0f));
        Vector3 lightSourceDir= lightSource.transform.forward.normalized;
        VoxelWorld.currentWorld.FrameUpdate(Time.deltaTime);
   //   if(isChunkFastLoadingEnabled==true){
   //  BuildAllChunksAsync();  
   //   }


        /*            if(Input.GetKeyDown(KeyCode.H)){

                            EntityBeh.SpawnNewEntity(0,100,0,0);

                     }
             //   EntityBeh.SpawnNewEntity(0,100,0,0);
             // EntityBeh.SpawnNewEntity(0,100,0,1);
             if (Input.GetKeyDown(KeyCode.I))
             {

                 EntityBeh.SpawnNewEntity(0, 100, 0, 3);
           //      ItemEntityBeh.SpawnNewItem(0, 100, 0, 157, new Vector3(0, 0, 0));
             }*/
        /*       if(Input.GetKeyDown(KeyCode.J))
               {

                   SceneManagementHelper. SwitchToWorldWithSceneChanged(1, 2);
                   VoxelWorld.worlds[1].actionOnSwitchedWorld = delegate () {

                       PlayerMove.instance.cc.enabled = false;
                       PlayerMove.instance.transform.position = new Vector3(0, 150, 0);
                       PlayerMove.instance.cc.enabled = true;
                       Debug.Log("action executed");
                   };
               }
               if (Input.GetKeyDown(KeyCode.K))
               {

                   SceneManagementHelper.SwitchToWorldWithSceneChanged(0, 1);
                   VoxelWorld.worlds[0].actionOnSwitchedWorld = delegate () {

                       PlayerMove.instance.cc.enabled = false;
                       PlayerMove.instance.transform.position = new Vector3(0, 150, 0);
                       PlayerMove.instance.cc.enabled = true;
                       Debug.Log("action executed world 0");
                   };
               }*/
        // foreach(ItemEntityBeh i in ItemEntityBeh.worldItemEntities){
        //   i.AddForceInvoke(Vector3.up*10f);
        //  }
        //   }
    }
}
