using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using MessagePack;
[MessagePackObject]
public struct ItemData{
    [Key(0)]
    public int itemID;
    [Key(1)]
    public int itemCount;
    [Key(2)]
    public float posX;
    [Key(3)]
    public float posY;
    [Key(4)]
    public float posZ;
    [Key(5)]
    public string guid;
    [Key(6)]
    public float lifeTime;
    [Key(7)]
    public int worldID;
    public ItemData(int itemID,int itemCount,float posX,float posY,float posZ,string guid,float lifeTime,int worldID){
        this.itemID=itemID;
         this.itemCount=itemCount;
          this.posX=posX;
           this.posY=posY;
            this.posZ=posZ;
             this.guid=guid;
             this.lifeTime=lifeTime;
        this.worldID=worldID;
    }
}
public class ItemEntityBeh : MonoBehaviour
{
    public int prevBlockOnItemID;
    public int curBlockOnItemID;
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
    public int itemInWorldID;
    public Vector3 lastItemPos;

    void ReleaseItem(){
    if(gameObject.activeInHierarchy==true){
            VoxelWorld.currentWorld.itemEntityPool.Remove(gameObject);    
    }
    
    }
    void OnEnable(){
    verts=new List<Vector3>();
    uvs=new List<Vector2>();
    tris=new List<int>();
    itemMesh=new Mesh();
    worldItemEntities.Add(this); 
    lastItemPos=transform.position;
        itemInWorldID = VoxelWorld.currentWorld.worldID;
    }

    public async void BuildItemModel(){
    itemMesh=new Mesh();
    float x=-0.5f;
    float y=-0.5f;
    float z=-0.5f;
    verts=new List<Vector3>();
    uvs=new List<Vector2>();
    tris=new List<int>();
    if(itemID>150&&itemID<=200&&itemID!=156){
      //  itemMesh=new Mesh();
        await BuildFlatItemModel(itemID,itemMesh);
           mf.mesh=itemMesh;
        mc.sharedMesh=itemMesh;
    }
    if(itemID==0){
        ReleaseItem();
        return;
    }
        if (itemID > 0 && itemID < 100)
        {
            BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris, 0);
            //Right

            BuildFace(itemID, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris, 1);

            //Bottom

            BuildFace(itemID, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris, 2);
            //Top

            BuildFace(itemID, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris, 3);

            //Back

            BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris, 4);
            //Front

            BuildFace(itemID, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris, 5);
        }
        else if (itemID == 100)
        {
            BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris, 0);
            //Right

            BuildFace(itemID, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris, 1);

            //Bottom

            BuildFace(itemID, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris, 2);
            //Top

            BuildFace(itemID, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris, 3);

            //Back

            BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris, 4);
            //Front

            BuildFace(itemID, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris, 5);
        }
        else if (itemID == 156) {
            BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris, 0);
            //Right

            BuildFace(itemID, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris, 1);

            //Bottom

            BuildFace(itemID, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris, 2);
            //Top

            BuildFace(itemID, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris, 3);

            //Back

            BuildFace(itemID, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris, 4);
            //Front

            BuildFace(itemID, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris, 5);


        }
        else
        {

            if (itemID >= 101 && itemID < 150)
            {
                if (itemID == 102)
                {
                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), new Vector2(0.0078125f, 0.0390625f), new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0f), false, verts, uvs, tris);
                    //Right

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), new Vector2(0.0078125f, 0.0390625f), new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0f), true, verts, uvs, tris);

                    //Bottom

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), new Vector2(0.0078125f, 0.0078125f), new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0f), false, verts, uvs, tris);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.625f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), new Vector2(0.0078125f, 0.0078125f), new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0.03125f), true, verts, uvs, tris);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), new Vector2(0.0078125f, 0.0390625f), new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0f), true, verts, uvs, tris);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0f, 0f, 0.125f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), new Vector2(0.0078125f, 0.0390625f), new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0f), false, verts, uvs, tris);

                }
                else
                {
                    Vector3 randomCrossModelOffset = new Vector3(0f, 0f, 0f);
                    BuildFace(itemID, new Vector3(x, y, z) + randomCrossModelOffset, new Vector3(0f, 1f, 0f) + randomCrossModelOffset, new Vector3(1f, 0f, 1f) + randomCrossModelOffset, false, verts, uvs, tris, 0);
                    BuildFace(itemID, new Vector3(x, y, z) + randomCrossModelOffset, new Vector3(0f, 1f, 0f) + randomCrossModelOffset, new Vector3(1f, 0f, 1f) + randomCrossModelOffset, true, verts, uvs, tris, 0);
                    BuildFace(itemID, new Vector3(x, y, z + 1f) + randomCrossModelOffset, new Vector3(0f, 1f, 0f) + randomCrossModelOffset, new Vector3(1f, 0f, -1f) + randomCrossModelOffset, false, verts, uvs, tris, 0);
                    BuildFace(itemID, new Vector3(x, y, z + 1f) + randomCrossModelOffset, new Vector3(0f, 1f, 0f) + randomCrossModelOffset, new Vector3(1f, 0f, -1f) + randomCrossModelOffset, true, verts, uvs, tris, 0);
                }

            }

        }

        itemMesh.vertices = verts.ToArray();
        itemMesh.uv = uvs.ToArray();
        itemMesh.triangles = tris.ToArray();
        itemMesh.RecalculateBounds();
        itemMesh.RecalculateNormals();
        itemMesh.RecalculateTangents();
        mc.sharedMesh=itemMesh;
        mf.mesh=itemMesh;

}
static void BuildFaceComplex(Vector3 corner, Vector3 up, Vector3 right,Vector2 uvWidth,Vector2 uvCorner, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris){
        int index = verts.Count;
        Vector3 vert0=corner;
        Vector3 vert1=corner+up;
        Vector3 vert2=corner+up+right;
        Vector3 vert3=corner+right;
        verts.Add (vert0);
        verts.Add (vert1);
        verts.Add (vert2);
        verts.Add (vert3);

      //  Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
      //  Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
    //    uvCorner=blockInfo[typeid][side];
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
static void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int side){

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
   
   
    public static bool isItemEntitiesLoad=false;
    public static string gameWorldItemEntityDataPath;
    public static List<ItemData> itemEntityDataReadFromDisk=new List<ItemData>();
    public static List<ItemEntityBeh> worldItemEntities=new List<ItemEntityBeh>();
    public static bool isItemEntitiesReadFromDisk=false;
    public static bool isItemEntitySavedInDisk=false;
    public string guid;
    public static bool isWorldItemEntityDataSaved=false;
    public bool isInUnloadedChunks=false;


    public async void OnDisable(){

        lastItemPos=transform.position;
        RemoveItemEntityFromSave();
        worldItemEntities.Remove(this); 
        await Task.Run(()=>{

        lifeTime=0f;
        isPosInited=false;

        itemID=0;
        

        });
    /*    lifeTime=0f;
        isPosInited=false;
        itemMesh=null;
        verts.Clear();
        uvs.Clear();
        tris.Clear();
        itemID=0;
        RemoveItemEntityFromSave();
        */ 
        
        
    }



    public void OnDestroy(){
        DestroyImmediate(itemMesh,true);
        DestroyImmediate(GetComponent<MeshFilter>().mesh,true);
        RemoveItemEntityFromSave();
        worldItemEntities.Remove(this);
          currentChunk=null;
    }
    public static void ReadItemEntityJson(){
   gameWorldItemEntityDataPath=WorldManager.gameWorldDataPath;
         
         if (!Directory.Exists(gameWorldItemEntityDataPath+"unityMinecraftData")){
                Directory.CreateDirectory(gameWorldItemEntityDataPath+"unityMinecraftData");
               
            }
          if(!Directory.Exists(gameWorldItemEntityDataPath+"unityMinecraftData/GameData")){
                    Directory.CreateDirectory(gameWorldItemEntityDataPath+"unityMinecraftData/GameData");
                }
       
        if(!File.Exists(gameWorldItemEntityDataPath+"unityMinecraftData"+"/GameData/worlditementities.json")){
             FileStream fs=File.Create(gameWorldItemEntityDataPath+"unityMinecraftData"+"/GameData/worlditementities.json");
             fs.Close();
        }
       
        byte[] worldItemEntitiesData=File.ReadAllBytes(gameWorldItemEntityDataPath+"unityMinecraftData/GameData/worlditementities.json");
      
  //          itemEntityDataReadFromDisk.Add(JsonSerializer.Deserialize<ItemData>(s));
  if(worldItemEntitiesData.Length>0){
    itemEntityDataReadFromDisk=MessagePackSerializer.Deserialize<List<ItemData>>(worldItemEntitiesData);
  }
        Debug.Log("item count:"+itemEntityDataReadFromDisk.Count);
            //isEntitiesReadFromDisk=true;
    }
    public void RemoveItemEntityFromSave(){
      ItemData tmpData=new ItemData(itemID,itemCount,lastItemPos.x,lastItemPos.y,lastItemPos.z,this.guid,lifeTime, itemInWorldID);
        tmpData.guid=this.guid;
        for(int i=0;i<itemEntityDataReadFromDisk.Count;i++){
            ItemData ed=itemEntityDataReadFromDisk[i];
            if(ed.guid==this.guid){
                itemEntityDataReadFromDisk.RemoveAt(i);
                break;
            }
        }
    }
    public void SaveSingleItemEntity(){
   //     Debug.Log(this.guid);
        lastItemPos=transform.position;
        ItemData tmpData=new ItemData(itemID,itemCount,lastItemPos.x,lastItemPos.y,lastItemPos.z,this.guid,lifeTime, itemInWorldID);
       // tmpData.guid=this.guid;
        foreach(ItemData ed in itemEntityDataReadFromDisk){
            if(ed.guid==this.guid){
             //   Debug.Log("dupli");
                itemEntityDataReadFromDisk.Remove(ed);
                break;
            }
        }
    /*    tmpData.itemID=itemID;
        tmpData.itemCount=itemCount;
        tmpData.posX=transform.position.x;
        tmpData.posY=transform.position.y;
        tmpData.lifeTime=lifeTime;
        tmpData.posZ=transform.position.z;*/
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
     /*  foreach(ItemData ed in itemEntityDataReadFromDisk){
        string tmpData=JsonSerializer.ToJsonString(ed);
        File.AppendAllText(gameWorldItemEntityDataPath+"unityMinecraftData/GameData/worlditementities.json",tmpData+"\n");
       }*/
       byte[] tmpData=MessagePackSerializer.Serialize(itemEntityDataReadFromDisk);
        File.WriteAllBytes(gameWorldItemEntityDataPath+"unityMinecraftData/GameData/worlditementities.json",tmpData);
      //  isWorldItemEntityDataSaved=true;
    }
    public static async void SpawnNewItem(float posX,float posY,float posZ,int itemID,Vector3 startingSpeed){
                if(itemID==-1){
                    return;
                }
   //     Debug.Log(VoxelWorld.currentWorld.itemEntityPool.Object);

            GameObject a = VoxelWorld.currentWorld.itemEntityPool.Get(new Vector3(posX,posY,posZ));
                ItemEntityBeh tmp=a.GetComponent<ItemEntityBeh>();
                
                tmp.itemID=itemID;
                tmp.guid=System.Guid.NewGuid().ToString("N");
                 tmp.SendMessage("InitPos");
             //    a.transform.position=new Vector3(posX,posY,posZ);
                await UniTask.WaitUntil(()=>tmp.isPosInited==true); 
                await UniTask.Delay(10);
                 tmp.AddForceInvoke(startingSpeed);
                
        }


    public static void SpawnItemEntityFromFile(){
        for(int i=0;i<itemEntityDataReadFromDisk.Count;i++){
          //  Debug.Log(ed.guid);

                ItemData ed=itemEntityDataReadFromDisk[i];
            if (ed.worldID == VoxelWorld.currentWorld.worldID)
            {
            GameObject a= VoxelWorld.currentWorld.itemEntityPool.Get(new Vector3(ed.posX,ed.posY,ed.posZ));
                ItemEntityBeh tmp=a.GetComponent<ItemEntityBeh>();
           //     a.transform.position=new Vector3(ed.posX,ed.posY,ed.posZ);
          //      a.transform.rotation=Quaternion.identity;
                tmp.itemID=ed.itemID;
                tmp.guid=ed.guid;
                tmp.lifeTime=ed.lifeTime;
            
                tmp.SendMessage("InitPos");
            }
          
          

            
        }
       
    }
    public async void AddForceInvoke(Vector3 f){
        if(isPosInited!=true){
            Debug.Log("pos not inited");
        }
        await UniTask.WaitUntil(()=>rb.constraints== RigidbodyConstraints.None);
        if (rb != null)
        {
    rb.velocity=f;
        }
     
    }
    public void InitPos(){
            isPosInited=true;
        BuildItemModel();
    
    }

    void Awake(){
      //  worldEntities.Add(this);
    /*   if(isFlatItemInfoAdded==false){
            AddFlatItemInfo();
        }*/
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
       
    if(Mathf.Abs(playerPos.position.x-transform.position.x)<1f&&Mathf.Abs(playerPos.position.y-transform.position.y)<2f&&Mathf.Abs(playerPos.position.z-transform.position.z)<1f&&lifeTime>0.5f){
         if( playerPos.gameObject.GetComponent<PlayerMove>().CheckInventoryIsFull(itemID)==true){
            return;
        }
        if(playerPos.gameObject.GetComponent<PlayerMove>().isPlayerKilled==true){
            return;
        }
        AudioSource.PlayClipAtPoint(PlayerMove.playerDropItemClip,transform.position,1f);
        playerPos.gameObject.GetComponent<PlayerMove>().AddItem(itemID,1);
       //playerPos.gameObject.GetComponent<PlayerMove>().playerHandItem.BuildItemModel(playerPos.gameObject.GetComponent<PlayerMove>().inventoryDic[playerPos.gameObject.GetComponent<PlayerMove>().currentSelectedHotbar-1]);
        ReleaseItem();
    }
    }
    void FixedUpdate(){
        if(currentChunk==null){
         currentChunk=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(transform.position));     
        }
        
        if(!WorldHelper.instance.CheckIsPosInChunk(transform.position,currentChunk)){
             currentChunk=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(transform.position));    
        }
          
        PlayerEatItem();
        if(transform.position.y<-40f){
            ReleaseItem();
            return;
        }
        
        curBlockOnItemID=WorldHelper.instance.GetBlock(transform.position,currentChunk);
        if(curBlockOnItemID==100){
            rb.AddForce(new Vector3(0f,20f,0f));
        }
        if(curBlockOnItemID>0&&curBlockOnItemID<100){
            rb.AddForce(new Vector3(0f,20f,0f));
        }
        if(prevBlockOnItemID!=curBlockOnItemID){
            if(curBlockOnItemID==100){
            rb.drag=2f;   
             AudioSource.PlayClipAtPoint(PlayerMove.playerSinkClip,transform.position,1f);
                ParticleEffectManagerBeh.instance.EmitWaterSplashParticleAtPosition(transform.position);
            }else{
                rb.drag=0f;
            }
             
        }
        prevBlockOnItemID=curBlockOnItemID;
        if(isPosInited==false){
             rb.constraints = RigidbodyConstraints.FreezeAll;
        }else{
           rb.constraints = RigidbodyConstraints.None;
        }
      

          if(currentChunk==null){
          rb.constraints = RigidbodyConstraints.FreezeAll;
            isInUnloadedChunks=true;
        }else{

            
             rb.constraints = RigidbodyConstraints.None;
             isInUnloadedChunks=false;
        }
        
      
    }



     public static Dictionary<int,Vector2> itemMaterialInfo=new Dictionary<int,Vector2>();
    public static Dictionary<int,Vector2Int> itemTexturePosInfo=new Dictionary<int,Vector2Int>();
    //151diamond pickaxe 152diamond sword 153diamond 154rotten flesh 155gun powder 156tnt 157bow 158armor upgrade
    public static Texture2D itemTextureInfo;
//    public List<Vector3> verts=new List<Vector3>();
  //  public List<Vector2> uvs=new List<Vector2>();
  //  public List<int> tris=new List<int>();
    public static bool isFlatItemInfoAdded=false;
   // public Texture2D texture;
    public static int textureXSize=64;
    public static int textureYSize=64;
 //   public Mesh itemMesh;
    public static void AddFlatItemInfo(){
        itemMaterialInfo.TryAdd(154,new Vector2(0.1875f,0.125f));
        itemMaterialInfo.TryAdd(153,new Vector2(0.125f,0.125f));
        itemMaterialInfo.TryAdd(151,new Vector2(0.0625f,0.125f));
        itemMaterialInfo.TryAdd(152,new Vector2(0.0f,0.125f));
        itemMaterialInfo.TryAdd(155, new Vector2(0.25f, 0.125f));
        itemMaterialInfo.TryAdd(156, new Vector2(0.3125f, 0.125f));
        itemMaterialInfo.TryAdd(157, new Vector2(0.5f, 0.125f));
        itemMaterialInfo.TryAdd(158, new Vector2(0.5625f, 0.125f));
        itemTexturePosInfo.TryAdd(154,new Vector2Int(192,128));
        itemTexturePosInfo.TryAdd(155, new Vector2Int(256, 128));
        itemTexturePosInfo.TryAdd(156, new Vector2Int(320, 128));
        itemTexturePosInfo.TryAdd(157, new Vector2Int(512, 128));
        itemTexturePosInfo.TryAdd(158, new Vector2Int(576, 128));
        itemTexturePosInfo.TryAdd(153,new Vector2Int(128,128));
        itemTexturePosInfo.TryAdd(152,new Vector2Int(0,128));
        itemTexturePosInfo.TryAdd(151,new Vector2Int(64,128));
        itemTextureInfo=Resources.Load<Texture2D>("Textures/itemterrain");
   //     itemTextureInfo.Add(0,Resources.Load<Texture2D>("Textures/diamond_pickaxe"));
      //  itemTextureInfo.Add(1,Resources.Load<Texture2D>("Textures/diamond_sword"));
        isFlatItemInfoAdded=true;
    }

    public async Task BuildFlatItemModel(int itemID,Mesh mesh)
    {
    float x=0f;
    float y=0f;
    float z=0f;
        BuildFlatItemFace(itemMaterialInfo[itemID].x,itemMaterialInfo[itemID].y,0.0625f, new Vector3(x, y, z)/16, Vector3.forward*textureXSize/4/16, Vector3.right*textureYSize/4/16, false, verts, uvs, tris);
        BuildFlatItemFace(itemMaterialInfo[itemID].x,itemMaterialInfo[itemID].y,0.0625f, new Vector3(x, y+1f, z)/16, Vector3.forward*textureXSize/4/16, Vector3.right*textureYSize/4/16, true, verts, uvs, tris);
        for(int i=0;i<textureXSize;i++){
            for(int j=0;j<textureYSize;j++){
                

                    BuildModelPixel(itemTexturePosInfo[itemID].x + i, itemTexturePosInfo[itemID].y + j, itemTexturePosInfo[itemID].x, itemTexturePosInfo[itemID].y,1f, verts, uvs, tris);
         //           Debug.Log((float)(itemTexturePosInfo[itemID].x + i) / textureXSize * 0.0625f);
                 
                 
                    
            }
        }
        float offsetX=-0.5f;
        float offsetY=-0.5f;
        float offsetZ=-0.5f;
        await Task.Run(()=>{for(int i=0;i<verts.Count;i++){
            verts[i]=new Vector3(verts[i].x+offsetX,verts[i].y+offsetY,verts[i].z+offsetZ);
           
        }});
        
      //  Debug.Log(Thread.CurrentThread.ManagedThreadId.ToString());
        mesh.vertices=verts.ToArray();
        mesh.uv=uvs.ToArray();
        mesh.triangles=tris.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
   public static void BuildModelPixel(int x,int y,int originX,int originY,float scale, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
    {

       
        if (itemTextureInfo.GetPixel(x, y).a != 0f && itemTextureInfo.GetPixel(x + 1,y).a == 0f)
        {
            //right
            BuildFlatItemFace((float)x / (float)textureXSize * 0.0625f, (float)y / (float)textureYSize * 0.0625f, (float)1f/64f/16f, new Vector3(x- originX + 1, 0, y-originY) / 4f / 16f * scale, Vector3.up / 16f * scale, Vector3.forward / 4f / 16f * scale, true, verts, uvs, tris);

        }
        if (itemTextureInfo.GetPixel(x, y).a != 0f && itemTextureInfo.GetPixel(x-1,y).a == 0f)
        {
            //left
            BuildFlatItemFace((float)x / (float)textureXSize * 0.0625f, (float)y / (float)textureYSize * 0.0625f,(float)1f / 64f / 16f, new Vector3(x - originX, 0, y- originY) / 4f / 16f * scale, Vector3.up / 16 * scale, Vector3.forward / 4 / 16 * scale, false, verts, uvs, tris);

        }
        if (itemTextureInfo.GetPixel(x, y).a != 0f && itemTextureInfo.GetPixel(x,y+1).a == 0f)
        {
            //front
            BuildFlatItemFace((float)x / (float)textureXSize * 0.0625f, (float)y / (float)textureYSize * 0.0625f, (float)1f / 64f / 16f, new Vector3(x - originX, 0,y - originY + 1) / 4f / 16f* scale, Vector3.up / 16 * scale, Vector3.right / 4 / 16 * scale, false, verts, uvs, tris);

        }
        if (itemTextureInfo.GetPixel(x, y).a != 0f && itemTextureInfo.GetPixel(x,y-1).a == 0f)
        {
            //back
            BuildFlatItemFace((float)x / (float)textureXSize * 0.0625f, (float)y / (float)textureYSize * 0.0625f, (float)1f / 64f / 16f, new Vector3(x - originX, 0,y - originY) / 4f / 16f * scale, Vector3.up / 16 * scale, Vector3.right / 4 / 16 * scale, true, verts, uvs, tris);

        }

    }
static void BuildFlatItemFace(float uvX,float uvY,float uvWidthXY,Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris){
        Vector2 uvCorner=new Vector2(uvX,uvY);
     
        int index = verts.Count;
    
        verts.Add (corner);
        verts.Add (corner + up);
        verts.Add (corner + up + right);
        verts.Add (corner + right);


        
        Vector2 uvWidth = new Vector2(uvWidthXY,uvWidthXY);
     

        //uvCorner.x = (float)(typeid - 1) / 16;

        
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




}
