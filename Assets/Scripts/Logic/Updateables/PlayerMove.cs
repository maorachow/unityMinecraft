using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MessagePack;
using System;
using Random = UnityEngine.Random;



public partial class PlayerMove: MonoBehaviour,IAttackableEntityTarget,IInventoryOwner
{
     

    
    public AudioSource AS;
    public float playerCameraZShakeValue;
    public float playerCameraZShakeLerpValue;

    public float playerHealth = 20f;
    public float playerMaxHealth = 20f;
    public float playerArmorPoints = 0f;
    public float playerMaxArmorPoints = 20f;
    public Chunk curChunk;
    public Animator am;

    public static string gameWorldPlayerDataPath;

    public static bool isBlockNameDicAdded = false;
    public int blockOnHandID = 0;
    public int cameraPosMode = 0; //0fp 1sp 2tp
    public static GameObject playerSweepParticlePrefab;
    public GameObject prefabBlockOutline;
    public GameObject blockOutline;
   
    public Transform cameraPos;
    public Transform headPos;
    public Transform playerHeadCenterRef;
    public Transform playerMoveRef;
    public Transform playerBodyPos;

    public Camera mainCam;
    public CharacterController cc;
    public float cameraX;
    public float critAttackCD = 1f;
    public float breakBlockCD = 0.2f;
    public float moveSpeed = 5f;
    public static float gravity = -9.8f;
    public float playerY = 0f;
    public float jumpHeight = 2;

 
    public float currentSpeed;
    public Vector3 playerVec;
    public Vector3 playerMotionVec;
    public bool isDied { get; set; }= false;
    public ItemOnHandBeh playerHandItem;
    public int currentSelectedHotbar = 5;

    public ItemInventory inventory { get; set; }
    public IAttackableEntityTarget primaryTargetEntity
    {
        get;
        set;
    }
    public List<IAttackableEntityTarget> primaryAttackerEntities
    {
        get;
        set;
    }

    public void ClearPrimaryTarget()
    {
         
    }

    public Transform entityTransformRef
    {
        get;
        set;
    }

    public static float chunkStrongLoadingRange = 48;

    //public static GameObject pauseMenu;
    public float lerpItemSlotAxis;
    public Vector3 lerpPlayerVec;
   
    public int curHeadBlockID;
    public int prevHeadBlockID;
    public int curFootBlockID;
    public int curUnderFootBlockID;
    public int prevUnderFootBlockID;
    public int prevFootBlockID;
    public float playerMoveDrag = 0f;
    public bool isPlayerInGround = false;
     
    public bool isUsingCC = true;
  

    public SimpleAxisAlignedBB playerBound;
    public Dictionary<Vector3Int, SimpleAxisAlignedBB> blocksAround;
    public static PlayerMove instance;
    public float playerTeleportingCD = 0f;


    public void TryGetInventoryData(out int[] inventoryDic, out int[] InventoryNumberDic)
    {
        inventoryDic = inventory.inventoryDic;
        InventoryNumberDic = inventory.inventoryItemNumberDic;
    }
  /*  public Dictionary<Vector3Int, SimpleAxisAlignedBB> GetBlocksAround(SimpleAxisAlignedBB aabb)
    {
        int minX = floorFloat(aabb.getMinX() - 0.1f);
        int minY = floorFloat(aabb.getMinY() - 0.1f);
        int minZ = floorFloat(aabb.getMinZ() - 0.1f);
        int maxX = ceilFloat(aabb.getMaxX() + 0.1f);
        int maxY = ceilFloat(aabb.getMaxY() + 0.1f);
        int maxZ = ceilFloat(aabb.getMaxZ() + 0.1f);

        this.blocksAround = new Dictionary<Vector3Int, SimpleAxisAlignedBB>();

        for (int z = minZ - 1; z <= maxZ + 1; z++)
        {
            for (int x = minX - 1; x <= maxX + 1; x++)
            {
                for (int y = minY - 1; y <= maxY + 1; y++)
                {
                    int blockID = WorldHelper.instance.GetBlock(new Vector3(x, y, z));
                    if (blockID > 0 && blockID < 100)
                    {
                        this.blocksAround.Add(new Vector3Int(x, y, z),
                            new SimpleAxisAlignedBB(x, y, z, x + 1, y + 1, z + 1));
                    }
                }
            }
        }


        return this.blocksAround;
    }

    public static int floorFloat(float n)
    {
        int i = (int)n;

        return n >= i ? i : i - 1;
    }

    public static int ceilFloat(float n)
    {
        int i = (int)(n + 1);
        return n >= i ? i : i - 1;
    }

    void Move(float dx, float dy, float dz)
    {
        //  this.ySize *= 0.4;

        float movX = dx;
        float movY = dy;
        float movZ = dz;
        if (blocksAround.Count == 0)
        {
            playerBound = playerBound.offset(0, dy, 0);
            playerBound = playerBound.offset(dx, 0, 0);
            playerBound = playerBound.offset(0, 0, dz);
        }


        foreach (var bb in blocksAround)
        {
            dy = bb.Value.calculateYOffset(playerBound, dy);
        }

        playerBound = playerBound.offset(0, dy, 0);

        //      bool fallingFlag = (this.onGround || (dy != movY && movY < 0));

        foreach (var bb in blocksAround)
        {
            dx = bb.Value.calculateXOffset(playerBound, dx);
        }

        playerBound = playerBound.offset(dx, 0, 0);

        foreach (var bb in blocksAround)
        {
            dz = bb.Value.calculateZOffset(playerBound, dz);
        }

        playerBound = playerBound.offset(0, 0, dz);
    }*/


    void Awake()
    {

        instance = this;
       
        chunkStrongLoadingRange = 64;
     
        am = transform.GetChild(0).GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        headPos = transform.GetChild(0).GetChild(0);
        playerMoveRef = headPos.GetChild(1);
        playerHeadCenterRef = headPos.GetChild(3);
        playerBodyPos = transform.GetChild(0).GetChild(1);
        mainCam = headPos.GetChild(0).gameObject.GetComponent<Camera>();
        mainCam.fieldOfView = GlobalGameOptions.inGameFOV;
        cameraPos = mainCam.transform;
        inventory = new ItemInventory(this, 18);
       
        PlayerUIMediator.instance.player = this;
        PlayerUIMediator.instance.UpdatePlayerStats();
    }

    void OnDestroy()
    {
            curChunk = null;
       
            PlayerUIMediator.instance.gameUI = null;
            
        
    }

    public void SetHotbarNum(int num)
    {
        Debug.Log(num);
        currentSelectedHotbar = num;
    }

    public void BreakBlockButtonPress()
    {
        if (isDied == true || GlobalGameOptions.isGamePaused == true || PlayerUIMediator.instance.isCraftingUIOpened == true)
        {
            return;
        }

        if (breakBlockCD <= 0f)
        {
            LeftClick();
            breakBlockCD = 0.3f;
        }
    }

    public void PlaceBlockButtonPress()
    {
        if (isDied == true || GlobalGameOptions.isGamePaused == true || PlayerUIMediator.instance.isCraftingUIOpened == true)
        {
            return;
        }

        if (breakBlockCD <= 0f)
        {
            RightClick();
            breakBlockCD = 0.3f;
        }
    }

    void Start()
    {
        //    ReadPlayerJson();

      
    

        //    pi=new PlayerInput();
        //    pi.Enable();
        Input.multiTouchEnabled = true;
        GameDataPersistenceManager.instance.LoadPlayerData();
        FetchPlayerData();
        AS = headPos. GetComponent<AudioSource>();
        // pauseMenu.SetActive(true);
        currentSelectedHotbar = 1;
      
        playerHandItem = transform.GetChild(0).GetChild(1).GetChild(1).GetChild(1).GetChild(0).gameObject
            .GetComponent<ItemOnHandBeh>();
        
        
        prefabBlockOutline = Resources.Load<GameObject>("Prefabs/blockoutline");
      
        blockOutline = Instantiate(prefabBlockOutline, new Vector3(0, 0, 0),Quaternion.identity);
    //    collidingBlockOutline = Instantiate(prefabBlockOutline, new Vector3(0,0,0), transform.rotation);


        //  InvokeRepeating("SendChunkReleaseMessage",1f,3f);
      

      //  transform.GetChild(0).localRotation = Quaternion.Euler(0f, 0f, 0f);
     //   transform.GetChild(0).localPosition = new Vector3(0f, 0f, 0f);
        playerSweepParticlePrefab = Resources.Load<GameObject>("Prefabs/playersweepparticle");
        //     playerBound=new SimpleAxisAlignedBB(transform.position-new Vector3(0.3f,0.5f,0.3f),transform.position+new Vector3(0.3f,0.9f,0.3f));
      //  GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
     //   GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);
        PlayerInputManager.instance.switchCameraPosAction = PlayerUIMediator.instance.PlayerSwitchCameraPosAction;
        PlayerInputManager.instance.pauseOrResumeAction = PlayerUIMediator.instance. PauseOrResume;
        PlayerInputManager.instance.dropItemButtonAction = PlayerUIMediator.instance.DropItemButtonOnClick;
        PlayerInputManager.instance.selectHotbarButtonAction = PlayerUIMediator.instance.SetHotbarNum;
        PlayerInputManager.instance.openInventoryButtonAction = PlayerUIMediator.instance.OpenOrCloseCraftingUI;
        PlayerInputManager.instance.playerLeftClickActions.Add(PlayerUIMediator.instance.MouseLock);
        PlayerInputManager.instance.playerLeftClickActions.Add(PlayerUIMediator.instance.BreakBlockButtonPress);

        PlayerInputManager.instance.playerRightClickActions.Add(PlayerUIMediator.instance.PlaceBlockButtonPress);
       
        entityTransformRef = headPos;
        primaryAttackerEntities = new List<IAttackableEntityTarget>();
        PlayerUIMediator.instance.UpdatePlayerStats(true);
    }


    //  void SendChunkReleaseMessage(){
    //      foreach(var c in Chunk.Chunks){
    //         c.Value.SendMessage("TryReleaseChunk");
    //      }
    //  }



    
    private float curAnimSpeed;
    float Speed()
    {
       
        if (GlobalGameOptions.isGamePaused == true)
        {
            return curAnimSpeed;
        }

        curAnimSpeed = Vector3.Magnitude(new Vector3(cc.velocity.x, 0f, cc.velocity.z));
        return curAnimSpeed;
    }
    float Speed(Vector3 optionalMotionVec)
    {

        if (GlobalGameOptions.isGamePaused == true)
        {
            return curAnimSpeed;
        }

        curAnimSpeed = Vector3.Magnitude(new Vector3(cc.velocity.x, 0f, cc.velocity.z)+ optionalMotionVec);
        return curAnimSpeed;
    }
    void SetAnimationStatePlayerViewAngle(float viewAngle)
    {
        float viewAngleFactor = -viewAngle / 180f;
        viewAngleFactor += 0.5f;
        am.SetFloat("playerviewanglefactorvertical", viewAngleFactor);
    }

    void InvokeRevertColor()
    {
        transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color =
            Color.white;
        transform.GetChild(0).GetChild(1).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color =
            Color.white;
        transform.GetChild(0).GetChild(1).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color =
            Color.white;
        transform.GetChild(0).GetChild(1).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color =
            Color.white;
    }
    void ChangePlayerColor(Color color)
    {
        transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<MeshRenderer>().material.color = color;
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = color;
        transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color =
            color;
        transform.GetChild(0).GetChild(1).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color =
            color;
        transform.GetChild(0).GetChild(1).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color =
            color;
        transform.GetChild(0).GetChild(1).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color =
            color;
    }
    public void ApplyDamageAndKnockback(float damageAmount, Vector3 knockback)
    {
        AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("playerHurtClip"));
        //GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        playerCameraZShakeLerpValue = Random.Range(-15f, 15f);
        float reducedDamage = Mathf.Max((1f - (playerArmorPoints * 4f / 100f)) * damageAmount, 0f);
        playerHealth -= reducedDamage;

        playerArmorPoints -= damageAmount * 0.1f;
        playerArmorPoints = MathF.Max(playerArmorPoints, 0f);
     //   GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
   //     GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);
        playerMotionVec = knockback;
        ChangePlayerColor(Color.red);
        Invoke("InvokeRevertColor", 0.2f);
    }

    private Vector3 lastKilledHeadPos=new Vector3();
    private Vector3 lastKilledHeadForward = new Vector3();
    private Quaternion lastKilledHeadRotation=Quaternion.identity;
    public void PlayerDie()
    {
        AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("playerHurtClip"));
        playerCameraZShakeLerpValue = 0f;
        playerCameraZShakeValue = 0f;
  //      GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        playerArmorPoints = 0f;
  //      GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);
        //   transform.position=new Vector3(0f,150f,0f);
        for (int i = 0; i < inventory.inventorySlotCount; i++)
        {
            int itemID, itemNumber;
            inventory.GetItemInfoFromSlot(i, out itemID, out itemNumber);
            DropItem(itemID, itemNumber);
            inventory.ClearSlot(i);
        }


        foreach (var item in primaryAttackerEntities)
        {
            if (item.primaryAttackerEntities != null)
            {
                item.primaryAttackerEntities.Clear();

                item.ClearPrimaryTarget();
            }

        }
        primaryAttackerEntities.Clear();
        primaryTargetEntity = null;


      
        /*    cc.enabled = false;
            transform.GetChild(0).localRotation = Quaternion.Euler(0f, 0f, -90f);
            transform.GetChild(0).localPosition = new Vector3(0.65f, -0.68f, 0f);
            cc.enabled = true;*/
        if (playerHeadCenterRef != null)
        {
            lastKilledHeadPos = playerHeadCenterRef.position;

            lastKilledHeadForward =Vector3.Normalize(playerHeadCenterRef.forward + new Vector3(0f, -0.7f, 0f));
            lastKilledHeadRotation=Quaternion.LookRotation(lastKilledHeadForward,Vector3.up);
        }
       
        isDied = true;

     

        am.SetBool("iskilled", true);
        am.SetFloat("speed", 0f);
     
    }



    public void PlayerRespawn()
    {
        playerMotionVec = Vector3.zero;
        am.SetBool("iskilled", false);
        am.SetFloat("speed", 0f);
        cc.enabled = false;
        transform.rotation = Quaternion.identity;
        transform.position = new Vector3(0f, 150f, 0f);
        //     playerBound=new SimpleAxisAlignedBB(new Vector3(0f,150f,0f)-new Vector3(0.3f,0.5f,0.3f),new Vector3(0f,150f,0f)+new Vector3(0.3f,0.9f,0.3f));
        headPos.localRotation = Quaternion.identity;
        playerBodyPos.localRotation = Quaternion.identity;
     

     //   transform.GetChild(0).GetChild(1).rotation = Quaternion.Euler(0f, 0f, 0f);
        cc.enabled = true;


        playerHealth = 20f;
        isDied = false;
        ChangePlayerColor(Color.white);
        Invoke("InvokeRevertColor",0.03f);
       
       
        transform.position = new Vector3(0f, 150f, 0f);
    }

  /*  public void PauseOrResume()
    {
        //     Debug.Log("Pause");
    
        if (GlobalGameOptions.isGamePaused == false)
        {
            GameUIBeh.instance.PauseGame();
            return;
        }
        else
        {
           
            GameUIBeh.instance.Resume();
            if (GameUIBeh.instance.isCraftingMenuOpened == true)
            {
                GameUIBeh.instance.CloseCraftingUI();
            }
            return;
        }
    }

    public void OpenOrCloseCraftingUI()
    {
       
        
        if (GameUIBeh.instance.isCraftingMenuOpened == true)
        {
            GameUIBeh.instance.CloseCraftingUI();
        }
        else
        {
            if (GameUIBeh.instance.isPauseMenuOpened == true)
            {
                return;
            }
            GameUIBeh.instance.OpenCraftingUI();
        }
    }*/

    public void SwitchCameraPosAction()
    {
        cameraPosMode++;
        if (cameraPosMode >= 3)
        {
            cameraPosMode = 0;
        }
    }
    [SerializeField]
    private bool isGroundedLastMove=false;

    private float playerLastJumpYVelocity = 0;
    void Update()
    {
        /*    if(Input.GetKeyDown(KeyCode.Escape)){
              PauseOrResume();
            }*/
        PlayerUIMediator.instance.UpdatePlayerStats();
        if (GlobalGameOptions.isGamePaused == true||Time.deltaTime<=0f)
        {
            return;
        }

        if (curChunk == null)
        {
            am.SetFloat("speed", 0f);
            return;
        }

        /*     if(Input.GetKeyDown(KeyCode.E)){

                 OpenOrCloseCraftingUI();
               }*/
        playerCameraZShakeLerpValue = Mathf.Lerp(playerCameraZShakeLerpValue, 0f, 15f * Time.deltaTime);
        playerCameraZShakeValue = Mathf.Lerp(playerCameraZShakeLerpValue, 0f, 15f * Time.deltaTime);


        if (currentSelectedHotbar - 1 >= 0 && currentSelectedHotbar - 1 < inventory.inventorySlotCount)
        {
            playerHandItem.blockID = inventory.inventoryDic[currentSelectedHotbar - 1];
        }

        if (playerHealth <= 0f && isDied == false)
        {
            PlayerDie();
        }

        if (transform.position.y < -40f && isDied == false)
        {
            PlayerDie();
        }

        if (isDied == true)
        {
            playerMotionVec = Vector3.Lerp(playerMotionVec, Vector3.zero, 5f * Time.deltaTime);
        }
        else
        {
            playerMotionVec = Vector3.Lerp(playerMotionVec, Vector3.zero, 2f * Time.deltaTime);
        }
       

        Vector3 speedMotionVec=new Vector3();

        if ((isGroundedLastMove != true))
        {
            if (curFootBlockID == 100)
            {
                playerY += gravity * Time.deltaTime;
               

                if (playerLastJumpYVelocity > 0f)
                {
                    playerY = playerLastJumpYVelocity*0.5f;
                    playerLastJumpYVelocity = 0f;

                }
                playerY = Mathf.Clamp(playerY, -5f,6f);
            }
            else
            {
                playerY += gravity * Time.deltaTime;
            }
        }
        else
        {
            playerY = -1e-5f;
            if (playerLastJumpYVelocity > 0f)
            {
                playerY = playerLastJumpYVelocity;
               playerLastJumpYVelocity = 0f;

            }

            // playerMotionVec.y=0f;
        }
        if (curFootBlockID == 100)
        {
            playerY = Mathf.Clamp(playerY, -1f, 5f);
        }
        playerVec.y = playerY;

        if (cc.enabled == true)
        {

            if (isPlayerInGround == true)
            {
                cc.enabled = false;
                transform.position += Time.deltaTime * new Vector3(0f, 5f, 0f);
                cc.enabled = true;
            }
            else
            {
                cc.Move(
                    playerMotionVec * Time.deltaTime + new Vector3(0f, playerVec.y, 0f) * 5f * Time.deltaTime);
                isGroundedLastMove = cc.isGrounded;
                speedMotionVec = new Vector3(cc.velocity.x, 0f, cc.velocity.z);

            }

           
        }
        
        if (isDied == true)
        {

            RaycastHit infoBack1 ;

            bool isBackPointHit1 = Physics.Linecast(lastKilledHeadPos, lastKilledHeadPos +lastKilledHeadForward * (-8f), out infoBack1, LayerMask.GetMask("Default"));

            Vector3 diedPlayerCameraPos = lastKilledHeadPos+ lastKilledHeadForward * (-8f);

            if (isBackPointHit1 == true)
            {
                cameraPos.position = Vector3.Lerp(lastKilledHeadPos, infoBack1.point, 0.92f);
                cameraPos.rotation = lastKilledHeadRotation;
            }
            else
            {
                cameraPos.position = diedPlayerCameraPos;
                cameraPos.rotation = lastKilledHeadRotation;
            }

            ChangePlayerColor(Color.red);
            return;
        }

     



      
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
        if (PlayerInputManager.instance.isPlayerSpeededUp == true)
        {
            moveSpeed = 10f;
        }
        else
        {
            moveSpeed = 5f;
        }

   
        // Debug.Log(Input.GetAxis("Mouse ScrollWheel"));
        //     blockOnHandID+=(int)(Input.GetAxis("Mouse ScrollWheel")*15f);


        if (isPlayerWieldingItem == false)
        {
            currentSelectedHotbar += (int)PlayerInputManager.instance.switchItemSlotAxis;
            currentSelectedHotbar = Mathf.Clamp(currentSelectedHotbar, 1, 9);
        }
   


        


        /*     pi.Player.SwitchCameraPos.performed+=ctx=>{
               cameraPosMode++;
               if(cameraPosMode>=3){
                   cameraPosMode=0;
               }
             };*/
      //  Ray playerHeadForwardRay = new Ray(headPos.position, headPos.position + headPos.forward * 5f);
       // Ray playerHeadBackRay = new Ray(headPos.position, headPos.position + headPos.forward * -5f);
        //    blockOnHandID=Mathf.Clamp(blockOnHandID,0,9);
        //     Debug.DrawLine(headPos.position,headPos.position+headPos.forward*5f,Color.green);
        //     Debug.DrawLine(headPos.position,headPos.position+headPos.forward*-5f,Color.green);
        RaycastHit infoForward = new RaycastHit();
        RaycastHit infoBack = new RaycastHit();
        bool isForwardPointHit = false;
        bool isBackPointHit = false;
        if (Physics.Linecast(playerHeadCenterRef.position, playerHeadCenterRef.position + playerHeadCenterRef.forward * 5f, out infoForward, LayerMask.GetMask("Default")))
        {
            isForwardPointHit = true;
            //     Debug.DrawLine(infoForward.point,infoForward.point+new Vector3(0f,1f,0f),Color.green);
        }

        if (Physics.Linecast(playerHeadCenterRef.position, playerHeadCenterRef.position + playerHeadCenterRef.forward * (-5f), out infoBack, LayerMask.GetMask("Default")))
        {
            isBackPointHit = true;
            //   Debug.DrawLine(infoBack.point,infoBack.point+new Vector3(0f,1f,0f),Color.green);
        }

        switch (cameraPosMode)
        {
            case 0:
                cameraPos.localPosition = new Vector3(0f, 0.28f, -0.1f);
                cameraPos.localEulerAngles = new Vector3(0f, 0f, 0f);
                break;
            case 1:
                if (isForwardPointHit == true)
                {
                    cameraPos.position = Vector3.Lerp(playerHeadCenterRef.position, infoForward.point, 0.92f);
                }
                else
                {
                    cameraPos.localPosition = new Vector3(0f, 0f, 5f) + new Vector3(0f, 0.28f, -0.1f);
                }

                cameraPos.localEulerAngles = new Vector3(0f, -180f, 0f);
                break;
            case 2:
                if (isBackPointHit == true)
                {
                    cameraPos.position = Vector3.Lerp(playerHeadCenterRef.position, infoBack.point, 0.92f);
                }
                else
                {
                    cameraPos.localPosition = new Vector3(0f, 0f, -5f) + new Vector3(0f, 0.28f, -0.1f);
                }

                cameraPos.localEulerAngles = new Vector3(0f, 0f, 0f);
                break;
        }

         







      





        float mouseX = PlayerInputManager.instance.mouseDelta.x;
        float mouseY = PlayerInputManager.instance.mouseDelta.y;


       // mouseY = Mathf.Clamp(mouseY, -90f, 90f);
        cameraX -= mouseY;
        cameraX = Mathf.Clamp(cameraX, -90f, 90f);

        SetAnimationStatePlayerViewAngle(cameraX);

        if (PlayerInputManager.instance.playerMoveVec != Vector2.zero && PlayerUIMediator.instance.isCraftingUIOpened == false)
        {
            playerVec = new Vector3(PlayerInputManager.instance.playerMoveVec.y, 0f,
                PlayerInputManager.instance.playerMoveVec.x);

            lerpPlayerVec = Vector3.Lerp(lerpPlayerVec, playerVec, 7f * Time.deltaTime);
        }
        else
        {
            lerpPlayerVec = Vector3.Lerp(lerpPlayerVec, Vector3.zero, 7f * Time.deltaTime);
        }


       
        headPos.localEulerAngles += new Vector3(0f, mouseX, 0f);
        headPos.localEulerAngles = new Vector3(cameraX, headPos.localEulerAngles.y, playerCameraZShakeValue);
      
        playerMoveRef.eulerAngles = new Vector3(0f, headPos.eulerAngles.y, headPos.eulerAngles.z);
        playerBodyPos.eulerAngles=new Vector3(0f,playerBodyPos.eulerAngles.y, 0f);
        Vector3 headHorizontalForward = playerMoveRef.forward;
        Vector3 bodyHorizontalForward = new Vector3(playerBodyPos.forward.x, 0, playerBodyPos.forward.z).normalized;
        if (Vector3.Dot(headHorizontalForward, bodyHorizontalForward) < 0.2f)
        {
            playerBodyPos.rotation = Quaternion.Slerp(playerBodyPos.rotation, playerMoveRef.rotation, 5f * Time.deltaTime);
        }else if (isPlayerWieldingItem==true)
        {
             
                playerBodyPos.rotation = Quaternion.Slerp(playerBodyPos.rotation, playerMoveRef.rotation, 5f * Time.deltaTime);
            
        }

        else
        {
            if (lerpPlayerVec.magnitude > 0.5f)
            {
                Vector3 playerBodyCurMoveVecForward =
                    (playerMoveRef.forward * (lerpPlayerVec.x * 0.5f + 0.6f) + playerMoveRef.right * lerpPlayerVec.z).normalized;
                Quaternion curMoveVecRotation = Quaternion.LookRotation(playerBodyCurMoveVecForward, Vector3.up);

                playerBodyPos.rotation = Quaternion.Slerp(playerBodyPos.rotation, curMoveVecRotation, 15f * Time.deltaTime);
            }

        
        }

        //


       

        if ((isGroundedLastMove == true || curFootBlockID == 100 ) &&
            PlayerInputManager.instance.isJumping == true)
        {
            if (curFootBlockID == 100)
            {
                playerLastJumpYVelocity = jumpHeight / 2f;
            }
            else
            {
                playerLastJumpYVelocity = jumpHeight;
            }
        }

      

        
            
            cc.Move((playerMoveRef.forward * lerpPlayerVec.x + playerMoveRef.right * lerpPlayerVec.z) * moveSpeed *
                    (1f - playerMoveDrag) * Time.deltaTime );
            currentSpeed = Speed(speedMotionVec) ;
           
            am.SetFloat("speed", currentSpeed);
        

        

        if (breakBlockCD > 0f)
        {
            breakBlockCD -= Time.deltaTime;
        }

        if (critAttackCD > 0f)
        {
            critAttackCD -= Time.deltaTime;
        
        }

        if (critAttackCD <= 0f && isPlayerWieldingItem == true)
        {
            isPlayerWieldingItem = false;
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


    public void PlayerGroundSinkPrevent(CharacterController cc, int blockID, float dt)
    {
        if (WorldHelper.instance.GetBlockShape(blockID) is BlockShape.Solid|| WorldHelper.instance.GetBlockShape(blockID) is BlockShape.SolidTransparent) 
        {
            //      cc.Move(new Vector3(0f,dt*5f,0f));
            gravity = 0f;
            isPlayerInGround = true;
        }
        else
        {
            gravity = -9.8f;
            isPlayerInGround = false;
        }
    }


    void FixedUpdate()
    {
        // if(cc.velocity.magnitude>0.1f){
        //       UpdateWorld();
        //  }


        Ray ray = new Ray(playerHeadCenterRef.position, playerHeadCenterRef.forward);//mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        RaycastHit info;
        if (Physics.Raycast(ray, out info, 5f, ~LayerMask.GetMask("Ignore Raycast")) && info.collider.gameObject.tag != "Entity")
        {
            blockOutline.GetComponentInChildren<MeshRenderer>().enabled = true;




            Vector3 blockPoint = info.point + playerHeadCenterRef.forward * 0.01f;
            VoxelWorldRay voxelWorldRay = new VoxelWorldRay(ray.origin, ray.direction);
            VoxelCast.CastSpecificArea(voxelWorldRay, blockPoint, 1, out Vector3Int voxelCastResult, out _);

            blockOutline.transform.position = new Vector3(voxelCastResult.x + 0.5f,
                voxelCastResult.y + 0.5f,
                voxelCastResult.z + 0.5f);
            blockOutline.GetComponentInChildren<MeshRenderer>().enabled = true;
        }
        else if (Physics.Raycast(ray, out info, 10f, ~LayerMask.GetMask("Ignore Raycast")) && info.collider.gameObject.tag == "Entity")
        {
            Vector3 blockPoint = info.point + playerHeadCenterRef.forward * 0.01f;

            blockOutline.GetComponentInChildren<MeshRenderer>().enabled = false;

            if (info.collider.GetComponent<EndermanBeh>() != null)
            {
                (info.collider.GetComponent<EndermanBeh>() as IAttackableEntityTarget)?.TryAddPriamryAttackerEntity(this);
            }
        }
        else
        {
            blockOutline.GetComponentInChildren<MeshRenderer>().enabled = false;

        }

        if (cameraPosMode == 1)
        {
            blockOutline.GetComponentInChildren<MeshRenderer>().enabled = false;

        }


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

       
        if (curChunk == null)
        {
            curChunk = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(transform.position));
          
        }

        if (WorldHelper.instance.CheckIsPosInChunk(transform.position, curChunk) == false)
        {
            curChunk = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(transform.position));
          
            //   curChunkStrongLoader.isChunksNeededStrongLoading=true;
        }

        // Debug.Log(finalMoveVec);
        curFootBlockID = WorldHelper.instance.GetBlock(transform.position + new Vector3(0f,0.2f, 0f), curChunk);
        curUnderFootBlockID = WorldHelper.instance.GetBlock(transform.position + new Vector3(0f, -0.2f, 0f), curChunk);
        curHeadBlockID = WorldHelper.instance.GetBlock(cameraPos.position, curChunk);
        //     Debug.Log("block ID:"+ curUnderFootBlockID);
        //   Debug.Log(curChunk.ToString());
        if (curHeadBlockID != prevHeadBlockID)
        {
            GlobalVolumeWaterEffectBeh.instance.SwitchEffects(curHeadBlockID == 100);
            //    WaterSplashParticleBeh.instance.EmitParticleAtPosition(transform.position);
        }

        prevHeadBlockID = curHeadBlockID;
        PlayerGroundSinkPrevent(cc, curFootBlockID, Time.deltaTime);
        if (curFootBlockID != prevFootBlockID)
        {
            if (curFootBlockID == 100)
            {
                gravity = -1f;
                playerMoveDrag = 0.3f;
                AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("entitySinkClip1"));
                ParticleEffectManagerBeh.instance.EmitWaterSplashParticleAtPosition(transform.position);
            }
            else
            {
                gravity = -9.8f;
                playerMoveDrag = 0f;
            }
        }

        if (curUnderFootBlockID != prevUnderFootBlockID)
        {
            if (curUnderFootBlockID == 13)
            {
                AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("enderPortalTriggerClip"));
                ParticleEffectManagerBeh.instance.EmitEndermanParticleAtPosition(transform.position);
            }
        }

        prevFootBlockID = curFootBlockID;
        prevUnderFootBlockID = curUnderFootBlockID;
        //      TryStrongLoadChunkThread();
      
        inventory.UpdateInventory();

        if (curUnderFootBlockID == 13)
        {
            PlayerTryTeleportToEnderWorld();
        }
        else
        {
            if (playerTeleportingCD > 0f)
            {
                playerTeleportingCD -= Time.deltaTime;
            }
        }
        TryInteractWithItemEntities();
    }

    public void DropItemButtonOnClick()
    {
      


        inventory.GetItemInfoFromSlot(currentSelectedHotbar - 1,out int itemID,out int itemCount);
        if (inventory.TryRemoveItemFromSlot(currentSelectedHotbar - 1, 1) == true)
        {
            DropItem(itemID, 1);
        }
         
    }



   
    public void PlayerTryTeleportToEnderWorld()
    {
        if (playerTeleportingCD >= 4f)
        {
            switch (VoxelWorld.currentWorld.worldID)
            {
                case 0:
                    teleportingThroughWorldParams = new ValueTuple<bool, int, bool>(true, 1, true);
                 
                    break;
                case 1:
                    teleportingThroughWorldParams = new ValueTuple<bool, int, bool>(true, 0, true);
               
                    break;
                default:
                 
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



    [Obsolete]
    public int ReadPlayerJson(bool ExludePlayerInWorldIDData = false)
    {
        inventory = new ItemInventory(this, 18);

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

        byte[] worldPlayerData =
            File.ReadAllBytes(gameWorldPlayerDataPath + "unityMinecraftData/GameData/playerdata.json");
        //   isEntitiesReadFromDisk=true;

        PlayerData pd;
        if (worldPlayerData.Length > 0)
        {
            pd = MessagePackSerializer.Deserialize<PlayerData>(worldPlayerData);
        }
        else
        {
            pd = new PlayerData(20f, 0f, 150f, 0f, new int[18], new int[18], 0, 0f);
            //    return;
        }

        Debug.Log("player in world ID" + pd.playerInWorldID);

        Debug.Log(!ExludePlayerInWorldIDData);
        Debug.Log(VoxelWorld.isWorldChanged);
        if (VoxelWorld.isWorldChanged == false && !ExludePlayerInWorldIDData)
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
        if (pd.posX != 0f && pd.posY != 0f && pd.posZ != 0f && pd.inventoryDic != null &&
            pd.inventoryItemNumberDic != null)
        {
            cc.enabled = false;
            transform.position = new Vector3(pd.posX, pd.posY, pd.posZ);
            cc.enabled = true;
            playerHealth = pd.playerHealth;
            inventory.inventoryDic = pd.inventoryDic;
            inventory.inventoryItemNumberDic = pd.inventoryItemNumberDic;
            playerArmorPoints = pd.playerArmorPoints;
        }
        else
        {
            cc.enabled = false;
            transform.position = new Vector3(0f, 150f, 0f);
            cc.enabled = true;
            inventory.inventoryDic = new int[18];
            inventory.inventoryItemNumberDic = new int[18];
            playerArmorPoints = 0f;
        }

        return pd.playerInWorldID;
    }

    public ValueTuple<bool, int,bool>//is going to teleport, teleport to world index, is teleporting invoked by player
        teleportingThroughWorldParams = new ValueTuple<bool, int,bool>(false, -1,false);
    public void FetchPlayerData()
    {
        if (GameDataPersistenceManager.instance.isPlayerDataLoaded==false)
        {
            Debug.Log("player data has not been loaded");

        }

        PlayerData pd = GameDataPersistenceManager.instance.playerDataReadFromFile;
        if (pd.posX != 0f && pd.posY != 0f && pd.posZ != 0f)
        {
            cc.enabled = false;
            transform.position = new Vector3(pd.posX, pd.posY+0.03f, pd.posZ);
            cc.enabled = true;
            playerHealth = pd.playerHealth;
          
            playerArmorPoints = pd.playerArmorPoints;
        }
        else
        {
            cc.enabled = false;
            transform.position = new Vector3(0f, 150f, 0f);
            cc.enabled = true;
            playerArmorPoints = 0f;
        }

        if (pd.inventoryDic.Length >= 18 && pd.inventoryItemNumberDic.Length >= 18)
        {
            inventory.inventoryDic = pd.inventoryDic;
            inventory.inventoryItemNumberDic = pd.inventoryItemNumberDic;
        }
        else
        {
            inventory.inventoryDic = new int[18];
            inventory.inventoryItemNumberDic = new int[18];
        }

        if (GameDataPersistenceManager.instance.isPlayerTeleportedThroughWorld)
        {
            if (GameDataPersistenceManager.instance.isPlayerTeleportedThroughWorldSelfByPlayer)
            {
                cc.enabled = false;
                transform.position = new Vector3(0f, 150f, 0f);
                cc.enabled = true;
            }
        }
        else
        {
            if (VoxelWorld.currentWorld.worldID != pd.playerInWorldID)
            {
                teleportingThroughWorldParams = new ValueTuple<bool, int,bool>(true, pd.playerInWorldID,false);
            }
        }
    }

    public void SavePlayerDataToPersistenceManager()
    {
       
        GameDataPersistenceManager.instance.playerDataReadFromFile= new PlayerData(playerHealth, transform.position.x, transform.position.y, transform.position.z,
            (int[])inventory.inventoryDic.Clone(), (int[])inventory.inventoryItemNumberDic.Clone(), VoxelWorld.currentWorld.worldID, playerArmorPoints);
        GameDataPersistenceManager.instance.SavePlayerData();
    }

 

    public void SavePlayerData()
    {
        PlayerData pd = new PlayerData(playerHealth, transform.position.x, transform.position.y, transform.position.z,
            inventory.inventoryDic, inventory.inventoryItemNumberDic, VoxelWorld.currentWorld.worldID, playerArmorPoints);

        FileStream fs;
        if (File.Exists(gameWorldPlayerDataPath + "unityMinecraftData/GameData/playerdata.json"))
        {
            fs = new FileStream(gameWorldPlayerDataPath + "unityMinecraftData/GameData/playerdata.json",
                FileMode.Truncate, FileAccess.Write); //Truncate模式打开文件可以清空。
        }
        else
        {
            fs = new FileStream(gameWorldPlayerDataPath + "unityMinecraftData/GameData/playerdata.json",
                FileMode.Create, FileAccess.Write);
        }

        fs.Close();
        byte[] tmpData = MessagePackSerializer.Serialize(pd);
        File.WriteAllBytes(gameWorldPlayerDataPath + "unityMinecraftData/GameData/playerdata.json", tmpData);
    }
}