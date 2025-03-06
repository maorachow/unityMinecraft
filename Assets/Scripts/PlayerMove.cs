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
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Events;
using UnityEngine.Rendering;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;


[MessagePackObject]
public class PlayerData
{
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

    public PlayerData(float playerHealth, float posX, float posY, float posZ, int[] inventoryDic,
        int[] inventoryItemNumberDic, int playerInWorldID, float playerArmorPoints)
    {
        this.playerHealth = playerHealth;
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.inventoryDic = inventoryDic;
        this.inventoryItemNumberDic = inventoryItemNumberDic;
        this.playerInWorldID = playerInWorldID;
        this.playerArmorPoints = playerArmorPoints;
    }
}

public partial class PlayerMove: MonoBehaviour,IAttackableEntityTarget
{
    public static AudioClip playerSinkClip
    {
        get { return Random.Range(0f, 2f) > 1f ? playerSinkClip1 : playerSinkClip2; }
    }

    public static AudioClip playerSinkClip1;
    public static AudioClip playerSinkClip2;
    public static AudioClip playerEatClip;
    public static AudioClip playerBowShootClip;
    public static AudioClip playerDropItemClip;
    public static AudioClip playerSweepAttackClip;
    public static AudioClip playerEnterPortalClip;
    public static AudioClip playerEquipArmorClip;
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
    public Vector2 playerChunkLoadingPos;
    public int curHeadBlockID;
    public int prevHeadBlockID;
    public int curFootBlockID;
    public int curUnderFootBlockID;
    public int prevUnderFootBlockID;
    public int prevFootBlockID;
    public float playerMoveDrag = 0f;
    public bool isPlayerInGround = false;
    public bool isPlayerGrounded = false;
    public bool isUsingCC = true;
    public ChunkLoaderBase curChunkLoader;

    public SimpleAxisAlignedBB playerBound;
    public Dictionary<Vector3Int, SimpleAxisAlignedBB> blocksAround;
    public static PlayerMove instance;
    public float playerTeleportingCD = 0f;

    public Dictionary<Vector3Int, SimpleAxisAlignedBB> GetBlocksAround(SimpleAxisAlignedBB aabb)
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
    }


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
    }

    void OnDestroy()
    {
        curChunk = null;
    }

    public void SetHotbarNum(int num)
    {
        Debug.Log(num);
        currentSelectedHotbar = num;
    }

    public void BreakBlockButtonPress()
    {
        if (isDied == true || GlobalGameOptions.isGamePaused == true || GameUIBeh.instance.isCraftingMenuOpened == true)
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
        if (isDied == true || GlobalGameOptions.isGamePaused == true || GameUIBeh.instance.isCraftingMenuOpened == true)
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

        curChunkLoader = GetComponent<ChunkLoaderBase>();
        curChunkLoader.AddChunkLoaderToList();

        //    pi=new PlayerInput();
        //    pi.Enable();
        Input.multiTouchEnabled = true;
        AS = GetComponent<AudioSource>();
        // pauseMenu.SetActive(true);
        currentSelectedHotbar = 1;
        playerEatClip = Resources.Load<AudioClip>("Audios/Drink");
        playerSinkClip1 = Resources.Load<AudioClip>("Audios/Entering_water");
        playerSinkClip2 = Resources.Load<AudioClip>("Audios/Exiting_water");
        playerHandItem = transform.GetChild(0).GetChild(1).GetChild(1).GetChild(1).GetChild(0).gameObject
            .GetComponent<ItemOnHandBeh>();
        playerSweepAttackClip = Resources.Load<AudioClip>("Audios/Sweep_attack1");
        playerEnterPortalClip = Resources.Load<AudioClip>("Audios/Nether_Portal_trigger");
        playerDropItemClip = Resources.Load<AudioClip>("Audios/Pop");
        prefabBlockOutline = Resources.Load<GameObject>("Prefabs/blockoutline");
        playerEquipArmorClip = Resources.Load<AudioClip>("Audios/Equip_diamond2");
        playerBowShootClip= Resources.Load<AudioClip>("Audios/Bow_shoot");
        blockOutline = Instantiate(prefabBlockOutline, new Vector3(0, 0, 0),Quaternion.identity);
    //    collidingBlockOutline = Instantiate(prefabBlockOutline, new Vector3(0,0,0), transform.rotation);


        //  InvokeRepeating("SendChunkReleaseMessage",1f,3f);
      

      //  transform.GetChild(0).localRotation = Quaternion.Euler(0f, 0f, 0f);
     //   transform.GetChild(0).localPosition = new Vector3(0f, 0f, 0f);
        playerSweepParticlePrefab = Resources.Load<GameObject>("Prefabs/playersweepparticle");
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
        entityTransformRef = headPos;
        primaryAttackerEntities = new List<IAttackableEntityTarget>();
    }


    //  void SendChunkReleaseMessage(){
    //      foreach(var c in Chunk.Chunks){
    //         c.Value.SendMessage("TryReleaseChunk");
    //      }
    //  }
    void MouseLock()
    {
        if (WorldManager.platform == RuntimePlatform.Android || WorldManager.platform == RuntimePlatform.IPhonePlayer ||
            MobileButtonHideBeh.isHidingButton == false)
        {
            return;
        }

        if (isDied == true || GlobalGameOptions.isGamePaused == true || GameUIBeh.instance.isCraftingMenuOpened == true||GameUIBeh.instance.isPauseMenuOpened==true)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }



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

    public void ApplyDamageAndKnockback(float damageAmount, Vector3 knockback)
    {
        AS.Play();
        //GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        playerCameraZShakeLerpValue = Random.Range(-15f, 15f);
        float reducedDamage = Mathf.Max((1f - (playerArmorPoints * 4f / 100f)) * damageAmount, 0f);
        playerHealth -= reducedDamage;

        playerArmorPoints -= damageAmount * 0.1f;
        playerArmorPoints = MathF.Max(playerArmorPoints, 0f);
        GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);
        playerMotionVec = knockback;
        transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color =
            Color.red;
        transform.GetChild(0).GetChild(1).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color =
            Color.red;
        transform.GetChild(0).GetChild(1).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color =
            Color.red;
        transform.GetChild(0).GetChild(1).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color =
            Color.red;
        Invoke("InvokeRevertColor", 0.2f);
    }

    public void PlayerDie()
    {
        AS.Play();
        playerCameraZShakeLerpValue = 0f;
        playerCameraZShakeValue = 0f;
        GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        playerArmorPoints = 0f;
        GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(playerArmorPoints);
        //   transform.position=new Vector3(0f,150f,0f);
        for (int i = 0; i < inventoryDic.Length; i++)
        {
            while (inventoryItemNumberDic[i] > 0)
            {
                PlayerDropItem(i);
            }
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


        Cursor.lockState = CursorLockMode.Confined;
        /*    cc.enabled = false;
            transform.GetChild(0).localRotation = Quaternion.Euler(0f, 0f, -90f);
            transform.GetChild(0).localPosition = new Vector3(0.65f, -0.68f, 0f);
            cc.enabled = true;*/

        isDied = true;
        am.SetBool("iskilled", true);
        am.SetFloat("speed", 0f);
        RespawnUI.instance.gameObject.SetActive(true);
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
        GameUIBeh.instance.PlayerHealthSliderOnValueChanged(playerHealth);
        RespawnUI.instance.gameObject.SetActive(false);
        transform.position = new Vector3(0f, 150f, 0f);
    }

    public void PauseOrResume()
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

        if (GlobalGameOptions.isGamePaused == true||Time.deltaTime<=0f)
        {
            return;
        }
        

        /*     if(Input.GetKeyDown(KeyCode.E)){

                 OpenOrCloseCraftingUI();
               }*/
        playerCameraZShakeLerpValue = Mathf.Lerp(playerCameraZShakeLerpValue, 0f, 15f * Time.deltaTime);
        playerCameraZShakeValue = Mathf.Lerp(playerCameraZShakeLerpValue, 0f, 15f * Time.deltaTime);


        if (currentSelectedHotbar - 1 >= 0 && currentSelectedHotbar - 1 < inventoryDic.Length)
        {
            playerHandItem.blockID = inventoryDic[currentSelectedHotbar - 1];
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
            return;
        }

        playerMotionVec = Vector3.Lerp(playerMotionVec, Vector3.zero, 3f * Time.deltaTime);


        if (curChunk == null)
        {
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
        if (PlayerInputBeh.instance.isPlayerSpeededUp == true)
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
            currentSelectedHotbar += (int)PlayerInputBeh.instance.switchItemSlotAxis;
            currentSelectedHotbar = Mathf.Clamp(currentSelectedHotbar, 1, 9);
        }
   


        GameUIBeh.instance.UpdateBlockOnHandText();


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
        if (Physics.Linecast(playerHeadCenterRef.position, playerHeadCenterRef.position + playerHeadCenterRef.forward * 5f, out infoForward, ~LayerMask.GetMask("Ignore Raycast")))
        {
            isForwardPointHit = true;
            //     Debug.DrawLine(infoForward.point,infoForward.point+new Vector3(0f,1f,0f),Color.green);
        }

        if (Physics.Linecast(playerHeadCenterRef.position, playerHeadCenterRef.position + playerHeadCenterRef.forward * (-5f), out infoBack, ~LayerMask.GetMask("Ignore Raycast")))
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

        WorldHelper.instance.cameraPos = cameraPos.position;







      



        if ((cc.isGrounded != true))
        {
            if (curFootBlockID == 100)
            {
                playerY += gravity * Time.deltaTime;
                playerY = Mathf.Clamp(playerY, -3f, 1f);
            }
            else
            {
                playerY += gravity * Time.deltaTime;
            }
        }
        else
        {
            playerY = 0f;


            // playerMotionVec.y=0f;
        }


        if ((cc.isGrounded == true || curFootBlockID == 100 || isPlayerGrounded == true) &&
            PlayerInputBeh.instance.isJumping == true)
        {
            if (curFootBlockID == 100)
            {
                playerY = jumpHeight / 2f;
            }
            else
            {
                playerY = jumpHeight;
            }
        }

        if (curFootBlockID == 100)
        {
            playerY = Mathf.Clamp(playerY, -1f, 5f);
        }

        float mouseX = PlayerInputBeh.instance.mouseDelta.x;
        float mouseY = PlayerInputBeh.instance.mouseDelta.y;


       // mouseY = Mathf.Clamp(mouseY, -90f, 90f);
        cameraX -= mouseY;
        cameraX = Mathf.Clamp(cameraX, -90f, 90f);

        SetAnimationStatePlayerViewAngle(cameraX);

        if (PlayerInputBeh.instance.playerMoveVec != Vector2.zero && GameUIBeh.instance.isCraftingMenuOpened == false)
        {
            playerVec = new Vector3(PlayerInputBeh.instance.playerMoveVec.y, 0f,
                PlayerInputBeh.instance.playerMoveVec.x);

            lerpPlayerVec = Vector3.Lerp(lerpPlayerVec, playerVec, 7f * Time.deltaTime);
        }
        else
        {
            lerpPlayerVec = Vector3.Lerp(lerpPlayerVec, Vector3.zero, 7f * Time.deltaTime);
        }


        playerVec.y = playerY;
        headPos.localEulerAngles += new Vector3(0f, mouseX, 0f);
        headPos.localEulerAngles = new Vector3(cameraX, headPos.localEulerAngles.y, playerCameraZShakeValue);
      
        playerMoveRef.eulerAngles = new Vector3(0f, headPos.eulerAngles.y, headPos.eulerAngles.z);
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

        if (isPlayerInGround == true)
        {
            cc.enabled = false;
            transform.position = transform.position + Time.deltaTime * new Vector3(0f, 5f, 0f);
            cc.enabled = true;
        }
        else
        {
            cc.Move((playerMoveRef.forward * lerpPlayerVec.x + playerMoveRef.right * lerpPlayerVec.z) * moveSpeed *
                    (1f - playerMoveDrag) * Time.deltaTime + new Vector3(0f, playerVec.y, 0f) * 5f * Time.deltaTime +
                    playerMotionVec * Time.deltaTime);

            currentSpeed = Speed();
            am.SetFloat("speed", currentSpeed);
        }
     

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
                info.collider.GetComponent<EndermanBeh>().primaryAttackerEntities.Add(this);
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
        curChunkLoader.chunkLoadingCenter = playerChunkLoadingPos;
        curChunkLoader.cameraFrustum = GeometryUtility.CalculateFrustumPlanes(mainCam);
        curChunkLoader.chunkLoadingRange = GlobalGameOptions.inGameRenderDistance;

        if (curChunk == null)
        {
            curChunk = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(transform.position));
            curChunkLoader.isChunksNeedLoading = true;
        }

        if (WorldHelper.instance.CheckIsPosInChunk(transform.position, curChunk) == false)
        {
            curChunk = Chunk.GetChunk(ChunkCoordsHelper.Vec3ToChunkPos(transform.position));
            curChunkLoader.isChunksNeedLoading = true;
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
                AudioSource.PlayClipAtPoint(playerSinkClip, transform.position, 1f);
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
                AudioSource.PlayClipAtPoint(playerEnterPortalClip, transform.position);
                ParticleEffectManagerBeh.instance.EmitEndermanParticleAtPosition(transform.position);
            }
        }

        prevFootBlockID = curFootBlockID;
        prevUnderFootBlockID = curUnderFootBlockID;
        //      TryStrongLoadChunkThread();
        playerChunkLoadingPos = new Vector2(transform.position.x, transform.position.z);
        UpdateInventory();

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
    }

    public void DropItemButtonOnClick()
    {
        PlayerDropItem(currentSelectedHotbar - 1);
    }



   
    public void PlayerTryTeleportToEnderWorld()
    {
        if (playerTeleportingCD >= 4f)
        {
            switch (VoxelWorld.currentWorld.worldID)
            {
                case 0:
                    SceneManagementHelper.SwitchToWorldWithSceneChanged(1, 2);
                    VoxelWorld.worlds[1].actionOnSwitchedWorld = delegate()
                    {
                        PlayerMove.instance.cc.enabled = false;
                        PlayerMove.instance.transform.position = new Vector3(0, 150, 0);
                        PlayerMove.instance.cc.enabled = true;
                        Debug.Log("action executed");
                    };
                    break;
                case 1:
                    SceneManagementHelper.SwitchToWorldWithSceneChanged(0, 1);
                    VoxelWorld.worlds[0].actionOnSwitchedWorld = delegate()
                    {
                        PlayerMove.instance.cc.enabled = false;
                        PlayerMove.instance.transform.position = new Vector3(0, 150, 0);
                        PlayerMove.instance.cc.enabled = true;
                        Debug.Log("action executed world 0");
                    };
                    break;
                default:
                    SceneManagementHelper.SwitchToWorldWithSceneChanged(0, 1);
                    VoxelWorld.worlds[0].actionOnSwitchedWorld = delegate()
                    {
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

   


    public int ReadPlayerJson(bool ExludePlayerInWorldIDData = false)
    {
        inventoryDic = new int[18];
        inventoryItemNumberDic = new int[18];

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
            inventoryDic = pd.inventoryDic;
            inventoryItemNumberDic = pd.inventoryItemNumberDic;
            playerArmorPoints = pd.playerArmorPoints;
        }
        else
        {
            cc.enabled = false;
            transform.position = new Vector3(0f, 150f, 0f);
            cc.enabled = true;
            inventoryDic = new int[18];
            inventoryItemNumberDic = new int[18];
            playerArmorPoints = 0f;
        }

        return pd.playerInWorldID;
    }


    public void SavePlayerData()
    {
        PlayerData pd = new PlayerData(playerHealth, transform.position.x, transform.position.y, transform.position.z,
            inventoryDic, inventoryItemNumberDic, VoxelWorld.currentWorld.worldID, playerArmorPoints);

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