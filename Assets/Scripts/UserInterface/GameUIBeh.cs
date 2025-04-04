using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class GameUIBeh : MonoBehaviour
{
    public bool isCraftingMenuOpened=false;
    public bool isPauseMenuOpened = false;
    public bool isRespawnUIOpened = false;
    public GameObject pauseMenu;
    public GameObject craftUI;
    public GameObject respawnUI;
 

    //  public static bool isPaused=false;
    public RectTransform selectedHotbarTransform;
    public Slider playerHealthSlider;
    public Slider playerArmorPointsSlider;
    public Image playerHealthbarBackgroundImage;
    public Sprite playerHealthbarBlack;
    public Sprite playerHealthbarWhite;
    
    public static Dictionary<int, Sprite> blockImageDic = new Dictionary<int, Sprite>();
    public Dictionary<int, Image> hotbarImageDic = new Dictionary<int, Image>();
    public Dictionary<int, TMP_Text> hotbarTextDic = new Dictionary<int, TMP_Text>();
    public static Dictionary<int, string> blockNameDic = new Dictionary<int, string>();
    public int selectedHotbar = 1;

    public Text blockOnHandText;
    /* blockNameDic.Add(0,"None");
         blockNameDic.Add(1,"Stone");
         blockNameDic.Add(2,"Grass");
         blockNameDic.Add(3,"Dirt");
         blockNameDic.Add(4,"Side Grass Block");
         blockNameDic.Add(5,"Bedrock");
         blockNameDic.Add(6,"WoodX");
         blockNameDic.Add(7,"WoodY");
         blockNameDic.Add(8,"WoodZ");
         blockNameDic.Add(9,"Leaves");*/

    public static void AddBlockNameInfo()
    {
        //       if(isBlockNameDicAdded==false){
        blockNameDic.Clear();
        blockNameDic.Add(0, "None");
        blockNameDic.Add(1, "Stone");
        blockNameDic.Add(2, "Grass");
        blockNameDic.Add(3, "Dirt");
        blockNameDic.Add(4, "Side Grass Block");
        blockNameDic.Add(5, "Bedrock");
        blockNameDic.Add(6, "WoodX");
        blockNameDic.Add(7, "WoodY");
        blockNameDic.Add(8, "WoodZ");
        blockNameDic.Add(9, "Leaves");
        blockNameDic.Add(11, "Sand");
        blockNameDic.Add(12, "End Stone");
        blockNameDic.Add(13, "Enderworld Portal");
        blockNameDic.Add(100, "Water");
        blockNameDic.Add(101, "Grass Crop");
        blockNameDic.Add(102, "Torch");
        blockNameDic.Add(103, "Fences");
        blockNameDic.Add(104, "Wooden Door");

        blockNameDic.Add(106, "Green Glass");
        blockNameDic.Add(107, "Blue Glass");
        blockNameDic.Add(108, "Black Glass");
        blockNameDic.Add(109, "Glass");
        blockNameDic.Add(110, "White Glass");
        blockNameDic.Add(111, "Red Glass");
        blockNameDic.Add(151, "Diamond Pickaxe");
        blockNameDic.Add(152, "Diamond Sword");
        blockNameDic.Add(153, "Diamond");
        blockNameDic.Add(154, "Rotten Flesh");
        blockNameDic.Add(155, "Gunpowder");
        blockNameDic.Add(156, "TNT");
        blockNameDic.Add(157, "Bow");
        blockNameDic.Add(158, "Armor Upgrader");
        //     isBlockNameDicAdded=true;
        //     }
    }

    private void Awake()
    {
       

        PlayerUIMediator.instance.gameUI = this;
    }

    public void OnDestroy()
    {
        PlayerUIMediator.instance.gameUI=null;
    }
    void Start()
    {
        //    DontDestroyOnLoad(gameObject);
        craftUI = transform.Find("inventoryconvertUI").gameObject;
        respawnUI = transform.Find("playerrespawnpanel").gameObject;
        //if(pauseMenu==null){
        pauseMenu = transform.Find("pausemenuUI").gameObject;
         

        blockOnHandText = GameObject.Find("blockonhandIDtext").GetComponent<Text>();

        //      }
        AddBlockNameInfo();
        blockImageDic.Clear();
        blockImageDic.TryAdd(-1, Sprite.Create(new Texture2D(16, 16), new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f)));
        blockImageDic.TryAdd(0, Resources.Load<Sprite>("Textures/blocksprites/emptyslot"));
        blockImageDic.TryAdd(1, Resources.Load<Sprite>("Textures/blocksprites/stone"));
        blockImageDic.TryAdd(2, Resources.Load<Sprite>("Textures/blocksprites/grass_side_carried"));
        blockImageDic.TryAdd(3, Resources.Load<Sprite>("Textures/blocksprites/dirt"));
        blockImageDic.TryAdd(4, Resources.Load<Sprite>("Textures/blocksprites/grass_side_carried"));
        blockImageDic.TryAdd(5, Resources.Load<Sprite>("Textures/blocksprites/bedrock"));
        blockImageDic.TryAdd(6, Resources.Load<Sprite>("Textures/blocksprites/log_oak"));
        blockImageDic.TryAdd(7, Resources.Load<Sprite>("Textures/blocksprites/log_oak"));
        blockImageDic.TryAdd(8, Resources.Load<Sprite>("Textures/blocksprites/log_oak"));
        blockImageDic.TryAdd(9, Resources.Load<Sprite>("Textures/blocksprites/leaves"));
        blockImageDic.TryAdd(11, Resources.Load<Sprite>("Textures/blocksprites/sand"));
        blockImageDic.TryAdd(12, Resources.Load<Sprite>("Textures/blocksprites/end_stone"));
        blockImageDic.TryAdd(13, Resources.Load<Sprite>("Textures/blocksprites/endframe_top"));
        blockImageDic.TryAdd(100, Resources.Load<Sprite>("Textures/blocksprites/water"));
        blockImageDic.TryAdd(101, Resources.Load<Sprite>("Textures/blocksprites/grass"));
        blockImageDic.TryAdd(102, Resources.Load<Sprite>("Textures/blocksprites/torch_on"));
        blockImageDic.TryAdd(103, Resources.Load<Sprite>("Textures/blocksprites/fences"));

        blockImageDic.TryAdd(104, Resources.Load<Sprite>("Textures/blocksprites/woodendoor"));

        blockImageDic.TryAdd(106, Resources.Load<Sprite>("Textures/blocksprites/glass_green"));
        blockImageDic.TryAdd(107, Resources.Load<Sprite>("Textures/blocksprites/glass_blue"));
        blockImageDic.TryAdd(108, Resources.Load<Sprite>("Textures/blocksprites/glass_black"));
        blockImageDic.TryAdd(109, Resources.Load<Sprite>("Textures/blocksprites/glass"));
        blockImageDic.TryAdd(110, Resources.Load<Sprite>("Textures/blocksprites/glass_white"));
        blockImageDic.TryAdd(111, Resources.Load<Sprite>("Textures/blocksprites/glass_red"));

        blockImageDic.TryAdd(151, Resources.Load<Sprite>("Textures/blocksprites/diamond_pickaxe"));
        blockImageDic.TryAdd(152, Resources.Load<Sprite>("Textures/blocksprites/diamond_sword"));
        blockImageDic.TryAdd(153, Resources.Load<Sprite>("Textures/blocksprites/diamond"));
        blockImageDic.TryAdd(154, Resources.Load<Sprite>("Textures/blocksprites/rotten_flesh"));
        blockImageDic.TryAdd(155, Resources.Load<Sprite>("Textures/blocksprites/gunpowder"));
        blockImageDic.TryAdd(156, Resources.Load<Sprite>("Textures/blocksprites/tnt_side"));
        blockImageDic.TryAdd(157, Resources.Load<Sprite>("Textures/blocksprites/bow"));
        blockImageDic.TryAdd(158, Resources.Load<Sprite>("Textures/blocksprites/netherite_upgrade_smithing_template"));
        hotbarImageDic.Clear();
        hotbarTextDic.Clear();
        for (int i = 1; i <= 9; i++)
        {
            hotbarImageDic.TryAdd(i - 1, transform.GetChild(0).Find("hotbarItem" + i.ToString()).GetComponent<Image>());
        }

        for (int i = 1; i <= 9; i++)
        {
            hotbarTextDic.TryAdd(i - 1,
                transform.GetChild(0).Find("hotbarItemnumbertext" + i.ToString()).GetComponent<TMP_Text>());
        }


         
        playerHealthbarBlack = Resources.Load<Sprite>("Textures/heartbarbackground");
        playerHealthbarWhite = Resources.Load<Sprite>("Textures/playerheartbarbackgroundflash");
        playerHealthSlider = GameObject.Find("healthbar").GetComponent<Slider>();
        playerArmorPointsSlider = GameObject.Find("armorbar").GetComponent<Slider>();
        playerHealthSlider.onValueChanged.AddListener(PlayerHealthSliderOnValueChanged);
        playerArmorPointsSlider.onValueChanged.AddListener(PlayerArmorPointsSliderOnValueChanged);
        playerHealthbarBackgroundImage = GameObject.Find("healthbar").GetComponent<Image>();

        selectedHotbarTransform = transform.GetChild(0).Find("selectedhotbar").GetComponent<RectTransform>();
      //  GameUIBeh.instance.PlayerHealthSliderOnValueChanged(player.playerHealth);
     //   GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(player.playerArmorPoints);
        CloseCraftingUI();
        Resume();
        CloseRespawnUI();
        PlayerUIMediator.instance.UpdatePlayerStats(true);
       PlayerUIMediator.instance.UpdateUIStats();
    }

    public void PlayerHealthSliderOnValueChanged(float f)
    {
        if (playerHealthSlider == null)
        {
            return;
        }
        playerHealthSlider.value =f;
        playerHealthbarBackgroundImage.sprite = playerHealthbarWhite;
        Invoke("PlayerHealthSliderInvokeChangeSprite", 0.3f);
    }

    public void PlayerArmorPointsSliderOnValueChanged(float f)
    {
        if (playerArmorPointsSlider == null)
        {
            return;
        }
        playerArmorPointsSlider.value = PlayerUIMediator.instance.playerArmorPoints;
    }

    void PlayerHealthSliderInvokeChangeSprite()
    {
        playerHealthbarBackgroundImage.sprite = playerHealthbarBlack;
    }

    public void OpenRespawnUI()
    {
        Cursor.lockState = CursorLockMode.None;
        if (respawnUI != null && respawnUI.activeInHierarchy == false)
        {
            respawnUI.SetActive(true);
            isRespawnUIOpened = true;
        }
       
    }
    public void MouseLock()
    {
      
        if (WorldManager.platform == RuntimePlatform.Android || WorldManager.platform == RuntimePlatform.IPhonePlayer ||
            MobileButtonHideBeh.isHidingButton == false)
        {
            return;
        }

        if (PlayerUIMediator.instance. isPlayerDied == true || GlobalGameOptions.isGamePaused == true || isCraftingMenuOpened == true || isPauseMenuOpened == true)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }
    public void CloseRespawnUI()
    {

        if (respawnUI != null && respawnUI.activeInHierarchy == true)
        {
            respawnUI.SetActive(false);
            isRespawnUIOpened=false;
        }
       

        if (respawnUI!= null && respawnUI.activeInHierarchy == false)
        {
            MouseLock();
        }
    }
    public void CloseCraftingUI()
    {
        if (respawnUI != null && respawnUI.activeInHierarchy == false)
        {
            MouseLock();
        }
        Time.timeScale = 1;
            GlobalGameOptions.isGamePaused = false;
        
      
        isCraftingMenuOpened = false;
        if (craftUI == null)
        {
            craftUI = transform.Find("inventoryconvertUI").gameObject;
            craftUI.SetActive(false);
        }
        else
        {
            craftUI.SetActive(false);
        }
    }

    public void OpenCraftingUI()
    {

        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
            GlobalGameOptions.isGamePaused = true;
        
        isCraftingMenuOpened = true;
        if (craftUI == null)
        {
            craftUI = transform.Find("inventoryconvertUI").gameObject;
            craftUI.SetActive(true);
        }
        else
        {
            craftUI.SetActive(true);
        }
    }

    public void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        //   Debug.Log("UIPause");
        pauseMenu = transform.Find("pausemenuUI").gameObject;
        GlobalGameOptions.isGamePaused = true;
        Time.timeScale = 0;


        pauseMenu.SetActive(true);
        isPauseMenuOpened=true;
    }

    public void Resume()
    {
        if (respawnUI != null && respawnUI.activeInHierarchy == false)
        {
            MouseLock();
        }
      
        //   Debug.Log("UIResume");
        pauseMenu = transform.Find("pausemenuUI").gameObject;

        Time.timeScale = 1;
        GlobalGameOptions.isGamePaused = false;

        pauseMenu.SetActive(false);
        isPauseMenuOpened=false;
    }

    public void UpdateBlockOnHandText()
    {
         
            if (blockOnHandText == null)
            {
                blockOnHandText = GameObject.Find("blockonhandIDtext").GetComponent<Text>();
            }

            if (GameUIBeh.blockNameDic.ContainsKey(PlayerUIMediator.instance.playerInventoryItemTypeSlots[PlayerUIMediator.instance.playerInventoryCurSelectedHotbarIndex - 1]))
            {
                blockOnHandText.text = GameUIBeh.blockNameDic[PlayerUIMediator.instance.playerInventoryItemTypeSlots[PlayerUIMediator.instance.playerInventoryCurSelectedHotbarIndex - 1]];
            }
            else
            {
                blockOnHandText.text = "Unknown Block Name,ID:" + PlayerUIMediator.instance.playerInventoryItemTypeSlots[PlayerUIMediator.instance.playerInventoryCurSelectedHotbarIndex - 1];
            }
        
    }

    void Update()
    {
        PlayerUIMediator.instance.UpdateUIStats();
    }
    void FixedUpdate()
    {
        UpdateBlockOnHandText();
        if (blockImageDic != null)
        {
            selectedHotbar = PlayerUIMediator.instance.playerInventoryCurSelectedHotbarIndex;
            selectedHotbarTransform.anchoredPosition = new Vector2(-400f + (selectedHotbar - 1) * 100f, 0f);
            foreach (KeyValuePair<int, Image> i in hotbarImageDic)
            {
                if (i.Key >= PlayerUIMediator.instance.playerInventorySlotCapacity)
                {
                   continue;
                }
                if (PlayerUIMediator.instance.playerInventoryItemNumberSlots[i.Key] == 0)
                {
                    i.Value.sprite = blockImageDic[0];
                    continue;
                }

                if (blockImageDic.ContainsKey(PlayerUIMediator.instance.playerInventoryItemTypeSlots[i.Key]))
                {
                    i.Value.sprite = blockImageDic[PlayerUIMediator.instance.playerInventoryItemTypeSlots[i.Key]];
                }
                else
                {
                    i.Value.sprite = blockImageDic[-1];
                }
            }

            foreach (KeyValuePair<int, TMP_Text> i in hotbarTextDic)
            {
                if (i.Key >= PlayerUIMediator.instance.playerInventorySlotCapacity)
                {
                    continue;
                }
                i.Value.text = PlayerUIMediator.instance.playerInventoryItemNumberSlots[i.Key].ToString();
            }
        }
    }
}
