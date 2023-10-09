using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using System.Threading.Tasks;
using System.Threading;
using MessagePack;
[MessagePackObject]
public class ItemData{
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
    public ItemData(int itemID,int itemCount,float posX,float posY,float posZ,string guid,float lifeTime){
        this.itemID=itemID;
         this.itemCount=itemCount;
          this.posX=posX;
           this.posY=posY;
            this.posZ=posZ;
             this.guid=guid;
             this.lifeTime=lifeTime;
    }
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

    public async void BuildItemModel(){
    itemMesh=new Mesh();
    float x=-0.5f;
    float y=-0.5f;
    float z=-0.5f;
    verts=new List<Vector3>();
    uvs=new List<Vector2>();
    tris=new List<int>();
    if(itemID>150&&itemID<=200){
      //  itemMesh=new Mesh();
        await BuildFlatItemModel(itemID,itemMesh);
           mf.mesh=itemMesh;
        mc.sharedMesh=itemMesh;
    }
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
        
            //isEntitiesReadFromDisk=true;
    }
    public void RemoveItemEntityFromSave(){
      ItemData tmpData=new ItemData(itemID,itemCount,transform.position.x,transform.position.y,transform.position.z,this.guid,lifeTime);
        tmpData.guid=this.guid;
        foreach(ItemData ed in itemEntityDataReadFromDisk){
            if(ed.guid==this.guid){
                itemEntityDataReadFromDisk.Remove(ed);
                break;
            }
        }
    }
    public void SaveSingleItemEntity(){
   //     Debug.Log(this.guid);
        ItemData tmpData=new ItemData(itemID,itemCount,transform.position.x,transform.position.y,transform.position.z,this.guid,lifeTime);
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
    public static IEnumerator SpawnNewItem(float posX,float posY,float posZ,int itemID,Vector3 startingSpeed){
                GameObject a=ObjectPools.itemEntityPool.Get(new Vector3(posX,posY,posZ));
                ItemEntityBeh tmp=a.GetComponent<ItemEntityBeh>();
                
                tmp.itemID=itemID;
                tmp.guid=System.Guid.NewGuid().ToString("N");
                 tmp.SendMessage("InitPos");
             //    a.transform.position=new Vector3(posX,posY,posZ);
                 yield return new WaitForSeconds(0.1f);
                 tmp.AddForceInvoke(startingSpeed);
                 yield break;
        }


    public static IEnumerator SpawnItemEntityFromFile(){
        for(int i=0;i<itemEntityDataReadFromDisk.Count;i++){
          //  Debug.Log(ed.guid);
          ItemData ed=itemEntityDataReadFromDisk[i];
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
        Invoke("InvokeInitPos",0.03f);
    
    }
    public void InvokeInitPos(){
        isPosInited=true;
        BuildItemModel();
    }
    void Awake(){
      //  worldEntities.Add(this);
       if(isFlatItemInfoAdded==false){
            AddFlatItemInfo();
        }
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
       playerPos.gameObject.GetComponent<PlayerMove>().playerHandItem.BuildItemModel(playerPos.gameObject.GetComponent<PlayerMove>().inventoryDic[playerPos.gameObject.GetComponent<PlayerMove>().currentSelectedHotbar-1]);
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
        if(currentChunk==null||currentChunk.isMeshBuildCompleted==false||currentChunk.isStrongLoaded==false||currentChunk.meshCollider.sharedMesh==null){
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            isInUnloadedChunks=true;
        }else{
             GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
             isInUnloadedChunks=false;
        }
    }



     public static Dictionary<int,Vector2> itemMaterialInfo=new Dictionary<int,Vector2>();
    public static Dictionary<int,Vector2Int> itemTexturePosInfo=new Dictionary<int,Vector2Int>();
    //151diamond pickaxe 152diamond sword 153diamond
    public static Texture2D itemTextureInfo;
//    public List<Vector3> verts=new List<Vector3>();
  //  public List<Vector2> uvs=new List<Vector2>();
  //  public List<int> tris=new List<int>();
    public static bool isFlatItemInfoAdded=false;
   // public Texture2D texture;
    public static int textureXSize=64;
    public static int textureYSize=64;
 //   public Mesh itemMesh;
    void AddFlatItemInfo(){
        itemMaterialInfo.Add(153,new Vector2(0.125f,0.125f));
        itemMaterialInfo.Add(151,new Vector2(0.0625f,0.125f));
        itemMaterialInfo.Add(152,new Vector2(0.0f,0.125f));
        itemTexturePosInfo.Add(153,new Vector2Int(128,128));
        itemTexturePosInfo.Add(152,new Vector2Int(0,128));
        itemTexturePosInfo.Add(151,new Vector2Int(64,128));
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
                if(i+1<textureXSize&&i-1>=0&&j+1<textureYSize&&j-1>=0){
                    if(itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j).a!=0f&&itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i+1,itemTexturePosInfo[itemID].y+j).a==0f){
                        //right
                        BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+(-0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+(-0.00001f), (float)0.0625f*0.0625f*0.25f,new Vector3(x+i + 1, y, z+j)/4/16, Vector3.up/16, Vector3.forward/4/16, true, verts, uvs, tris);

                    }
                    if(itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j).a!=0f&&itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i-1,itemTexturePosInfo[itemID].y+j).a==0f){
                        //left
                        BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+(-0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+(-0.00001f), (float)0.0625f*0.0625f*0.25f,new Vector3(x+i, y, z+j)/4/16, Vector3.up/16, Vector3.forward/4/16, false, verts, uvs, tris);

                    }
                    if(itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j).a!=0f&&itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j+1).a==0f){
                        //front
                        BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+(-0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+(-0.00001f), (float)0.0625f*0.0625f*0.25f,new Vector3(x+i, y, z+j + 1)/4/16, Vector3.up/16, Vector3.right/4/16, false, verts, uvs, tris);

                    }
                    if(itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j).a!=0f&&itemTextureInfo.GetPixel(itemTexturePosInfo[itemID].x+i,itemTexturePosInfo[itemID].y+j-1).a==0f){
                        //back
                        BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+(-0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+(-0.00001f),  (float)0.0625f*0.0625f*0.25f,new Vector3(x+i,y, z+j)/4/16, Vector3.up/16, Vector3.right/4/16, true, verts, uvs, tris);

                    }
                }else{
                    if(i+1>=textureXSize){
                          BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+(-0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+(-0.00001f), (float)1/16*0.0625f,new Vector3(x+i + 1, y,z+ j)/4/16, Vector3.up/16, Vector3.forward/4/16, true, verts, uvs, tris);
                    }
                    if(i-1<0){
                          BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+(-0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+(-0.00001f), (float)1/16*0.0625f,new Vector3(x+i, y, z+j)/4/16, Vector3.up/16, Vector3.forward/4/16, false, verts, uvs, tris);
                    }
                    if(j+1>=textureYSize){
                          BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+(-0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+(-0.00001f), (float)1/16*0.0625f,new Vector3(x+i, y, z+j + 1)/4/16, Vector3.up/16, Vector3.right/4/16, false, verts, uvs, tris);
                    }
                    if(j-1<0){
                         BuildFlatItemFace(itemMaterialInfo[itemID].x+(float)i/textureXSize*0.0625f+(-0.00001f),itemMaterialInfo[itemID].y+(float)j/textureYSize*0.0625f+(-0.00001f),  (float)1/16*0.0625f,new Vector3(x+i, y, z+j)/4/16, Vector3.up/16, Vector3.right/4/16, true, verts, uvs, tris);
                    }
                   
                }
                    
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
     
    }

void BuildFlatItemFace(float uvX,float uvY,float uvWidthXY,Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris){
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
