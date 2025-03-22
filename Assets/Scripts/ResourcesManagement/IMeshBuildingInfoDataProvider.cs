using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMeshBuildingInfoDataProvider
{
    public void InitDefault();

    public void Init();
    public BlockInfo GetBlockInfo(BlockData blockID);
    public bool IsBlockDataValid(BlockData blockID);

    public ItemMeshBuildingInfo GetItemMeshBuildingInfo(int itemID);
    public bool IsItemIDValid(int itemID);
}
