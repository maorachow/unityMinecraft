using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
public class MyChunkObjectPool{
 
    public GameObject Object;
 
    public Queue<GameObject> objectPool = new Queue<GameObject>();
   
    public int defaultCount = 16;
 
    public int maxCount = 200;
 
    public void Init()
    {
        objectPool.Clear();
        GameObject obj;
        for (int i = 0; i < defaultCount; i++)
        {
            obj = GameObject.Instantiate(Object,new Vector3(0f,0f,0f),Quaternion.identity);
            objectPool.Enqueue(obj);
            obj.SetActive(false);
        }
    }
    // 从池子中取出物体
    public GameObject Get(Vector2Int pos)
    {
        GameObject tmp;
    
        if (objectPool.Count > 0)
        {
            // 将对象出队
            tmp = objectPool.Dequeue();
 
            tmp.transform.position=new Vector3(pos.x,0,pos.y);
            tmp.SetActive(true);
            tmp.GetComponent<Chunk>().ReInitData();

        }
  
        else
        {
            GameObject c=GameObject.Instantiate(Object, new Vector3(pos.x,0,pos.y),Quaternion.identity);
                c.GetComponent<Chunk>().ReInitData();
            return c;
            
        }
        return tmp;
    }

    // 将物体回收进池子
    public void Remove(GameObject obj)
    {
        // 池子中的物体数目不超过最大容量
        if (objectPool.Count <= maxCount)
        {
        	// 该对象没有在队列中
            if (!objectPool.Contains(obj))
            {
                // 将对象入队
                objectPool.Enqueue(obj);
                obj.GetComponent<Chunk>().ChunkOnDisable();
                obj.SetActive(false);
            }
        }
        // 超过最大容量就销毁
        else
        {
             obj.GetComponent<Chunk>().ChunkOnDisable();
            GameObject.Destroy(obj);
        }
    }

}




public class MyItemObjectPool{
 
    public GameObject Object;
 
    public Queue<GameObject> objectPool = new Queue<GameObject>();
   
    public int defaultCount = 16;
 
    public int maxCount = 100;
 
    public void Init()
    {
        objectPool.Clear();
        GameObject obj;
        for (int i = 0; i < defaultCount; i++)
        {
            obj = GameObject.Instantiate(Object,new Vector3(0f,0f,0f),Quaternion.identity);
            objectPool.Enqueue(obj);
            obj.SetActive(false);
        }
    }
    // 从池子中取出物体
    public GameObject Get(Vector3 pos)
    {
        GameObject tmp;
    
        if (objectPool.Count > 0)
        {
            // 将对象出队
            tmp = objectPool.Dequeue();
            tmp.transform.position=pos;
            tmp.GetComponent<Rigidbody>().position=pos;
            tmp.SetActive(true);
        }
  
        else
        {
            return GameObject.Instantiate(Object, pos,Quaternion.identity);
            
        }
        return tmp;
    }
    // 将物体回收进池子
    public void Remove(GameObject obj)
    {
        // 池子中的物体数目不超过最大容量
        if (objectPool.Count <= maxCount)
        {
        	// 该对象没有在队列中
            if (!objectPool.Contains(obj))
            {
                // 将对象入队
                objectPool.Enqueue(obj);
                obj.SetActive(false);
            }
        }
        // 超过最大容量就销毁
        else
        {
            GameObject.Destroy(obj);
        }
    }

}

public class ObjectPools : MonoBehaviour
{
        public static GameObject chunkPrefab;
        public static GameObject particlePrefab;
        public static GameObject itemPrefab;
        public static GameObject pointLightPrefab;
        public static ObjectPool<GameObject> particleEffectPool;
        public static MyChunkObjectPool chunkPool=new MyChunkObjectPool();
        public static ObjectPool<GameObject> creeperEntityPool;
        public static ObjectPool<GameObject> zombieEntityPool;
        public static ObjectPool<GameObject> tntEntityPool;
        public static ObjectPool<GameObject> skeletonEntityPool;
        public static ObjectPool<GameObject> arrowEntityPool;
    // public static ObjectPool<GameObject> itemEntityPool;
    public static MyItemObjectPool itemEntityPool=new MyItemObjectPool();
        public void Start(){
        
      //  pointLightPrefab =Resources.Load<GameObject>("Prefabs/chunkpointlightprefab");
       // particlePrefab=Resources.Load<GameObject>("Prefabs/blockbreakingparticle");
     //   itemPrefab=Resources.Load<GameObject>("Prefabs/itementity");
        /*      particleEffectPool=new ObjectPool<GameObject>(CreateEffect, GetEffect, ReleaseEffect, DestroyEffect, true, 10, 300);
        creeperEntityPool=new ObjectPool<GameObject>(CreateCreeper,GetCreeper,ReleaseCreeper,DestroyCreeper,true,10,300);
        zombieEntityPool=new ObjectPool<GameObject>(CreateZombie,GetZombie,ReleaseZombie,DestroyZombie,true,10,300);
        tntEntityPool = new ObjectPool<GameObject>(CreateTNT, GetTNT, ReleaseTNT, DestroyTNT, true, 10, 300);
        skeletonEntityPool= new ObjectPool<GameObject>(CreateSkeleton, GetSkeleton, ReleaseSkeleton, DestroySkeleton, true, 10, 300);
        arrowEntityPool = new ObjectPool<GameObject>(CreateArrow,GetArrow,ReleaseArrow,DestroyArrow,true,10,300);*/
        //   itemEntityPool=new ObjectPool<GameObject>(CreateItem,GetItem,ReleaseItem,DestroyItem,true,10,300);
      //  chunkPrefab =Resources.Load<GameObject>("Prefabs/chunk");
       
      
        
    }
  
   
}
