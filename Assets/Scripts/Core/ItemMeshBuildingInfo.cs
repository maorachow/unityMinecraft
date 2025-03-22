using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ItemMeshBuildingInfo
{ 
    
    public bool isItemBlockShaped;
    public Vector2 uvCorner;
    public Vector2 uvSize;

    public ItemMeshBuildingInfo(bool isItemBlockShaped, Vector2 uvCorner, Vector2 uvSize)
    {
        this.isItemBlockShaped=isItemBlockShaped;
        this.uvCorner=uvCorner;
        this.uvSize=uvSize;
    }

    public ItemMeshBuildingInfo(bool isItemBlockShaped)
    {
        this.isItemBlockShaped = isItemBlockShaped;
        this.uvCorner = new Vector2(1-0.0625f,0f);
        this.uvSize = new Vector2(0.0625f, 0.0625f); 
    }
}
