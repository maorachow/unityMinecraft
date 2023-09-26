using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Priority_Queue;
using UnityEngine.Events;
public class WorldManager : MonoBehaviour
{
  public static RuntimePlatform platform = Application.platform;
  public static string gameWorldDataPath;
    public bool isChunkFastLoadingEnabled=false;
    public bool doMonstersSpawn=true;
    public static SimplePriorityQueue<Vector2Int> chunkSpawningQueue=new SimplePriorityQueue<Vector2Int>();
    public static SimplePriorityQueue<Chunk> chunkLoadingQueue=new SimplePriorityQueue<Chunk>();
    public static SimplePriorityQueue<Chunk> chunkUnloadingQueue=new SimplePriorityQueue<Chunk>();
    public Transform playerPos;
    public Camera playerCam;
    public GameObject lightSource;
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

          Chunk.AddBlockInfo();  
        
       Task t=Task.Run(()=>Chunk.ReadJson());
      t.Wait();
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
            chunkLoadingQueue=new SimplePriorityQueue<Chunk>();
            chunkUnloadingQueue=new  SimplePriorityQueue<Chunk>();
            UnityAction t2ThreadFunc=new UnityAction(playerPos.GetComponent<PlayerMove>().TryUpdateWorldThread);
          Thread t2=new Thread(()=>t2ThreadFunc());
          t2.Start();
          Thread t3=new Thread(()=>Chunk.TryReleaseChunkThread());
          t3.Start();
    }
    void FixedUpdate(){
  //   if(isChunkFastLoadingEnabled==false){
      SpawnChunksAsync();
      BuildAllChunksAsync();
      DisableChunksAsync();
      
  //   }
        if(Random.Range(0f,100f)>99.7f&&EntityBeh.worldEntities.Count<70&&doMonstersSpawn){
            Vector2 randomSpawnPos=new Vector2(Random.Range(playerPos.position.x-40f,playerPos.position.x+40f),Random.Range(playerPos.position.z-40f,playerPos.position.z+40f));
          EntityBeh.SpawnNewEntity(randomSpawnPos.x,Chunk.GetChunkLandingPoint(randomSpawnPos.x,randomSpawnPos.y),randomSpawnPos.y,(int)Random.Range(0f,1.999f));  
        }
    }
    void DisableChunksAsync(){
  //  Debug.Log(chunkUnloadingQueue.Count);
     if(chunkUnloadingQueue.Count>0){
      if(chunkUnloadingQueue.First==null){
        chunkUnloadingQueue.Dequeue();
      }
      ObjectPools.chunkPool.Remove(chunkUnloadingQueue.First.gameObject);
      chunkUnloadingQueue.Dequeue();
       
        
       }
    }
    async void SpawnChunksAsync(){
    //  (var c in chunkSpawningQueue){
    //  await Task.Delay(10);
      if(chunkSpawningQueue.Count>0){
        
        Vector2Int cPos=chunkSpawningQueue.First;
        if(Chunk.GetChunk(cPos)!=null){
           chunkSpawningQueue.Dequeue();
          return;
        }
        Chunk c=ObjectPools.chunkPool.Get(cPos).GetComponent<Chunk>();
        c.ReInitData();
        chunkSpawningQueue.Dequeue();
      }
        
   //   }



    }
       void BuildAllChunksAsync(){
         if(isChunkFastLoadingEnabled==true){
          for(int i=0;i<2;i++){
              if(chunkLoadingQueue.Count>0){
                if(!chunkUnloadingQueue.Contains(chunkLoadingQueue.First)){
                  chunkLoadingQueue.First.StartLoadChunk();
                  chunkLoadingQueue.Dequeue(); 
                }else{
                   chunkUnloadingQueue.Remove(chunkLoadingQueue.First); 
                   chunkLoadingQueue.First.StartLoadChunk();
                  chunkLoadingQueue.Dequeue(); 
                }
                 
              }
          
          }
          return;
          
        }else{
          if(chunkLoadingQueue.Count>0){
      //  lock(chunkLoadingQueue){
         if(!chunkUnloadingQueue.Contains(chunkLoadingQueue.First)){
                  chunkLoadingQueue.First.StartLoadChunk();
                  chunkLoadingQueue.Dequeue(); 
                }else{
                   chunkUnloadingQueue.Remove(chunkLoadingQueue.First); 
                   chunkLoadingQueue.First.StartLoadChunk();
                  chunkLoadingQueue.Dequeue(); 
                }
     //   Debug.Log("loading speed:"+1/Time.deltaTime);
        
        return;
        
   
      }
        }
      

    }
public void FastChunkLoadingButtonOnValueChanged(bool b){
  isChunkFastLoadingEnabled=b;
}
    void Update(){
      Chunk.playerPosVec=playerPos.position;
      lightSource.transform.Rotate(new Vector3(Time.deltaTime,0f,0f));
   //   if(isChunkFastLoadingEnabled==true){
    //  BuildAllChunksAsync();  
   //   }
       
      //  if(Input.GetKeyDown(KeyCode.H)){
     //   EntityBeh.SpawnNewEntity(0,100,0,0);
       // EntityBeh.SpawnNewEntity(0,100,0,1);
      // StartCoroutine(ItemEntityBeh.SpawnNewItem(0,70,0,151,Vector3.up));
   //   playerPos.gameObject.GetComponent<PlayerMove>().ApplyDamageAndKnockback(1.1f,new Vector3(1f,1f,1f));
      //  }
           //     if(Input.GetKeyDown(KeyCode.L)){

               //         StartCoroutine(ItemEntityBeh.SpawnNewItem(0,70,0,151,Vector3.up));
///
               // }
     //   EntityBeh.SpawnNewEntity(0,100,0,0);
       // EntityBeh.SpawnNewEntity(0,100,0,1);
     //  StartCoroutine(ItemEntityBeh.SpawnNewItem(0,70,0,1,Vector3.up));
    // foreach(ItemEntityBeh i in ItemEntityBeh.worldItemEntities){
     //   i.AddForceInvoke(Vector3.up*10f);
   //  }
     //   }
    }
}
