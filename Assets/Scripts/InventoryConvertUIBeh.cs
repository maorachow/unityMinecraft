using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryConvertUIBeh : MonoBehaviour
{

    public Dictionary<int, Image> inventoryImageDic = new Dictionary<int, Image>();
    public Dictionary<int, TMP_Text> inventoryTextDic = new Dictionary<int, TMP_Text>();
    public int curSelectedInventorySlot=-1;
    public bool isSlotSelected = false;
   public void SetSelectedInventorySlotOrSwap(int slot)
    {
        if (slot < 0 || slot >= PlayerMove.instance.inventoryDic.Length)
        {
            Debug.Log("slot out of range");
            return;
        }
        if (isSlotSelected == true)
        {
            PlayerMove.instance?.SwapItemSlot(curSelectedInventorySlot,slot);
            curSelectedInventorySlot = -1;
            isSlotSelected = false;
        }
        else
        {
            curSelectedInventorySlot = slot;
            isSlotSelected = true;
        }  
       
    }

    public void OnDisable()
    {
        curSelectedInventorySlot = -1;
        isSlotSelected = false;
    }
    void Start()
    {
        inventoryImageDic.Clear();
        inventoryTextDic.Clear();
        for (int i = 1; i <= 18; i++)
        {

            inventoryImageDic.TryAdd(i - 1, GameObject.Find("inventoryItem" + i.ToString()).GetComponent<Image>());
        }
        for (int i = 1; i <= 18; i++)
        {
            inventoryTextDic.TryAdd(i - 1, GameObject.Find("inventoryItemnumbertext" + i.ToString()).GetComponent<TMP_Text>());
        }

    }

    
    void Update()
    {
        PlayerMove player=PlayerMove.instance;
        if (player != null && GameUIBeh.blockImageDic != null)
        {

 
            foreach (KeyValuePair<int, Image> i in inventoryImageDic)
            {

                if (player.inventoryItemNumberDic[i.Key] == 0)
                {
                    i.Value.sprite = GameUIBeh.blockImageDic[0];
                    continue;
                }
                if (GameUIBeh.blockImageDic.ContainsKey(player.inventoryDic[i.Key]))
                {
                    i.Value.sprite = GameUIBeh.blockImageDic[player.inventoryDic[i.Key]];
                }
                else
                {
                    i.Value.sprite = GameUIBeh.blockImageDic[-1];
                }
                i.Value.rectTransform.localScale=new Vector3(1,1,1);
            }
            foreach (KeyValuePair<int, TMP_Text> i in inventoryTextDic)
            {
                i.Value.text = player.inventoryItemNumberDic[i.Key].ToString();
            }

            if (curSelectedInventorySlot != -1)
            {
                inventoryImageDic[curSelectedInventorySlot].rectTransform.localScale=new Vector3(1.2f,1.2f,1f);
            }
        }
    }
}

