using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPools : MonoBehaviour
{
        public static GameObject chunkPrefab;
    //(Create, Get, Release, MyDestroy, true, 10, poolMaxSize)
    public static ObjectPool<GameObject> chunkPool;

    public void Awake(){
        chunkPrefab=Resources.Load<GameObject>("Prefabs/chunk");
        chunkPool=new ObjectPool<GameObject>(CreateChunk, GetChunk, ReleaseChunk, DestroyChunk, true, 0, 1000);
    }
    public GameObject CreateChunk()
    {
        GameObject c=Instantiate(chunkPrefab,transform.position,transform.rotation);
        return c;
    }
    
    void GetChunk(GameObject c)
    {
     
        c.SetActive(true);
      
    }
    void ReleaseChunk(GameObject c)
    {   
      
        c.SetActive(false);
       
    }
    void DestroyChunk(GameObject c)
    {
        Destroy(c);
    }

}
