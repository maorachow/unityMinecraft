using System;
using Cysharp.Threading.Tasks;
using MessagePack;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public class ItemEntityBeh : MonoBehaviour
{
    public int prevBlockOnItemID;
    public int curBlockOnItemID;
    public float lifeTime;
    public int itemID;
    public int itemCount;
    public Mesh itemMesh;
    public bool isPosInited=false;
   
    public MeshCollider mc;
    public MeshFilter mf;
    public Rigidbody rb;
   
    public int itemInWorldID;
    public Vector3 lastItemPos;

    void ReleaseItem()
    {
        if (gameObject.activeInHierarchy == true)
        {
            RemoveItemEntityFromSave(ref VoxelWorld.currentWorld.itemEntityManager.itemEntityDataReadFromDisk);
            VoxelWorld.currentWorld.itemEntityManager.worldItemEntities.Remove(this);

            VoxelWorld.currentWorld.itemEntityManager.itemEntityPool.Release(gameObject);
        }
    }

    void OnEnable()
    {
        
        itemMesh = new Mesh();
        VoxelWorld.currentWorld.itemEntityManager.worldItemEntities.Add(this);
        lastItemPos = transform.position;
        itemInWorldID = VoxelWorld.currentWorld.worldID;
        rb.constraints = RigidbodyConstraints.None;
        
    }

    public void BuildItemModel()
    {
        itemMesh = new Mesh();
        float x = -0.5f;
        float y = -0.5f;
        float z = -0.5f;
        /*    NativeList<Vector3> verts = new NativeList<Vector3>(Allocator.Temp);
            NativeList<Vector2> uvs = new NativeList<Vector2>(Allocator.Temp);
            NativeList<int> tris = new NativeList<int>(Allocator.Temp);
            NativeList<Vector3> norms = new NativeList<Vector3>(Allocator.Temp);

             ItemEntityMeshBuildingHelper.BuildItemMesh(itemID, ref verts, ref uvs, ref tris, ref norms);
             NativeArray<Vector3> vertsArray = verts.ToArray(Allocator.Temp);
             NativeArray<Vector2> uvsArray = uvs.ToArray(Allocator.Temp);
             NativeArray<int> trisArray = tris.ToArray(Allocator.Temp);
             NativeArray<Vector3> normsArray = norms.ToArray(Allocator.Temp);
             itemMesh.SetVertices(vertsArray);
             itemMesh.SetNormals(normsArray);
             itemMesh.SetUVs(0, uvsArray);
             itemMesh.SetIndices(trisArray, MeshTopology.Triangles, 0);
             itemMesh.RecalculateBounds();

             itemMesh.RecalculateTangents();
             vertsArray.Dispose();
             uvsArray.Dispose();
             trisArray.Dispose();
             normsArray.Dispose();
             verts.Dispose();
             norms.Dispose();
             tris.Dispose();
             uvs.Dispose();*/
        ItemEntityMeshBuildingHelper.BuildItemMesh(ref itemMesh, itemID);
        mc.sharedMesh = itemMesh;
        mf.mesh = itemMesh;
    }
    /*
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
    */

    public Chunk currentChunk;
   // public static Dictionary<int,GameObject> worldEntityTypes=new Dictionary<int,GameObject>(); 
   
   
    public static bool isItemEntitiesLoad=false;
    [Obsolete]
    public static string gameWorldItemEntityDataPath;
    [Obsolete]
    public static List<ItemEntityData> itemEntityDataReadFromDisk=new List<ItemEntityData>();
    [Obsolete]
    public static List<ItemEntityBeh> worldItemEntities=new List<ItemEntityBeh>();
    public static bool isItemEntitiesReadFromDisk=false;
    public static bool isItemEntitySavedInDisk=false;
    public string guid;
    public static bool isWorldItemEntityDataSaved=false;
    public bool isInUnloadedChunks=false;


    public void OnDisable()
    {
        rb.velocity = new Vector3(0, 0, 0);
        lastItemPos=transform.position;
   

        lifeTime=0f;
        isPosInited=false;

        itemID=0;
        

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


    public void OnDestroy()
    {
        DestroyImmediate(itemMesh, true);
        DestroyImmediate(GetComponent<MeshFilter>().mesh, true);
     //   RemoveItemEntityFromSave(ref VoxelWorld.currentWorld.itemEntityManager.itemEntityDataReadFromDisk);
      //  VoxelWorld.currentWorld.itemEntityManager.worldItemEntities.Remove(this);
        currentChunk = null;
    }
    [Obsolete]
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
    itemEntityDataReadFromDisk=MessagePackSerializer.Deserialize<List<ItemEntityData>>(worldItemEntitiesData);
  }
        Debug.Log("item count:"+itemEntityDataReadFromDisk.Count);
            //isEntitiesReadFromDisk=true;
    }
    [Obsolete]
    public void RemoveItemEntityFromSave(){
        ItemEntityData tmpData =new ItemEntityData(itemID,itemCount,lastItemPos.x,lastItemPos.y,lastItemPos.z,this.guid,lifeTime, itemInWorldID);
        tmpData.guid=this.guid;
        for(int i=0;i<itemEntityDataReadFromDisk.Count;i++){
            ItemEntityData ed =itemEntityDataReadFromDisk[i];
            if(ed.guid==this.guid){
                itemEntityDataReadFromDisk.RemoveAt(i);
                break;
            }
        }
    }
    [Obsolete]
    public void SaveSingleItemEntity(){
   //     Debug.Log(this.guid);
        lastItemPos=transform.position;
        ItemEntityData tmpData =new ItemEntityData(itemID,itemCount,lastItemPos.x,lastItemPos.y,lastItemPos.z,this.guid,lifeTime, itemInWorldID);
       // tmpData.guid=this.guid;
        foreach(ItemEntityData ed in itemEntityDataReadFromDisk){
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

    public void SaveSingleItemEntity(ref List<ItemEntityData> targetItemEntityDatas)
    {
         
        lastItemPos = transform.position;
        ItemEntityData tmpData = new ItemEntityData(itemID, itemCount, lastItemPos.x, lastItemPos.y, lastItemPos.z, this.guid, lifeTime, itemInWorldID);
 
        foreach (ItemEntityData ed in targetItemEntityDatas)
        {
            if (ed.guid == this.guid)
            {
                //   Debug.Log("dupli");
                targetItemEntityDatas.Remove(ed);
                break;
            }
        }

        targetItemEntityDatas.Add(tmpData);
    }

    public void RemoveItemEntityFromSave(ref List<ItemEntityData> targetItemEntityDatas)
    {
        ItemEntityData tmpData = new ItemEntityData(itemID, itemCount, lastItemPos.x, lastItemPos.y, lastItemPos.z, this.guid, lifeTime, itemInWorldID);
        tmpData.guid = this.guid;
        for (int i = 0; i < targetItemEntityDatas.Count; i++)
        {
            ItemEntityData ed = targetItemEntityDatas[i];
            if (ed.guid == this.guid)
            {
                targetItemEntityDatas.RemoveAt(i);
                break;
            }
        }
    }
    [Obsolete]
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
    
        byte[] tmpData=MessagePackSerializer.Serialize(itemEntityDataReadFromDisk);
        File.WriteAllBytes(gameWorldItemEntityDataPath+"unityMinecraftData/GameData/worlditementities.json",tmpData);
     
    }
  /*  public static async void SpawnNewItem(float posX,float posY,float posZ,int itemID,Vector3 startingSpeed){
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

            ItemEntityData ed =itemEntityDataReadFromDisk[i];
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
       
    }*/
  public void AddForceInvoke(Vector3 f)
  {
     

     
      if (rb != null)
      {
          rb.AddForce(f,ForceMode.Impulse);
      }
  }


  public void StartBuildItem()
  {
      BuildItemModel();
  }
  void Awake()
  {
      //  worldEntities.Add(this);
      /*   if(isFlatItemInfoAdded==false){
              AddFlatItemInfo();
          }*/
      rb = GetComponent<Rigidbody>();
      mc = GetComponent<MeshCollider>();
      mf = GetComponent<MeshFilter>();
  }

 

  void Update()
  {
      lifeTime += Time.deltaTime;
      if (lifeTime >= 60f)
      {
          ReleaseItem();
          return;
      }
  }
  /* void PlayerEatItem(){

   if(Mathf.Abs(playerPos.position.x-transform.position.x)<1f&&Mathf.Abs(playerPos.position.y-transform.position.y)<2f&&Mathf.Abs(playerPos.position.z-transform.position.z)<1f&&lifeTime>0.5f){
        if( playerPos.gameObject.GetComponent<PlayerMove>().CheckInventoryIsFull(itemID)==true){
           return;
       }
       if(playerPos.gameObject.GetComponent<PlayerMove>().isDied==true){
           return;
       }

   }
   }*/

  public void TryEatItem(IInventoryOwner owner)
  {
      if (lifeTime < 0.5f)
      {
          return;
      }

      if (owner.inventory.CheckInventoryIsFull(itemID) == true)
      {
          return;
      }

      GlobalAudioResourcesManager.PlayClipAtPointCustomRollOff(
          GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("itemPopClip"),
          transform.position, 1f, 40f);
      owner.inventory.AddItem(itemID, 1);

      ReleaseItem();
  }

  void FixedUpdate()
  {
      if (currentChunk == null)
      {
          currentChunk = WorldUpdateablesMediator.instance.GetChunk(transform.position);
      }

      if (!WorldUpdateablesMediator.instance.CheckIsPosInChunk(transform.position, currentChunk))
      {
          currentChunk = WorldUpdateablesMediator.instance.GetChunk(transform.position);
      }


      if (transform.position.y < -40f)
      {
          ReleaseItem();
          return;
      }

      curBlockOnItemID = WorldUpdateablesMediator.instance.GetBlockData(transform.position, currentChunk).blockID;
      if (curBlockOnItemID == 100)
      {
          rb.AddForce(new Vector3(0f, 20f, 0f));
      }

      if (curBlockOnItemID > 0 && curBlockOnItemID < 100)
      {
          rb.AddForce(new Vector3(0f, 20f, 0f));
      }

      if (prevBlockOnItemID != curBlockOnItemID)
      {
          if (curBlockOnItemID == 100)
          {
              rb.AddForce(new Vector3(0, 2f, 0f));
              GlobalAudioResourcesManager.PlayClipAtPointCustomRollOff(
                  GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("entitySinkClip1"),
                  transform.position, 1f, 40f);
              ParticleEffectManagerBeh.instance.EmitWaterSplashParticleAtPosition(transform.position);
          }
          else
          {
              //    rb.linearDamping=0f;
          }
      }

      prevBlockOnItemID = curBlockOnItemID;


      if (currentChunk == null||currentChunk?.isColliderBuildingCompleted==false)
      {
          rb.constraints = RigidbodyConstraints.FreezeAll;
          isInUnloadedChunks = true;
      }
      else
      {
          rb.constraints = RigidbodyConstraints.None;
          isInUnloadedChunks = false;
      }
  }


  
     public static Dictionary<int, ItemMeshBuildingInfo> itemMaterialInfo=new Dictionary<int, ItemMeshBuildingInfo>();
    public static Dictionary<int,Vector2Int> itemTexturePosInfo=new Dictionary<int,Vector2Int>();
    //151diamond pickaxe 152diamond sword 153diamond 154rotten flesh 155gun powder 156tnt 157bow 158armor upgrade
    public static Texture2D itemTexture;
//    public List<Vector3> verts=new List<Vector3>();
  //  public List<Vector2> uvs=new List<Vector2>();
  //  public List<int> tris=new List<int>();
    public static bool isFlatItemInfoAdded=false;
   // public Texture2D texture;
    public static int textureXSize=64;
    public static int textureYSize=64;
    public static int fullTextureXSize = 1024;
    public static int fullTextureYSize = 1024;
    //   public Mesh itemMesh;

    [Obsolete]
    public static void AddFlatItemInfo(){


        itemMaterialInfo.TryAdd(154, new ItemMeshBuildingInfo(false, new Vector2(0.1875f, 0.25f),new Vector2(0.0625f,0.0625f)));
        itemMaterialInfo.TryAdd(153, new ItemMeshBuildingInfo(false, new Vector2(0.125f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(151, new ItemMeshBuildingInfo(false, new Vector2(0.0625f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(152, new ItemMeshBuildingInfo(false, new Vector2(0.0f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(155, new ItemMeshBuildingInfo(false, new Vector2(0.25f, 0.25f), new Vector2(0.0625f, 0.0625f)));
      
        itemMaterialInfo.TryAdd(157, new ItemMeshBuildingInfo(false, new Vector2(0.5f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(158, new ItemMeshBuildingInfo(false, new Vector2(0.5625f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(104, new ItemMeshBuildingInfo(false, new Vector2(0.625f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(1, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(2, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(3, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(4, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(5, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(6, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(7, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(8, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(9, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(10, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(11, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(12, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(13, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(14, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(15, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(16, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(17, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(18, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(19, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(20, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(21, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(22, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(23, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(100, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(101, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(102, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(103, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(104, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(105, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(106, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(107, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(108, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(109, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(110, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(111, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(156, new ItemMeshBuildingInfo(true));
        itemTexturePosInfo.TryAdd(154,new Vector2Int(192, 256));
        itemTexturePosInfo.TryAdd(155, new Vector2Int(256, 256));
        itemTexturePosInfo.TryAdd(156, new Vector2Int(320, 256));
        itemTexturePosInfo.TryAdd(157, new Vector2Int(512, 256));
        itemTexturePosInfo.TryAdd(158, new Vector2Int(576, 256));
        itemTexturePosInfo.TryAdd(153,new Vector2Int(128, 256));
        itemTexturePosInfo.TryAdd(104, new Vector2Int(640, 256));
        itemTexturePosInfo.TryAdd(152,new Vector2Int(0, 256));
        itemTexturePosInfo.TryAdd(151,new Vector2Int(64, 256));
        itemTexture=Resources.Load<Texture2D>("Textures/itemterrain");
   //     itemTextureInfo.Add(0,Resources.Load<Texture2D>("Textures/diamond_pickaxe"));
      //  itemTextureInfo.Add(1,Resources.Load<Texture2D>("Textures/diamond_sword"));
        isFlatItemInfoAdded=true;
    }

    /*public async Task BuildFlatItemModel(int itemID,Mesh mesh)
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
    
    }*/




}
