using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ItemEntityManager 
{
    public VoxelWorld curWorld;

    public ItemEntityManager(VoxelWorld world)
    {
        curWorld = world;
    }
    public List<ItemEntityData> itemEntityDataReadFromDisk = new List<ItemEntityData>();
   
    public List<ItemEntityBeh> worldItemEntities = new List<ItemEntityBeh>();

    public ObjectPool<GameObject> itemEntityPool;

    public GameObject CreateItemEntity()
    {
        GameObject gameObject = GameObject.Instantiate(VoxelWorld.itemPrefab, new Vector3(0, 0f, 0), Quaternion.identity);

        return gameObject;
    }

    void GetObjectPoolItem(GameObject gameObject)
    {
        //enable object at SpawnNewItem

    }
    void ReleaseObjectPoolItem(GameObject gameObject)
    {
        gameObject.SetActive(false);

    }
    void DestroyObjectPoolItem(GameObject gameObject)
    {

        GameObject.Destroy(gameObject);
    }

    public void InitObjectPools()
    {
        itemEntityPool = new ObjectPool<GameObject>(CreateItemEntity, GetObjectPoolItem, ReleaseObjectPoolItem, DestroyObjectPoolItem, true, 10, 600);
        
    }

    public void FetchItemEntityData()
    {
        if (!GameDataPersistenceManager.instance.IsItemEntityDataLoaded(curWorld.worldID))
        {
            Debug.Log("item entity data has not been loaded");
            itemEntityDataReadFromDisk = new List<ItemEntityData>();
        }
        itemEntityDataReadFromDisk = new List<ItemEntityData>(GameDataPersistenceManager.instance.itemEntityDataReadFromFile[curWorld.worldID]);
    }

    public void SaveItemEntityDataToPersistenceManager()
    {
        foreach (ItemEntityBeh e in worldItemEntities)
        {
            e.SaveSingleItemEntity(ref itemEntityDataReadFromDisk);
        }
        GameDataPersistenceManager.instance.itemEntityDataReadFromFile[curWorld.worldID] =new List<ItemEntityData>(itemEntityDataReadFromDisk) ;
    }

    public void ReInit()
    {
        itemEntityDataReadFromDisk = new List<ItemEntityData>();
        worldItemEntities.Clear();
    }


    public void SpawnNewItem(float posX, float posY, float posZ, int itemID, Vector3 startingSpeed)
    {
        if (itemID == -1)
        {
            return;
        }
        //     Debug.Log(VoxelWorld.currentWorld.itemEntityPool.Object);

        GameObject a =  itemEntityPool.Get();
        a.transform.position = new Vector3(posX, posY, posZ);
        a.GetComponent<Rigidbody>().position= new Vector3(posX, posY, posZ);
        a.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        a.SetActive(true);
        ItemEntityBeh tmp = a.GetComponent<ItemEntityBeh>();

        tmp.itemID = itemID;
        tmp.guid = System.Guid.NewGuid().ToString("N");
        tmp.StartBuildItem();
        //    a.transform.position=new Vector3(posX,posY,posZ);
        
        tmp.AddForceInvoke(startingSpeed);

    }


    public void SpawnItemEntityFromFile()
    {
         
        for (int i = 0; i < itemEntityDataReadFromDisk.Count; i++)
        {
            //  Debug.Log(ed.guid);

            ItemEntityData ed = itemEntityDataReadFromDisk[i];
            
                GameObject a = itemEntityPool.Get();
                a.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                a.GetComponent<Rigidbody>().position = new Vector3(ed.posX, ed.posY, ed.posZ);
                a.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                a.SetActive(true);
                ItemEntityBeh tmp = a.GetComponent<ItemEntityBeh>();
                
                tmp.itemID = ed.itemID;
                tmp.guid = ed.guid;
                tmp.lifeTime = ed.lifeTime;
                tmp.StartBuildItem();






        }

    }
}
