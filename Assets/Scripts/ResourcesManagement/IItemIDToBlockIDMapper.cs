using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemIDToBlockIDMapper
{
    public int ToItemID(BlockData blockID);

    public bool CanMapToItemID(BlockData blockID);
    public bool CanMapToBlockID(int itemID);
    public BlockData ToBlockID(int itemID);

    public void Initialize();
}
