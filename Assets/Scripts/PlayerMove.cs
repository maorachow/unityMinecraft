using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.IO;
using Utf8Json;
public class PlayerData{
    public float posX;
    public float posY;
    public float posZ;
    public int[] inventoryDic;
    public int[] inventoryItemNumberDic;
}
public class PlayerMove : MonoBehaviour
{
    public static RuntimePlatform platform{get{return EntityBeh.platform;}set{platform=EntityBeh.platform;}}
    public static string gameWorldPlayerDataPath;
    public static Dictionary<int,string> blockNameDic=new Dictionary<int,string>();
    public static bool isBlockNameDicAdded=false;
    public int blockOnHandID=0;
    public static bool isPaused=false;
    public GameObject prefabBlockOutline;
    public GameObject blockOutline;
    public GameObject collidingBlockOutline;
    public Transform cameraPos;
    public Text blockOnHandText;
    public Camera mainCam;
    public CharacterController cc;
    public float cameraX;
    public float breakBlockCD=0.2f;
    public float moveSpeed=5f;
    public float gravity=-9.8f;
    public float playerY=0f;
    public float jumpHeight=2f;
    public float mouseSens=10f;
    public Vector3 playerVec;
    public Chunk chunkPrefab;
    public int currentSelectedHotbar;
    public int[] inventoryDic=new int[9];
    public int[] inventoryItemNumberDic=new int[9];
    public static float viewRange=32;
    public GameObject pauseMenu;
    void Awake(){
        if(isBlockNameDicAdded==false){
        blockNameDic.Add(0,"None");
        blockNameDic.Add(1,"Stone");
        blockNameDic.Add(2,"Grass");
        blockNameDic.Add(3,"Dirt");
        blockNameDic.Add(4,"Side Grass Block");
        blockNameDic.Add(5,"Bedrock");
        blockNameDic.Add(6,"WoodX");
        blockNameDic.Add(7,"WoodY");
        blockNameDic.Add(8,"WoodZ");
        blockNameDic.Add(9,"Leaves");
        isBlockNameDicAdded=true;
        }
        ReadPlayerJson();
    }
    void Start()
    {   

        Application.targetFrameRate = 1024;
        prefabBlockOutline=Resources.Load<GameObject>("Prefabs/blockoutline");
        blockOutline=Instantiate(prefabBlockOutline,transform.position,transform.rotation);
        collidingBlockOutline=Instantiate(prefabBlockOutline,transform.position,transform.rotation);
        pauseMenu=GameObject.Find("pausemenuUI");
        blockOnHandText=GameObject.Find("blockonhandIDtext").GetComponent<Text>();
        pauseMenu.SetActive(false);
        viewRange=64;
        cc=GetComponent<CharacterController>();
        cameraPos=transform.GetChild(0);
        mainCam=cameraPos.gameObject.GetComponent<Camera>();
        chunkPrefab=Resources.Load<Chunk>("Prefabs/chunk");
      //  InvokeRepeating("SendChunkReleaseMessage",1f,3f);
    }


  //  void SendChunkReleaseMessage(){
  //      foreach(var c in Chunk.Chunks){
   //         c.Value.SendMessage("TryReleaseChunk");
  //      }
  //  }
    void OnApplicationFocus(bool focus)
    {
    Cursor.lockState = CursorLockMode.Locked;
    }

    void AddItem(int itemTypeID,int itemCount){
       // inventoryDic[0]=1;
      //  inventoryItemNumberDic[0]=100;
        int itemCountTmp=itemCount;
        for(int i=0;i<inventoryDic.Length;i++){
            if(inventoryItemNumberDic[i]==64){
                continue;
            }else if(inventoryDic[i]==0||inventoryDic[i]==itemTypeID&&inventoryItemNumberDic[i]<64){
                inventoryDic[i]=itemTypeID;

                while(inventoryItemNumberDic[i]<64){
  
                       inventoryItemNumberDic[i]+=1;
                    itemCountTmp--;
                    if(itemCountTmp==0){
                        return;
                    } 
                }
              
                  
                 continue;   
                    
                
                
            }

        }
    }
    void Update()
    {      
       
        if(Input.GetKeyDown(KeyCode.K)){
             AddItem(2,10);
             AddItem(3,10);
             AddItem(4,10);
             AddItem(5,10);
             AddItem(6,10);
        }
        if(Input.GetKeyDown(KeyCode.U)){
                Chunk.SaveWorldData();
        }
        if(Input.GetKey(KeyCode.LeftShift)){
            moveSpeed=10f;
        }else{
            moveSpeed=5f;
        }
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)&&info.collider.gameObject.tag!="Entity"){
           
        
            blockOutline.GetComponent<MeshRenderer>().enabled=true;
           
                
            
        
        collidingBlockOutline.GetComponent<MeshRenderer>().enabled=false;
       
        Vector3 blockPoint=info.point+cameraPos.forward*0.01f;
        Vector3 blockPoint2=info.point-cameraPos.forward*0.01f;
        blockOutline.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPoint).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint).z+0.5f);
        collidingBlockOutline.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPoint2).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint2).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint2).z+0.5f);
        }else if(Physics.Raycast(ray,out info,10f)&&info.collider.gameObject.tag=="Entity"){
            Vector3 blockPoint=info.point+cameraPos.forward*0.01f;
        Vector3 blockPoint2=info.point-cameraPos.forward*0.01f;
            collidingBlockOutline.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPoint2).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint2).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint2).z+0.5f);
            blockOutline.GetComponent<MeshRenderer>().enabled=false;
            collidingBlockOutline.GetComponent<MeshRenderer>().enabled=false;
        }else{
             blockOutline.GetComponent<MeshRenderer>().enabled=false;
            collidingBlockOutline.GetComponent<MeshRenderer>().enabled=false;
        }
           // Debug.Log(Input.GetAxis("Mouse ScrollWheel"));
       //     blockOnHandID+=(int)(Input.GetAxis("Mouse ScrollWheel")*15f);
            currentSelectedHotbar+=(int)(Input.GetAxis("Mouse ScrollWheel")*15f);
            currentSelectedHotbar=Mathf.Clamp(currentSelectedHotbar,1,9);
        //    blockOnHandID=Mathf.Clamp(blockOnHandID,0,9);
            blockOnHandText.text=blockNameDic[inventoryDic[currentSelectedHotbar-1]];
       
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            if(isPaused==false){
                PauseGame();
            }else{
                Resume();
            }
        }
        if(isPaused==true){
            return;
        }
        if(cc.isGrounded!=true){
            playerY+=gravity*Time.deltaTime;
        }else{
            playerY=0f;
        }
        if(cc.isGrounded==true&&Input.GetButton("Jump")){
            playerY=jumpHeight;
        }
        float mouseX=Input.GetAxis("Mouse X")*mouseSens;
        float mouseY=Input.GetAxis("Mouse Y")*mouseSens;

        mouseY=Mathf.Clamp(mouseY,-90f,90f);
        cameraX-=mouseY;
        cameraX=Mathf.Clamp(cameraX,-90f,90f);
        playerVec=new Vector3(Input.GetAxis("Vertical"),0f,Input.GetAxis("Horizontal"));
        playerVec.y=playerY;
        transform.eulerAngles+=new Vector3(0f,mouseX,0f);
        cameraPos.localEulerAngles=new Vector3(cameraX,0f,0f);
        cc.Move((transform.forward*playerVec.x+transform.right*playerVec.z+new Vector3(0f,playerVec.y,0f))*moveSpeed*Time.deltaTime);
        if(breakBlockCD>0f){
            breakBlockCD-=Time.deltaTime;
        }
        if(Input.GetMouseButton(0)&&breakBlockCD<=0f){
            BreakBlock();
            breakBlockCD=0.15f;
        }
        if(Input.GetMouseButton(1)&&breakBlockCD<=0f){
            PlaceBlock();
            breakBlockCD=0.15f;
        }
       
    }
    void FixedUpdate(){
         UpdateWorld();
         UpdateInventory();
    }
    void UpdateInventory(){
        for(int i=0;i<inventoryItemNumberDic.Length;i++){
        inventoryItemNumberDic[i]=Mathf.Clamp(inventoryItemNumberDic[i],0,64);
       }
        for(int i=0;i<inventoryDic.Length;i++){
            if(inventoryItemNumberDic[i]<=0){
                inventoryDic[i]=0;
            }
        
       }
    }
    void UpdateWorld()
    {
     
    for (float x = transform.position.x - viewRange; x < transform.position.x + viewRange; x += Chunk.chunkWidth)
        {
            for (float z = transform.position.z - viewRange; z < transform.position.z + viewRange; z += Chunk.chunkWidth)
            {
                Vector3 pos = new Vector3(x, 0, z);
                pos.x = Mathf.Floor(pos.x / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
                pos.z = Mathf.Floor(pos.z / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
                Vector2Int chunkPos=Chunk.Vec3ToChunkPos(pos);
                Chunk chunk = Chunk.GetChunk(chunkPos);
                if (chunk != null) {continue;}else{
                    chunk=ObjectPools.chunkPool.Get(chunkPos).GetComponent<Chunk>();
               //     chunk.transform.position=new Vector3(chunkPos.x,0,chunkPos.y);
               //     chunk.isChunkPosInited=true;
                    chunk.SendMessage("ReInitData");
         //          WorldManager.chunksToLoad.Add(chunk);
                }
            }
        }
     
    }

    
    void BreakBlock(){
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)){
            Vector3 blockPoint=info.point+cameraPos.forward*0.01f;
            int tmpID=Chunk.GetBlock(blockPoint);
            Chunk.SetBlockByHand(blockPoint,0);
            GameObject a=ObjectPools.particleEffectPool.Get();

            a.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPoint).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint).z+0.5f);
            a.GetComponent<particleAndEffectBeh>().blockID=tmpID;
            a.GetComponent<particleAndEffectBeh>().SendMessage("EmitParticle");
           Vector3Int intPos=new Vector3Int(Chunk.FloatToInt(blockPoint.x),Chunk.FloatToInt(blockPoint.y),Chunk.FloatToInt(blockPoint.z));
        Chunk chunkNeededUpdate=Chunk.GetChunk(Chunk.Vec3ToChunkPos(blockPoint));
        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(chunkNeededUpdate.transform.position);
     //   chunkNeededUpdate.BFSInit(chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z,7,0);
        }
    }

    void PlaceBlock(){
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)&&info.collider.gameObject.tag!="Entity"){
            Vector3 blockPoint=info.point-cameraPos.forward*0.01f;
            if(collidingBlockOutline.GetComponent<BlockOutlineBeh>().isCollidingWithPlayer==true||collidingBlockOutline.GetComponent<BlockOutlineBeh>().isCollidingWithEntity==true){
               return;
            }
           
            Chunk.SetBlockByHand(blockPoint,inventoryDic[currentSelectedHotbar-1]);
            inventoryItemNumberDic[currentSelectedHotbar-1]--;
        }
    }
    
    void PauseGame(){
        isPaused=true;
        Time.timeScale=0;
        pauseMenu.SetActive(true);
    }
    void Resume(){
        isPaused=false;
        Time.timeScale=1;
        pauseMenu.SetActive(false);
    }
    public void ReadPlayerJson(){
     if(platform==RuntimePlatform.WindowsPlayer||platform==RuntimePlatform.WindowsEditor){
        gameWorldPlayerDataPath="C:/";
      }else{
        gameWorldPlayerDataPath=Application.persistentDataPath;
      }
         
         if (!Directory.Exists(gameWorldPlayerDataPath+"unityMinecraftData")){
                Directory.CreateDirectory(gameWorldPlayerDataPath+"unityMinecraftData");
               
            }
          if(!Directory.Exists(gameWorldPlayerDataPath+"unityMinecraftData/GameData")){
                    Directory.CreateDirectory(gameWorldPlayerDataPath+"unityMinecraftData/GameData");
                }
       
        if(!File.Exists(gameWorldPlayerDataPath+"unityMinecraftData"+"/GameData/playerdata.json")){
            File.Create(gameWorldPlayerDataPath+"unityMinecraftData"+"/GameData/playerdata.json");
        }
       
        string[] worldPlayerData=File.ReadAllLines(gameWorldPlayerDataPath+"unityMinecraftData/GameData/playerdata.json");
         //   isEntitiesReadFromDisk=true;

          PlayerData pd=new PlayerData();
         if(worldPlayerData.Length>0){
                pd=JsonSerializer.Deserialize<PlayerData>(worldPlayerData[0]);  
         }
         
         if(pd.posX!=0f&&pd.posY!=0f&&pd.posZ!=0f&&pd.inventoryDic!=null&&pd.inventoryItemNumberDic!=null){
           transform.position=new Vector3(pd.posX,pd.posY,pd.posZ); 
           inventoryDic=pd.inventoryDic;
           inventoryItemNumberDic=pd.inventoryItemNumberDic;
         }else{
            transform.position=new Vector3(0f,150f,0f);
            inventoryDic=new int[9];
            inventoryItemNumberDic=new int[9];

         }
         
    }
    public void SavePlayerData(){
        PlayerData pd=new PlayerData();
        pd.posX=transform.position.x;
        pd.posY=transform.position.y;
        pd.posZ=transform.position.z;
        pd.inventoryDic=inventoryDic;
        pd.inventoryItemNumberDic=inventoryItemNumberDic;
        FileStream fs;
        if (File.Exists(gameWorldPlayerDataPath+"unityMinecraftData/GameData/playerdata.json"))
        {
                 fs = new FileStream(gameWorldPlayerDataPath+"unityMinecraftData/GameData/playerdata.json", FileMode.Truncate, FileAccess.Write);//Truncate模式打开文件可以清空。
        }
        else
        {
                 fs = new FileStream(gameWorldPlayerDataPath+"unityMinecraftData/GameData/playerdata.json", FileMode.Create, FileAccess.Write);
        }
        fs.Close();
        string tmpData=JsonSerializer.ToJsonString(pd);
        File.AppendAllText(gameWorldPlayerDataPath+"unityMinecraftData/GameData/playerdata.json",tmpData+"\n");
    }
}
