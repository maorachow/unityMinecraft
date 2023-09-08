using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.IO;
using Utf8Json;
using UnityEngine.EventSystems;

public class PlayerData{
    public float playerHealth;
    public float posX;
    public float posY;
    public float posZ;
    public int[] inventoryDic;
    public int[] inventoryItemNumberDic;
}
public class PlayerMove : MonoBehaviour
{   
    public static AudioClip playerDropItemClip;
    public AudioSource AS;
    public float playerCameraZShakeValue;
    public float playerCameraZShakeLerpValue;
    public PlayerInput pi; 
    public float playerHealth=20f;
    public float playerMaxHealth=20f;
    public Chunk curChunk;
    public Animator am;
    public static RuntimePlatform platform{get{return EntityBeh.platform;}set{platform=EntityBeh.platform;}}
    public static string gameWorldPlayerDataPath;
    public static Dictionary<int,string> blockNameDic=new Dictionary<int,string>();
    public static bool isBlockNameDicAdded=false;
    public int blockOnHandID=0;
    public int cameraPosMode=0;
    
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
    
    public float breakBlockCD=0.2f;
    public float moveSpeed=5f;
    public float gravity=-9.8f;
    public float playerY=0f;
    public float jumpHeight=2f;
    public float mouseSens=10f;
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
    public static GameObject pauseMenu;
    public float lerpItemSlotAxis;
    public Vector3 lerpPlayerVec;
    void Awake(){
        pi=new PlayerInput();
        pi.Enable();
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
        blockNameDic.Add(100,"Water");
        blockNameDic.Add(101,"Grass Crop");
        blockNameDic.Add(151,"Diamond Pickaxe");
        blockNameDic.Add(152,"Diamond Sword");
        blockNameDic.Add(153,"Diamond");
        isBlockNameDicAdded=true;
        }
        ReadPlayerJson();
    }
    public void SetHotbarNum(int num){
        
        currentSelectedHotbar=num;
        blockOnHandText.text=blockNameDic[inventoryDic[currentSelectedHotbar-1]];
    }
    public void BreakBlockButtonPress(){
        if(breakBlockCD<=0f){
            BreakBlock();
            breakBlockCD=0.3f;}
    }
    public void PlaceBlockButtonPress(){
        if(breakBlockCD<=0f){
            PlaceBlock();
            breakBlockCD=0.3f;}
    }

    void Start()
    {   
         Input.multiTouchEnabled = true;
        AS=GetComponent<AudioSource>();
        // pauseMenu.SetActive(true);
        currentSelectedHotbar=1;
        playerHandItem=transform.GetChild(0).GetChild(1).GetChild(1).GetChild(1).GetChild(0).gameObject.GetComponent<ItemOnHandBeh>();
        if(platform==RuntimePlatform.Android||platform==RuntimePlatform.IPhonePlayer){
            Application.targetFrameRate = 60;
        }else{
         Application.targetFrameRate = 1024;   
        }        
        playerDropItemClip=Resources.Load<AudioClip>("Audios/Pop");
        prefabBlockOutline=Resources.Load<GameObject>("Prefabs/blockoutline");
        blockOutline=Instantiate(prefabBlockOutline,transform.position,transform.rotation);
        collidingBlockOutline=Instantiate(prefabBlockOutline,transform.position,transform.rotation);

        blockOnHandText=GameObject.Find("blockonhandIDtext").GetComponent<Text>();
   
        viewRange=64;
        am=transform.GetChild(0).GetComponent<Animator>();
        cc=GetComponent<CharacterController>();
        headPos=transform.GetChild(0).GetChild(0);
        playerMoveRef=headPos.GetChild(1);
        playerBodyPos=transform.GetChild(0).GetChild(1);
        mainCam=headPos.GetChild(0).gameObject.GetComponent<Camera>();
        chunkPrefab=Resources.Load<Chunk>("Prefabs/chunk");
      //  InvokeRepeating("SendChunkReleaseMessage",1f,3f);
      GameUIBeh.instance.CloseCraftingUI();
         GameUIBeh.instance.Resume();
    }


  //  void SendChunkReleaseMessage(){
  //      foreach(var c in Chunk.Chunks){
   //         c.Value.SendMessage("TryReleaseChunk");
  //      }
  //  }
    void MouseLock()
    {
        if(isPlayerKilled==true||GameUIBeh.isPaused==true|GameUIBeh.instance.isCraftingMenuOpened==true){
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
         playerHandItem.SendMessage("OnBlockIDChanged",inventoryDic[currentSelectedHotbar-1]);  
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
    public void ApplyDamageAndKnockback(float damageAmount,Vector3 knockback){
        AS.Play();
        //GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        playerCameraZShakeLerpValue=Random.Range(-15f,15f);
        playerHealth-=damageAmount;
         GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        playerMotionVec=knockback;
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
        transform.rotation=Quaternion.Euler(0f,0f,-90f);
        cc.enabled = true;
         
        isPlayerKilled=true;
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
        if(curChunk==null||curChunk.isMeshBuildCompleted==false){
            UpdateWorld();
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
           // Debug.Log(Input.GetAxis("Mouse ScrollWheel"));
       //     blockOnHandID+=(int)(Input.GetAxis("Mouse ScrollWheel")*15f);
       
            pi.Player.SwitchItemSlot.performed+=ctx=>{ 
                lerpItemSlotAxis=pi.Player.SwitchItemSlot.ReadValue<Vector2>().y*0.5f;
            lerpItemSlotAxis=Mathf.Lerp(lerpItemSlotAxis,1.5f,Time.deltaTime);
            
                if(Mathf.Abs(pi.Player.SwitchItemSlot.ReadValue<Vector2>().y)>0f){
                currentSelectedHotbar-=(int)pi.Player.SwitchItemSlot.ReadValue<Vector2>().y;
                currentSelectedHotbar=Mathf.Clamp(currentSelectedHotbar,1,9);
                blockOnHandText.text=blockNameDic[inventoryDic[currentSelectedHotbar-1]];
        
          } };
           
          
        //    blockOnHandID=Mathf.Clamp(blockOnHandID,0,9);
            
       
        
      
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
                 if(platform==RuntimePlatform.Android||platform==RuntimePlatform.IPhonePlayer){
            for(int i=0;i<Input.touches.Length;i++){
             EventSystem eventSystem = EventSystem.current;
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.touches[i].rawPosition;
            //射线检测ui
            List<RaycastResult> uiRaycastResultCache = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerEventData, uiRaycastResultCache);
            if (uiRaycastResultCache.Count == 0){
                 mouseX=Input.touches[i].deltaPosition.x;mouseY=Input.touches[i].deltaPosition.y;
            }
        }
        }else{
            mouseX=pi.Player.MouseDrag.ReadValue<Vector2>().x;mouseY=pi.Player.MouseDrag.ReadValue<Vector2>().y;
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
     if(cc.velocity.magnitude>0.1f){
            UpdateWorld();
     }
         UpdateInventory();
        
        
    }
    public void DropItemButtonOnClick(){
        PlayerDropItem(currentSelectedHotbar-1);
    }
    void PlayerDropItem(int slotID){

        if(inventoryItemNumberDic[slotID]>0){
            AudioSource.PlayClipAtPoint(playerDropItemClip,transform.position,1f);
            StartCoroutine(ItemEntityBeh.SpawnNewItem(headPos.position.x,headPos.position.y,headPos.position.z,inventoryDic[slotID],(headPos.forward*12)));
            inventoryItemNumberDic[slotID]--;
            if(inventoryItemNumberDic[slotID]-1<=0){
       
            }
  
                AttackAnimate();
                Invoke("cancelAttackInvoke",0.1f);
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
    void UpdateWorld()
    {
        

    for (float x = transform.position.x - viewRange; x < transform.position.x + viewRange; x += Chunk.chunkWidth)
        {
            for (float z = transform.position.z - viewRange; z < transform.position.z + viewRange; z += Chunk.chunkWidth)
            {
                Vector3 pos = new Vector3(x, 0, z);
               // pos.x = Mathf.Floor(pos.x / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
            //    pos.z = Mathf.Floor(pos.z / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
                Vector2Int chunkPos=Chunk.Vec3ToChunkPos(pos);
                Chunk chunk = Chunk.GetChunk(chunkPos);
                if (chunk != null||Chunk.GetUnloadedChunk(chunkPos)!=null) {continue;}else{
                    chunk=ObjectPools.chunkPool.Get(chunkPos).GetComponent<Chunk>();
               //     chunk.transform.position=new Vector3(chunkPos.x,0,chunkPos.y);
               //     chunk.isChunkPosInited=true;
               if(chunk!=null){
                chunk.SendMessage("ReInitData");
               }
                    
         //          WorldManager.chunksToLoad.Add(chunk);
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
    void AttackEnemy(GameObject go){
         AttackAnimate();
     Invoke("cancelAttackInvoke",0.1f);
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
            Chunk.SetBlockByHand(blockPoint,0);
            GameObject a=ObjectPools.particleEffectPool.Get();
            a.transform.position=new Vector3(Chunk.Vec3ToBlockPos(blockPoint).x+0.5f,Chunk.Vec3ToBlockPos(blockPoint).y+0.5f,Chunk.Vec3ToBlockPos(blockPoint).z+0.5f);
            a.GetComponent<particleAndEffectBeh>().blockID=tmpID;
            a.GetComponent<particleAndEffectBeh>().SendMessage("EmitParticle");
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
     Invoke("cancelAttackInvoke",0.1f);
        }
    }

    void PlaceBlock(){
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)&&info.collider.gameObject.tag!="Entity"&&info.collider.gameObject.tag!="Player"){
            Vector3 blockPoint=info.point-headPos.forward*0.01f;
            if(inventoryDic[currentSelectedHotbar-1]>150&&inventoryDic[currentSelectedHotbar-1]<=200){
                return;
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
     Invoke("cancelAttackInvoke",0.1f);
        }
    }
    
   
    public void ReadPlayerJson(){
          inventoryDic=new int[9];
            inventoryItemNumberDic=new int[9];
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
        PlayerData pd=new PlayerData();
        pd.playerHealth=playerHealth;
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
