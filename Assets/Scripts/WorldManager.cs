using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Priority_Queue;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using System;
using Cysharp.Threading.Tasks;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
public class ChunkLoadingQueueItem{
  public Chunk c;
  public bool isStrongLoading;
  public ChunkLoadingQueueItem(Chunk c,bool isStrongLoading){
    this.c=c;
    this.isStrongLoading=isStrongLoading;
  }
}
public class WorldManager : MonoBehaviour
{



  
//  public static UnityAction<Scene,Scene> sceneChangedEvent;
    public static RuntimePlatform platform = Application.platform;
    public static string gameWorldDataPath;
    public bool isChunkFastLoadingEnabled=false;
    public bool doMonstersSpawn=true;
    public static bool isGoingToQuitGame=false;

    public static SimplePriorityQueue<Vector2Int> chunkSpawningQueue=new SimplePriorityQueue<Vector2Int>();
    public static SimplePriorityQueue<ChunkLoadingQueueItem> chunkLoadingQueue=new SimplePriorityQueue<ChunkLoadingQueueItem>();
    public static SimplePriorityQueue<Vector2Int> chunkUnloadingQueue=new SimplePriorityQueue<Vector2Int>();
    public Transform playerPos;
    public Camera playerCam;
    public GameObject lightSource;
    public Task t2;
    public Task t3;
    public Task t4;
   
    void Awake(){

       if(platform==RuntimePlatform.WindowsPlayer||platform==RuntimePlatform.WindowsEditor){
        gameWorldDataPath="C:/";
      }else{
        gameWorldDataPath=Application.persistentDataPath;
      }
         if(platform==RuntimePlatform.Android||platform==RuntimePlatform.IPhonePlayer){
            Application.targetFrameRate = 60;
        }else{
         Application.targetFrameRate = 1024;   
        }    

    }
    async void Start(){
          ChunkLoaderBase.InitChunkLoader();
          ItemIDToBlockID.InitDic();
          Chunk.AddBlockInfo();  
          ItemEntityBeh.AddFlatItemInfo();
         Chunk.ReadJson();
      //    t.Wait();
        //    Chunk.ReadJson();
            lightSource=GameObject.Find("Directional Light");
            playerPos=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            playerCam=GameObject.Find("Main Camera").GetComponent<Camera>();
            if(!EntityBeh.isEntitiesLoad){
            EntityBeh.LoadEntities();
            }
         
           await Task.Run(()=>EntityBeh.ReadEntityJson());
        
            EntityBeh.SpawnEntityFromFile();
            await Task.Run(()=>ItemEntityBeh.ReadItemEntityJson());
            ItemEntityBeh.playerPos=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            StartCoroutine(ItemEntityBeh.SpawnItemEntityFromFile());
            chunkSpawningQueue=new SimplePriorityQueue<Vector2Int>();
            chunkLoadingQueue=new SimplePriorityQueue<ChunkLoadingQueueItem>();
            chunkUnloadingQueue=new  SimplePriorityQueue<Vector2Int>();
            isGoingToQuitGame=false;
        //    UnityAction t2ThreadFunc=new UnityAction(playerPos.GetComponent<PlayerMove>().TryUpdateWorldThread);
           
  
       //     sceneChangedEvent=(Scene s,Scene s2)=>{t2.Abort();t3.Abort();t4.Abort();Debug.Log("ChangeScene");};
         // SceneManager.activeSceneChanged-=sceneChangedEvent;  
        //  //SceneManager.activeSceneChanged+=  sceneChangedEvent;
          Chunk.Chunks.Clear();
          t2=Task.Run(()=>ChunkLoaderBase.TryUpdateAllChunkLoadersThread());
       //   t2.Start();
         t3=Task.Run(()=>Chunk.TryReleaseChunkThread());
       //   t3.Start();
         t4=Task.Run(()=>Chunk.TryUpdateChunkThread());
       //   t4.Start();
       
    }

    void FixedUpdate(){
  //   if(isChunkFastLoadingEnabled==false){
      SpawnChunks();
      BuildAllChunks();
      DisableChunks();
    
  //   }
        if(Random.Range(0f,100f)>99.7f&&EntityBeh.worldEntities.Count<70&&doMonstersSpawn){
            Vector2 randomSpawnPos=new Vector2(Random.Range(playerPos.position.x-40f,playerPos.position.x+40f),Random.Range(playerPos.position.z-40f,playerPos.position.z+40f));

          EntityBeh.SpawnNewEntity(randomSpawnPos.x,WorldHelper.instance.GetChunkLandingPoint(randomSpawnPos.x,randomSpawnPos.y),randomSpawnPos.y,0);  
        }
        if (Random.Range(0f, 100f) > 99.7f && EntityBeh.worldEntities.Count < 70 && doMonstersSpawn)
        {
            Vector2 randomSpawnPos = new Vector2(Random.Range(playerPos.position.x - 40f, playerPos.position.x + 40f), Random.Range(playerPos.position.z - 40f, playerPos.position.z + 40f));

            EntityBeh.SpawnNewEntity(randomSpawnPos.x, WorldHelper.instance.GetChunkLandingPoint(randomSpawnPos.x, randomSpawnPos.y), randomSpawnPos.y,1);
        }
        if (Random.Range(0f, 100f) > 99.7f && EntityBeh.worldEntities.Count < 70 && doMonstersSpawn)
        {
            Vector2 randomSpawnPos = new Vector2(Random.Range(playerPos.position.x - 40f, playerPos.position.x + 40f), Random.Range(playerPos.position.z - 40f, playerPos.position.z + 40f));

            EntityBeh.SpawnNewEntity(randomSpawnPos.x, WorldHelper.instance.GetChunkLandingPoint(randomSpawnPos.x, randomSpawnPos.y), randomSpawnPos.y, 3);
        }
    }
    int blockedDisablingCount=0;
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
    }


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
     void SpawnChunks(){
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



    }
      public void BuildAllChunks(){
     
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
     
             
          
          
        
      

    }
public void FastChunkLoadingButtonOnValueChanged(bool b){
  isChunkFastLoadingEnabled=b;
}
 public static void DestroyAllChunks(){
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
 }
void OnApplicationQuit(){
  
  isGoingToQuitGame=true;
}

 
    void Update(){
      Chunk.playerPosVec=playerPos.position;
      lightSource.transform.Rotate(new Vector3(Time.deltaTime,0f,0f));
   //   if(isChunkFastLoadingEnabled==true){
    //  BuildAllChunksAsync();  
   //   }
       
  
               if(Input.GetKeyDown(KeyCode.H)){

                       EntityBeh.SpawnNewEntity(0,100,0,0);

                }
        //   EntityBeh.SpawnNewEntity(0,100,0,0);
        // EntityBeh.SpawnNewEntity(0,100,0,1);
        if (Input.GetKeyDown(KeyCode.I))
        {

            EntityBeh.SpawnNewEntity(0, 100, 0, 3);
      //      ItemEntityBeh.SpawnNewItem(0, 100, 0, 157, new Vector3(0, 0, 0));
        }
        
    // foreach(ItemEntityBeh i in ItemEntityBeh.worldItemEntities){
     //   i.AddForceInvoke(Vector3.up*10f);
   //  }
     //   }
    }
}
