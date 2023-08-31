using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public Transform playerPos;
    void Start(){

          Chunk.AddBlockInfo();  
        
      
            Chunk.ReadJson();
        
        playerPos=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            if(!EntityBeh.isEntitiesLoad){
            EntityBeh.LoadEntities();
        }
        
            EntityBeh.ReadEntityJson();
        
            EntityBeh.SpawnEntityFromFile();
            ItemEntityBeh.ReadItemEntityJson();
            ItemEntityBeh.playerPos=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            StartCoroutine(ItemEntityBeh.SpawnItemEntityFromFile());
            
    }
    void FixedUpdate(){
        if(Random.Range(0f,100f)>99.7f&&EntityBeh.worldEntities.Count<70){
            Vector2 randomSpawnPos=new Vector2(Random.Range(playerPos.position.x-40f,playerPos.position.x+40f),Random.Range(playerPos.position.z-40f,playerPos.position.z+40f));
          EntityBeh.SpawnNewEntity(randomSpawnPos.x,Chunk.GetChunkLandingPoint(randomSpawnPos.x,randomSpawnPos.y),randomSpawnPos.y,(int)Random.Range(0f,1.999f));  
        }
    }
    void Update(){
 


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
