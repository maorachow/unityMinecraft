using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryOwner 
{
    public ItemInventory inventory { get; }

    public void DropItem(int itemID, int itemCount);

    public void TryInteractWithItemEntities();
}
