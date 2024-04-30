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

using MessagePack;
[MessagePackObject]
public struct EntityData{
  [Key(0)]
    public float posX;
    [Key(1)]
    public float posY;
    [Key(2)]
    public float posZ;
    [Key(3)]
    public float rotationX;
    [Key(4)]
    public float rotationY;
    [Key(5)]
    public float rotationZ;
    [Key(6)]
    public int entityTypeID;
    [Key(7)]
    public string guid;
    [Key(8)]
    public int entityInWorldID;
    public bool Equals(EntityData other)
    {   
       
        if (this.guid.Equals(other.guid)) {
            return true;
        }
        else {
            return false;
        }
    }
    public EntityData(float posX,float posY,float posZ,float rotationX,float rotationY,float rotationZ,int entityTypeID,string guid,int entityInWorldID){
        this.posX=posX;
        this.posY=posY;
        this.posZ=posZ;
        this.rotationX=rotationX;
        this.rotationY=rotationY;
        this.rotationZ=rotationZ;
        this.entityTypeID=entityTypeID;
        this.guid=guid;
        this.entityInWorldID=entityInWorldID;
    }
}
public class EntityBeh : MonoBehaviour
{
    public Chunk currentChunk;
    public static Dictionary<int,GameObject> worldEntityTypes=new Dictionary<int,GameObject>(); 
    //0Creeper 1zombie 2tnt 3skeleton 4arrow 5enderman
    public CharacterController cc;
    public static bool isEntitiesLoad=false;
    public static string gameWorldEntityDataPath;
    public static List<EntityData> entityDataReadFromDisk=new List<EntityData>();
    public static List<EntityBeh> worldEntities=new List<EntityBeh>();
    public static bool isEntitiesReadFromDisk=false;
    public static bool isEntitySavedInDisk=false;
    public int entityTypeID;
    public int entityInWorldID;
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
          currentChunk=null;
    }
    public static void DestroyAllEntities()
    {
        for(int i=0;i<worldEntities.Count;i++ )
        {
            worldEntities[i].OnDestroy();
        }
    }
    public static void ReadEntityJson(){
   gameWorldEntityDataPath=WorldManager.gameWorldDataPath;
         
         if (!Directory.Exists(gameWorldEntityDataPath+"unityMinecraftData")){
                Directory.CreateDirectory(gameWorldEntityDataPath+"unityMinecraftData");
               
            }
          if(!Directory.Exists(gameWorldEntityDataPath+"unityMinecraftData/GameData")){
                    Directory.CreateDirectory(gameWorldEntityDataPath+"unityMinecraftData/GameData");
                }
       
        if(!File.Exists(gameWorldEntityDataPath+"unityMinecraftData"+"/GameData/worldentities.json")){
            FileStream fs=File.Create(gameWorldEntityDataPath+"unityMinecraftData"+"/GameData/worldentities.json");
            fs.Close();
        }
       
        byte[] worldEntitiesData=File.ReadAllBytes(gameWorldEntityDataPath+"unityMinecraftData/GameData/worldentities.json");
        if(worldEntitiesData.Length>0){
            entityDataReadFromDisk=MessagePackSerializer.Deserialize<List<EntityData>>(worldEntitiesData);
        }
     
            isEntitiesReadFromDisk=true;
            return;
    }
    public void RemoveEntityFromSave(){
      EntityData tmpData=new EntityData(transform.position.x,transform.position.y,transform.position.z,transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z,entityTypeID,guid, entityInWorldID);
        foreach(EntityData ed in entityDataReadFromDisk){
            if(ed.guid==this.guid){
                entityDataReadFromDisk.Remove(ed);
                break;
            }
        }
    }
    public void SaveSingleEntity(){
    //    Debug.Log(this.guid);
        EntityData tmpData=new EntityData(transform.position.x,transform.position.y,transform.position.z,transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z,entityTypeID, guid,entityInWorldID);
    //    tmpData.guid=this.guid;
        foreach(EntityData ed in entityDataReadFromDisk){
            if(ed.guid==this.guid){
             //   Debug.Log("dupli");
                entityDataReadFromDisk.Remove(ed);
                break;
            }
        }
   /*     tmpData.entityTypeID=entityTypeID;
        tmpData.posX=transform.position.x;
        tmpData.posY=transform.position.y;
        tmpData.posZ=transform.position.z;
        tmpData.rotationX=transform.eulerAngles.x;
        tmpData.rotationY=transform.eulerAngles.y;
        tmpData.rotationZ=transform.eulerAngles.z;*/
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
     /*  foreach(EntityData ed in entityDataReadFromDisk){
        string tmpData=JsonSerializer.ToJsonString(ed);
        File.AppendAllText(gameWorldEntityDataPath+"unityMinecraftData/GameData/worldentities.json",tmpData+"\n");
       }*/
       byte[] tmpData=MessagePackSerializer.Serialize(entityDataReadFromDisk);
       File.WriteAllBytes(gameWorldEntityDataPath+"unityMinecraftData/GameData/worldentities.json",tmpData);
        isWorldEntityDataSaved=true;
    }
    public static EntityBeh SpawnNewEntity(float posX,float posY,float posZ,int entityID){
      
                switch(entityID){
                case 0:
                GameObject a= VoxelWorld.currentWorld.creeperEntityPool.Get();
                a.transform.position=new Vector3(posX,posY,posZ);
         

          
                a.GetComponent<EntityBeh>().entityTypeID=entityID;
                a.GetComponent<EntityBeh>().guid=System.Guid.NewGuid().ToString("N");
                a.GetComponent<CreeperBeh>().SendMessage("InitPos");
                return a.GetComponent<EntityBeh>();
                break;
                case 1:
                GameObject b= VoxelWorld.currentWorld.zombieEntityPool.Get();
                b.transform.position=new Vector3(posX,posY,posZ);
                b.GetComponent<EntityBeh>().entityTypeID=entityID;
                b.GetComponent<EntityBeh>().guid=System.Guid.NewGuid().ToString("N");

                b.GetComponent<ZombieBeh>().SendMessage("InitPos");
   

                 return b.GetComponent<EntityBeh>();
                break;
                case 2:
                GameObject c = VoxelWorld.currentWorld.tntEntityPool.Get();
                c.transform.position = new Vector3(posX, posY, posZ);
                c.GetComponent<Rigidbody>().position = new Vector3(posX,posY,posZ); 
                c.GetComponent<EntityBeh>().entityTypeID = entityID;
                c.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                c.GetComponent<TNTBeh>().SendMessage("InitPos");
                return c.GetComponent<EntityBeh>();
                break;
                case 3:
                GameObject d =  VoxelWorld.currentWorld.skeletonEntityPool.Get();
                d.transform.position = new Vector3(posX, posY, posZ);
             
                d.GetComponent<EntityBeh>().entityTypeID = entityID;
                d.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                d.GetComponent<SkeletonBeh>().SendMessage("InitPos");
                return d.GetComponent<EntityBeh>();
                break;
                case 4:
                GameObject e = VoxelWorld.currentWorld.arrowEntityPool.Get();
                e.transform.position = new Vector3(posX, posY, posZ);
                e.GetComponent<Rigidbody>().position = new Vector3(posX, posY, posZ);
                e.GetComponent<EntityBeh>().entityTypeID = entityID;
                e.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");

             //   e.GetComponent<ArrowBeh>().isPosInited = true;
                return e.GetComponent<EntityBeh>();
                case 5:
                GameObject f = VoxelWorld.currentWorld.endermanEntityPool.Get();
                f.transform.position = new Vector3(posX, posY, posZ);
              
                f.GetComponent<EntityBeh>().entityTypeID = entityID;
                f.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                f.GetComponent<EndermanBeh>().SendMessage("InitPos");
                //   e.GetComponent<ArrowBeh>().isPosInited = true;
                return f.GetComponent<EntityBeh>();
            default: return null;
        }
        
    }
    public static void SpawnEntityFromFile(){
        foreach(EntityData ed in entityDataReadFromDisk){
            //        Debug.Log(ed.guid);
            if (ed.entityInWorldID == VoxelWorld.currentWorld.worldID) { switch(ed.entityTypeID){
                case 0:
                GameObject a= VoxelWorld.currentWorld.creeperEntityPool.Get();
                        
                a.transform.position=new Vector3(ed.posX,ed.posY,ed.posZ);
                a.transform.rotation=Quaternion.Euler(ed.rotationX,ed.rotationY,ed.rotationZ);
                a.GetComponent<CreeperBeh>().SendMessage("InitPos");
                a.GetComponent<EntityBeh>().entityTypeID=ed.entityTypeID;
                a.GetComponent<EntityBeh>().guid=ed.guid;
                  
                break;
                case 1:
                GameObject b= VoxelWorld.currentWorld.zombieEntityPool.Get();
                b.GetComponent<ZombieBeh>().SendMessage("InitPos");
                b.transform.position=new Vector3(ed.posX,ed.posY,ed.posZ);
                b.transform.rotation=Quaternion.Euler(ed.rotationX,ed.rotationY,ed.rotationZ);
                b.GetComponent<EntityBeh>().entityTypeID=ed.entityTypeID;
                b.GetComponent<EntityBeh>().guid=ed.guid;
              
                break;
                case 2:
                    GameObject c = VoxelWorld.currentWorld.tntEntityPool.Get();
                    c.GetComponent<TNTBeh>().SendMessage("InitPos");
                    c.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                    c.GetComponent<Rigidbody>().position = new Vector3(ed.posX, ed.posY, ed.posZ);
                    c.transform.rotation = Quaternion.Euler(ed.rotationX, ed.rotationY, ed.rotationZ);
                    c.GetComponent<EntityBeh>().entityTypeID = ed.entityTypeID;
                    c.GetComponent<EntityBeh>().guid = ed.guid;

                    break;

                case 3:
                    GameObject d =  VoxelWorld.currentWorld.skeletonEntityPool.Get();
                    d.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                    d.transform.rotation = Quaternion.Euler(ed.rotationX, ed.rotationY, ed.rotationZ);
                    d.GetComponent<EntityBeh>().entityTypeID = ed.entityTypeID;
                    d.GetComponent<EntityBeh>().guid = ed.guid;
                    d.GetComponent<SkeletonBeh>().SendMessage("InitPos");
                   
                    break;

                case 4:
                    GameObject e = VoxelWorld.currentWorld.arrowEntityPool.Get();
                    e.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                    e.GetComponent<Rigidbody>().position = new Vector3(ed.posX, ed.posY, ed.posZ);
                    e.GetComponent<EntityBeh>().entityTypeID = ed.entityTypeID;
                    e.GetComponent<EntityBeh>().guid = ed.guid;
                    break;

                    case 5:
                        GameObject f = VoxelWorld.currentWorld.endermanEntityPool.Get();
                        f.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                        f.transform.rotation = Quaternion.Euler(ed.rotationX, ed.rotationY, ed.rotationZ);
                        f.GetComponent<EntityBeh>().entityTypeID = ed.entityTypeID ;
                        f.GetComponent<EntityBeh>().guid = ed.guid;
                        f.GetComponent<EndermanBeh>().SendMessage("InitPos");
                     
                        break;
                        
                }
            }
            


        }
    }
    
    public static void LoadEntities(){
        worldEntityTypes.TryAdd(0,Resources.Load<GameObject>("Prefabs/creeper"));
        worldEntityTypes.TryAdd(1,Resources.Load<GameObject>("Prefabs/zombie"));
        worldEntityTypes.TryAdd(2, Resources.Load<GameObject>("Prefabs/tnt"));
        worldEntityTypes.TryAdd(3, Resources.Load<GameObject>("Prefabs/skeleton"));
        worldEntityTypes.TryAdd(4, Resources.Load<GameObject>("Prefabs/arrow"));
        worldEntityTypes.TryAdd(5, Resources.Load<GameObject>("Prefabs/enderman"));
        isEntitiesLoad =true;
       
    }
    void OnEnable(){
        entityInWorldID = VoxelWorld.currentWorld.worldID;
        worldEntities.Add(this); 
        cc=GetComponent<CharacterController>();
    }
   // void Awake(){
    //    worldEntities.Add(this);
   // }
    void FixedUpdate(){
         if(currentChunk==null){
         currentChunk=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(transform.position));   
         }
        if(WorldHelper.instance.CheckIsPosInChunk(transform.position,currentChunk)==false){
             currentChunk=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(transform.position));   
        }
        if(currentChunk==null||(currentChunk!=null&&currentChunk.isStrongLoaded==false)){
            if (cc != null)
            {
            cc.enabled=false;
            }
            
            isInUnloadedChunks=true;
        }else{
            if (cc != null)
            {
                cc.enabled=true;
            }
                
             isInUnloadedChunks=false;
        }
    }
    
}
