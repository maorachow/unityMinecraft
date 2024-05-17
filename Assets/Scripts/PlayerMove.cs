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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Events;
using UnityEngine.Rendering;


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
    [Key(6)]
    public int playerInWorldID;
    [Key(7)]
    public float playerArmorPoints;
    public PlayerData(float playerHealth,float posX,float posY,float posZ,int[] inventoryDic,int[] inventoryItemNumberDic, int playerInWorldID,float playerArmorPoints)
    {
        this.playerHealth=playerHealth;
        this.posX=posX;
        this.posY=posY;
        this.posZ=posZ;
        this.inventoryDic=inventoryDic;
        this.inventoryItemNumberDic=inventoryItemNumberDic;
        this.playerInWorldID=playerInWorldID;
        this.playerArmorPoints=playerArmorPoints;
    }
}
public class PlayerMove : MonoBehaviour
{   public static AudioClip playerSinkClip{
    get{return Random.Range(0f,2f)>1f?playerSinkClip1:playerSinkClip2;}
   
    }
    public static AudioClip playerSinkClip1;
    public static AudioClip playerSinkClip2;
    public static AudioClip playerEatClip;
    public static AudioClip playerDropItemClip;
    public static AudioClip playerSweepAttackClip;
    public static AudioClip playerEnterPortalClip;
    public static AudioClip playerEquipArmorClip;
    public AudioSource AS;
    public float playerCameraZShakeValue;
    public float playerCameraZShakeLerpValue;
     
    public float playerHealth=20f;
    public float playerMaxHealth=20f;
    public float playerArmorPoints = 0f;
    public float playerMaxArmorPoints = 20f;
    public Chunk curChunk;
    public Animator am;
  
    public static string gameWorldPlayerDataPath;
    
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
    public static float gravity=-9.8f;
    public float playerY=0f;
    public float jumpHeight=2;
 
    public static float cameraFOV=90f;
    public float currentSpeed;
    public Vector3 playerVec;
    public Vector3 playerMotionVec;
    public bool isPlayerKilled=false;
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
    public int curHeadBlockID;
    public int prevHeadBlockID;
    public int curFootBlockID;
    public int curUnderFootBlockID;
    public int prevUnderFootBlockID;
    public int prevFootBlockID;
    public float playerMoveDrag=0f;
    public bool isPlayerInGround=false;
    public bool isPlayerGrounded=false;
    public bool isUsingCC=true;
    public ChunkLoaderBase curChunkLoader;
   
    public SimpleAxisAlignedBB playerBound;
    public Dictionary<Vector3Int,SimpleAxisAlignedBB> blocksAround;
    public static PlayerMove instance;
    public float playerTeleportingCD = 0f;
    public Dictionary<Vector3Int,SimpleAxisAlignedBB> GetBlocksAround(SimpleAxisAlignedBB aabb){
      
            int minX = floorFloat(aabb.getMinX()-0.1f);
            int minY =floorFloat(aabb.getMinY()-0.1f);
            int minZ =floorFloat(aabb.getMinZ()-0.1f);
            int maxX =ceilFloat(aabb.getMaxX()+0.1f);
            int maxY =ceilFloat(aabb.getMaxY()+0.1f);
            int maxZ =ceilFloat(aabb.getMaxZ()+0.1f);

            this.blocksAround = new Dictionary<Vector3Int,SimpleAxisAlignedBB>();

            for (int z = minZ-1; z <= maxZ+1;z++) {
                for (int x = minX-1; x <= maxX+1; x++) {
                    for (int y = minY-1; y <=maxY+1; y++) {
                        int blockID=WorldHelper.instance.GetBlock(new Vector3(x,y,z));
                        if(blockID>0&&blockID<100){
                         this.blocksAround.Add(new Vector3Int(x,y,z),new SimpleAxisAlignedBB(x,y,z,x+1,y+1,z+1));   
                        }
                    }
                }
            }
        

        return this.blocksAround;


    }
     public static int floorFloat(float n) {
        int i = (int) n;

        return n >= i ? i : i - 1;
    }

    public static int ceilFloat(float n) {
        int i = (int) (n + 1);
        return n >= i ? i : i- 1 ;
    }
    void Move(float dx,float dy,float dz){
       //  this.ySize *= 0.4;
        
            float movX = dx;
            float movY = dy;
            float movZ = dz;
        if(blocksAround.Count==0){
        playerBound= playerBound.offset(0, dy, 0);
        playerBound= playerBound.offset(dx, 0, 0);
        playerBound= playerBound.offset(0, 0, dz);
        }

         

           

            foreach (var bb in blocksAround) {
                dy = bb.Value.calculateYOffset(playerBound, dy);
            }

          playerBound=  playerBound.offset(0, dy, 0);

      //      bool fallingFlag = (this.onGround || (dy != movY && movY < 0));

            foreach (var bb in blocksAround) {
                dx = bb.Value.calculateXOffset(playerBound, dx);
            }

            playerBound= playerBound.offset(dx, 0, 0);

            foreach (var bb in blocksAround) {
                dz = bb.Value.calculateZOffset(playerBound, dz);
            }

            playerBound= playerBound.offset(0, 0, dz);
    }



    
    void Awake(){
        instance = this;
        chunkStrongLoadingRange = 64;
        viewRange = 64;
        am = transform.GetChild(0).GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        headPos = transform.GetChild(0).GetChild(0);
        playerMoveRef = headPos.GetChild(1);
        playerBodyPos = transform.GetChild(0).GetChild(1);
        mainCam = headPos.GetChild(0).gameObject.GetComponent<Camera>();
        mainCam.fieldOfView = cameraFOV;
        cameraPos = mainCam.transform;

    }
    void OnDestroy(){
        curChunk=null;
    }
    public void SetHotbarNum(int num){
        Debug.Log(num);
        currentSelectedHotbar=num;
        if(blockOnHandText==null){
            blockOnHandText=GameObject.Find("blockonhandIDtext").GetComponent<Text>();
        }
        blockOnHandText.text=GameUIBeh.blockNameDic[inventoryDic[currentSelectedHotbar-1]];
    }
    public void BreakBlockButtonPress(){
        if(isPlayerKilled==true||GameUIBeh.isPaused==true||GameUIBeh.instance.isCraftingMenuOpened==true){
            return;
        }
        if(breakBlockCD<=0f){
            LeftClick();
            breakBlockCD=0.3f;}
    }
    public void PlaceBlockButtonPress(){
          if(isPlayerKilled==true||GameUIBeh.isPaused==true||GameUIBeh.instance.isCraftingMenuOpened==true){
            return;
        }
        if(breakBlockCD<=0f){
            RightClick();
            breakBlockCD=0.3f;}
    }
 
    void Start()
    {   
      //    ReadPlayerJson();
    
        curChunkLoader=GetComponent<ChunkLoaderBase>();
        curChunkLoader.AddChunkLoaderToList();
        
    //    pi=new PlayerInput();
    //    pi.Enable();
         Input.multiTouchEnabled = true;
        AS=GetComponent<AudioSource>();
        // pauseMenu.SetActive(true);
        currentSelectedHotbar=1;
        playerEatClip=Resources.Load<AudioClip>("Audios/Drink");
        playerSinkClip1=Resources.Load<AudioClip>("Audios/Entering_water");
        playerSinkClip2=Resources.Load<AudioClip>("Audios/Exiting_water");
        playerHandItem=transform.GetChild(0).GetChild(1).GetChild(1).GetChild(1).GetChild(0).gameObject.GetComponent<ItemOnHandBeh>();
        playerSweepAttackClip=Resources.Load<AudioClip>("Audios/Sweep_attack1");
        playerEnterPortalClip = Resources.Load<AudioClip>("Audios/Nether_Portal_trigger");
        playerDropItemClip=Resources.Load<AudioClip>("Audios/Pop");
        prefabBlockOutline=Resources.Load<GameObject>("Prefabs/blockoutline");
        playerEquipArmorClip = Resources.Load<AudioClip>("Audios/Equip_diamond2");
        blockOutline =Instantiate(prefabBlockOutline,transform.position,transform.rotation);
        collidingBlockOutline=Instantiate(prefabBlockOutline,transform.position,transform.rotation);

        blockOnHandText=GameObject.Find("blockonhandIDtext").GetComponent<Text>();
        
      //  InvokeRepeating("SendChunkReleaseMessage",1f,3f);
        GameUIBeh.instance.CloseCraftingUI();
        GameUIBeh.instance.Resume();
       
                transform.GetChild(0).localRotation=Quaternion.Euler(0f,0f,0f);
        transform.GetChild(0).localPosition=new Vector3(0f,0f,0f);
        playerSweepParticlePrefab=Resources.Load<GameObject>("Prefabs/playersweepparticle");
        //     playerBound=new SimpleAxisAlignedBB(transform.position-new Vector3(0.3f,0.5f,0.3f),transform.position+new Vector3(0.3f,0.9f,0.3f));
        GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);    
        PlayerInputBeh.instance.switchCameraPosAction = SwitchCameraPosAction;
        PlayerInputBeh.instance.pauseOrResumeAction = PauseOrResume;
        PlayerInputBeh.instance.dropItemButtonAction = DropItemButtonOnClick;
        PlayerInputBeh.instance.selectHotbarButtonAction = SetHotbarNum;
        PlayerInputBeh.instance.openInventoryButtonAction = OpenOrCloseCraftingUI;
        PlayerInputBeh.instance.playerLeftClickActions.Add(MouseLock);
        PlayerInputBeh.instance.playerLeftClickActions.Add(BreakBlockButtonPress);

        PlayerInputBeh.instance.playerRightClickActions.Add(PlaceBlockButtonPress);
        PlayerInputBeh.instance.AddButtonActions();
    }


  //  void SendChunkReleaseMessage(){
  //      foreach(var c in Chunk.Chunks){
   //         c.Value.SendMessage("TryReleaseChunk");
  //      }
  //  }
    void MouseLock()
    {
        if (WorldManager.platform == RuntimePlatform.Android || WorldManager.platform == RuntimePlatform.IPhonePlayer || MobileButtonHideBeh.isHidingButton == false)
        {
            return;
        }
        if(isPlayerKilled==true||GameUIBeh.isPaused==true||GameUIBeh.instance.isCraftingMenuOpened==true){
            return;
        }
        
         Cursor.lockState = CursorLockMode.Locked;   
         
    
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
        ItemEntityBeh.SpawnNewItem(headPos.position.x,headPos.position.y,headPos.position.z,itemTypeID,(headPos.forward*3));
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
        float reducedDamage = Mathf.Max((1f - (playerArmorPoints * 4f / 100f)) * damageAmount,0f);
        playerHealth -= reducedDamage;
        playerArmorPoints -= damageAmount * 0.1f;
         GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);
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
        playerArmorPoints = 0f;
        GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);
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
        }else if(exchangeID == 3)
        {
            if (GetItemFromSlot(155) == -1)
            {
                return;
            }
            else
            {

                inventoryItemNumberDic[GetItemFromSlot(155)]--;
                AddItem(156, 1);
            }

        }
        else if (exchangeID == 4)
        {
            if (GetItemFromSlot(153) == -1)
            {
                return;
            }
            else
            {

                inventoryItemNumberDic[GetItemFromSlot(153)]--;
                AddItem(158, 1);
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
    //     playerBound=new SimpleAxisAlignedBB(new Vector3(0f,150f,0f)-new Vector3(0.3f,0.5f,0.3f),new Vector3(0f,150f,0f)+new Vector3(0.3f,0.9f,0.3f));
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
    public void SwitchCameraPosAction()
    {
        cameraPosMode++;
        if (cameraPosMode >= 3)
        {
            cameraPosMode = 0;
        }
    }
    void Update()
    {     
      
      /*    if(Input.GetKeyDown(KeyCode.Escape)){
            PauseOrResume();
          }*/
        
        if(GameUIBeh.isPaused==true){
            return;
        }
   /*     if(Input.GetKeyDown(KeyCode.E)){
          
            OpenOrCloseCraftingUI();
          }*/
        playerCameraZShakeLerpValue=Mathf.Lerp(playerCameraZShakeLerpValue,0f,15f*Time.deltaTime);
       playerCameraZShakeValue=Mathf.Lerp(playerCameraZShakeLerpValue,0f,15f*Time.deltaTime);

     
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
      
        
       if(curChunk==null){
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
        if(PlayerInputBeh.instance.isPlayerSpeededUp==true){
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
        blockOutline.transform.position=new Vector3(WorldHelper.instance.Vec3ToBlockPos(blockPoint).x+0.5f,WorldHelper.instance.Vec3ToBlockPos(blockPoint).y+0.5f,WorldHelper.instance.Vec3ToBlockPos(blockPoint).z+0.5f);
        collidingBlockOutline.transform.position=new Vector3(WorldHelper.instance.Vec3ToBlockPos(blockPoint2).x+0.5f,WorldHelper.instance.Vec3ToBlockPos(blockPoint2).y+0.5f,WorldHelper.instance.Vec3ToBlockPos(blockPoint2).z+0.5f);
        }else if(Physics.Raycast(ray,out info,10f)&&info.collider.gameObject.tag=="Entity"){
            Vector3 blockPoint=info.point+headPos.forward*0.01f;
        Vector3 blockPoint2=info.point-headPos.forward*0.01f;
            collidingBlockOutline.transform.position=new Vector3(WorldHelper.instance.Vec3ToBlockPos(blockPoint2).x+0.5f,WorldHelper.instance.Vec3ToBlockPos(blockPoint2).y+0.5f,WorldHelper.instance.Vec3ToBlockPos(blockPoint2).z+0.5f);
            blockOutline.GetComponent<MeshRenderer>().enabled=false;
            collidingBlockOutline.GetComponent<MeshRenderer>().enabled=false;
            if (info.collider.GetComponent<EndermanBeh>() != null)
            {
                info.collider.GetComponent<EndermanBeh>().isTargetingPlayer = true;
            }
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
       
         
           
            
                
                currentSelectedHotbar+=(int)PlayerInputBeh.instance.switchItemSlotAxis;
                currentSelectedHotbar=Mathf.Clamp(currentSelectedHotbar,1,9);
                if(blockOnHandText==null){
                blockOnHandText=GameObject.Find("blockonhandIDtext").GetComponent<Text>();
                }
                if(GameUIBeh.blockNameDic.ContainsKey(inventoryDic[currentSelectedHotbar-1])){
                blockOnHandText.text= GameUIBeh.blockNameDic[inventoryDic[currentSelectedHotbar-1]];    
                }else{
                blockOnHandText.text=  "Unknown Block Name,ID:"+inventoryDic[currentSelectedHotbar-1];  
                }
                
        
          
           
     /*     pi.Player.SwitchCameraPos.performed+=ctx=>{
            cameraPosMode++;
            if(cameraPosMode>=3){
                cameraPosMode=0;
            }
          };*/
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
                cameraPos.position=Vector3.Lerp(headPos.position,infoForward.point,0.92f);
            }else{
                cameraPos.localPosition=new Vector3(0f,0f,5f)+new Vector3(0f,0.28f,-0.1f);
            }
            cameraPos.localEulerAngles=new Vector3(0f,-180f,0f);
            break;
            case 2:if(isBackPointHit==true){
                cameraPos.position=Vector3.Lerp(headPos.position,infoBack.point,0.92f);
            }else{
                cameraPos.localPosition=new Vector3(0f,0f,-5f)+new Vector3(0f,0.28f,-0.1f);
            }
            cameraPos.localEulerAngles=new Vector3(0f,0f,0f);
            break;
        }
        WorldHelper.instance.cameraPos = cameraPos.position;
            if((cc.isGrounded!=true)){
            if(curFootBlockID==100){
            playerY+=gravity*Time.deltaTime;
            playerY=Mathf.Clamp(playerY,-3f,1f);
           }else{
            playerY+=gravity*Time.deltaTime;
           }
           
           
        }else{
          
           playerY=0f; 
           
            
           // playerMotionVec.y=0f;
        } 
        
      
      
        if((cc.isGrounded==true||curFootBlockID==100||isPlayerGrounded==true)&&PlayerInputBeh.instance.isJumping==true){
            if(curFootBlockID==100){
                playerY=jumpHeight/2f;
            }else{
            playerY=jumpHeight;    
            }
            
        }
        if(curFootBlockID==100){
            playerY=Mathf.Clamp(playerY,-1f,5f);
        }

         float mouseX=PlayerInputBeh.instance.mouseDelta.x;
        float mouseY= PlayerInputBeh.instance.mouseDelta.y;
 
   
        
     
        
       

        mouseY=Mathf.Clamp(mouseY,-90f,90f);
        cameraX-=mouseY;
        cameraX=Mathf.Clamp(cameraX,-90f,90f);
       
          if (PlayerInputBeh.instance.playerMoveVec!=Vector2.zero && GameUIBeh.instance.isCraftingMenuOpened==false){
           playerVec=new Vector3(PlayerInputBeh.instance.playerMoveVec.y,0f, PlayerInputBeh.instance.playerMoveVec.x); 
        
           lerpPlayerVec=Vector3.Lerp(lerpPlayerVec,playerVec,7f*Time.deltaTime);
          }else{
            lerpPlayerVec=Vector3.Lerp(lerpPlayerVec,Vector3.zero,7f*Time.deltaTime);
          }
        

        playerVec.y=playerY;
        headPos.eulerAngles+=new Vector3(0f,mouseX,0f);
        headPos.localEulerAngles=new Vector3(cameraX,headPos.localEulerAngles.y,playerCameraZShakeValue);
        playerMoveRef.eulerAngles=new Vector3(0f,headPos.eulerAngles.y,headPos.eulerAngles.z);
        playerBodyPos.rotation=Quaternion.Slerp(playerBodyPos.rotation,playerMoveRef.rotation,5f*Time.deltaTime);
        
            if(isPlayerInGround==true){
            cc.enabled=false;
            transform.position=transform.position+Time.deltaTime*new Vector3(0f,5f,0f);
            cc.enabled=true;
        }else{
             cc.Move((playerMoveRef.forward*lerpPlayerVec.x+playerMoveRef.right*lerpPlayerVec.z)*moveSpeed*(1f-playerMoveDrag)*Time.deltaTime+new Vector3(0f,playerVec.y,0f)*5f*Time.deltaTime+playerMotionVec*Time.deltaTime);  
        }  
        
      
       
        if(breakBlockCD>0f){
            breakBlockCD-=Time.deltaTime;
        }
        if(critAttackCD>0f){
        critAttackCD-=Time.deltaTime;    
        }
        
     /*   pi.Player.DropItem.performed+=ctx=>{
            
            PlayerDropItem(currentSelectedHotbar-1);
         //    playerHandItem.BuildItemModel(inventoryDic[currentSelectedHotbar-1]);  
        };*/
        // if (!EventSystem.current.IsPointerOverGameObject())
        // {
      //  Debug.Log(Input.touches.Length);
        
      /*  if (Input.touches.Length==1){
            pi.Player.LeftClick.performed+=ctx=>{ if(breakBlockCD<=0f){
            LeftClick();
            breakBlockCD=0.3f;}};
            
             pi.Player.RightClick.performed+=ctx=>{ if(breakBlockCD<=0f){
            RightClick();
            breakBlockCD=0.3f;}};
         


          }else{
            
            if(pi.Player.LeftClick.ReadValue<float>()>=0.5f&&breakBlockCD<=0f){
            LeftClick();
            breakBlockCD=0.3f;
        }
        if(pi.Player.RightClick.ReadValue<float>()>=0.5f&&breakBlockCD<=0f){
            RightClick();
            breakBlockCD=0.3f;
        }
    //    if(pi.Player.RightClick.ReadValue<float>()>=1f&&breakBlockCD<=0f){
     //       PlaceBlock();
      //      breakBlockCD=0.3f;
     //   }
          }*/
       
   // }
       
    }


    public void PlayerGroundSinkPrevent(CharacterController cc,int blockID,float dt){
         if(blockID>0f&&blockID<100f){
      //      cc.Move(new Vector3(0f,dt*5f,0f));
            gravity=0f;
            isPlayerInGround=true;
         }  else{
            gravity=-9.8f;
            isPlayerInGround=false;
         }
        }



    void FixedUpdate(){
    // if(cc.velocity.magnitude>0.1f){
    //       UpdateWorld();
   //  }
         
   //playerBound.setMinX(transform.position.x-0.3f);
   //playerBound.setMinY(transform.position.y-0.9f);
   //playerBound.setMinZ(transform.position.z-0.3f);
   //playerBound.setMaxX(transform.position.x+0.3f);
   ///playerBound.setMaxY(transform.position.y+0.9f);
   //playerBound.setMaxZ(transform.position.z+0.3f);
 //  playerBound.Visualize();
  // GetBlocksAround(playerBound);
 /*  foreach(var aabb in blocksAround){
    aabb.Value.Visualize();
    if(aabb.Key==Vector3Int.RoundToInt(transform.position-new Vector3(0f,0.3f,0f))){
          isPlayerGrounded=true; 
          Debug.Log("true");
    }
   }*/
        curChunkLoader.chunkLoadingCenter=playerChunkLoadingPos;
        curChunkLoader.cameraFrustum = GeometryUtility.CalculateFrustumPlanes(mainCam);
        curChunkLoader.chunkLoadingRange=viewRange;

      if(curChunk==null){
            curChunk=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(transform.position));  
            curChunkLoader.isChunksNeedLoading=true;
        }
        if(WorldHelper.instance.CheckIsPosInChunk(transform.position,curChunk)==false){
        curChunk=Chunk.GetChunk(WorldHelper.instance.Vec3ToChunkPos(transform.position));    
        curChunkLoader.isChunksNeedLoading=true;
     //   curChunkStrongLoader.isChunksNeededStrongLoading=true;
        }
   // Debug.Log(finalMoveVec);
   curFootBlockID=WorldHelper.instance.GetBlock(transform.position+new Vector3(0f,-0.2f,0f),curChunk);
   curUnderFootBlockID = WorldHelper.instance.GetBlock(transform.position + new Vector3(0f, -1.2f, 0f), curChunk);
   curHeadBlockID =WorldHelper.instance.GetBlock(cameraPos.position,curChunk);
   //     Debug.Log("block ID:"+ curUnderFootBlockID);
     //   Debug.Log(curChunk.ToString());
   if(curHeadBlockID!=prevHeadBlockID){

        GlobalVolumeWaterEffectBeh.instance.SwitchEffects(curHeadBlockID==100);
    //    WaterSplashParticleBeh.instance.EmitParticleAtPosition(transform.position);
   }
   prevHeadBlockID=curHeadBlockID;
   PlayerGroundSinkPrevent(cc,curFootBlockID,Time.deltaTime);
    if(curFootBlockID!=prevFootBlockID){
       
        if(curFootBlockID==100){
            gravity=-1f;
            playerMoveDrag=0.3f;
         AudioSource.PlayClipAtPoint(playerSinkClip,transform.position,1f);
                ParticleEffectManagerBeh.instance.EmitWaterSplashParticleAtPosition(transform.position);
        }else{
            gravity=-9.8f;
            playerMoveDrag=0f;
        }
           
    }
    if(curUnderFootBlockID!=prevUnderFootBlockID)
        {
        if (curUnderFootBlockID == 13)
            {
                AudioSource.PlayClipAtPoint(playerEnterPortalClip,transform.position);
                ParticleEffectManagerBeh.instance.EmitEndermanParticleAtPosition(transform.position);
            }
        }
   prevFootBlockID=curFootBlockID;
        prevUnderFootBlockID = curUnderFootBlockID;
     //      TryStrongLoadChunkThread();
     playerChunkLoadingPos =new Vector2(transform.position.x,transform.position.z);
         UpdateInventory();

        if (curUnderFootBlockID == 13)
        {
            
            PlayerTryTeleportToEnderWorld();
        }
        else
        {
            if (playerTeleportingCD > 0f)
            {
            playerTeleportingCD -=Time.deltaTime;
            }
          
        }
        
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
            ItemEntityBeh.SpawnNewItem(headPos.position.x,headPos.position.y,headPos.position.z,inventoryDic[slotID],(headPos.forward*12));
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
    public void PlayerTryTeleportToEnderWorld()
    {

        if (playerTeleportingCD >= 4f)
        {
            switch (VoxelWorld.currentWorld.worldID)
            {
                case 0:
                SceneManagementHelper.SwitchToWorldWithSceneChanged(1,2);
            VoxelWorld.worlds[1].actionOnSwitchedWorld = delegate () {

                PlayerMove.instance.cc.enabled = false;
                PlayerMove.instance.transform.position = new Vector3(0, 150, 0);
                PlayerMove.instance.cc.enabled = true;
                Debug.Log("action executed");
            };
                    break;
                    case 1:
                    SceneManagementHelper.SwitchToWorldWithSceneChanged(0, 1);
                    VoxelWorld.worlds[0].actionOnSwitchedWorld = delegate () {

                        PlayerMove.instance.cc.enabled = false;
                        PlayerMove.instance.transform.position = new Vector3(0, 150, 0);
                        PlayerMove.instance.cc.enabled = true;
                        Debug.Log("action executed world 0");
                    };
                    break;
                    default:
                    SceneManagementHelper.SwitchToWorldWithSceneChanged(0, 1);
                    VoxelWorld.worlds[0].actionOnSwitchedWorld = delegate () {

                        PlayerMove.instance.cc.enabled = false;
                        PlayerMove.instance.transform.position = new Vector3(0, 150, 0);
                        PlayerMove.instance.cc.enabled = true;
                        Debug.Log("action executed world 0");
                    };
                    break;
            }
            playerTeleportingCD = 0f;
        }
        else
        {
            playerTeleportingCD += Time.deltaTime;
        }
    }
   /*public void TryUpdateWorldThread(){
        while(true){
             if(WorldManager.isGoingToQuitGame==true){
                return;
            }
            Thread.Sleep(50);  
            for (float x = playerChunkLoadingPos.x - viewRange; x < playerChunkLoadingPos.x + viewRange; x += Chunk.chunkWidth)
            {
            for (float z = playerChunkLoadingPos.y - viewRange; z <playerChunkLoadingPos.y + viewRange; z += Chunk.chunkWidth)
                {
                Vector3 pos = new Vector3(x, 0, z);

                Vector2Int chunkPos=  WorldHelper.instance.Vec3ToChunkPos(pos);
               
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
        
    }*/

     

 
   
  /*public void TryStrongLoadChunkThread(){
   
        NativeList<int> meshesID=new NativeList<int>(Allocator.TempJob);
        for (float x = playerChunkLoadingPos.x - chunkStrongLoadingRange; x < playerChunkLoadingPos.x + chunkStrongLoadingRange; x += Chunk.chunkWidth)
            {
            for (float z = playerChunkLoadingPos.y - chunkStrongLoadingRange; z <playerChunkLoadingPos.y + chunkStrongLoadingRange; z += Chunk.chunkWidth)
                {
                Vector3 pos = new Vector3(x, 0, z);

                Vector2Int chunkPos=WorldHelper.instance.Vec3ToChunkPos(pos);
               
                Chunk chunk = Chunk.GetChunk(chunkPos);
                if(chunk==null||chunk.isChunkPosInited==false){
                 //   if(!WorldManager.chunkSpawningQueue.Contains(chunkPos)){
                  //  WorldManager.chunkSpawningQueue.Enqueue(chunkPos,-50);     
                  //  }
                    continue;
                }
         //     chunk.meshCollider.sharedMesh=chunk.chunkMesh;
                if(chunk.isStrongLoaded==false||chunk.meshCollider.sharedMesh==null){
                    if(chunk.isMeshBuildCompleted==true){

                 chunk.StrongLoadChunk();  
                    chunk.isStrongLoaded=true; 
                
                    }else{
                        continue;
                    }
                }
                
               
             //   chunk.isStrongLoaded=true;
              
            }
        }
    
    }*/
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
                Vector2Int chunkPos=  WorldHelper.instance.Vec3ToChunkPos(pos);
               
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
    isPlayerWieldingItem=true;
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
        /* GameObject a=Instantiate(playerSweepParticlePrefab,attackEffectPoint,Quaternion.identity);
         a.GetComponent<ParticleSystem>().Emit(1);
         Destroy(a,2f);*/
       ParticleEffectManagerBeh.instance.EmitPlayerSweepParticleAtPosition(attackEffectPoint);
        AudioSource.PlayClipAtPoint(playerSweepAttackClip,headPos.position,1f);
          Collider[] colliders = Physics.OverlapSphere(transform.position, 4f);
          foreach(var c in colliders){
              if(c.gameObject.tag=="Entity"){

                if (c.GetComponent(typeof(ILivingEntity)) != null)
                {
                    ILivingEntity livingEntity = (ILivingEntity)c.GetComponent(typeof(ILivingEntity));
                    livingEntity.ApplyDamageAndKnockback(10f + Random.Range(-5f, 5f), (transform.position - c.transform.position).normalized * Random.Range(-20f, -30f));
                }
             
                if(c.GetComponent<ItemEntityBeh>()!=null){
                    c.GetComponent<Rigidbody>().velocity=(transform.position-c.transform.position).normalized*Random.Range(-20f,-30f);
                }
            }
          }
          isPlayerWieldingItem=false;
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
        if (go.GetComponent<SkeletonBeh>() != null)
        {
            if (inventoryDic[currentSelectedHotbar - 1] == 152)
            {
                go.GetComponent<SkeletonBeh>().ApplyDamageAndKnockback(7f, (transform.position - go.transform.position).normalized * -20f);
            }
            else if (inventoryDic[currentSelectedHotbar - 1] == 151)
            {
                go.GetComponent<SkeletonBeh>().ApplyDamageAndKnockback(5f, (transform.position - go.transform.position).normalized * -20f);
            }
            else
            {
                go.GetComponent<SkeletonBeh>().ApplyDamageAndKnockback(1f, (transform.position - go.transform.position).normalized * -10f);
            }
        }

        if (go.GetComponent<EndermanBeh>() != null)
        {
            if (inventoryDic[currentSelectedHotbar - 1] == 152)
            {
                go.GetComponent<EndermanBeh>().ApplyDamageAndKnockback(7f, (transform.position - go.transform.position).normalized * -20f);
            }
            else if (inventoryDic[currentSelectedHotbar - 1] == 151)
            {
                go.GetComponent<EndermanBeh>().ApplyDamageAndKnockback(5f, (transform.position - go.transform.position).normalized * -20f);
            }
            else
            {
                go.GetComponent<EndermanBeh>().ApplyDamageAndKnockback(1f, (transform.position - go.transform.position).normalized * -10f);
            }
            go.GetComponent<EndermanBeh>().isTargetingPlayer = true;
        }
    }
   async void LeftClick(){
        if(cameraPosMode==1){
            return;
        }
        if(isPlayerWieldingItem){
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
            int tmpID=WorldHelper.instance.GetBlock(blockPoint);
            if(tmpID==0){
                return;
            }
            if(inventoryDic[currentSelectedHotbar-1]==151){
             //   return;
               AttackAnimate();
            Invoke("cancelAttackInvoke",0.16f);
            WorldHelper.instance.BreakBlockInArea(blockPoint,new Vector3(-1f,-1f,-1f),new Vector3(1f,1f,1f));
             
        
             return;
            }

     /*       WorldHelper.instance.SetBlockByHand(blockPoint,0);
            GameObject b=ObjectPools.particleEffectPool.Get();
            b.transform.position=new Vector3(WorldHelper.instance.Vec3ToBlockPos(blockPoint).x+0.5f,WorldHelper.instance.Vec3ToBlockPos(blockPoint).y+0.5f,WorldHelper.instance.Vec3ToBlockPos(blockPoint).z+0.5f);
            b.GetComponent<particleAndEffectBeh>().blockID=tmpID;
            b.GetComponent<particleAndEffectBeh>().SendMessage("EmitParticle");*/
            WorldHelper.instance.BreakBlockAtPoint(blockPoint);
         
            
           WorldHelper.instance.StartUpdateAtPoint(blockPoint);
            AttackAnimate();
            Invoke("cancelAttackInvoke",0.16f);
        
       
        
         


        }
    }
    void PlayerEatAnimate(){
        am.SetBool("iseating",true);
    }
    void PlayerCancelEatAnimateInvoke(){
        am.SetBool("iseating",false);
    }
    public bool isPlayerWieldingItem=false;
    async void PlayerEat(){
            
           
           
           if(playerHealth<20f){
            isPlayerWieldingItem=true;
            PlayerEatAnimate();
             for(int i=0;i<3;i++){
                await UniTask.Delay(300);
                AudioSource.PlayClipAtPoint(playerEatClip,transform.position+new Vector3(0f,0.5f,0f),1f);
             }
             Invoke("PlayerCancelEatAnimateInvoke",0f);
                playerHealth+=4f;   
                playerHealth=Mathf.Clamp(playerHealth,0f,20f);
                inventoryItemNumberDic[currentSelectedHotbar-1]--;
                GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
                  if(GameUIBeh.blockNameDic.ContainsKey(inventoryDic[currentSelectedHotbar-1])){
                blockOnHandText.text= GameUIBeh.blockNameDic[inventoryDic[currentSelectedHotbar-1]];    
                }else{
                blockOnHandText.text=  "Unknown Block Name,ID:"+inventoryDic[currentSelectedHotbar-1];  
                }
               isPlayerWieldingItem=false;   
            }else{
                return;
            }
             
    }
    async void ThrowTNT()
    {
        Vector3 tntPos = headPos.position + headPos.forward;
        EntityBeh tntEntity=EntityBeh.SpawnNewEntity(tntPos.x,tntPos.y,tntPos.z,2);
        await UniTask.WaitUntil(() => tntEntity.GetComponent<TNTBeh>().rigidbody!=null);
        tntEntity.GetComponent<TNTBeh>().AddForce((headPos.forward * 16));
        inventoryItemNumberDic[currentSelectedHotbar - 1]--;
        AttackAnimate();
        Invoke("cancelAttackInvoke", 0.16f);
    }
    public void UpgradeArmor()
    {
        if (playerArmorPoints >= playerMaxArmorPoints)
        {
            return;
        }
        playerArmorPoints= Mathf.Min(playerArmorPoints + 2f,playerMaxArmorPoints);
        AudioSource.PlayClipAtPoint(playerEquipArmorClip, transform.position + new Vector3(0f, 0.5f, 0f), 1f);
        GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);
        inventoryItemNumberDic[currentSelectedHotbar - 1]--;
        AttackAnimate();
        Invoke("cancelAttackInvoke", 0.16f);
    }
    void RightClick(){
        if(cameraPosMode==1){
            return;
        }
        if(isPlayerWieldingItem){
            return;
        }
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(inventoryDic[currentSelectedHotbar-1]==154){
             PlayerEat();
                return;
        }
        if (inventoryDic[currentSelectedHotbar - 1] == 156)
        {
            ThrowTNT();
            return;
        }
        if (inventoryDic[currentSelectedHotbar - 1] == 158)
        {
            UpgradeArmor();
            return;
        }
        if (Physics.Raycast(ray,out info,5f)&&info.collider.gameObject.tag=="Entity"&&critAttackCD<=0f){
            
             if(inventoryDic[currentSelectedHotbar-1]==152){
                PlayerCritAttack();
                critAttackCD=1f;
                return;
            }
        }
        if(Physics.Raycast(ray,out info,5f)&&info.collider.gameObject.tag!="Entity"&&info.collider.gameObject.tag!="Player"){
            Vector3 blockPoint=info.point-mainCam.transform.forward*0.01f;

            if(!ItemIDToBlockID.ItemIDToBlockIDDic.ContainsKey(inventoryDic[currentSelectedHotbar-1])||ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar-1]]==-1){
                return;
            }
            if(collidingBlockOutline.GetComponent<BlockOutlineBeh>().isCollidingWithPlayer==true||collidingBlockOutline.GetComponent<BlockOutlineBeh>().isCollidingWithEntity==true){
               return;
            }
           
            WorldHelper.instance.SetBlockByHand(blockPoint,(short)ItemIDToBlockID.ItemIDToBlockIDDic[inventoryDic[currentSelectedHotbar-1]]);

            if(Chunk.blockAudioDic.ContainsKey(inventoryDic[currentSelectedHotbar-1])){
            AudioSource.PlayClipAtPoint(Chunk.blockAudioDic[inventoryDic[currentSelectedHotbar-1]],blockPoint,1f);
            }else{
                    Debug.Log("missing file");
            }
             
            inventoryItemNumberDic[currentSelectedHotbar-1]--;
                   
            WorldHelper.instance.StartUpdateAtPoint(blockPoint);
            AttackAnimate();
            Invoke("cancelAttackInvoke",0.16f);

        }
    }


    public int ReadPlayerJson(bool ExludePlayerInWorldIDData=false)
    {
        inventoryDic = new int[9];
        inventoryItemNumberDic = new int[9];

        gameWorldPlayerDataPath = WorldManager.gameWorldDataPath;

        if (!Directory.Exists(gameWorldPlayerDataPath + "unityMinecraftData"))
        {
            Directory.CreateDirectory(gameWorldPlayerDataPath + "unityMinecraftData");

        }
        if (!Directory.Exists(gameWorldPlayerDataPath + "unityMinecraftData/GameData"))
        {
            Directory.CreateDirectory(gameWorldPlayerDataPath + "unityMinecraftData/GameData");
        }

        if (!File.Exists(gameWorldPlayerDataPath + "unityMinecraftData" + "/GameData/playerdata.json"))
        {
            FileStream fs = File.Create(gameWorldPlayerDataPath + "unityMinecraftData" + "/GameData/playerdata.json");
            fs.Close();
        }

        byte[] worldPlayerData = File.ReadAllBytes(gameWorldPlayerDataPath + "unityMinecraftData/GameData/playerdata.json");
        //   isEntitiesReadFromDisk=true;

        PlayerData pd;
        if (worldPlayerData.Length > 0)
        {
            pd = MessagePackSerializer.Deserialize<PlayerData>(worldPlayerData);
        }
        else
        {
            pd = new PlayerData(20f, 0f, 150f, 0f, new int[9], new int[9], 0,0f);
            //    return;
        }
        Debug.Log("player in world ID" + pd.playerInWorldID);

        Debug.Log(!ExludePlayerInWorldIDData);
        Debug.Log(VoxelWorld.isWorldChanged);
        if (VoxelWorld.isWorldChanged == false&&!ExludePlayerInWorldIDData)
        {
            Debug.Log("switch world");
            if (VoxelWorld.currentWorld.worldID != pd.playerInWorldID)
            {
                switch (pd.playerInWorldID)
                {
                    case 0:
                        Debug.Log("in world 0");
                        SceneManagementHelper.SwitchToWorldWithoutSavingWithSceneChanged(0, 1);
                        break;
                    case 1:
                        Debug.Log("in world 1");
                        SceneManagementHelper.SwitchToWorldWithoutSavingWithSceneChanged(1, 2);
                        break;
                    default:

                        SceneManagementHelper.SwitchToWorldWithoutSavingWithSceneChanged(0, 1);
                        break;
                }
            }

        }
        Debug.Log(pd.posX + " " + pd.posY + " " + pd.posZ);
        if (pd.posX != 0f && pd.posY != 0f && pd.posZ != 0f && pd.inventoryDic != null && pd.inventoryItemNumberDic != null)
        {
            cc.enabled = false;
            transform.position = new Vector3(pd.posX, pd.posY, pd.posZ);
            cc.enabled = true;
            playerHealth = pd.playerHealth;
            inventoryDic = pd.inventoryDic;
            inventoryItemNumberDic = pd.inventoryItemNumberDic;
            playerArmorPoints = pd.playerArmorPoints;
        }
        else
        {
            cc.enabled = false;
            transform.position = new Vector3(0f, 150f, 0f);
            cc.enabled = true;
            inventoryDic = new int[9];
            inventoryItemNumberDic = new int[9];
            playerArmorPoints = 0f;
        }
      
        return pd.playerInWorldID;
    }



    
    public void SavePlayerData(){
        PlayerData pd=new PlayerData(playerHealth,transform.position.x,transform.position.y,transform.position.z,inventoryDic,inventoryItemNumberDic, VoxelWorld.currentWorld.worldID,playerArmorPoints);
   
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
