using MessagePack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[MessagePackObject]
public class PlayerData
{
    [Key(0)]
    public float playerHealth;

    [Key(1)]
    public float posX;

    [Key(2)]
    public float posY;

    [Key(3)]
    public float posZ;

    [Key(4)]
    public int[] inventoryDic;

    [Key(5)]
    public int[] inventoryItemNumberDic;

    [Key(6)]
    public int playerInWorldID;

    [Key(7)]
    public float playerArmorPoints;

    public PlayerData(float playerHealth, float posX, float posY, float posZ, int[] inventoryDic,
        int[] inventoryItemNumberDic, int playerInWorldID, float playerArmorPoints)
    {
        this.playerHealth = playerHealth;
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.inventoryDic = inventoryDic;
        this.inventoryItemNumberDic = inventoryItemNumberDic;
        this.playerInWorldID = playerInWorldID;
        this.playerArmorPoints = playerArmorPoints;
    }

    public PlayerData()
    {

    }
    public void SetPlayerInventoryData(int[] inInventoryDic, int[] inInventoryItemNumberDic)
    {
        this.inventoryDic = (int[])inInventoryDic.Clone();
        this.inventoryItemNumberDic = (int[])inInventoryItemNumberDic.Clone();
    }


    public void SetPlayerCommonData(float playerHealth, float playerArmorPoints, float posX, float posY, float posZ, int playerInWorldID)
    {
        this.playerHealth = playerHealth;
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;

        this.playerInWorldID = playerInWorldID;
        this.playerArmorPoints = playerArmorPoints;
    }
}