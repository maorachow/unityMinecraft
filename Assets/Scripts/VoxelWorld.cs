using MessagePack;
using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
public class VoxelWorld
{
    public static GameObject chunkPrefab;
    public static GameObject particlePrefab;
    public static GameObject itemPrefab;
    public static GameObject pointLightPrefab;
    public static MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
    public static List<VoxelWorld> worlds = new List<VoxelWorld> { 
    new VoxelWorld("world.json",0,0),
    new VoxelWorld("worldender.json",2,1)
    };
    public static bool isWorldChanged = false;
    public int worldID = 0;

    public ConcurrentDictionary<Vector2Int, Chunk> chunks = new ConcurrentDictionary<Vector2Int, Chunk>();
    public ConcurrentDictionary<Vector2Int, WorldData> chunkDataReadFromDisk = new ConcurrentDictionary<Vector2Int, WorldData>();
    public static string gameWorldDataPath;
    public string curWorldSaveName="default.json";
    public int worldGenType = 0;
    public static VoxelWorld currentWorld=worlds[0];

    public SimplePriorityQueue<Vector2Int> chunkSpawningQueue = new SimplePriorityQueue<Vector2Int>();
    public SimplePriorityQueue<ChunkLoadingQueueItem> chunkLoadingQueue = new SimplePriorityQueue<ChunkLoadingQueueItem>();
    public SimplePriorityQueue<Vector2Int> chunkUnloadingQueue = new SimplePriorityQueue<Vector2Int>();
    public MyChunkObjectPool chunkPool = new MyChunkObjectPool();
    public bool isWorldDataSaved = false;
    public bool isJsonReadFromDisk = false;
    public bool isGoingToQuitWorld = false;
    public bool isFastChunkLoadingEnabled = false;
    public FastNoise noiseGenerator = new FastNoise();
    public FastNoise biomeNoiseGenerator = new FastNoise();
    public FastNoise frequentNoiseGenerator = new FastNoise();
    public Task updateAllChunkLoadersThread;
    public Task tryReleaseChunksThread;
    public Task tryUpdateChunksThread;
    public Action actionOnSwitchedWorld;
    public Chunk GetChunk(Vector2Int chunkPos)
    {
     //   Debug.Log(VoxelWorld.currentWorld.chunks.Count);
        if (chunks.ContainsKey(chunkPos))
        {
            Chunk tmp = chunks[chunkPos];
            return tmp;
        }
        else
        {
            return null;
        }

    }
    public List<ChunkLoaderBase> allChunkLoaders = new List<ChunkLoaderBase>();

    public void TryUpdateAllChunkLoadersThread()
    {
        while (true)
        {
            Thread.Sleep(50);
         //   Debug.Log("run");   
            
            if (VoxelWorld.currentWorld.isGoingToQuitWorld == true)
            {
                return;
            }
            foreach (var cl in allChunkLoaders)
            {
           //     Debug.Log(cl.chunkLoadingCenter);
                if (cl.isChunksNeedLoading == true)
                {
                    cl.TryUpdateWorldThread();
                }

            }
        }
      
    }
    public void TryReleaseChunkThread()
    {
        while (true)
        {
            /*   if(WorldManager.isGoingToQuitGame==true){
                      return;
                  }
               Thread.Sleep(100);
                 List<Vector2Int> keys=new List<Vector2Int>(Chunks.Keys);
             for(int i=0;i<keys.Count;i++){
              if(!Chunks.ContainsKey(keys[i])){
                  return;
              }
               if(Chunks[keys[i]].isChunkPosInited==false){
                  continue;
               }
                  Vector2Int cPos=Chunks[keys[i]].chunkPos;
                   if(Mathf.Abs(cPos.x-playerPosVec.x)>PlayerMove.viewRange+Chunk.chunkWidth+3||Mathf.Abs(cPos.y-playerPosVec.z)>PlayerMove.viewRange+Chunk.chunkWidth+3&&Chunks[keys[i]].isMeshBuildCompleted==true&&!WorldManager.chunkUnloadingQueue.Contains(Chunks[keys[i]])){

                 WorldManager.chunkUnloadingQueue.Enqueue(Chunks[keys[i]],1-((int)Mathf.Abs(cPos.x-playerPosVec.x)+(int)Mathf.Abs(cPos.y-playerPosVec.z)));
                 Chunks[keys[i]].isChunkPosInited=false;
              } 
             }*/
            Thread.Sleep(200);
            if (isGoingToQuitWorld == true)
            {
                return;
            }
            foreach (var c in chunks)
            {
                if (!chunks.ContainsKey(c.Key))
                {
                    continue;
                }
                if (chunkUnloadingQueue.Contains(c.Key))
                {
                    continue;
                }
                Vector2Int cPos = c.Key;
                bool isChunkNeededRemoving = true;
                foreach (var cl in allChunkLoaders)
                {
                    if (!(Mathf.Abs(cPos.x - cl.chunkLoadingCenter.x) > cl.chunkLoadingRange + Chunk.chunkWidth + 3 || Mathf.Abs(cPos.y - cl.chunkLoadingCenter.y) > cl.chunkLoadingRange + Chunk.chunkWidth + 3))
                    {
                        isChunkNeededRemoving = false;

                        //    c.Value.isChunkPosInited=false;
                    }
                }
                if (isChunkNeededRemoving == true && !chunkUnloadingQueue.Contains(c.Key))
                {
                    chunkUnloadingQueue.Enqueue(c.Key, 0);
                }
            }
        }


    }

    public void TryUpdateChunkThread()
    {
        //     delegate void mainBuildChunk();
        //   mainBuildChunk callback;

        while (true)
        {
            Thread.Sleep(5);
            if (isGoingToQuitWorld == true)
            {
                return;
            }


            foreach (var c in chunks)
            {
                if (c.Value.isChunkMapUpdated == true)
                {
                    c.Value.isModifiedInGame = true;
                    if (c.Value.isMeshBuildCompleted == true)
                    {
                        chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(c.Value, true), -50);
                    }
                    else
                    {
                        continue;
                    }
                    //InitMap(chunkPos);
                    c.Value.isChunkMapUpdated = false;
                }
            }



        }

    }




    int blockedDisablingCount = 0;


    public void DisableChunks()
    {
     //   Debug.Log("disable");
        //  Debug.Log(chunkUnloadingQueue.Count);
        if (chunkUnloadingQueue.Count > 0)
        {

            Chunk c = GetChunk(chunkUnloadingQueue.First);

            if (c != null)
            {
                lock (c.taskLock)
                {
                    if (c.isTaskCompleted == true)
                    {
                        chunkPool.Remove(c.gameObject);
                        chunkUnloadingQueue.Dequeue();
                        //  Debug.Log("completed");
                        return;
                    }
                    else
                    {
                        blockedDisablingCount++;
                        if (blockedDisablingCount > 15)
                        {
                            blockedDisablingCount = 0;
                            GameObject.Destroy(c.gameObject);
                            chunkUnloadingQueue.Dequeue();
                            Debug.Log("destroy");
                            return;
                        }
                        return;
                    }
                }

            }
            else
            {
                chunkUnloadingQueue.Dequeue();
                //  Debug.Log("none");
                return;
            }
        }
    }




   public void SpawnChunks()
    {
        //  (var c in chunkSpawningQueue){
        //     await Task.Delay(20);
        if (isFastChunkLoadingEnabled == true)
        {
            for (int i = 0; i < 6; i++)
            {
                if (chunkSpawningQueue.Count > 0)
                {

                    Vector2Int cPos = chunkSpawningQueue.First;
                    if (GetChunk(cPos) != null)
                    {
                        chunkSpawningQueue.Dequeue();
                        continue;
                    }
                    if (chunkUnloadingQueue.Contains(cPos))
                    {
                        chunkSpawningQueue.Dequeue();
                        continue;
                    }

                    chunkPool.Get(cPos);


                    chunkSpawningQueue.Dequeue();
                    continue;
                }

            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                if (chunkSpawningQueue.Count > 0)
                {

                    Vector2Int cPos = chunkSpawningQueue.First;
                    if (GetChunk(cPos) != null)
                    {
                        chunkSpawningQueue.Dequeue();
                        continue;
                    }
                    if (chunkUnloadingQueue.Contains(cPos))
                    {
                        chunkSpawningQueue.Dequeue();
                        continue;
                    }
                    //ObjectPools.chunkPool.Get(cPos);

                    chunkPool.Get(cPos);

                    chunkSpawningQueue.Dequeue();
                    continue;
                }

            }
        }

    }



    public void BuildAllChunks()
    {

        if (isFastChunkLoadingEnabled == true)
        {
            for (int i = 0; i < 10; i++)
            {
                if (chunkLoadingQueue.Count > 0)
                {
                    if (chunkLoadingQueue.First.c == null)
                    {
                        chunkLoadingQueue.Dequeue();
                        continue;
                    }




                    chunkLoadingQueue.First.c.StartLoadChunk();



                    chunkLoadingQueue.Dequeue();


                }
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (chunkLoadingQueue.Count > 0)
                {
                    if (chunkLoadingQueue.First.c == null)
                    {
                        chunkLoadingQueue.Dequeue();
                        continue;
                    }




                    chunkLoadingQueue.First.c.StartLoadChunk();



                    chunkLoadingQueue.Dequeue();


                }
            }
        }







    }
    public void ReadJson()
    {
        chunkDataReadFromDisk.Clear();
        gameWorldDataPath = WorldManager.gameWorldDataPath;

        if (!Directory.Exists(gameWorldDataPath + "unityMinecraftData"))
        {
            Directory.CreateDirectory(gameWorldDataPath + "unityMinecraftData");

        }
        if (!Directory.Exists(gameWorldDataPath + "unityMinecraftData/GameData"))
        {
            Directory.CreateDirectory(gameWorldDataPath + "unityMinecraftData/GameData");
        }

        if (!File.Exists(gameWorldDataPath + "unityMinecraftData" + "/GameData/" + curWorldSaveName))
        {
            FileStream fs = File.Create(gameWorldDataPath + "unityMinecraftData" + "/GameData/" + curWorldSaveName);
            fs.Close();
        }

        byte[] worldData = File.ReadAllBytes(gameWorldDataPath + "unityMinecraftData/GameData/" + curWorldSaveName);
        //  List<WorldData> tmpList=new List<WorldData>();
        /* foreach(string s in worldData){
             WorldData tmp=JsonSerializer.Deserialize<WorldData>(s);
             tmpList.Add(tmp);
         }
         foreach(WorldData w in tmpList){
             chunkDataReadFromDisk.Add(new Vector2Int(w.posX,w.posZ),w);
         }*/
        if (worldData.Length > 0)
        {
            chunkDataReadFromDisk = MessagePackSerializer.Deserialize<ConcurrentDictionary<Vector2Int, WorldData>>(worldData, lz4Options);
        }
        Debug.Log("saved chunks count:"+chunkDataReadFromDisk.Count);
        isJsonReadFromDisk = true;
        biomeNoiseGenerator.SetSeed(20000);
        biomeNoiseGenerator.SetFrequency(0.008f);
        biomeNoiseGenerator.SetNoiseType(FastNoise.NoiseType.Cellular);
        noiseGenerator.SetNoiseType(FastNoise.NoiseType.Perlin);
        noiseGenerator.SetFrequency(0.001f);
        noiseGenerator.SetFractalType(FastNoise.FractalType.FBM);
        noiseGenerator.SetFractalOctaves(1);
        RandomGenerator3D.InitNoiseGenerator();
        //    noiseGenerator.SetFractalLacunarity(100f);
        //    noiseGenerator. SetNoiseType(FastNoise.NoiseType.Value);
    }


    public void SaveWorldData()
    {

        FileStream fs;
        if (File.Exists(gameWorldDataPath + "unityMinecraftData/GameData/" + curWorldSaveName))
        {
            fs = new FileStream(gameWorldDataPath + "unityMinecraftData/GameData/" + curWorldSaveName, FileMode.Truncate, FileAccess.Write);//Truncate模式打开文件可以清空。
        }
        else
        {
            fs = new FileStream(gameWorldDataPath + "unityMinecraftData/GameData/" + curWorldSaveName, FileMode.Create, FileAccess.Write);
        }
        fs.Close();
        foreach (KeyValuePair<Vector2Int, Chunk> c in chunks)
        {
            // int[] worldDataMap=ThreeDMapToWorldData(c.Value.map);
            //   int x=(int)c.Value.transform.position.x;
            //  int z=(int)c.Value.transform.position.z;
            //   WorldData wd=new WorldData();
            //   wd.map=worldDataMap;
            //   wd.posX=x;
            //   wd.posZ=z;
            //   string tmpData=JsonMapper.ToJson(wd);
            //   File.AppendAllText(Application.dataPath+"/GameData/world.json",tmpData+"\n");
            c.Value.SaveSingleChunk(this);
        }
        Debug.Log("saving chunks count:"+chunkDataReadFromDisk.Count);
        //    foreach(KeyValuePair<Vector2Int,WorldData> wd in chunkDataReadFromDisk){
        //  string tmpData=JsonSerializer.ToJsonString(wd.Value);
        //  File.AppendAllText(gameWorldDataPath+"unityMinecraftData/GameData/world.json",tmpData+"\n");
        //    }
        byte[] tmpData = MessagePackSerializer.Serialize(chunkDataReadFromDisk, lz4Options);
        File.WriteAllBytes(gameWorldDataPath + "unityMinecraftData/GameData/"+ curWorldSaveName, tmpData);
        isWorldDataSaved = true;
    }

    public VoxelWorld(string curWorldSaveName,int worldGenType, int worldID)
    {
        this.curWorldSaveName = curWorldSaveName;
        this.worldGenType = worldGenType;
        this.worldID = worldID;
    }

    public void InitChunkLoader()
    {
        allChunkLoaders.Clear();
    }
   
    public ObjectPool<GameObject> creeperEntityPool;
    public ObjectPool<GameObject> zombieEntityPool;
    public ObjectPool<GameObject> tntEntityPool;
    public ObjectPool<GameObject> skeletonEntityPool;
    public ObjectPool<GameObject> endermanEntityPool;
    public ObjectPool<GameObject> arrowEntityPool;
    public ObjectPool<GameObject> particleEffectPool;
    public MyItemObjectPool itemEntityPool = new MyItemObjectPool();
    public static bool initObjects=InitObjects();
    public static bool InitObjects()
    {
        chunkPrefab = Resources.Load<GameObject>("Prefabs/chunk");
        pointLightPrefab = Resources.Load<GameObject>("Prefabs/chunkpointlightprefab");
        particlePrefab = Resources.Load<GameObject>("Prefabs/blockbreakingparticle");
        itemPrefab = Resources.Load<GameObject>("Prefabs/itementity");
        
        return true;
    }
    public void InitObjectPools()
    {
    /*    chunkPrefab = Resources.Load<GameObject>("Prefabs/chunk");
        pointLightPrefab = Resources.Load<GameObject>("Prefabs/chunkpointlightprefab");
        particlePrefab = Resources.Load<GameObject>("Prefabs/blockbreakingparticle");
        itemPrefab = Resources.Load<GameObject>("Prefabs/itementity");*/
        
        chunkPool.Object = chunkPrefab;
      //  Debug.Log(chunkPool.Object);
        chunkPool.maxCount = 3000;
        chunkPool.Init();

        itemEntityPool.Object = itemPrefab;
    //    Debug.Log(itemEntityPool.Object);
        itemEntityPool.maxCount = 300;
        itemEntityPool.Init();
        particleEffectPool = new ObjectPool<GameObject>(CreateEffect, GetEffect, ReleaseEffect, DestroyEffect, true, 10, 300);
        creeperEntityPool = new ObjectPool<GameObject>(CreateCreeper, GetCreeper, ReleaseCreeper, DestroyCreeper, true, 10, 300);
        zombieEntityPool = new ObjectPool<GameObject>(CreateZombie, GetZombie, ReleaseZombie, DestroyZombie, true, 10, 300);
        tntEntityPool = new ObjectPool<GameObject>(CreateTNT, GetTNT, ReleaseTNT, DestroyTNT, true, 10, 300);
        skeletonEntityPool = new ObjectPool<GameObject>(CreateSkeleton, GetSkeleton, ReleaseSkeleton, DestroySkeleton, true, 10, 300);
        arrowEntityPool = new ObjectPool<GameObject>(CreateArrow, GetArrow, ReleaseArrow, DestroyArrow, true, 10, 300);
        endermanEntityPool = new ObjectPool<GameObject>(CreateEnderman,GetEnderman,ReleaseEnderman,DestroyEnderman,true,10,300);
    }

    public GameObject CreateEffect()
    {
        GameObject gameObject = GameObject.Instantiate(particlePrefab,new Vector3(0,100,0), Quaternion.identity);

        return gameObject;
    }

    void GetEffect(GameObject gameObject)
    {

        gameObject.SetActive(true);

    }
    void ReleaseEffect(GameObject gameObject)
    {
        gameObject.SetActive(false);

    }
    void DestroyEffect(GameObject gameObject)
    {

        GameObject.Destroy(gameObject);
    }



    public GameObject CreateCreeper()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[0], new Vector3(0, 100, 0), Quaternion.identity);

        return gameObject;
    }

    void GetCreeper(GameObject gameObject)
    {

        gameObject.SetActive(true);

    }
    void ReleaseCreeper(GameObject gameObject)
    {
        gameObject.SetActive(false);

    }
    void DestroyCreeper(GameObject gameObject)
    {

        GameObject.Destroy(gameObject);
    }
    public GameObject CreateZombie()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[1], new Vector3(100f, 0f, 100f), Quaternion.identity);

        return gameObject;
    }

    void GetZombie(GameObject gameObject)
    {

        gameObject.SetActive(true);

    }
    void ReleaseZombie(GameObject gameObject)
    {
        gameObject.SetActive(false);

    }
    void DestroyZombie(GameObject gameObject)
    {

        GameObject.Destroy(gameObject);
    }

    public GameObject CreateTNT()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[2], new Vector3(100f, 0f, 100f), Quaternion.identity);

        return gameObject;
    }

    void GetTNT(GameObject gameObject)
    {

        gameObject.SetActive(true);

    }
    void ReleaseTNT(GameObject gameObject)
    {
        gameObject.SetActive(false);

    }
    void DestroyTNT(GameObject gameObject)
    {

        GameObject.Destroy(gameObject);
    }

    public GameObject CreateSkeleton()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[3], new Vector3(100f, 0f, 100f), Quaternion.identity);

        return gameObject;
    }

    void GetSkeleton(GameObject gameObject)
    {

        gameObject.SetActive(true);

    }
    void ReleaseSkeleton(GameObject gameObject)
    {
        gameObject.SetActive(false);

    }
    void DestroySkeleton(GameObject gameObject)
    {

        GameObject.Destroy(gameObject);
    }


    public GameObject CreateArrow()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[4], new Vector3(100f, 0f, 100f), Quaternion.identity);

        return gameObject;
    }

    void GetArrow(GameObject gameObject)
    {

        gameObject.SetActive(true);

    }
    void ReleaseArrow(GameObject gameObject)
    {
        gameObject.SetActive(false);

    }
    void DestroyArrow(GameObject gameObject)
    {

        GameObject.Destroy(gameObject);
    }

    public GameObject CreateEnderman()
    {
        GameObject gameObject = GameObject.Instantiate(EntityBeh.worldEntityTypes[5], new Vector3(100f, 0f, 100f), Quaternion.identity);

        return gameObject;
    }

    void GetEnderman(GameObject gameObject)
    {

        gameObject.SetActive(true);

    }
    void ReleaseEnderman(GameObject gameObject)
    {
        gameObject.SetActive(false);

    }
    void DestroyEnderman(GameObject gameObject)
    {

        GameObject.Destroy(gameObject);
    }
    public void ReInitEntityPlayerPosition()
    {
        ZombieBeh.isZombiePrefabLoaded = false;
        SkeletonBeh.isSkeletonPrefabLoaded = false;  
        CreeperBeh.isCreeperPrefabLoaded = false;
        EndermanBeh.isEndermanPrefabLoaded = false;
       
    }
    public void InitWorld()
    {
        Debug.Log("current world ID:" + worldID);
         
        
        int playerInWorldID=0;
        if (isWorldChanged == true)
        {
         playerInWorldID = PlayerMove.instance.ReadPlayerJson(true);
        }
        else
        {
          playerInWorldID = PlayerMove.instance.ReadPlayerJson();

            if (playerInWorldID != worldID)
            {

                return;
            }
        }
       
       
      
        // InitChunkPool();
       
        ReInitEntityPlayerPosition();
        ReadJson();
        chunks.Clear();
       
        EntityBeh.ReadEntityJson();
        EntityBeh.SpawnEntityFromFile();
        ItemEntityBeh.ReadItemEntityJson();
        ItemEntityBeh.SpawnItemEntityFromFile();
        chunkSpawningQueue = new SimplePriorityQueue<Vector2Int>();
        chunkLoadingQueue = new SimplePriorityQueue<ChunkLoadingQueueItem>();
        chunkUnloadingQueue = new SimplePriorityQueue<Vector2Int>();
        isGoingToQuitWorld = false;
        updateAllChunkLoadersThread = Task.Run(() => VoxelWorld.currentWorld.TryUpdateAllChunkLoadersThread());
         
        //   t2.Start();
        tryReleaseChunksThread = Task.Run(() => VoxelWorld.currentWorld.TryReleaseChunkThread());
        //   t3.Start();
        tryUpdateChunksThread = Task.Run(() => VoxelWorld.currentWorld.TryUpdateChunkThread());
        if(actionOnSwitchedWorld != null)
        {
        actionOnSwitchedWorld();
            actionOnSwitchedWorld=null;
        }
        

    }

    public void DestroyAllChunks()
    {
        
        foreach (var cKvp in chunks)
        {
            var c = cKvp.Value;
            lock(c.taskLock)
            {
            c.leftChunk = null;
            c.rightChunk = null;
            c.frontChunk = null;
            c.backChunk = null;
            c.backLeftChunk = null;
            c.backRightChunk = null;
            c.frontLeftChunk = null;
            c.frontRightChunk = null;
       //     GameObject.Destroy(c.gameObject); 
                c = null;
            }
            
         
           
          
        }
        chunks.Clear();
    }
    public static void SwitchToWorldWithoutSaving(int worldIndex)
    {
        if (worldIndex >= worlds.Count)
        {
            Debug.Log("invalid index");
            return;
        }
        isWorldChanged = true;
        currentWorld = worlds[worldIndex];
    }
    public void SaveAndQuitWorld()
    {

       // PlayerMove player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        PlayerMove.instance.SavePlayerData();
      
        EntityBeh.SaveWorldEntityData();
        ItemEntityBeh.SaveWorldItemEntityData();
        SaveWorldData();
        DestroyAllChunks();
      
        //     chunks.Clear();
        isGoingToQuitWorld = true;

    }
    public static void SwitchToWorld(int worldIndex)
    {
        if (worldIndex >= worlds.Count)
        {
            Debug.Log("invalid index");
            return;
        }
      
        isWorldChanged =true;
        currentWorld.SaveAndQuitWorld();
        
        currentWorld = worlds[worldIndex];
    //    currentWorld.InitWorld();
    }
}