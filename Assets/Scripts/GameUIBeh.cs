using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameUIBeh : MonoBehaviour
{
   public RectTransform selectedHotbarTransform;
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
   void Awake(){
    blockImageDic.Add(0,Resources.Load<Sprite>("Textures/emptyslot"));
    blockImageDic.Add(1,Resources.Load<Sprite>("Textures/stone"));
    blockImageDic.Add(2,Resources.Load<Sprite>("Textures/grass_side_carried"));
    blockImageDic.Add(3,Resources.Load<Sprite>("Textures/dirt"));
    blockImageDic.Add(4,Resources.Load<Sprite>("Textures/grass_side_carried"));
    blockImageDic.Add(5,Resources.Load<Sprite>("Textures/bedrock"));
    blockImageDic.Add(6,Resources.Load<Sprite>("Textures/log_oak"));
    blockImageDic.Add(7,Resources.Load<Sprite>("Textures/log_oak"));
    blockImageDic.Add(8,Resources.Load<Sprite>("Textures/log_oak"));
    blockImageDic.Add(9,Resources.Load<Sprite>("Textures/leaves"));
    blockImageDic.Add(100,Resources.Load<Sprite>("Textures/water"));
    blockImageDic.Add(101,Resources.Load<Sprite>("Textures/grass"));
    for(int i=1;i<=9;i++){
        hotbarImageDic.Add(i-1,GameObject.Find("hotbarItem"+i.ToString()).GetComponent<Image>());
    }
    for(int i=1;i<=9;i++){
        hotbarTextDic.Add(i-1,GameObject.Find("hotbarItemnumbertext"+i.ToString()).GetComponent<TMP_Text>());
    }
   }
    void Start()
    {
        player=GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        selectedHotbarTransform=GameObject.Find("selectedhotbar").GetComponent<RectTransform>();
    }
    void FixedUpdate(){
        foreach(KeyValuePair<int,Image> i in hotbarImageDic){
            if(player.inventoryItemNumberDic[i.Key]==0){
                i.Value.sprite=blockImageDic[0];
                continue;
            }
            i.Value.sprite=blockImageDic[player.inventoryDic[i.Key]];
        }
        foreach(KeyValuePair<int,TMP_Text> i in hotbarTextDic){
            i.Value.text=player.inventoryItemNumberDic[i.Key].ToString();
        }
    }
    // Update is called once per frame
    void Update()
    {
        selectedHotbar=player.currentSelectedHotbar;
        selectedHotbarTransform.anchoredPosition=new Vector2(-160f+(selectedHotbar-1)*40f,0f);
    }
}
