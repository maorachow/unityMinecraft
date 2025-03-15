using System.Collections;
using System.Collections.Generic;
using MessagePack.Resolvers;
using UnityEngine;

public class ItemInventory
{
    public static readonly int itemSlotMaxCapacity = 64;
    public int[] inventoryDic = new int[18];
    public int[] inventoryItemNumberDic = new int[18];

    public int inventorySlotCount = 18;
    public IInventoryOwner owner;
    public ItemInventory(IInventoryOwner owner,int inventorySlotCount)
    {
        this.inventorySlotCount=inventorySlotCount;
        inventoryDic = new int[inventorySlotCount];
        inventoryItemNumberDic = new int[inventorySlotCount];
        this.owner= owner;
    }
    public bool CheckInventoryIsFull(int itemID)
    {
        for (int i = 0; i < inventoryDic.Length; i++)
        {
            if (inventoryDic[i] == 0)
            {
                return false;
            }

            if (inventoryDic[i] == itemID && inventoryItemNumberDic[i] < 64)
            {
                return false;
            }
        }

        return true;
    }


    public void AddItem(int itemTypeID, int itemCount,bool dropWhenInventoryFull=true)
    {
      
        // inventoryDic[0]=1;
        //  inventoryItemNumberDic[0]=100;
        int itemCountTmp = itemCount;
        for (int i = 0; i < inventoryDic.Length; i++)
        {
            if (inventoryItemNumberDic[i] == 64)
            {
                continue;
            }
            else if (inventoryDic[i] == itemTypeID && inventoryItemNumberDic[i] < 64)
            {
                inventoryDic[i] = itemTypeID;

                while (inventoryItemNumberDic[i] < 64)
                {
                    inventoryItemNumberDic[i] += 1;
                    itemCountTmp--;
                    if (itemCountTmp == 0)
                    {
                        return;
                    }
                }


                continue;
            }
        }
        for (int i = 0; i < inventoryDic.Length; i++)
        {
            if (inventoryItemNumberDic[i] == 64)
            {
                continue;
            }
            else if (inventoryDic[i] == 0)
            {
                inventoryDic[i] = itemTypeID;

                while (inventoryItemNumberDic[i] < 64)
                {
                    inventoryItemNumberDic[i] += 1;
                    itemCountTmp--;
                    if (itemCountTmp == 0)
                    {
                        return;
                    }
                }


                continue;
            }
        }

        if (dropWhenInventoryFull == true)
        {
            owner.DropItem(itemTypeID, itemCountTmp);
        }
       
    /*    for (int i = 0; i < itemCountTmp; i++)
        {
            ItemEntityBeh.SpawnNewItem(headPos.position.x, headPos.position.y, headPos.position.z, itemTypeID,
                (headPos.forward * 3));
        }*/
        //   playerHandItem.SendMessage("OnBlockIDChanged",inventoryDic[currentSelectedHotbar-1]);  
    }



    //0diamond to pickaxe 1diamond to sword
    public void ExchangeItem(int exchangeID)
    {
        if (exchangeID == 0)
        {
            if (GetItemFromSlot(153) == -1)
            {
                return;
            }
            else
            {
                inventoryItemNumberDic[GetItemFromSlot(153)]--;
                AddItem(151, 1);
            }
        }
        else if (exchangeID == 1)
        {
            if (GetItemFromSlot(153) == -1)
            {
                return;
            }
            else
            {
                inventoryItemNumberDic[GetItemFromSlot(153)]--;
                AddItem(152, 1);
            }
        }
        else if (exchangeID == 2)
        {
            if (GetItemFromSlot(7) == -1)
            {
                return;
            }
            else
            {
                inventoryItemNumberDic[GetItemFromSlot(7)]--;
                AddItem(102, 4);
            }
        }
        else if (exchangeID == 3)
        {
            if (GetItemFromSlot(155) == -1)
            {
                return;
            }
            else
            {
                inventoryItemNumberDic[GetItemFromSlot(155)]--;
                AddItem(156, 4);
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
                AddItem(158, 8);
            }
        }
        else if (exchangeID == 5)
        {
            if (GetItemFromSlot(7) == -1)
            {
                return;
            }
            else
            {
                inventoryItemNumberDic[GetItemFromSlot(7)]--;
                AddItem(103, 4);

            }
        }

        else if (exchangeID == 6)
        {
            if (GetItemFromSlot(11) == -1)
            {
                return;
            }
            else
            {
                inventoryItemNumberDic[GetItemFromSlot(11)]--;
                AddItem(107, 1);
                AddItem(108, 1);
                AddItem(109, 1);
                AddItem(110, 1);
                AddItem(111, 1);
            }
        }

        else if (exchangeID == 7)
        {
            if (GetItemFromSlot(7) == -1)
            {
                return;
            }
            else
            {
                inventoryItemNumberDic[GetItemFromSlot(7)]--;
                AddItem(104, 6);

            }
        }
    }

   

    public bool TryRemoveItem(int itemID, int itemCount)
    {
        int itemSlotOfCurItem = GetItemFromSlot(itemID);
        if (itemSlotOfCurItem == -1)
        {
            return false;
        }

        if (inventoryItemNumberDic[itemSlotOfCurItem] < itemCount)
        {
            return false;
        }
        inventoryItemNumberDic[itemSlotOfCurItem]-=itemCount;
        return true;

    }
    public bool TryRemoveItemFromSlot(int slotID, int itemCount)
    {
        if (slotID < 0 || slotID >= inventorySlotCount)
        {
            return false;
        }
        if (inventoryItemNumberDic[slotID] < itemCount)
        {
            return false;
        }
        inventoryItemNumberDic[slotID] -= itemCount;
        return true;
    }

    public void GetItemInfoFromSlot(int slotID, out int itemID, out int itemCount)
    {
        if (slotID < 0 || slotID >= inventorySlotCount)
        {
            itemID=0; itemCount=0; return;

        }
        itemID = inventoryDic[slotID];
        itemCount= inventoryItemNumberDic[slotID];
    }

    public void ClearSlot(int slotID)
    {
        if (slotID < 0 || slotID >= inventorySlotCount)
        {
          return;

        }
        inventoryItemNumberDic[slotID] = 0;
        inventoryDic[slotID] = 0;
      
    }
    public int GetItemFromSlot(int itemID)
    {
        for (int i = 0; i < inventoryDic.Length; i++)
        {
            if (inventoryDic[i] == itemID && inventoryItemNumberDic[i] > 0)
            {
                return i;
            }
        }

        return -1;
    }


     public void UpdateInventory()
    {
        for (int i = 0; i < inventoryItemNumberDic.Length; i++)
        {
            inventoryItemNumberDic[i] = Mathf.Clamp(inventoryItemNumberDic[i], 0, 64);
        }

        for (int i = 0; i < inventoryDic.Length; i++)
        {
            if (inventoryItemNumberDic[i] <= 0)
            {
                inventoryDic[i] = 0;
            }
        }
    }

    public void SwapItemSlot(int slotIDSrc, int slotIDDst)
    {
        int tmpInventoryItemID = inventoryDic[slotIDDst];
        int tmpInventoryItemCount = inventoryItemNumberDic[slotIDDst];
        inventoryDic[slotIDDst] = inventoryDic[slotIDSrc];
        inventoryItemNumberDic[slotIDDst] = inventoryItemNumberDic[slotIDSrc];
        inventoryDic[slotIDSrc] = tmpInventoryItemID;

        inventoryItemNumberDic[slotIDSrc] = tmpInventoryItemCount;
    }

    

}
