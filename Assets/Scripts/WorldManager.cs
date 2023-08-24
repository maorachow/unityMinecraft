using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    void Awake(){
            if(!EntityBeh.isEntitiesLoad){
            EntityBeh.LoadEntities();
        }
                if(EntityBeh.isEntitiesReadFromDisk==false){
            EntityBeh.ReadEntityJson();
        }
            EntityBeh.SpawnEntityFromFile();
    }
    void Update(){
 


        if(Input.GetKeyDown(KeyCode.H)){
     //   EntityBeh.SpawnNewEntity(0,100,0,0);
        EntityBeh.SpawnNewEntity(0,100,0,1);
        }
    }
}
