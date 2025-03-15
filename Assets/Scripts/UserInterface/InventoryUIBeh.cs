using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIBeh : MonoBehaviour
{

    public Dictionary<int, Image> inventoryImageDic = new Dictionary<int, Image>();
    public Dictionary<int, TMP_Text> inventoryTextDic = new Dictionary<int, TMP_Text>();
    public Button closeInventoryButton;
    public int curSelectedInventorySlot=-1;
    public bool isSlotSelected = false;
   public void SetSelectedInventorySlotOrSwap(int slot)
    {
        if (slot < 0 || slot >= PlayerUIMediator.instance.playerInventorySlotCapacity)
        {
            Debug.Log("slot out of range");
            return;
        }
        if (isSlotSelected == true)
        {
            PlayerUIMediator.instance.PlayerInventorySwapSlot(curSelectedInventorySlot,slot);
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
        for (int i = 1; i <=  Math.Max(PlayerUIMediator.instance.playerInventorySlotCapacity,18); i++)
        {

            inventoryImageDic.TryAdd(i - 1, GameObject.Find("inventoryItem" + i.ToString()).GetComponent<Image>());
        }
        for (int i = 1; i <= Math.Max(PlayerUIMediator.instance.playerInventorySlotCapacity, 18); i++)
        {
            inventoryTextDic.TryAdd(i - 1, GameObject.Find("inventoryItemnumbertext" + i.ToString()).GetComponent<TMP_Text>());
        }

        closeInventoryButton = transform.Find("closemenubutton").GetComponent<Button>();
        closeInventoryButton.onClick.AddListener(PlayerUIMediator.instance.OpenOrCloseCraftingUI);
    }

    
    void Update()
    {
        PlayerMove player=PlayerMove.instance;
        if (player != null && GameUIBeh.blockImageDic != null)
        {

 
            foreach (KeyValuePair<int, Image> i in inventoryImageDic)
            {

                if (PlayerUIMediator.instance.playerInventoryItemTypeSlots[i.Key] == 0)
                {
                    i.Value.sprite = GameUIBeh.blockImageDic[0];
                    continue;
                }
                if (GameUIBeh.blockImageDic.ContainsKey(PlayerUIMediator.instance.playerInventoryItemTypeSlots[i.Key]))
                {
                    i.Value.sprite = GameUIBeh.blockImageDic[PlayerUIMediator.instance.playerInventoryItemTypeSlots[i.Key]];
                }
                else
                {
                    i.Value.sprite = GameUIBeh.blockImageDic[-1];
                }
                i.Value.rectTransform.localScale=new Vector3(1,1,1);
            }
            foreach (KeyValuePair<int, TMP_Text> i in inventoryTextDic)
            {
                i.Value.text = PlayerUIMediator.instance.playerInventoryItemNumberSlots[i.Key].ToString();
            }

            if (curSelectedInventorySlot != -1)
            {
                inventoryImageDic[curSelectedInventorySlot].rectTransform.localScale=new Vector3(1.2f,1.2f,1f);
            }
        }
    }
}

