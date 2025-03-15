using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class EntityManager
{
    public VoxelWorld curWorld;

    public EntityManager(VoxelWorld world)
    {
        curWorld=world;
    }
    public List<EntityData> entityDataReadFromDisk;
    public List<EntityBeh> worldEntities = new List<EntityBeh>();


    public ObjectPool<GameObject> creeperEntityPool;
    public ObjectPool<GameObject> zombieEntityPool;
    public ObjectPool<GameObject> tntEntityPool;
    public ObjectPool<GameObject> skeletonEntityPool;
    public ObjectPool<GameObject> endermanEntityPool;
    public ObjectPool<GameObject> arrowEntityPool;


    public GameObject CreateCreeper()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[0], new Vector3(0, 100, 0), Quaternion.identity);

        return gameObject;
    }
    public GameObject CreateZombie()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[1], new Vector3(0, 0f, 0), Quaternion.identity);

        return gameObject;
    }
    public GameObject CreateTNT()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[2], new Vector3(0, 0f, 0), Quaternion.identity);

        return gameObject;
    }

    public GameObject CreateSkeleton()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[3], new Vector3(0, 0f, 0), Quaternion.identity);

        return gameObject;
    }

    public GameObject CreateArrow()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[4], new Vector3(0, 0f, 0), Quaternion.identity);

        return gameObject;
    }

    public GameObject CreateEnderman()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[5], new Vector3(0, 0f, 0), Quaternion.identity);

        return gameObject;
    }

    void GetObjectPoolItem(GameObject gameObject)
    {

        gameObject.SetActive(true);

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
        creeperEntityPool = new ObjectPool<GameObject>(CreateCreeper, GetObjectPoolItem, ReleaseObjectPoolItem, DestroyObjectPoolItem, true, 10, 300);
        zombieEntityPool = new ObjectPool<GameObject>(CreateZombie, GetObjectPoolItem, ReleaseObjectPoolItem, DestroyObjectPoolItem, true, 10, 300);
        tntEntityPool = new ObjectPool<GameObject>(CreateTNT, GetObjectPoolItem, ReleaseObjectPoolItem, DestroyObjectPoolItem, true, 10, 300);
        skeletonEntityPool = new ObjectPool<GameObject>(CreateSkeleton, GetObjectPoolItem, ReleaseObjectPoolItem, DestroyObjectPoolItem, true, 10, 300);
        arrowEntityPool = new ObjectPool<GameObject>(CreateArrow, GetObjectPoolItem, ReleaseObjectPoolItem, DestroyObjectPoolItem, true, 10, 300);
        endermanEntityPool = new ObjectPool<GameObject>(CreateEnderman, GetObjectPoolItem, ReleaseObjectPoolItem, DestroyObjectPoolItem, true, 10, 300);
    }

    public void FetchEntityData()
    {
        if (!GameDataPersistenceManager.instance.IsEntityDataLoaded(curWorld.worldID))
        {
            Debug.Log("entity data has not been loaded");
            entityDataReadFromDisk=new List<EntityData>();
        }
        entityDataReadFromDisk=new List<EntityData>(GameDataPersistenceManager.instance.entityDataReadFromFile[curWorld.worldID]);
    }

    public void SaveEntityDataToPersistenceManager()
    {
        foreach (EntityBeh e in worldEntities)
        {
            e.SaveSingleEntity(ref entityDataReadFromDisk);
        }
        GameDataPersistenceManager.instance.entityDataReadFromFile[curWorld.worldID]= new List<EntityData>(entityDataReadFromDisk);
    }

    public void ReInit()
    {
        entityDataReadFromDisk = new List<EntityData>();
        worldEntities.Clear();
    }




    public EntityBeh SpawnNewEntity(float posX, float posY, float posZ, int entityID)
    {

        switch (entityID)
        {
            case 0:
                GameObject a = creeperEntityPool.Get();
                a.transform.position = new Vector3(posX, posY, posZ);



                a.GetComponent<EntityBeh>().entityTypeID = entityID;
                a.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                a.GetComponent<CreeperBeh>().SendMessage("InitPos");
                return a.GetComponent<EntityBeh>();
                break;
            case 1:
                GameObject b = zombieEntityPool.Get();
                b.transform.position = new Vector3(posX, posY, posZ);
                b.GetComponent<EntityBeh>().entityTypeID = entityID;
                b.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");

                b.GetComponent<ZombieBeh>().SendMessage("InitPos");


                return b.GetComponent<EntityBeh>();
                break;
            case 2:
                GameObject c = tntEntityPool.Get();
                c.transform.position = new Vector3(posX, posY, posZ);
                c.GetComponent<Rigidbody>().position = new Vector3(posX, posY, posZ);
                c.GetComponent<EntityBeh>().entityTypeID = entityID;
                c.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                c.GetComponent<TNTBeh>().SendMessage("InitPos");
                return c.GetComponent<EntityBeh>();
                break;
            case 3:
                GameObject d = skeletonEntityPool.Get();
                d.transform.position = new Vector3(posX, posY, posZ);

                d.GetComponent<EntityBeh>().entityTypeID = entityID;
                d.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                d.GetComponent<SkeletonBeh>().SendMessage("InitPos");
                return d.GetComponent<EntityBeh>();
                break;
            case 4:
                GameObject e = arrowEntityPool.Get();
                e.transform.position = new Vector3(posX, posY, posZ);
                e.GetComponent<Rigidbody>().position = new Vector3(posX, posY, posZ);
                e.GetComponent<EntityBeh>().entityTypeID = entityID;
                e.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");

                //   e.GetComponent<ArrowBeh>().isPosInited = true;
                return e.GetComponent<EntityBeh>();
            case 5:
                GameObject f = endermanEntityPool.Get();
                f.transform.position = new Vector3(posX, posY, posZ);

                f.GetComponent<EntityBeh>().entityTypeID = entityID;
                f.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                f.GetComponent<EndermanBeh>().SendMessage("InitPos");
                //   e.GetComponent<ArrowBeh>().isPosInited = true;
                return f.GetComponent<EntityBeh>();
            default: return null;
        }

    }

    public EntityBeh SpawnNewEntity(float posX, float posY, float posZ, int entityID, Vector3 initialDirection)
    {

        switch (entityID)
        {
            case 0:
                GameObject a = creeperEntityPool.Get();
                a.transform.position = new Vector3(posX, posY, posZ);



                a.GetComponent<EntityBeh>().entityTypeID = entityID;
                a.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                a.GetComponent<CreeperBeh>().SendMessage("InitPos");
                return a.GetComponent<EntityBeh>();
                break;
            case 1:
                GameObject b = zombieEntityPool.Get();
                b.transform.position = new Vector3(posX, posY, posZ);
                b.GetComponent<EntityBeh>().entityTypeID = entityID;
                b.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");

                b.GetComponent<ZombieBeh>().SendMessage("InitPos");


                return b.GetComponent<EntityBeh>();
                break;
            case 2:
                GameObject c = tntEntityPool.Get();
                c.transform.position = new Vector3(posX, posY, posZ);
                c.GetComponent<Rigidbody>().position = new Vector3(posX, posY, posZ);
                c.GetComponent<EntityBeh>().entityTypeID = entityID;
                c.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                c.GetComponent<TNTBeh>().SendMessage("InitPos");
                return c.GetComponent<EntityBeh>();
                break;
            case 3:
                GameObject d = skeletonEntityPool.Get();
                d.transform.position = new Vector3(posX, posY, posZ);

                d.GetComponent<EntityBeh>().entityTypeID = entityID;
                d.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                d.GetComponent<SkeletonBeh>().SendMessage("InitPos");
                return d.GetComponent<EntityBeh>();
                break;
            case 4:
                GameObject e = arrowEntityPool.Get();
                e.transform.position = new Vector3(posX, posY, posZ);
                e.GetComponent<Rigidbody>().position = new Vector3(posX, posY, posZ);
                e.GetComponent<Transform>().rotation = Quaternion.LookRotation(initialDirection);
                e.GetComponent<Rigidbody>().rotation = Quaternion.LookRotation(initialDirection);
                e.GetComponent<EntityBeh>().entityTypeID = entityID;
                e.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");

                //   e.GetComponent<ArrowBeh>().isPosInited = true;
                return e.GetComponent<EntityBeh>();
            case 5:
                GameObject f = endermanEntityPool.Get();
                f.transform.position = new Vector3(posX, posY, posZ);

                f.GetComponent<EntityBeh>().entityTypeID = entityID;
                f.GetComponent<EntityBeh>().guid = System.Guid.NewGuid().ToString("N");
                f.GetComponent<EndermanBeh>().SendMessage("InitPos");
                //   e.GetComponent<ArrowBeh>().isPosInited = true;
                return f.GetComponent<EntityBeh>();
            default: return null;
        }

    }

    public void SpawnEntityFromFile()
    {
        foreach (EntityData ed in entityDataReadFromDisk)
        {
            //        Debug.Log(ed.guid);
           
                switch (ed.entityTypeID)
                {
                    case 0:
                        GameObject a = creeperEntityPool.Get();

                        a.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                        a.transform.rotation = Quaternion.Euler(ed.rotationX, ed.rotationY, ed.rotationZ);
                        a.GetComponent<CreeperBeh>().SendMessage("InitPos");
                        a.GetComponent<EntityBeh>().entityTypeID = ed.entityTypeID;
                        a.GetComponent<EntityBeh>().guid = ed.guid;

                        break;
                    case 1:
                        GameObject b = zombieEntityPool.Get();
                        b.GetComponent<ZombieBeh>().SendMessage("InitPos");
                        b.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                        b.transform.rotation = Quaternion.Euler(ed.rotationX, ed.rotationY, ed.rotationZ);
                        b.GetComponent<EntityBeh>().entityTypeID = ed.entityTypeID;
                        b.GetComponent<EntityBeh>().guid = ed.guid;

                        break;
                    case 2:
                        GameObject c = tntEntityPool.Get();
                        c.GetComponent<TNTBeh>().SendMessage("InitPos");
                        c.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                        c.GetComponent<Rigidbody>().position = new Vector3(ed.posX, ed.posY, ed.posZ);
                        c.transform.rotation = Quaternion.Euler(ed.rotationX, ed.rotationY, ed.rotationZ);
                        c.GetComponent<EntityBeh>().entityTypeID = ed.entityTypeID;
                        c.GetComponent<EntityBeh>().guid = ed.guid;

                        break;

                    case 3:
                        GameObject d = skeletonEntityPool.Get();
                        d.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                        d.transform.rotation = Quaternion.Euler(ed.rotationX, ed.rotationY, ed.rotationZ);
                        d.GetComponent<EntityBeh>().entityTypeID = ed.entityTypeID;
                        d.GetComponent<EntityBeh>().guid = ed.guid;
                        d.GetComponent<SkeletonBeh>().SendMessage("InitPos");

                        break;

                    case 4:
                        GameObject e = arrowEntityPool.Get();
                        e.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                        e.GetComponent<Rigidbody>().position = new Vector3(ed.posX, ed.posY, ed.posZ);
                        e.GetComponent<EntityBeh>().entityTypeID = ed.entityTypeID;
                        e.GetComponent<EntityBeh>().guid = ed.guid;
                        break;

                    case 5:
                        GameObject f = endermanEntityPool.Get();
                        f.transform.position = new Vector3(ed.posX, ed.posY, ed.posZ);
                        f.transform.rotation = Quaternion.Euler(ed.rotationX, ed.rotationY, ed.rotationZ);
                        f.GetComponent<EntityBeh>().entityTypeID = ed.entityTypeID;
                        f.GetComponent<EntityBeh>().guid = ed.guid;
                        f.GetComponent<EndermanBeh>().SendMessage("InitPos");

                        break;

                }
            



        }
    }
}
