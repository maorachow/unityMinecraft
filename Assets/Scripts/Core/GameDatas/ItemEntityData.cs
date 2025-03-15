using MessagePack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public struct ItemEntityData
{
    [Key(0)]
    public int itemID;
    [Key(1)]
    public int itemCount;
    [Key(2)]
    public float posX;
    [Key(3)]
    public float posY;
    [Key(4)]
    public float posZ;
    [Key(5)]
    public string guid;
    [Key(6)]
    public float lifeTime;
    [Key(7)]
    public int worldID;
    public ItemEntityData(int itemID, int itemCount, float posX, float posY, float posZ, string guid, float lifeTime, int worldID)
    {
        this.itemID = itemID;
        this.itemCount = itemCount;
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.guid = guid;
        this.lifeTime = lifeTime;
        this.worldID = worldID;
    }
}