using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.IO;
using Utf8Json;
public class EntityData{
  
    public float posX;
    public float posY;
    public float posZ;
    public float rotationX;
    public float rotationY;
    public float rotationZ;
    public int entityTypeID;
    public string guid;
    public bool Equals(EntityData other)
    {   
        if(other==null){
            return false;
        }
        if (this.guid.Equals(other.guid)) {
            return true;
        }
        else {
            return false;
        }
    }
}
public class EntityBeh : MonoBehaviour
{
    public Chunk currentChunk;
    public static Dictionary<int,GameObject> worldEntityTypes=new Dictionary<int,GameObject>(); 
    //0Creeper 1zombie
    public static RuntimePlatform platform = Application.platform;
    public static bool isEntitiesLoad=false;
    public static string gameWorldEntityDataPath;
    public static List<EntityData> entityDataReadFromDisk=new List<EntityData>();
    public static List<EntityBeh> worldEntities=new List<EntityBeh>();
    public static bool isEntitiesReadFromDisk=false;
    public static bool isEntitySavedInDisk=false;
    public int entityTypeID;
    public string guid;
    public static bool isWorldEntityDataSaved=false;
    public bool isInUnloadedChunks=false;
    public void OnDisable(){
     
            RemoveEntityFromSave();
            worldEntities.Remove(this);  
        
        
    }
    public void OnDestroy(){
        RemoveEntityFromSave();
        worldEntities.Remove(this);
    }
    public static void ReadEntityJson(){
     if(platform==RuntimePlatform.WindowsPlayer||platform==RuntimePlatform.WindowsEditor){
        gameWorldEntityDataPath="C:/";
      }else{
        gameWorldEntityDataPath=Application.persistentDataPath;
      }
         
         if (!Directory.Exists(gameWorldEntityDataPath+"unityMinecraftData")){
                Directory.CreateDirectory(gameWorldEntityDataPath+"unityMinecraftData");
               
            }
          if(!Directory.Exists(gameWorldEntityDataPath+"unityMinecraftData/GameData")){
                    Directory.CreateDirectory(gameWorldEntityDataPath+"unityMinecraftData/GameData");
                }
       
        if(!File.Exists(gameWorldEntityDataPath+"unityMinecraftData"+"/GameData/worldentities.json")){
            File.Create(gameWorldEntityDataPath+"unityMinecraftData"+"/GameData/worldentities.json");
        }
       
        string[] worldEntitiesData=File.ReadAllLines(gameWorldEntityDataPath+"unityMinecraftData/GameData/worldentities.json");
        foreach(string s in worldEntitiesData){
       //     Debug.Log(s);
            entityDataReadFromDisk.Add(JsonSerializer.Deserialize<EntityData>(s));
        }
            isEntitiesReadFromDisk=true;
    }
    public void RemoveEntityFromSave(){
        EntityData tmpData=new EntityData();
        tmpData.guid=this.guid;
        foreach(EntityData ed in entityDataReadFromDisk){
            if(ed.guid==this.guid){
                entityDataReadFromDisk.Remove(ed);
                break;
            }
        }
    }
    public void SaveSingleEntity(){
        Debug.Log(this.guid);
        EntityData tmpData=new EntityData();
        tmpData.guid=this.guid;
        foreach(EntityData ed in entityDataReadFromDisk){
            if(ed.guid==this.guid){
             //   Debug.Log("dupli");
                entityDataReadFromDisk.Remove(ed);
                break;
            }
        }
        tmpData.entityTypeID=entityTypeID;
        tmpData.posX=transform.position.x;
        tmpData.posY=transform.position.y;
        tmpData.posZ=transform.position.z;
        tmpData.rotationX=transform.eulerAngles.x;
        tmpData.rotationY=transform.eulerAngles.y;
        tmpData.rotationZ=transform.eulerAngles.z;
        entityDataReadFromDisk.Add(tmpData);
    }
    public static void SaveWorldEntityData(){
        
        FileStream fs;
        if (File.Exists(gameWorldEntityDataPath+"unityMinecraftData/GameData/worldentities.json"))
        {
                 fs = new FileStream(gameWorldEntityDataPath+"unityMinecraftData/GameData/worldentities.json", FileMode.Truncate, FileAccess.Write);//Truncate模式打开文件可以清空。
        }
        else
        {
                 fs = new FileStream(gameWorldEntityDataPath+"unityMinecraftData/GameData/worldentities.json", FileMode.Create, FileAccess.Write);
        }
        fs.Close();
    
        foreach(EntityBeh e in worldEntities){
        e.SaveSingleEntity();
        }
     //   Debug.Log(entityDataReadFromDisk.Count);
       foreach(EntityData ed in entityDataReadFromDisk){
        string tmpData=JsonSerializer.ToJsonString(ed);
        File.AppendAllText(gameWorldEntityDataPath+"unityMinecraftData/GameData/worldentities.json",tmpData+"\n");
       }
        isWorldEntityDataSaved=true;
    }
    public static void SpawnNewEntity(float posX,float posY,float posZ,int entityID){
      
                switch(entityID){
                case 0:
                GameObject a=ObjectPools.creeperEntityPool.Get();
                a.transform.position=new Vector3(posX,posY,posZ);
         

          
                a.GetComponent<EntityBeh>().entityTypeID=entityID;
                a.GetComponent<EntityBeh>().guid=System.Guid.NewGuid().ToString("N");
                a.GetComponent<CreeperBeh>().SendMessage("InitPos");
                break;
                case 1:
                GameObject b=ObjectPools.zombieEntityPool.Get();
                b.transform.position=new Vector3(posX,posY,posZ);
                b.GetComponent<EntityBeh>().entityTypeID=entityID;
                b.GetComponent<EntityBeh>().guid=System.Guid.NewGuid().ToString("N");

                b.GetComponent<ZombieBeh>().SendMessage("InitPos");
   

                 
                break;
            }
        
    }
    public static void SpawnEntityFromFile(){
        foreach(EntityData ed in entityDataReadFromDisk){
    //        Debug.Log(ed.guid);
            switch(ed.entityTypeID){
                case 0:
                GameObject a=ObjectPools.creeperEntityPool.Get();
                a.transform.position=new Vector3(ed.posX,ed.posY,ed.posZ);
                a.transform.rotation=Quaternion.Euler(ed.rotationX,ed.rotationY,ed.rotationZ);
                a.GetComponent<CreeperBeh>().SendMessage("InitPos");
                a.GetComponent<EntityBeh>().entityTypeID=ed.entityTypeID;
                a.GetComponent<EntityBeh>().guid=ed.guid;
                break;
                case 1:
                GameObject b=ObjectPools.zombieEntityPool.Get();
                b.GetComponent<ZombieBeh>().SendMessage("InitPos");
                b.transform.position=new Vector3(ed.posX,ed.posY,ed.posZ);
                b.transform.rotation=Quaternion.Euler(ed.rotationX,ed.rotationY,ed.rotationZ);
                b.GetComponent<EntityBeh>().entityTypeID=ed.entityTypeID;
                b.GetComponent<EntityBeh>().guid=ed.guid;
                break;
            }

            
        }
    }
    public static void LoadEntities(){
        worldEntityTypes.Add(0,Resources.Load<GameObject>("Prefabs/creeper"));
        worldEntityTypes.Add(1,Resources.Load<GameObject>("Prefabs/zombie"));
        isEntitiesLoad=true;
    }
    void OnEnable(){
        worldEntities.Add(this); 
    }
   // void Awake(){
    //    worldEntities.Add(this);
   // }
    void FixedUpdate(){
        currentChunk=Chunk.GetChunk(Chunk.Vec3ToChunkPos(transform.position));
        if(currentChunk==null){
            GetComponent<CharacterController>().enabled=false;
            isInUnloadedChunks=true;
        }else{
             GetComponent<CharacterController>().enabled=true;
             isInUnloadedChunks=false;
        }
    }
}
