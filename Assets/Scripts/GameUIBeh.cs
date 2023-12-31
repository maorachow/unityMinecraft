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
   public Image playerHealthbarBackgroundImage;
   public Sprite playerHealthbarBlack;
   public Sprite playerHealthbarWhite;
   public PlayerMove player;
   public static Dictionary<int,Sprite> blockImageDic=new Dictionary<int,Sprite>();
   public static Dictionary<int,Image> hotbarImageDic=new Dictionary<int,Image>();
   public static Dictionary<int,TMP_Text> hotbarTextDic=new Dictionary<int,TMP_Text>();
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
   void Start(){
    instance=this;
    craftUI=transform.Find("inventoryconvertUI").gameObject;
    if(pauseMenu==null){
             pauseMenu=transform.Find("pausemenuUI").gameObject;
             pauseMenu.SetActive(true);
        }
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
    blockImageDic.TryAdd(100,Resources.Load<Sprite>("Textures/water"));
    blockImageDic.TryAdd(101,Resources.Load<Sprite>("Textures/grass"));
    blockImageDic.TryAdd(102,Resources.Load<Sprite>("Textures/torch_on"));
    blockImageDic.TryAdd(151,Resources.Load<Sprite>("Textures/diamond_pickaxe"));
    blockImageDic.TryAdd(152,Resources.Load<Sprite>("Textures/diamond_sword"));
    blockImageDic.TryAdd(153,Resources.Load<Sprite>("Textures/diamond"));
    blockImageDic.TryAdd(154,Resources.Load<Sprite>("Textures/rotten_flesh"));
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
        playerHealthSlider.onValueChanged.AddListener(PlayerHealthSliderOnValueChanged);
        playerHealthbarBackgroundImage=GameObject.Find("healthbar").GetComponent<Image>();
        
        selectedHotbarTransform=GameObject.Find("selectedhotbar").GetComponent<RectTransform>();
        GameUIBeh.instance.PlayerHealthSliderOnValueChanged(player.playerHealth);
       
    }
    public void PlayerHealthSliderOnValueChanged(float f){
        playerHealthSlider.value=player.playerHealth;
        playerHealthbarBackgroundImage.sprite=playerHealthbarWhite;
        Invoke("PlayerHealthSliderInvokeChangeSprite",0.2f);

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
