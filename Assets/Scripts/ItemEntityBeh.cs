using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Utf8Json;
public class ItemData{
    public int itemID;
    public int itemCount;
    public float posX;
    public float posY;
    public float posZ;
    public string guid;
    public float lifeTime;
}
public class ItemEntityBeh : MonoBehaviour
{
    public float lifeTime;
    public int itemID;
    public int itemCount;
    public Mesh itemMesh;
    public bool isPosInited=false;
    public List<Vector3> verts=new List<Vector3>();
    public List<Vector2> uvs=new List<Vector2>();
    public List<int> tris=new List<int>();
    public MeshCollider mc;
    public MeshFilter mf;
    public Rigidbody rb;
    public static Transform playerPos;



    void ReleaseItem(){
    if(gameObject.activeInHierarchy==true){
    ObjectPools.itemEntityPool.Remove(gameObject);    
    }
    
    }
    void OnEnable(){
    verts=new List<Vector3>();
    uvs=new List<Vector2>();
    tris=new List<int>();
    itemMesh=new Mesh();
    worldItemEntities.Add(this); 
    }

    public void BuildItemModel(){
    itemMesh=new Mesh();
    float x=-0.5f;
    float y=-0.5f;
    float z=-0.5f;
    verts=new List<Vector3>();
    uvs=new List<Vector2>();
    tris=new List<int>();
    if(itemID==0){
        ReleaseItem();
        return;
    }
    if(itemID>0&&itemID<100){
        BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris,0);
        //Right
   
         BuildFace(itemID, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris,1);

        //Bottom
   
         BuildFace(itemID, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris,2);
        //Top
  
        BuildFace(itemID, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris,3);

        //Back
     
        BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris,4);
        //Front
       
        BuildFace(itemID, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris,5); 
    }else if(itemID==100){
        BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris,0);
        //Right
   
         BuildFace(itemID, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris,1);

        //Bottom
   
         BuildFace(itemID, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris,2);
        //Top
  
        BuildFace(itemID, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris,3);

        //Back
     
        BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris,4);
        //Front
       
        BuildFace(itemID, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris,5); 
    }else{
    
        if(itemID>=101&&itemID<150){
            Vector3 randomCrossModelOffset=new Vector3(0f,0f,0f);
            BuildFace(itemID, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, false, verts, uvs, tris,0);
            BuildFace(itemID, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, true, verts, uvs, tris,0);
            BuildFace(itemID, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, false, verts, uvs, tris,0);
            BuildFace(itemID, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, true, verts, uvs, tris,0);
        }
    }

        itemMesh.vertices = verts.ToArray();
        itemMesh.uv = uvs.ToArray();
        itemMesh.triangles = tris.ToArray();
        itemMesh.RecalculateBounds();
        itemMesh.RecalculateNormals();
        mc.sharedMesh=itemMesh;
        mf.mesh=itemMesh;

}
 void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int side){

        int index = verts.Count;
    
        verts.Add (corner);
        verts.Add (corner + up);
        verts.Add (corner + up + right);
        verts.Add (corner + right);

        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        uvCorner=Chunk.itemBlockInfo[typeid][side];
        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
    
        if (reversed)
            {
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 0);
            }
            else
            {
            tris.Add(index + 1);
            tris.Add(index + 0);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 0);
        }
    
    }


    public Chunk currentChunk;
   // public static Dictionary<int,GameObject> worldEntityTypes=new Dictionary<int,GameObject>(); 
   
    public static RuntimePlatform platform = Application.platform;
    public static bool isItemEntitiesLoad=false;
    public static string gameWorldItemEntityDataPath;
    public static List<ItemData> itemEntityDataReadFromDisk=new List<ItemData>();
    public static List<ItemEntityBeh> worldItemEntities=new List<ItemEntityBeh>();
    public static bool isItemEntitiesReadFromDisk=false;
    public static bool isItemEntitySavedInDisk=false;
    public string guid;
    public static bool isWorldItemEntityDataSaved=false;
    public bool isInUnloadedChunks=false;


    public void OnDisable(){
        lifeTime=0f;
        isPosInited=false;
        itemMesh=null;
        verts.Clear();
        uvs.Clear();
        tris.Clear();
        itemID=0;
        RemoveItemEntityFromSave();
        worldItemEntities.Remove(this);  
        
        
    }



    public void OnDestroy(){
        RemoveItemEntityFromSave();
        worldItemEntities.Remove(this);
    }
    public static void ReadItemEntityJson(){
     if(platform==RuntimePlatform.WindowsPlayer||platform==RuntimePlatform.WindowsEditor){
        gameWorldItemEntityDataPath="C:/";
      }else{
        gameWorldItemEntityDataPath=Application.persistentDataPath;
      }
         
         if (!Directory.Exists(gameWorldItemEntityDataPath+"unityMinecraftData")){
                Directory.CreateDirectory(gameWorldItemEntityDataPath+"unityMinecraftData");
               
            }
          if(!Directory.Exists(gameWorldItemEntityDataPath+"unityMinecraftData/GameData")){
                    Directory.CreateDirectory(gameWorldItemEntityDataPath+"unityMinecraftData/GameData");
                }
       
        if(!File.Exists(gameWorldItemEntityDataPath+"unityMinecraftData"+"/GameData/worlditementities.json")){
            File.Create(gameWorldItemEntityDataPath+"unityMinecraftData"+"/GameData/worlditementities.json");
        }
       
        string[] worldItemEntitiesData=File.ReadAllLines(gameWorldItemEntityDataPath+"unityMinecraftData/GameData/worlditementities.json");
        foreach(string s in worldItemEntitiesData){
            Debug.Log(s);
            itemEntityDataReadFromDisk.Add(JsonSerializer.Deserialize<ItemData>(s));
        }
            //isEntitiesReadFromDisk=true;
    }
    public void RemoveItemEntityFromSave(){
        ItemData tmpData=new ItemData();
        tmpData.guid=this.guid;
        foreach(ItemData ed in itemEntityDataReadFromDisk){
            if(ed.guid==this.guid){
                itemEntityDataReadFromDisk.Remove(ed);
                break;
            }
        }
    }
    public void SaveSingleItemEntity(){
        Debug.Log(this.guid);
        ItemData tmpData=new ItemData();
        tmpData.guid=this.guid;
        foreach(ItemData ed in itemEntityDataReadFromDisk){
            if(ed.guid==this.guid){
             //   Debug.Log("dupli");
                itemEntityDataReadFromDisk.Remove(ed);
                break;
            }
        }
        tmpData.itemID=itemID;
        tmpData.itemCount=itemCount;
        tmpData.posX=transform.position.x;
        tmpData.posY=transform.position.y;
        tmpData.lifeTime=lifeTime;
        tmpData.posZ=transform.position.z;
      //  tmpData.rotationX=transform.eulerAngles.x;
      //  tmpData.rotationY=transform.eulerAngles.y;
      //  tmpData.rotationZ=transform.eulerAngles.z;
        itemEntityDataReadFromDisk.Add(tmpData);
    }
    public static void SaveWorldItemEntityData(){
        
        FileStream fs;
        if (File.Exists(gameWorldItemEntityDataPath+"unityMinecraftData/GameData/worlditementities.json"))
        {
                 fs = new FileStream(gameWorldItemEntityDataPath+"unityMinecraftData/GameData/worlditementities.json", FileMode.Truncate, FileAccess.Write);//Truncate模式打开文件可以清空。
        }
        else
        {
                 fs = new FileStream(gameWorldItemEntityDataPath+"unityMinecraftData/GameData/worlditementities.json", FileMode.Create, FileAccess.Write);
        }
        fs.Close();
    
        foreach(ItemEntityBeh e in worldItemEntities){
            e.SaveSingleItemEntity();
        }
     //   Debug.Log(itemEntityDataReadFromDisk.Count);
       foreach(ItemData ed in itemEntityDataReadFromDisk){
        string tmpData=JsonSerializer.ToJsonString(ed);
        File.AppendAllText(gameWorldItemEntityDataPath+"unityMinecraftData/GameData/worlditementities.json",tmpData+"\n");
       }
      //  isWorldItemEntityDataSaved=true;
    }
    public static IEnumerator SpawnNewItem(float posX,float posY,float posZ,int itemID,Vector3 startingSpeed){
                GameObject a=ObjectPools.itemEntityPool.Get(new Vector3(posX,posY,posZ));
                ItemEntityBeh tmp=a.GetComponent<ItemEntityBeh>();
                
                tmp.itemID=itemID;
                tmp.guid=System.Guid.NewGuid().ToString("N");
                 tmp.SendMessage("InitPos");
             //    a.transform.position=new Vector3(posX,posY,posZ);
                 yield return new WaitForSeconds(0.11f);
                 tmp.AddForceInvoke(startingSpeed);
                 yield break;
        }


    public static IEnumerator SpawnItemEntityFromFile(){
        foreach(ItemData ed in itemEntityDataReadFromDisk){
          //  Debug.Log(ed.guid);
          
                GameObject a=ObjectPools.itemEntityPool.Get(new Vector3(ed.posX,ed.posY,ed.posZ));
                ItemEntityBeh tmp=a.GetComponent<ItemEntityBeh>();
           //     a.transform.position=new Vector3(ed.posX,ed.posY,ed.posZ);
          //      a.transform.rotation=Quaternion.identity;
                tmp.itemID=ed.itemID;
                tmp.guid=ed.guid;
                tmp.lifeTime=ed.lifeTime;
                yield return new WaitForSeconds(0.01f);
                tmp.SendMessage("InitPos");
          

            
        }
    }
    public void AddForceInvoke(Vector3 f){
        if(isPosInited!=true){
            Debug.Log("pos not inited");
        }
        rb.velocity=f;
    }
    public void InitPos(){
        Invoke("InvokeInitPos",0.1f);
    
    }
    public void InvokeInitPos(){
        isPosInited=true;
        BuildItemModel();
    }
    void Awake(){
      //  worldEntities.Add(this);
      rb=GetComponent<Rigidbody>();
      mc=GetComponent<MeshCollider>();
      mf=GetComponent<MeshFilter>();
    }
    void Update(){
        lifeTime+=Time.deltaTime;
        if(lifeTime>=60f){
            ReleaseItem();
            return;
        }
    }
    void PlayerEatItem(){ 
   if(Mathf.Abs(playerPos.position.x-transform.position.x)<1f&&Mathf.Abs(playerPos.position.y-transform.position.y)<2f&&Mathf.Abs(playerPos.position.z-transform.position.z)<1f&&lifeTime>2f){
    playerPos.gameObject.GetComponent<PlayerMove>().AddItem(itemID,1);
    ReleaseItem();
   }
    }
    void FixedUpdate(){
        PlayerEatItem();
        if(transform.position.y<-40f){
            ReleaseItem();
            return;
        }
        if(isPosInited==false){
             GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }else{
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
        currentChunk=Chunk.GetChunk(Chunk.Vec3ToChunkPos(transform.position));
        if(currentChunk==null){
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            isInUnloadedChunks=true;
        }else{
             GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
             isInUnloadedChunks=false;
        }
    }
}
