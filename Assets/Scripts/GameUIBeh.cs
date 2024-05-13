using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameUIBeh : MonoBehaviour
{
    public bool isCraftingMenuOpened=false;
    public GameObject pauseMenu;
    public GameObject craftUI;
    public static GameUIBeh instance;
    public static bool isPaused=false;
   public RectTransform selectedHotbarTransform;
   public Slider playerHealthSlider;
    public Slider playerArmorPointsSlider;
   public Image playerHealthbarBackgroundImage;
   public Sprite playerHealthbarBlack;
   public Sprite playerHealthbarWhite;
   public PlayerMove player;
   public static Dictionary<int,Sprite> blockImageDic=new Dictionary<int,Sprite>();
   public static Dictionary<int,Image> hotbarImageDic=new Dictionary<int,Image>();
   public static Dictionary<int,TMP_Text> hotbarTextDic=new Dictionary<int,TMP_Text>();
    public static Dictionary<int, string> blockNameDic = new Dictionary<int, string>();
    public int selectedHotbar=1;
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
        blockNameDic.Add(151, "Diamond Pickaxe");
        blockNameDic.Add(152, "Diamond Sword");
        blockNameDic.Add(153, "Diamond");
        blockNameDic.Add(154, "Rotten Flesh");
        blockNameDic.Add(155, "Gunpowder");
        blockNameDic.Add(156, "TNT");
        blockNameDic.Add(158, "Armor Upgrader");
        //     isBlockNameDicAdded=true;
        //     }
    }
    private void Awake()
    {
         if (instance == null)
        {
        instance=this;
        }
    }
    void Start(){
       
     
    
    //    DontDestroyOnLoad(gameObject);
    craftUI=transform.Find("inventoryconvertUI").gameObject;
    //if(pauseMenu==null){
             pauseMenu=transform.Find("pausemenuUI").gameObject;
             pauseMenu.SetActive(true);
  //      }
        AddBlockNameInfo();
    blockImageDic.Clear();
    blockImageDic.TryAdd(-1,Sprite.Create(new Texture2D(16,16),new Rect(0,0,16,16),new Vector2(0.5f,0.5f)));
    blockImageDic.TryAdd(0,Resources.Load<Sprite>("Textures/emptyslot"));
    blockImageDic.TryAdd(1,Resources.Load<Sprite>("Textures/stone"));
    blockImageDic.TryAdd(2,Resources.Load<Sprite>("Textures/grass_side_carried"));
    blockImageDic.TryAdd(3,Resources.Load<Sprite>("Textures/dirt"));
    blockImageDic.TryAdd(4,Resources.Load<Sprite>("Textures/grass_side_carried"));
    blockImageDic.TryAdd(5,Resources.Load<Sprite>("Textures/bedrock"));
    blockImageDic.TryAdd(6,Resources.Load<Sprite>("Textures/log_oak"));
    blockImageDic.TryAdd(7,Resources.Load<Sprite>("Textures/log_oak"));
    blockImageDic.TryAdd(8,Resources.Load<Sprite>("Textures/log_oak"));
    blockImageDic.TryAdd(9,Resources.Load<Sprite>("Textures/leaves"));
    blockImageDic.TryAdd(11,Resources.Load<Sprite>("Textures/sand"));
    blockImageDic.TryAdd(12, Resources.Load<Sprite>("Textures/end_stone"));
    blockImageDic.TryAdd(13, Resources.Load<Sprite>("Textures/endframe_top"));
    blockImageDic.TryAdd(100,Resources.Load<Sprite>("Textures/water"));
    blockImageDic.TryAdd(101,Resources.Load<Sprite>("Textures/grass"));
    blockImageDic.TryAdd(102,Resources.Load<Sprite>("Textures/torch_on"));
    blockImageDic.TryAdd(151,Resources.Load<Sprite>("Textures/diamond_pickaxe"));
    blockImageDic.TryAdd(152,Resources.Load<Sprite>("Textures/diamond_sword"));
    blockImageDic.TryAdd(153,Resources.Load<Sprite>("Textures/diamond"));
    blockImageDic.TryAdd(154,Resources.Load<Sprite>("Textures/rotten_flesh"));
    blockImageDic.TryAdd(155, Resources.Load<Sprite>("Textures/gunpowder"));
    blockImageDic.TryAdd(156, Resources.Load<Sprite>("Textures/tnt_side"));
    blockImageDic.TryAdd(158, Resources.Load<Sprite>("Textures/netherite_upgrade_smithing_template"));
        hotbarImageDic.Clear();
    hotbarTextDic.Clear();
    for(int i=1;i<=9;i++){
        
        hotbarImageDic.TryAdd(i-1,GameObject.Find("hotbarItem"+i.ToString()).GetComponent<Image>());
    }
    for(int i=1;i<=9;i++){
        hotbarTextDic.TryAdd(i-1,GameObject.Find("hotbarItemnumbertext"+i.ToString()).GetComponent<TMP_Text>());
    }
        player=GameObject.Find("player").GetComponent<PlayerMove>();
        RespawnUI.instance.gameObject.SetActive(false);
        playerHealthbarBlack=Resources.Load<Sprite>("Textures/heartbarbackground");
        playerHealthbarWhite=Resources.Load<Sprite>("Textures/playerheartbarbackgroundflash");
        playerHealthSlider=GameObject.Find("healthbar").GetComponent<Slider>();
        playerArmorPointsSlider = GameObject.Find("armorbar").GetComponent<Slider>();
        playerHealthSlider.onValueChanged.AddListener(PlayerHealthSliderOnValueChanged);
        playerArmorPointsSlider.onValueChanged.AddListener(PlayerArmorPointsSliderOnValueChanged);
        playerHealthbarBackgroundImage =GameObject.Find("healthbar").GetComponent<Image>();
        
        selectedHotbarTransform=GameObject.Find("selectedhotbar").GetComponent<RectTransform>();
        GameUIBeh.instance.PlayerHealthSliderOnValueChanged(player.playerHealth);
       GameUIBeh.instance.PlayerArmorPointsSliderOnValueChanged(player.playerArmorPoints);
    }
    public void PlayerHealthSliderOnValueChanged(float f){
        playerHealthSlider.value=player.playerHealth;
        playerHealthbarBackgroundImage.sprite=playerHealthbarWhite;
        Invoke("PlayerHealthSliderInvokeChangeSprite",0.2f);

    }
    public void PlayerArmorPointsSliderOnValueChanged(float f)
    {
        playerArmorPointsSlider.value = player.playerArmorPoints;
       
     

    }
    void PlayerHealthSliderInvokeChangeSprite(){
         playerHealthbarBackgroundImage.sprite=playerHealthbarBlack;
    }


    public void CloseCraftingUI(){
        isCraftingMenuOpened=false;
        if(craftUI==null){
             craftUI=transform.Find("inventoryconvertUI").gameObject;
             craftUI.SetActive(false);
        }else{
             craftUI.SetActive(false);
        }
    }
    public void OpenCraftingUI(){
        isCraftingMenuOpened=true;
        if(craftUI==null){
             craftUI=transform.Find("inventoryconvertUI").gameObject;
             craftUI.SetActive(true);
        }else{
                craftUI.SetActive(true);
        }
    }
    public void PauseGame(){
  //   Debug.Log("UIPause");
        pauseMenu=transform.Find("pausemenuUI").gameObject;
          
        
        isPaused=true;
        Time.timeScale=0;
        pauseMenu.SetActive(true);
    }
    public void Resume(){
   //   Debug.Log("UIResume");
            pauseMenu=transform.Find("pausemenuUI").gameObject;
          
        
        isPaused=false;
        Time.timeScale=1;
        pauseMenu.SetActive(false);
    }

    void FixedUpdate(){
        if(player!=null&&blockImageDic!=null){

      
        selectedHotbar=player.currentSelectedHotbar;
        selectedHotbarTransform.anchoredPosition=new Vector2(-400f+(selectedHotbar-1)*100f,0f);
        foreach(KeyValuePair<int,Image> i in hotbarImageDic){
            if(player.inventoryItemNumberDic[i.Key]==0){
                i.Value.sprite=blockImageDic[0];
                continue;
            }
            if(blockImageDic.ContainsKey(player.inventoryDic[i.Key])){
             i.Value.sprite=blockImageDic[player.inventoryDic[i.Key]];   
            }else{
                i.Value.sprite=blockImageDic[-1];
            }
            
        }
        foreach(KeyValuePair<int,TMP_Text> i in hotbarTextDic){
            i.Value.text=player.inventoryItemNumberDic[i.Key].ToString();
        }  }
    }
  
}
