using MessagePack;
using Priority_Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using monogameMinecraftShared.World;
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
    public ConcurrentDictionary<Vector2Int, ChunkData> chunkDataReadFromDisk = new ConcurrentDictionary<Vector2Int, ChunkData>();
    public static string gameWorldDataPath;
    public string curWorldSaveName="default.json";
    public int worldGenType = 0;
    public static VoxelWorld currentWorld=worlds[0];

    public SimplePriorityQueue<Vector2Int> chunkSpawningQueue = new SimplePriorityQueue<Vector2Int>();
    public SimplePriorityQueue<Chunk> chunkLoadingQueue = new SimplePriorityQueue<Chunk>();
    public SimplePriorityQueue<Vector2Int> chunkUnloadingQueue = new SimplePriorityQueue<Vector2Int>();
    public MyChunkObjectPool chunkPool = new MyChunkObjectPool();
    public bool isWorldDataSaved = false;
    public bool isJsonReadFromDisk = false;
    public bool isGoingToQuitWorld = false;
    public bool isFastChunkLoadingEnabled = false;
    public FastNoiseLite noiseGenerator = new FastNoiseLite();
    public FastNoiseLite biomeNoiseGenerator = new FastNoiseLite();
    public FastNoiseLite frequentNoiseGenerator = new FastNoiseLite();
    public Task updateAllChunkLoadersThread;
    public Task tryReleaseChunksThread;
    public Task tryUpdateChunksThread;
    public Action actionOnSwitchedWorld;
    public WorldUpdater worldUpdater;

    public EntityManager entityManager;
    public ItemEntityManager itemEntityManager;
    public WorldAccessor worldAccessor;
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
            
         //   Debug.Log("running world ID:"+worldID);   
            
            if (isGoingToQuitWorld == true)
            {
                return;
            }
            foreach (var cl in allChunkLoaders)
            {
                //     Debug.Log(cl.chunkLoadingCenter);
                if (cl.isChunksNeedLoading == true)
                {
                    cl.TryUpdateWorldThread(this);
                }

            }
            Thread.Sleep(50);
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
            Thread.Sleep(200);
        }


    }

    public void TryUpdateChunkThread()
    {
        //     delegate void mainBuildChunk();
        //   mainBuildChunk callback;

        while (true)
        {
            
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
                        chunkLoadingQueue.Enqueue(c.Value, -50);
                    }
                    else
                    {
                        continue;
                    }
                    //InitMap(chunkPos);
                    c.Value.isChunkMapUpdated = false;
                }
            }
            Thread.Sleep(5);


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
                        return;
                        /*   blockedDisablingCount++;
                           if (blockedDisablingCount > 15)
                           {
                               blockedDisablingCount = 0;
                               GameObject.Destroy(c.gameObject);
                               chunkUnloadingQueue.Dequeue();
                               Debug.Log("destroy");
                               return;
                           }*/
                         
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


    public bool CheckIsChunkPosSpawning(Vector2Int chunkPos)
    {
        return chunkSpawningQueue.Contains(chunkPos);
    }

    public void AddSpawningChunkPosition(Vector2Int chunkPos,int priority)
    {
        chunkSpawningQueue.Enqueue(chunkPos,
          priority);
    }

    public void EnqueueBuildingChunk(Chunk c, int buildingPriority)
    {
        chunkLoadingQueue.Enqueue(c,buildingPriority);
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
            for (int i = 0; i < 1; i++)
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
                    if (chunkLoadingQueue.First == null)
                    {
                        chunkLoadingQueue.Dequeue();
                        continue;
                    }




                    chunkLoadingQueue.First.StartLoadChunk();



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
                    if (chunkLoadingQueue.First == null)
                    {
                        chunkLoadingQueue.Dequeue();
                        continue;
                    }




                    chunkLoadingQueue.First.StartLoadChunk();



                    chunkLoadingQueue.Dequeue();


                }
            }
        }







    }
    [Obsolete]
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
            chunkDataReadFromDisk = MessagePackSerializer.Deserialize<ConcurrentDictionary<Vector2Int, ChunkData>>(worldData, lz4Options);
        }
        Debug.Log("saved chunks count:"+chunkDataReadFromDisk.Count);
        isJsonReadFromDisk = true;
      
        
        //    noiseGenerator.SetFractalLacunarity(100f);
        //    noiseGenerator. SetNoiseType(FastNoise.NoiseType.Value);
    }

    public void SetDefaultNoiseGeneratorParams()
    {
        biomeNoiseGenerator.SetSeed(20000);
        biomeNoiseGenerator.SetFrequency(0.012f);
        biomeNoiseGenerator.SetFractalType(FastNoiseLite.FractalType.FBm);
        biomeNoiseGenerator.SetFractalOctaves(3);
        biomeNoiseGenerator.SetFractalLacunarity(3f);
        biomeNoiseGenerator.SetFractalGain(0.3f);
        biomeNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noiseGenerator.SetFrequency(0.002f);
        noiseGenerator.SetFractalType(FastNoiseLite.FractalType.FBm);
        noiseGenerator.SetFractalOctaves(2);
        noiseGenerator.SetFractalLacunarity(5f);
        noiseGenerator.SetFractalGain(0.3f);
        noiseGenerator.SetFractalWeightedStrength(0.5f);
    }
    public void FetchWorldData()
    {
        
            if (!GameDataPersistenceManager.instance.IsWorldDataLoaded(worldID))
            {
                Debug.Log("world data of world "+worldID+" has not been loaded");
                chunkDataReadFromDisk = new ConcurrentDictionary<Vector2Int, ChunkData>();
            }
            chunkDataReadFromDisk = GameDataPersistenceManager.instance.allWorldsDataReadFormFile[worldID];
        

    }

    public void SaveWorldDataToPersistanceManager()
    {
        foreach (KeyValuePair<Vector2Int, Chunk> c in chunks)
        {
         
            c.Value.SaveSingleChunkToDictionary(ref chunkDataReadFromDisk);
        }
        GameDataPersistenceManager.instance.allWorldsDataReadFormFile[worldID] = chunkDataReadFromDisk;
    }
    [Obsolete]
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
        worldAccessor = new WorldAccessor(this);
        worldUpdater = new WorldUpdater(this, worldAccessor);
        entityManager = new EntityManager(this);
        itemEntityManager = new ItemEntityManager(this);
    }

    public void InitChunkLoader()
    {
        allChunkLoaders.Clear();
    }
   
     public ObjectPool<GameObject> particleEffectPool;
    
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

       
        particleEffectPool = new ObjectPool<GameObject>(CreateEffect, GetEffect, ReleaseEffect, DestroyEffect, true, 10, 300);
        
        entityManager.InitObjectPools();
        itemEntityManager.InitObjectPools();
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
   /*     if (isWorldChanged == true)
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
        }*/
       
       
        
            
        
        ReInitEntityPlayerPosition();
       

        GameDataPersistenceManager.instance.LoadAllDataOfSingleWorld(worldID);
        SetDefaultNoiseGeneratorParams();
        FetchWorldData();
        chunks.Clear();

        entityManager.FetchEntityData();
        entityManager.SpawnEntityFromFile();

        itemEntityManager.FetchItemEntityData();
        itemEntityManager.SpawnItemEntityFromFile();
        chunkSpawningQueue = new SimplePriorityQueue<Vector2Int>();
        chunkLoadingQueue = new SimplePriorityQueue<Chunk>();
        chunkUnloadingQueue = new SimplePriorityQueue<Vector2Int>();
        isGoingToQuitWorld = false;
        updateAllChunkLoadersThread = Task.Run(TryUpdateAllChunkLoadersThread);
         
        //   t2.Start();
        tryReleaseChunksThread = Task.Run(TryReleaseChunkThread);
        //   t3.Start();
        tryUpdateChunksThread = Task.Run(TryUpdateChunkThread);
        worldUpdater.Init();
        if (actionOnSwitchedWorld != null)
        {
            actionOnSwitchedWorld();
            actionOnSwitchedWorld=null;
        }
        

    }
    public void FrameUpdate(float deltaTime)
    {
        worldUpdater.MainThreadUpdate(deltaTime);
    }

    public void OnFixedUpdate(float deltaTime)
    {
       SpawnChunks();
       BuildAllChunks();
       DisableChunks();
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
        chunkDataReadFromDisk.Clear();
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
     //   PlayerMove.instance.SavePlayerData();
        
       // EntityBeh.SaveWorldEntityData();
        entityManager.SaveEntityDataToPersistenceManager();
        SaveWorldDataToPersistanceManager();
        itemEntityManager.SaveItemEntityDataToPersistenceManager();
        GameDataPersistenceManager.instance.SaveAllDataOfSingleWorld(worldID,true);
        DestroyAllChunks();
        entityManager.ReInit();
        itemEntityManager.ReInit();
      
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