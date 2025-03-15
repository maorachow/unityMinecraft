using MessagePack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public struct EntityData
{
    [Key(0)]
    public float posX;
    [Key(1)]
    public float posY;
    [Key(2)]
    public float posZ;
    [Key(3)]
    public float rotationX;
    [Key(4)]
    public float rotationY;
    [Key(5)]
    public float rotationZ;
    [Key(6)]
    public int entityTypeID;
    [Key(7)]
    public string guid;
    [Key(8)]
    public int entityInWorldID;
    public bool Equals(EntityData other)
    {

        if (this.guid.Equals(other.guid))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public EntityData(float posX, float posY, float posZ, float rotationX, float rotationY, float rotationZ, int entityTypeID, string guid, int entityInWorldID)
    {
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.rotationX = rotationX;
        this.rotationY = rotationY;
        this.rotationZ = rotationZ;
        this.entityTypeID = entityTypeID;
        this.guid = guid;
        this.entityInWorldID = entityInWorldID;
    }
}
