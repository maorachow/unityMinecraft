using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using UnityEngine.EventSystems;
using MessagePack;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Cysharp.Threading.Tasks;
using System;
using Random=UnityEngine.Random;
[MessagePackObject]
public class PlayerData{
    [Key(0)]
    public float playerHealth;
    [Key(1)]
    public float posX;
    [Key(2)]
    public float posY;
    [Key(3)]
    public float posZ;
    [Key(4)]
    public int[] inventoryDic;
    [Key(5)]
    public int[] inventoryItemNumberDic;
    public PlayerData(float playerHealth,float posX,float posY,float posZ,int[] inventoryDic,int[] inventoryItemNumberDic){
        this.playerHealth=playerHealth;
        this.posX=posX;
        this.posY=posY;
        this.posZ=posZ;
        this.inventoryDic=inventoryDic;
        this.inventoryItemNumberDic=inventoryItemNumberDic;
    }
}
public class PlayerMove : MonoBehaviour
{   
    public static AudioClip playerDropItemClip;
    public static AudioClip playerSweepAttackClip;
    public AudioSource AS;
    public float playerCameraZShakeValue;
    public float playerCameraZShakeLerpValue;
    public PlayerInput pi; 
    public float playerHealth=20f;
    public float playerMaxHealth=20f;
    public Chunk curChunk;
    public Animator am;
  
    public static string gameWorldPlayerDataPath;
    public static Dictionary<int,string> blockNameDic=new Dictionary<int,string>();
    public static bool isBlockNameDicAdded=false;
    public int blockOnHandID=0;
    public int cameraPosMode=0;//0fp 1sp 2tp
    public static GameObject playerSweepParticlePrefab;
    public GameObject prefabBlockOutline;
    public GameObject blockOutline;
    public GameObject collidingBlockOutline;
    public Transform cameraPos;
    public Transform headPos;
    public Transform playerMoveRef;
    public Transform playerBodyPos;
    public Text blockOnHandText;
    public Camera mainCam;
    public CharacterController cc;
    public float cameraX;
    public float critAttackCD=1f;
    public float breakBlockCD=0.2f;
    public float moveSpeed=5f;
    public float gravity=-9.8f;
    public float playerY=0f;
    public float jumpHeight=2f;
    public static float mouseSens=1f;
    public static float cameraFOV=90f;
    public float currentSpeed;
    public Vector3 playerVec;
    public Vector3 playerMotionVec;
    public bool isPlayerKilled=false;
    public Chunk chunkPrefab;
    public ItemOnHandBeh playerHandItem;
    public int currentSelectedHotbar=5;
    public int[] inventoryDic=new int[9];
    public int[] inventoryItemNumberDic=new int[9];
    public static float viewRange=32;
    public static float chunkStrongLoadingRange=48;
    //public static GameObject pauseMenu;
    public float lerpItemSlotAxis;
    public Vector3 lerpPlayerVec;
    public Vector2 playerChunkLoadingPos;
    public List<Chunk> chunksToStrongLoad=new List<Chunk>();
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
        blockNameDic.Add(11,"Sand");
        blockNameDic.Add(100,"Water");
        blockNameDic.Add(101,"Grass Crop");
        blockNameDic.Add(102,"Torch");
        blockNameDic.Add(151,"Diamond Pickaxe");
        blockNameDic.Add(152,"Diamond Sword");
        blockNameDic.Add(153,"Diamond");
        isBlockNameDicAdded=true;
        }
      
    }
    public void SetHotbarNum(int num){
        
        currentSelectedHotbar=num;
        if(blockOnHandText==null){
            blockOnHandText=GameObject.Find("blockonhandIDtext").GetComponent<Text>();
        }
        blockOnHandText.text=blockNameDic[inventoryDic[currentSelectedHotbar-1]];
    }
    public void BreakBlockButtonPress(){
        if(isPlayerKilled==true||GameUIBeh.isPaused==true||GameUIBeh.instance.isCraftingMenuOpened==true){
            return;
        }
        if(breakBlockCD<=0f){
            BreakBlock();
            breakBlockCD=0.3f;}
    }
    public void PlaceBlockButtonPress(){
          if(isPlayerKilled==true||GameUIBeh.isPaused==true||GameUIBeh.instance.isCraftingMenuOpened==true){
            return;
        }
        if(breakBlockCD<=0f){
            PlaceBlock();
            breakBlockCD=0.3f;}
    }

    void Start()
    {   
          ReadPlayerJson();
         pi=new PlayerInput();
        pi.Enable();
         Input.multiTouchEnabled = true;
        AS=GetComponent<AudioSource>();
        // pauseMenu.SetActive(true);
        currentSelectedHotbar=1;
        playerHandItem=transform.GetChild(0).GetChild(1).GetChild(1).GetChild(1).GetChild(0).gameObject.GetComponent<ItemOnHandBeh>();
        playerSweepAttackClip=Resources.Load<AudioClip>("Audios/Sweep_attack1");
        playerDropItemClip=Resources.Load<AudioClip>("Audios/Pop");
        prefabBlockOutline=Resources.Load<GameObject>("Prefabs/blockoutline");
        blockOutline=Instantiate(prefabBlockOutline,transform.position,transform.rotation);
        collidingBlockOutline=Instantiate(prefabBlockOutline,transform.position,transform.rotation);

        blockOnHandText=GameObject.Find("blockonhandIDtext").GetComponent<Text>();
        chunkStrongLoadingRange=64;
        viewRange=64;
        am=transform.GetChild(0).GetComponent<Animator>();
        cc=GetComponent<CharacterController>();
        headPos=transform.GetChild(0).GetChild(0);
        playerMoveRef=headPos.GetChild(1);
        playerBodyPos=transform.GetChild(0).GetChild(1);
        mainCam=headPos.GetChild(0).gameObject.GetComponent<Camera>();
        mainCam.fieldOfView=cameraFOV;
        chunkPrefab=Resources.Load<Chunk>("Prefabs/chunk");
      //  InvokeRepeating("SendChunkReleaseMessage",1f,3f);
      GameUIBeh.instance.CloseCraftingUI();
         GameUIBeh.instance.Resume();
        cameraPos=mainCam.transform;
                transform.GetChild(0).localRotation=Quaternion.Euler(0f,0f,0f);
        transform.GetChild(0).localPosition=new Vector3(0f,0f,0f);
        playerSweepParticlePrefab=Resources.Load<GameObject>("Prefabs/playersweepparticle");
    }


  //  void SendChunkReleaseMessage(){
  //      foreach(var c in Chunk.Chunks){
   //         c.Value.SendMessage("TryReleaseChunk");
  //      }
  //  }
    void MouseLock()
    {
        if(isPlayerKilled==true||GameUIBeh.isPaused==true||GameUIBeh.instance.isCraftingMenuOpened==true){
            return;
        }
        if(pi.Player.LeftClick.ReadValue<float>()>0.5f){
         Cursor.lockState = CursorLockMode.Locked;   
        }
    
    }


    public bool CheckInventoryIsFull(int itemID){
        for(int i=0;i<inventoryDic.Length;i++){
            if(inventoryDic[i]==0){
                return false;
            }
            if(inventoryDic[i]==itemID&&inventoryItemNumberDic[i]<64){
                return false;
            }
        }
        return true;
    }



    public void AddItem(int itemTypeID,int itemCount){
         playerHandItem.blockID=inventoryDic[currentSelectedHotbar-1];  
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
        for(int i=0;i<itemCount;i++){
         StartCoroutine(ItemEntityBeh.SpawnNewItem(headPos.position.x,headPos.position.y,headPos.position.z,itemTypeID,(headPos.forward*3)));
        }
     //   playerHandItem.SendMessage("OnBlockIDChanged",inventoryDic[currentSelectedHotbar-1]);  
    }

     float Speed()
	{
   
		return Vector3.Magnitude(new Vector3(cc.velocity.x,0f,cc.velocity.z));
	}

   
    void InvokeRevertColor(){
            transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<MeshRenderer>().material.color=Color.white;
            transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
            transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
            transform.GetChild(0).GetChild(1).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
            transform.GetChild(0).GetChild(1).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
            transform.GetChild(0).GetChild(1).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
    }
    public void ApplyDamageAndKnockback(float damageAmount,Vector3 knockback){
        AS.Play();
        //GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        playerCameraZShakeLerpValue=Random.Range(-15f,15f);
        playerHealth-=damageAmount;
         GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        playerMotionVec=knockback;
        transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<MeshRenderer>().material.color=Color.red;
            transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
            transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
            transform.GetChild(0).GetChild(1).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
            transform.GetChild(0).GetChild(1).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
            transform.GetChild(0).GetChild(1).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
            Invoke("InvokeRevertColor",0.2f);
    }
    public void PlayerDie(){
          AS.Play();
           playerCameraZShakeLerpValue=0f;
            playerCameraZShakeValue=0f;
          GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
      //   transform.position=new Vector3(0f,150f,0f);
        for(int i=0;i<inventoryDic.Length;i++){
                   while(inventoryItemNumberDic[i]>0){
                    PlayerDropItem(i);
                  
                }
        }
        Cursor.lockState = CursorLockMode.Confined;
          cc.enabled = false;
        transform.GetChild(0).localRotation=Quaternion.Euler(0f,0f,-90f);
        transform.GetChild(0).localPosition=new Vector3(0.65f,-0.68f,0f);
        cc.enabled = true;
         
        isPlayerKilled=true;
        am.SetBool("iskilled",true);
        am.SetFloat("speed",0f);
        RespawnUI.instance.gameObject.SetActive(true);
        
    }

//0diamond to pickaxe 1diamond to sword
    public void ExchangeItem(int exchangeID){
        if(exchangeID==0){
        if(GetItemFromSlot(153)==-1){
            return;
        }else{
            
          inventoryItemNumberDic[GetItemFromSlot(153)]--;
        AddItem(151,1);  
        }

        }else if(exchangeID==1){

        if(GetItemFromSlot(153)==-1){
            return;
        }else{
            
          inventoryItemNumberDic[GetItemFromSlot(153)]--;
        AddItem(152,1);  
        }

        }else if(exchangeID==2){
        if(GetItemFromSlot(7)==-1){
            return;
        }else{
            
          inventoryItemNumberDic[GetItemFromSlot(7)]--;
        AddItem(102,1);  
        }
        }
        
        
    }
    public int GetItemFromSlot(int itemID){
        for(int i=0;i<inventoryDic.Length;i++){
            if(inventoryDic[i]==itemID&&inventoryItemNumberDic[i]>0){
                    return i;
            }
        }
        return -1;
    }
    public void PlayerRespawn(){
            playerMotionVec=Vector3.zero;
        am.SetBool("iskilled",false);
         am.SetFloat("speed",0f);
        cc.enabled = false;
         transform.rotation=Quaternion.identity;
        transform.position = new Vector3(0f,150f,0f);
        headPos.rotation=Quaternion.identity;
        playerBodyPos.rotation=Quaternion.identity;
        transform.GetChild(0).rotation=Quaternion.Euler(0f,0f,0f);
        transform.GetChild(0).localPosition=new Vector3(0f,0f,0f);
        cc.enabled = true;
        
        
        playerHealth=20f;
        isPlayerKilled=false;
        GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        RespawnUI.instance.gameObject.SetActive(false);
         transform.position=new Vector3(0f,150f,0f);
    }

    public void PauseOrResume(){
       //     Debug.Log("Pause");
            Cursor.lockState = CursorLockMode.None;
            if(GameUIBeh.isPaused==false){
                GameUIBeh.instance.PauseGame();
                return;
            }else{
                GameUIBeh.instance.Resume();
                return;
            }
    }
    public void OpenOrCloseCraftingUI(){
         Cursor.lockState = CursorLockMode.None;  
          if(GameUIBeh.instance.isCraftingMenuOpened==true){
              GameUIBeh.instance.CloseCraftingUI();  
            }else{
            GameUIBeh.instance.OpenCraftingUI();  
            }
    }
    void Update()
    {     
      
          if(Input.GetKeyDown(KeyCode.Escape)){
            PauseOrResume();
          }
        
        if(GameUIBeh.isPaused==true){
            return;
        }
        if(Input.GetKeyDown(KeyCode.E)){
          
            OpenOrCloseCraftingUI();
          }
        playerCameraZShakeLerpValue=Mathf.Lerp(playerCameraZShakeLerpValue,0f,15f*Time.deltaTime);
       playerCameraZShakeValue=Mathf.Lerp(playerCameraZShakeLerpValue,0f,15f*Time.deltaTime);

        MouseLock();
        if(currentSelectedHotbar-1>=0&&currentSelectedHotbar-1<inventoryDic.Length){
        playerHandItem.blockID=inventoryDic[currentSelectedHotbar-1];    
        }
        
        if(playerHealth<=0f&&isPlayerKilled==false){
            PlayerDie();
        }
         if(transform.position.y<-40f&&isPlayerKilled==false){
            PlayerDie();
        }
        if(isPlayerKilled==true){
            return;
        }
        playerMotionVec=Vector3.Lerp(playerMotionVec,Vector3.zero, 3f * Time.deltaTime);
        curChunk=Chunk.GetChunk(Chunk.Vec3ToChunkPos(transform.position));
         if(curChunk==null||curChunk.isMeshBuildCompleted==false||curChunk.isStrongLoaded==false){
            return;
        }
          if(curChunk!=null&&(curChunk.isMeshBuildCompleted==false||curChunk.isStrongLoaded==false)){
            return;
        }
        currentSpeed=Speed();
        am.SetFloat("speed",currentSpeed);
     /*   if(Input.GetKeyDown(KeyCode.K)){
             AddItem(2,10);
             AddItem(3,10);
             AddItem(4,10);
             AddItem(5,10);
             AddItem(6,10);
        }*/
 //       if(Input.GetKeyDown(KeyCode.U)){
    //            Chunk.SaveWorldData();
   //     }
        if(pi.Player.SpeedUp.ReadValue<float>()>=0.5f){
            moveSpeed=10f;
        }else{
            moveSpeed=5f;
        }
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)&&info.collider.gameObject.tag!="Entity"){
           
        
            blockOutline.GetComponent<MeshRenderer>().enabled=true;
           
                
            
        
        collidingBlockOutline.GetComponent<MeshRenderer>().enabled=false;
       
        Vector3 blockPoint=info.point+headPos.forward*0.01f;
        Vector3 blockPoint2=info.point-headPos.forward*0.01f;
        blockOutline.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPoint).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint).z+0.5f);
        collidingBlockOutline.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPoint2).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint2).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint2).z+0.5f);
        }else if(Physics.Raycast(ray,out info,10f)&&info.collider.gameObject.tag=="Entity"){
            Vector3 blockPoint=info.point+headPos.forward*0.01f;
        Vector3 blockPoint2=info.point-headPos.forward*0.01f;
            collidingBlockOutline.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPoint2).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint2).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint2).z+0.5f);
            blockOutline.GetComponent<MeshRenderer>().enabled=false;
            collidingBlockOutline.GetComponent<MeshRenderer>().enabled=false;
        }else{
             blockOutline.GetComponent<MeshRenderer>().enabled=false;
            collidingBlockOutline.GetComponent<MeshRenderer>().enabled=false;
        }
        if(cameraPosMode==1){
            blockOutline.GetComponent<MeshRenderer>().enabled=false;
            collidingBlockOutline.GetComponent<MeshRenderer>().enabled=false;
        }
           // Debug.Log(Input.GetAxis("Mouse ScrollWheel"));
       //     blockOnHandID+=(int)(Input.GetAxis("Mouse ScrollWheel")*15f);
       
            pi.Player.SwitchItemSlot.performed+=ctx=>{ 
                lerpItemSlotAxis=pi.Player.SwitchItemSlot.ReadValue<Vector2>().y*0.5f;
            lerpItemSlotAxis=Mathf.Lerp(lerpItemSlotAxis,1.5f,Time.deltaTime);
            
                if(Mathf.Abs(pi.Player.SwitchItemSlot.ReadValue<Vector2>().y)>0f){
                currentSelectedHotbar-=(int)pi.Player.SwitchItemSlot.ReadValue<Vector2>().y;
                currentSelectedHotbar=Mathf.Clamp(currentSelectedHotbar,1,9);
                if(blockOnHandText==null){
                blockOnHandText=GameObject.Find("blockonhandIDtext").GetComponent<Text>();
                }
                    blockOnHandText.text=blockNameDic[inventoryDic[currentSelectedHotbar-1]];
        
          } };
           
          pi.Player.SwitchCameraPos.performed+=ctx=>{
            cameraPosMode++;
            if(cameraPosMode>=3){
                cameraPosMode=0;
            }
          };
          Ray playerHeadForwardRay=new Ray(headPos.position,headPos.position+headPos.forward*5f);
            Ray playerHeadBackRay=new Ray(headPos.position,headPos.position+headPos.forward*-5f);
        //    blockOnHandID=Mathf.Clamp(blockOnHandID,0,9);
       //     Debug.DrawLine(headPos.position,headPos.position+headPos.forward*5f,Color.green);
       //     Debug.DrawLine(headPos.position,headPos.position+headPos.forward*-5f,Color.green);
        RaycastHit infoForward=new RaycastHit();
        RaycastHit infoBack=new RaycastHit();
        bool isForwardPointHit=false;
        bool isBackPointHit=false;
        if(Physics.Linecast(headPos.position,headPos.position+headPos.forward*5f,out infoForward)){
            isForwardPointHit=true;
       //     Debug.DrawLine(infoForward.point,infoForward.point+new Vector3(0f,1f,0f),Color.green);
        }
        if(Physics.Linecast(headPos.position,headPos.position+headPos.forward*(-5f),out infoBack)){
            isBackPointHit=true;
          //   Debug.DrawLine(infoBack.point,infoBack.point+new Vector3(0f,1f,0f),Color.green);
        }
       
        switch(cameraPosMode){
            case 0:cameraPos.localPosition=new Vector3(0f,0.28f,-0.1f);cameraPos.localEulerAngles=new Vector3(0f,0f,0f);break;
            case 1: if(isForwardPointHit==true){
                cameraPos.position=Vector3.Lerp(headPos.position,infoForward.point,0.97f);
            }else{
                cameraPos.localPosition=new Vector3(0f,0f,5f)+new Vector3(0f,0.28f,-0.1f);
            }
            cameraPos.localEulerAngles=new Vector3(0f,-180f,0f);
            break;
            case 2:if(isBackPointHit==true){
                cameraPos.position=Vector3.Lerp(headPos.position,infoBack.point,0.97f);
            }else{
                cameraPos.localPosition=new Vector3(0f,0f,-5f)+new Vector3(0f,0.28f,-0.1f);
            }
            cameraPos.localEulerAngles=new Vector3(0f,0f,0f);
            break;
        }

        if(cc.isGrounded!=true){
            playerY+=gravity*Time.deltaTime;
        }else{
            playerY=0f;
           // playerMotionVec.y=0f;
        }
        if(cc.isGrounded==true&&pi.Player.Jump.ReadValue<float>()>=1f){
            playerY=jumpHeight;
        }


         float mouseX=0f;
        float mouseY=0f;
        if(!GameUIBeh.instance.isCraftingMenuOpened){
                 if(WorldManager.platform==RuntimePlatform.Android||WorldManager.platform==RuntimePlatform.IPhonePlayer){
            for(int i=0;i<Input.touches.Length;i++){
             EventSystem eventSystem = EventSystem.current;
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.touches[i].rawPosition;
            //射线检测ui
            List<RaycastResult> uiRaycastResultCache = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerEventData, uiRaycastResultCache);
            if (uiRaycastResultCache.Count == 0){
                 mouseX=Input.touches[i].deltaPosition.x*mouseSens;mouseY=Input.touches[i].deltaPosition.y*mouseSens;
            }
        }
        }else{
            mouseX=pi.Player.MouseDrag.ReadValue<Vector2>().x*mouseSens;mouseY=pi.Player.MouseDrag.ReadValue<Vector2>().y*mouseSens;
        }
        }
   
        
     
        
       

        mouseY=Mathf.Clamp(mouseY,-90f,90f);
        cameraX-=mouseY;
        cameraX=Mathf.Clamp(cameraX,-90f,90f);

          if(pi.Player.Move.ReadValue<Vector2>()!=Vector2.zero&&GameUIBeh.instance.isCraftingMenuOpened==false){
           playerVec=new Vector3(pi.Player.Move.ReadValue<Vector2>().y,0f,pi.Player.Move.ReadValue<Vector2>().x); 
        
           lerpPlayerVec=Vector3.Lerp(lerpPlayerVec,playerVec,7f*Time.deltaTime);
          }else{
            lerpPlayerVec=Vector3.Lerp(lerpPlayerVec,Vector3.zero,7f*Time.deltaTime);
          }
        
      
        playerVec.y=playerY;
        headPos.eulerAngles+=new Vector3(0f,mouseX,0f);
        headPos.localEulerAngles=new Vector3(cameraX,headPos.localEulerAngles.y,playerCameraZShakeValue);
        playerMoveRef.eulerAngles=new Vector3(0f,headPos.eulerAngles.y,headPos.eulerAngles.z);
        playerBodyPos.rotation=Quaternion.Slerp(playerBodyPos.rotation,playerMoveRef.rotation,5f*Time.deltaTime);
        cc.Move((playerMoveRef.forward*lerpPlayerVec.x+playerMoveRef.right*lerpPlayerVec.z)*moveSpeed*Time.deltaTime+new Vector3(0f,playerVec.y,0f)*5f*Time.deltaTime+playerMotionVec*Time.deltaTime);
        if(breakBlockCD>0f){
            breakBlockCD-=Time.deltaTime;
        }
        if(critAttackCD>0f){
        critAttackCD-=Time.deltaTime;    
        }
        
        pi.Player.DropItem.performed+=ctx=>{
            
            PlayerDropItem(currentSelectedHotbar-1);
         //    playerHandItem.BuildItemModel(inventoryDic[currentSelectedHotbar-1]);  
        };
    if (!EventSystem.current.IsPointerOverGameObject())
    {
          if(Input.touches.Length==1){
            pi.Player.LeftClick.performed+=ctx=>{ if(breakBlockCD<=0f){
            BreakBlock();
            breakBlockCD=0.3f;}};
            
             pi.Player.RightClick.performed+=ctx=>{ if(breakBlockCD<=0f){
            PlaceBlock();
            breakBlockCD=0.3f;}};
         


          }else{
            if(pi.Player.LeftClick.ReadValue<float>()>=1f&&breakBlockCD<=0f){
            BreakBlock();
            breakBlockCD=0.3f;
        }
        if(pi.Player.RightClick.ReadValue<float>()>=1f&&breakBlockCD<=0f){
            PlaceBlock();
            breakBlockCD=0.3f;
        }
    //    if(pi.Player.RightClick.ReadValue<float>()>=1f&&breakBlockCD<=0f){
     //       PlaceBlock();
      //      breakBlockCD=0.3f;
     //   }
          }
       
    }
       
    }
    void FixedUpdate(){
    // if(cc.velocity.magnitude>0.1f){
    //       UpdateWorld();
   //  }
  TryStrongLoadChunkThread();
     playerChunkLoadingPos=new Vector2(transform.position.x,transform.position.z);
         UpdateInventory();
        
        
    }
    public void DropItemButtonOnClick(){
        PlayerDropItem(currentSelectedHotbar-1);
    }
    void PlayerDropItem(int slotID){
        if(this==null){
           return; 
        }
        if(inventoryItemNumberDic[slotID]>0){
            AudioSource.PlayClipAtPoint(playerDropItemClip,gameObject.transform.position,1f);
            StartCoroutine(ItemEntityBeh.SpawnNewItem(headPos.position.x,headPos.position.y,headPos.position.z,inventoryDic[slotID],(headPos.forward*12)));
            inventoryItemNumberDic[slotID]--;
            if(inventoryItemNumberDic[slotID]-1<=0){
       
            }
  
                AttackAnimate();
                Invoke("cancelAttackInvoke",0.16f);
            }else{
          
        }
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
   public void TryUpdateWorldThread(){
        while(true){
             if(WorldManager.isGoingToQuitGame==true){
                return;
            }
            Thread.Sleep(20);  
            for (float x = playerChunkLoadingPos.x - viewRange; x < playerChunkLoadingPos.x + viewRange; x += Chunk.chunkWidth)
            {
            for (float z = playerChunkLoadingPos.y - viewRange; z <playerChunkLoadingPos.y + viewRange; z += Chunk.chunkWidth)
                {
                Vector3 pos = new Vector3(x, 0, z);

                Vector2Int chunkPos=  Chunk.Vec3ToChunkPos(pos);
               
                Chunk chunk = Chunk.GetChunk(chunkPos);
                if (chunk != null||Chunk.GetUnloadedChunk(chunkPos)!=null||WorldManager.chunkSpawningQueue.Contains(chunkPos)) {
                                        continue;
                            }else{
                 //   chunk=ObjectPools.chunkPool.Get(chunkPos).GetComponent<Chunk>();
               //     chunk.transform.position=new Vector3(chunkPos.x,0,chunkPos.y);
               //     chunk.isChunkPosInited=true;
             //   if(chunk!=null){
              //    chunk.ReInitData();
             //  }
               
               WorldManager.chunkSpawningQueue.Enqueue(chunkPos,(int)Mathf.Abs(chunkPos.x-playerChunkLoadingPos.x)+(int)Mathf.Abs(chunkPos.y-playerChunkLoadingPos.y)); 


         //          WorldManager.chunksToLoad.Add(chunk);
                }
            }
        }

     

        }
        
    }

     

      [BurstCompile]
    public struct BakeJobFor:IJobParallelFor{
        public NativeArray<int> meshID;
        public void Execute(int i){
            Physics.BakeMesh(meshID[i],false);
        }
    }

  public void TryStrongLoadChunkThread(){
      //  chunksToStrongLoad.Clear();
      
        for (float x = playerChunkLoadingPos.x - chunkStrongLoadingRange; x < playerChunkLoadingPos.x + chunkStrongLoadingRange; x += Chunk.chunkWidth)
            {
            for (float z = playerChunkLoadingPos.y - chunkStrongLoadingRange; z <playerChunkLoadingPos.y + chunkStrongLoadingRange; z += Chunk.chunkWidth)
                {
                Vector3 pos = new Vector3(x, 0, z);

                Vector2Int chunkPos=  Chunk.Vec3ToChunkPos(pos);
               
                Chunk chunk = Chunk.GetChunk(chunkPos);
                if(chunk==null||chunk.isChunkPosInited==false){
                 //   if(!WorldManager.chunkSpawningQueue.Contains(chunkPos)){
                  //  WorldManager.chunkSpawningQueue.Enqueue(chunkPos,-50);     
                  //  }
                    continue;
                }
         //     chunk.meshCollider.sharedMesh=chunk.chunkMesh;
                if(chunk.isStrongLoaded==false||chunk.meshCollider.sharedMesh==null){
                    if(chunk.isMeshBuildCompleted==false){
                        continue;
                    }else{
                    chunk.StrongLoadChunk();
                    }
                  
                
                 
                }
                
               
             //   chunk.isStrongLoaded=true;
              
            }
        }
     
        
        
    }
 /*  async void UpdateWorld()
    {
        Vector3 curPos=transform.position;
      //  await Task.Delay(100);
      await Task.Run(()=>{  
    
        for (float x = curPos.x - viewRange; x < curPos.x + viewRange; x += Chunk.chunkWidth)
        {
            for (float z = curPos.z - viewRange; z <curPos.z + viewRange; z += Chunk.chunkWidth)
            {
                Vector3 pos = new Vector3(x, 0, z);
               // pos.x = Mathf.Floor(pos.x / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
            //    pos.z = Mathf.Floor(pos.z / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
                Vector2Int chunkPos=  Chunk.Vec3ToChunkPos(pos);
               
                Chunk chunk = Chunk.GetChunk(chunkPos);
                if (chunk != null||Chunk.GetUnloadedChunk(chunkPos)!=null||WorldManager.chunkSpawningQueue.Contains(chunkPos)) {
                                        continue;
                            }else{
                 //   chunk=ObjectPools.chunkPool.Get(chunkPos).GetComponent<Chunk>();
               //     chunk.transform.position=new Vector3(chunkPos.x,0,chunkPos.y);
               //     chunk.isChunkPosInited=true;
             //   if(chunk!=null){
              //    chunk.ReInitData();
             //  }
             if(!WorldManager.chunkSpawningQueue.Contains(chunkPos)){
               WorldManager.chunkSpawningQueue.Enqueue(chunkPos,(int)Mathf.Abs(chunkPos.x-curPos.x)+(int)Mathf.Abs(chunkPos.y-curPos.z)); 
             }
                    
         //          WorldManager.chunksToLoad.Add(chunk);
                }
            }
        }});  
       
     
    }*/

   async void PlayerCritAttack(){
        CritAttackAnimate();
        Invoke("cancelCritAttackInvoke",0.75f);
      await UniTask.Delay(TimeSpan.FromSeconds(0.75), ignoreTimeScale: false);
       
        Vector3 attackEffectPoint;
        RaycastHit infoForward;
        if(Physics.Linecast(headPos.position,headPos.position+headPos.forward*4f,out infoForward)){
                attackEffectPoint=infoForward.point;
        }else{
             attackEffectPoint=headPos.position+headPos.forward*4f;
        }
         GameObject a=Instantiate(playerSweepParticlePrefab,attackEffectPoint,Quaternion.identity);
         a.GetComponent<ParticleSystem>().Emit(1);
         Destroy(a,2f);
        AudioSource.PlayClipAtPoint(playerSweepAttackClip,headPos.position,1f);
          Collider[] colliders = Physics.OverlapSphere(transform.position, 4f);
          foreach(var c in colliders){
              if(c.gameObject.tag=="Entity"){
                if(c.GetComponent<CreeperBeh>()!=null){
                    c.GetComponent<CreeperBeh>().ApplyDamageAndKnockback(10f+Random.Range(-5f,5f),(transform.position-c.transform.position).normalized*Random.Range(-20f,-30f));
                }
                if(c.GetComponent<ZombieBeh>()!=null){
                    c.GetComponent<ZombieBeh>().ApplyDamageAndKnockback(10f+Random.Range(-5f,5f),(transform.position-c.transform.position).normalized*Random.Range(-20f,-30f));
                }
                if(c.GetComponent<ItemEntityBeh>()!=null){
                    c.GetComponent<Rigidbody>().velocity=(transform.position-c.transform.position).normalized*Random.Range(-20f,-30f);
                }
            }
          }
    }
    void cancelAttackInvoke(){
        am.SetBool("isattacking",false);
    }
    void AttackAnimate(){
        am.SetBool("isattacking",true);
    }
     void cancelCritAttackInvoke(){
        am.SetBool("isattackingcrit",false);
    }
    void CritAttackAnimate(){
        am.SetBool("isattackingcrit",true);
    }
    void AttackEnemy(GameObject go){
         AttackAnimate();
     Invoke("cancelAttackInvoke",0.16f);
        if(go.GetComponent<ZombieBeh>()!=null){
            if(inventoryDic[currentSelectedHotbar-1]==152){
             go.GetComponent<ZombieBeh>().ApplyDamageAndKnockback(7f,(transform.position-go.transform.position).normalized*-20f);   
            }else if(inventoryDic[currentSelectedHotbar-1]==151){
                 go.GetComponent<ZombieBeh>().ApplyDamageAndKnockback(5f,(transform.position-go.transform.position).normalized*-20f);   
            }else{
                  go.GetComponent<ZombieBeh>().ApplyDamageAndKnockback(1f,(transform.position-go.transform.position).normalized*-10f);   
            }
        
        }
        if(go.GetComponent<CreeperBeh>()!=null){
              if(inventoryDic[currentSelectedHotbar-1]==152){
             go.GetComponent<CreeperBeh>().ApplyDamageAndKnockback(7f,(transform.position-go.transform.position).normalized*-20f);   
            }else if(inventoryDic[currentSelectedHotbar-1]==151){
                 go.GetComponent<CreeperBeh>().ApplyDamageAndKnockback(5f,(transform.position-go.transform.position).normalized*-20f);   
            }else{
                  go.GetComponent<CreeperBeh>().ApplyDamageAndKnockback(1f,(transform.position-go.transform.position).normalized*-10f);   
            }
        }
    }
    void BreakBlock(){
        if(cameraPosMode==1){
            return;
        }
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)){
            if(info.collider.gameObject.tag=="Entity"){
               
                    AttackEnemy(info.collider.gameObject);
                    return;
            }
            Vector3 blockPoint=info.point+headPos.forward*0.01f;
            int tmpID=Chunk.GetBlock(blockPoint);
            if(tmpID==0){
                return;
            }
            if(inventoryDic[currentSelectedHotbar-1]==151){
             //   return;
               AttackAnimate();
            Invoke("cancelAttackInvoke",0.16f);
             for(float x=-1f;x<=1f;x++){
                for(float y=-1f;y<=1f;y++){
                    for(float z=-1f;z<=1f;z++){
                        Vector3 blockPointArea=blockPoint+new Vector3(x,y,z);
                    int tmpID2=Chunk.GetBlock(blockPointArea);
                    Chunk.SetBlockByHand(blockPointArea,0);

                   if(tmpID2==0){
                    continue;
                    }
                       GameObject a=ObjectPools.particleEffectPool.Get();
                    a.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPointArea).x+0.5f,Chunk.Vec3ToBlockPos(blockPointArea).y+0.5f,Chunk.Vec3ToBlockPos(blockPointArea).z+0.5f);
                    a.GetComponent<particleAndEffectBeh>().blockID=tmpID2;
                    a.GetComponent<particleAndEffectBeh>().SendMessage("EmitParticle");
                    if(tmpID2==10){
                        StartCoroutine(ItemEntityBeh.SpawnNewItem(Chunk.Vec3ToBlockPos(blockPointArea).x+0.5f,Chunk.Vec3ToBlockPos(blockPointArea).y+0.5f,Chunk.Vec3ToBlockPos(blockPointArea).z+0.5f,153,new Vector3(Random.Range(-3f,3f),Random.Range(-3f,3f),Random.Range(-3f,3f))));
                    }else{
                        StartCoroutine(ItemEntityBeh.SpawnNewItem(Chunk.Vec3ToBlockPos(blockPointArea).x+0.5f,Chunk.Vec3ToBlockPos(blockPointArea).y+0.5f,Chunk.Vec3ToBlockPos(blockPointArea).z+0.5f,tmpID2,new Vector3(Random.Range(-3f,3f),Random.Range(-3f,3f),Random.Range(-3f,3f))));   
                    }

                    }
                }
             }
             return;
            }

            Chunk.SetBlockByHand(blockPoint,0);
            GameObject b=ObjectPools.particleEffectPool.Get();
            b.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPoint).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint).z+0.5f);
            b.GetComponent<particleAndEffectBeh>().blockID=tmpID;
            b.GetComponent<particleAndEffectBeh>().SendMessage("EmitParticle");
            if(tmpID==10){
                 StartCoroutine(ItemEntityBeh.SpawnNewItem(Chunk.Vec3ToBlockPos(blockPoint).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint).z+0.5f,153,new Vector3(Random.Range(-3f,3f),Random.Range(-3f,3f),Random.Range(-3f,3f))));
            }else{
             StartCoroutine(ItemEntityBeh.SpawnNewItem(Chunk.Vec3ToBlockPos(blockPoint).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint).z+0.5f,tmpID,new Vector3(Random.Range(-3f,3f),Random.Range(-3f,3f),Random.Range(-3f,3f))));   
            }
            
           Vector3Int intPos=new Vector3Int(Chunk.FloatToInt(blockPoint.x),Chunk.FloatToInt(blockPoint.y),Chunk.FloatToInt(blockPoint.z));
        Chunk chunkNeededUpdate=Chunk.GetChunk(Chunk.Vec3ToChunkPos(blockPoint));
        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(chunkNeededUpdate.transform.position);
        chunkNeededUpdate.BFSInit(chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z,7,0);
     AttackAnimate();
     Invoke("cancelAttackInvoke",0.16f);
        }
    }

    void PlaceBlock(){
        if(cameraPosMode==1){
            return;
        }
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)&&info.collider.gameObject.tag=="Entity"&&critAttackCD<=0f){
            
             if(inventoryDic[currentSelectedHotbar-1]==152){
                PlayerCritAttack();
                critAttackCD=1f;
                return;
            }
        }
        if(Physics.Raycast(ray,out info,10f)&&info.collider.gameObject.tag!="Entity"&&info.collider.gameObject.tag!="Player"){
            Vector3 blockPoint=info.point-headPos.forward*0.01f;
            if(inventoryDic[currentSelectedHotbar-1]>150&&inventoryDic[currentSelectedHotbar-1]<=200){
                return;
            }
            if(inventoryDic[currentSelectedHotbar-1]==102){
                if(Chunk.GetBlock(Chunk.Vec3ToBlockPos(blockPoint)+new Vector3Int(0,-1,0))==0||(Chunk.GetBlock(Chunk.Vec3ToBlockPos(blockPoint)+new Vector3Int(0,-1,0))>=100&&Chunk.GetBlock(Chunk.Vec3ToBlockPos(blockPoint)+new Vector3Int(0,-1,0))<=200)){
                    return;
                }
            }
            if(inventoryDic[currentSelectedHotbar-1]==0){
                return;
            }
            if(collidingBlockOutline.GetComponent<BlockOutlineBeh>().isCollidingWithPlayer==true||collidingBlockOutline.GetComponent<BlockOutlineBeh>().isCollidingWithEntity==true){
               return;
            }

            Chunk.SetBlockByHand(blockPoint,inventoryDic[currentSelectedHotbar-1]);
          
             AudioSource.PlayClipAtPoint(Chunk.blockAudioDic[inventoryDic[currentSelectedHotbar-1]],blockPoint,1f);
            inventoryItemNumberDic[currentSelectedHotbar-1]--;
             AttackAnimate();
     Invoke("cancelAttackInvoke",0.16f);
        }
    }
    
   
    public void ReadPlayerJson(){
          inventoryDic=new int[9];
            inventoryItemNumberDic=new int[9];
    
    gameWorldPlayerDataPath=WorldManager.gameWorldDataPath;
         
         if (!Directory.Exists(gameWorldPlayerDataPath+"unityMinecraftData")){
                Directory.CreateDirectory(gameWorldPlayerDataPath+"unityMinecraftData");
               
            }
          if(!Directory.Exists(gameWorldPlayerDataPath+"unityMinecraftData/GameData")){
                    Directory.CreateDirectory(gameWorldPlayerDataPath+"unityMinecraftData/GameData");
                }
       
        if(!File.Exists(gameWorldPlayerDataPath+"unityMinecraftData"+"/GameData/playerdata.json")){
            FileStream fs=File.Create(gameWorldPlayerDataPath+"unityMinecraftData"+"/GameData/playerdata.json");
            fs.Close();
        }
       
       byte[] worldPlayerData=File.ReadAllBytes(gameWorldPlayerDataPath+"unityMinecraftData/GameData/playerdata.json");
         //   isEntitiesReadFromDisk=true;

          PlayerData pd;
         if(worldPlayerData.Length>0){
                pd=MessagePackSerializer.Deserialize<PlayerData>(worldPlayerData);  
         }else{
            pd=new PlayerData(20f,0f,150f,0f,new int[9],new int[9]);
        //    return;
         }
         
         if(pd.posX!=0f&&pd.posY!=0f&&pd.posZ!=0f&&pd.inventoryDic!=null&&pd.inventoryItemNumberDic!=null){
           transform.position=new Vector3(pd.posX,pd.posY,pd.posZ); 
           playerHealth=pd.playerHealth;
           inventoryDic=pd.inventoryDic;
           inventoryItemNumberDic=pd.inventoryItemNumberDic;
         }else{
            transform.position=new Vector3(0f,150f,0f);
            inventoryDic=new int[9];
            inventoryItemNumberDic=new int[9];

         }
         
    }
    public void SavePlayerData(){
        PlayerData pd=new PlayerData(playerHealth,transform.position.x,transform.position.y,transform.position.z,inventoryDic,inventoryItemNumberDic);
   
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
        byte[] tmpData=MessagePackSerializer.Serialize(pd);
        File.WriteAllBytes(gameWorldPlayerDataPath+"unityMinecraftData/GameData/playerdata.json",tmpData);
    }
}
