using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Priority_Queue;
public class WorldManager : MonoBehaviour
{
    public bool isChunkFastLoadingEnabled=false;
    public bool doMonstersSpawn=true;
    public static SimplePriorityQueue<Chunk> chunkLoadingQueue=new SimplePriorityQueue<Chunk>();
    public Transform playerPos;
    public Camera playerCam;
    async void Start(){

          Chunk.AddBlockInfo();  
        
       Task t= Task.Run(()=>Chunk.ReadJson());
       t.Wait();
         //   Chunk.ReadJson();
        
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
            
    }
    void FixedUpdate(){
  //   if(isChunkFastLoadingEnabled==false){
      BuildAllChunksAsync();
  //   }
        if(Random.Range(0f,100f)>99.7f&&EntityBeh.worldEntities.Count<70&&doMonstersSpawn){
            Vector2 randomSpawnPos=new Vector2(Random.Range(playerPos.position.x-40f,playerPos.position.x+40f),Random.Range(playerPos.position.z-40f,playerPos.position.z+40f));
          EntityBeh.SpawnNewEntity(randomSpawnPos.x,Chunk.GetChunkLandingPoint(randomSpawnPos.x,randomSpawnPos.y),randomSpawnPos.y,(int)Random.Range(0f,1.999f));  
        }
    }
       void BuildAllChunksAsync(){
         if(isChunkFastLoadingEnabled==true){
          for(int i=0;i<2;i++){
              if(chunkLoadingQueue.Count>0){
                 chunkLoadingQueue.First.StartLoadChunk();
              chunkLoadingQueue.Dequeue(); 
              }
          
          }
          return;
          
        }else{
          if(chunkLoadingQueue.Count>0){
      //  lock(chunkLoadingQueue){
       
             chunkLoadingQueue.First.StartLoadChunk();
     
              chunkLoadingQueue.Dequeue();
     //   Debug.Log("loading speed:"+1/Time.deltaTime);
        
        return;
        
   
      }
        }
      

    }
public void FastChunkLoadingButtonOnValueChanged(bool b){
  isChunkFastLoadingEnabled=b;
}
    void Update(){
 
   //   if(isChunkFastLoadingEnabled==true){
    //  BuildAllChunksAsync();  
   //   }
       
        if(Input.GetKeyDown(KeyCode.H)){
     //   EntityBeh.SpawnNewEntity(0,100,0,0);
       // EntityBeh.SpawnNewEntity(0,100,0,1);
      // StartCoroutine(ItemEntityBeh.SpawnNewItem(0,70,0,151,Vector3.up));
   //   playerPos.gameObject.GetComponent<PlayerMove>().ApplyDamageAndKnockback(1.1f,new Vector3(1f,1f,1f));
        }
                if(Input.GetKeyDown(KeyCode.L)){

                        StartCoroutine(ItemEntityBeh.SpawnNewItem(0,70,0,152,Vector3.up));

                }
     //   EntityBeh.SpawnNewEntity(0,100,0,0);
       // EntityBeh.SpawnNewEntity(0,100,0,1);
     //  StartCoroutine(ItemEntityBeh.SpawnNewItem(0,70,0,1,Vector3.up));
    // foreach(ItemEntityBeh i in ItemEntityBeh.worldItemEntities){
     //   i.AddForceInvoke(Vector3.up*10f);
   //  }
     //   }
    }
}
