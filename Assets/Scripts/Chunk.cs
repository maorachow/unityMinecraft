using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Rendering;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Priority_Queue;
using System.Collections.Concurrent;
using MessagePack;
using System.IO;
using Microsoft.SqlServer.Server;
using UnityEngine.EventSystems;
using Unity.Burst.Intrinsics;
using System.Linq;
//using FastNoise;
[MessagePackObject]
public struct WorldData{
    [Key(0)]
    public int posX;
    [Key(1)]
    public int posZ;
    [Key(2)]
    public short[,,] map;

    public WorldData(int posX,int posZ,short[,,] map){
        this.posX=posX;
        this.posZ=posZ;
        this.map=map;
    }
}
public struct RandomGenerator3D{
  //  public System.Random rand=new System.Random(0);
    public static FastNoise randomNoiseGenerator=new FastNoise();
    public static void InitNoiseGenerator(){
      //  randomNoiseGenerator.SetSeed(0);
        randomNoiseGenerator.SetNoiseType(FastNoise.NoiseType.Value);
        randomNoiseGenerator.SetFrequency(100f);
       // randomNoiseGenerator.SetFractalType(FastNoise.FractalType.None);
    }
    public static int GenerateIntFromVec3(Vector3Int pos){
        float value=randomNoiseGenerator.GetSimplex(pos.x*2f,pos.y*2f,pos.z*2f);
        value+=1f;
        int finalValue=(int)(value*53f);
        finalValue=Mathf.Clamp(finalValue,0,100);
     //   Debug.Log(finalValue);
     //   System.Random rand=new System.Random(pos.x*pos.y*pos.z*100);
        return finalValue;
    } 
}

public class Chunk : MonoBehaviour
{   
    public struct Vertex{
        public Vector3 pos;
        public Vector3 normal;
        public Vector2 uvPos;
        public Vertex(Vector3 v3,Vector3 nor,Vector2 v2){
            pos=v3;
            normal=nor;
            uvPos=v2; 
        }
    }
   [BurstCompile]
    public struct BakeJob:IJob{
        public int meshID;
        public void Execute(){
            Physics.BakeMesh(meshID,false);
        }
    }
 
  //[BurstCompile]
    public struct MeshBuildJob:IJob{
       // public NativeArray<VertexAttributeDescriptor> vertexAttributes;
        public NativeArray<Vector3> verts;
     //   public NativeArray<VertexAttributeDescriptor> vertsDes;
        public NativeArray<Vector2> uvs;
        public NativeArray<int> tris;
            public int vertLen;
    
        public Mesh.MeshDataArray dataArray;
         public void Execute(){
             // Allocate mesh data for one mesh.
      //  dataArray = Mesh.AllocateWritableMeshData(1);
        var data = dataArray[0];
       
        // Tetrahedron vertices with positions and normals.
        // 4 faces with 3 unique vertices in each -- the faces
        // don't share the vertices since normals have to be
        // different for each face.
         /*   */
     
        // Four tetrahedron vertex positions:
   //     var sqrt075 = Mathf.Sqrt(0.75f);
   //     var p0 = new Vector3(0, 0, 0);
   //     var p1 = new Vector3(1, 0, 0);
   //     var p2 = new Vector3(0.5f, 0, sqrt075);
   //     var p3 = new Vector3(0.5f, sqrt075, sqrt075 / 3);
        // The first vertex buffer data stream is just positions;
        // fill them in.
  
        NativeArray<Vertex> pos = data.GetVertexData<Vertex>();
        if(pos==null){
             data.SetVertexBufferParams(verts.Length,new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
             pos = data.GetVertexData<Vertex>();
        }
        //int posLen=;
      //  pos=verts;
     // Debug.Log(pos.Length);
        for(int i=0;i<pos.Length;i++){
            if(pos==null){
                Debug.Log("null");
                return;
            }
            pos[i]=new Vertex(verts[i],new Vector3(1f,1f,1f),uvs[i]);
           
        }
        // Note: normals will be calculated later in RecalculateNormals.
        // Tetrahedron index buffer: 4 triangles, 3 indices per triangle.
        // All vertices are unique so the index buffer is just a
        // 0,1,2,...,11 sequence.
    //    data.SetIndexBufferParams(verts.Length, IndexFormat.UInt16);
       data.SetIndexBufferParams((int)(pos.Length/2)*3, IndexFormat.UInt32);
        var ib = data.GetIndexData<int>();
      
        for (int i = 0; i < ib.Length; ++i)
            ib[i] = tris[i];
        // One sub-mesh with all the indices.
        data.subMeshCount = 1;
        data.SetSubMesh(0, new SubMeshDescriptor(0, ib.Length));
        // Create the mesh and apply data to it:
     //   Debug.Log("job");
  //   int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
   //        UnityEngine.Debug.Log(threadId == 1 ? "Main thread" : $"Worker thread {threadId}");
           pos.Dispose();
           ib.Dispose();
        }

    }
    //public enum BlockType
  //  {
    //    None = 0,
  //      Stone = 1,
   //     Grass = 2,
    //    Dirt=3,
  //  }
  //0None 1Stone 2Grass 3Dirt 4Side grass block 5Bedrock 6WoodX 7WoodY 8WoodZ 9Leaves 10Diamond Ore 11Sand
  //100Water 101Grass
  //102torch
  //200Leaves
  //0-99solid blocks
  //100-199no hitbox blocks
  //200-299hitbox nonsolid blocks
    public static Dictionary<int,AudioClip> blockAudioDic=new Dictionary<int,AudioClip>();
    public static FastNoise noiseGenerator { get { return VoxelWorld.currentWorld.noiseGenerator; } }
    public static FastNoise frequentNoiseGenerator { get { return VoxelWorld.currentWorld.frequentNoiseGenerator; } }
    public static FastNoise biomeNoiseGenerator { get { return VoxelWorld.currentWorld.biomeNoiseGenerator; } }
    public static MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

    public static string gameWorldDataPath;
    public delegate bool TmpCheckFace(int x,int y,int z);
    public delegate void TmpBuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int side);
    public static bool isBlockInfoAdded=false;
    public static bool isJsonReadFromDisk=false;
    public static bool isWorldDataSaved=false;
    public bool isMapGenCompleted=false;
    public bool isMeshBuildCompleted=false;
    public bool isChunkMapUpdated=false;
    public bool isSavedInDisk=false;
    public bool isModifiedInGame=false;
    public bool isChunkPosInited=false;

    public static Dictionary<int,List<Vector2>> itemBlockInfo=new Dictionary<int,List<Vector2>>();
    public static Dictionary<int,List<Vector2>> blockInfo=new Dictionary<int,List<Vector2>>();
    public Mesh chunkMesh;
    public Mesh chunkMeshLOD1;
    public Mesh chunkMeshLOD2;
    public Mesh chunkWaterMesh;
    public Mesh chunkNonSolidMesh;
    public static int worldGenType=0;
    //0Inf 1Superflat
    public static int chunkWidth=16;
    public static int chunkHeight=256;
    public static int chunkSeaLevel=63;
    public static int chunkDensityMapHeight = chunkHeight / 4;
  //  public static System.Random worldRandomGenerator=new System.Random(0);
//    public static ConcurrentDictionary<Vector2Int,Chunk> Chunks=new ConcurrentDictionary<Vector2Int,Chunk>();
//    public static ConcurrentDictionary<Vector2Int,WorldData> chunkDataReadFromDisk=new ConcurrentDictionary<Vector2Int,WorldData>();
  //  public static object chunkLock=new object();
   // public static object chunkDataLock=new object();
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;
    public MeshFilter meshFilter;
   // public MeshCollider meshColliderNS;
    public MeshRenderer meshRendererNS;
    public MeshFilter meshFilterNS;
    public MeshRenderer meshRendererWT;
    public MeshFilter meshFilterWT;
    public MeshFilter meshFilterLOD1;
    public MeshRenderer meshRendererLOD1;
    public MeshFilter meshFilterLOD2;
    public MeshRenderer meshRendererLOD2;
    public LODGroup lodGroup;
    public short[,,] map = new short[chunkWidth, chunkHeight, chunkWidth];
    
    public Chunk frontChunk;
    public Chunk backChunk;
    public Chunk leftChunk;
    public Chunk rightChunk;
    public Chunk frontLeftChunk;
    public Chunk frontRightChunk;
    public Chunk backLeftChunk;
    public Chunk backRightChunk;
    
  /*  public float[,] leftHeightMap;
    public float[,] rightHeightMap;
    public float[,] frontHeightMap;
    public float[,] backHeightMap; */
    public float[,] thisHeightMap;

    public float[,,] this3DDensityMap;
    public Vector2Int chunkPos;
//    public static Transform playerPos;
    public static Vector3 playerPosVec;
    public float playerDistance;
   // public TransformAccessArray thisTransArray= new TransformAccessArray(transform);
      public Vector3[] opqVerts;
    public Vector2[] opqUVs;
    public int[] opqTris;
    public Vector3[] NSVerts;
    public Vector2[] NSUVs;
    public int[] NSTris;
    public List<Vector3> lightPoints;
    public List<GameObject> pointLightGameObjects=new List<GameObject>();
    public NativeArray<Vector3> opqVertsNA;
    public NativeArray<Vector2> opqUVsNA;
    public NativeArray<int> opqTrisNA;
    public NativeArray<Vector3> NSVertsNA;
    public NativeArray<Vector2> NSUVsNA;
    public NativeArray<int> NSTrisNA;
  //  public bool isChunkColliderUpdated=false;
 
    public static void AddBlockInfo(){
        //left right bottom top back front
        blockAudioDic.Clear();
        blockAudioDic.TryAdd(1,Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDic.TryAdd(2,Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDic.TryAdd(3,Resources.Load<AudioClip>("Audios/Gravel_dig1"));
        blockAudioDic.TryAdd(4,Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDic.TryAdd(5,Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDic.TryAdd(6,Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDic.TryAdd(7,Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDic.TryAdd(8,Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDic.TryAdd(9,Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDic.TryAdd(10,Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDic.TryAdd(11,Resources.Load<AudioClip>("Audios/Sand_dig1"));
        blockAudioDic.TryAdd(12, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDic.TryAdd(13, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDic.TryAdd(100,Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDic.TryAdd(101,Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDic.TryAdd(102,Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockInfo.Clear();
        blockInfo.TryAdd(1,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)});
        blockInfo.TryAdd(2,new List<Vector2>{new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f)});
        blockInfo.TryAdd(3,new List<Vector2>{new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f)});
        blockInfo.TryAdd(4,new List<Vector2>{new Vector2(0.1875f,0f),new Vector2(0.1875f,0f),new Vector2(0.125f,0f),new Vector2(0.0625f,0f),new Vector2(0.1875f,0f),new Vector2(0.1875f,0f)});
        blockInfo.TryAdd(100,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)});
        blockInfo.TryAdd(101,new List<Vector2>{new Vector2(0f,0.0625f)});
        blockInfo.TryAdd(5,new List<Vector2>{new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f)});
        blockInfo.TryAdd(6,new List<Vector2>{new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f)});
        blockInfo.TryAdd(7,new List<Vector2>{new Vector2(0.3125f,0f),new Vector2(0.3125f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.3125f,0f),new Vector2(0.3125f,0f)});
        blockInfo.TryAdd(8,new List<Vector2>{new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f)});
        blockInfo.TryAdd(9,new List<Vector2>{new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f)});
        blockInfo.TryAdd(10,new List<Vector2>{new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f)});
        blockInfo.TryAdd(11,new List<Vector2>{new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f)});
        blockInfo.TryAdd(12, new List<Vector2> { new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f) });
        blockInfo.TryAdd(13, new List<Vector2> { new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.8125f, 0f), new Vector2(0.75f, 0f), new Vector2(0.75f, 0f) });
        itemBlockInfo.Clear();
        itemBlockInfo.TryAdd(1,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)});
        itemBlockInfo.TryAdd(2,new List<Vector2>{new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f)});
        itemBlockInfo.TryAdd(3,new List<Vector2>{new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f)});
        itemBlockInfo.TryAdd(4,new List<Vector2>{new Vector2(0.1875f,0f),new Vector2(0.1875f,0f),new Vector2(0.125f,0f),new Vector2(0.0625f,0f),new Vector2(0.1875f,0f),new Vector2(0.1875f,0f)});
        itemBlockInfo.TryAdd(100,new List<Vector2>{new Vector2(0.125f,0.0625f),new Vector2(0.125f,0.0625f),new Vector2(0.125f,0.0625f),new Vector2(0.125f,0.0625f),new Vector2(0.125f,0.0625f),new Vector2(0.125f,0.0625f)});
        itemBlockInfo.TryAdd(101,new List<Vector2>{new Vector2(0f,0.0625f)});
        itemBlockInfo.TryAdd(5,new List<Vector2>{new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f)});
        itemBlockInfo.TryAdd(6,new List<Vector2>{new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f)});
        itemBlockInfo.TryAdd(7,new List<Vector2>{new Vector2(0.3125f,0f),new Vector2(0.3125f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.3125f,0f),new Vector2(0.3125f,0f)});
        itemBlockInfo.TryAdd(8,new List<Vector2>{new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f)});
        itemBlockInfo.TryAdd(11,new List<Vector2>{new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f)});
        itemBlockInfo.TryAdd(12, new List<Vector2> { new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f) });
        itemBlockInfo.TryAdd(13, new List<Vector2> { new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.8125f, 0f), new Vector2(0.75f, 0f), new Vector2(0.75f, 0f) });
        itemBlockInfo.TryAdd(9,new List<Vector2>{new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f)});
        itemBlockInfo.TryAdd(156, new List<Vector2> { new Vector2(320f/1024f, 128f/1024f), new Vector2(320f / 1024f, 128f / 1024f), new Vector2(448f / 1024f, 128f / 1024f), new Vector2(384f / 1024f, 128f / 1024f), new Vector2(320f / 1024f, 128f / 1024f), new Vector2(320f / 1024f, 128f / 1024f) });
        isBlockInfoAdded =true;

    }
 /*   public static void ReadJson(){
    chunkDataReadFromDisk.Clear();
     gameWorldDataPath=WorldManager.gameWorldDataPath;
         
         if (!Directory.Exists(gameWorldDataPath+"unityMinecraftData")){
                Directory.CreateDirectory(gameWorldDataPath+"unityMinecraftData");
               
            }
          if(!Directory.Exists(gameWorldDataPath+"unityMinecraftData/GameData")){
                    Directory.CreateDirectory(gameWorldDataPath+"unityMinecraftData/GameData");
                }
       
        if(!File.Exists(gameWorldDataPath+"unityMinecraftData"+"/GameData/world.json")){
            FileStream fs=File.Create(gameWorldDataPath+"unityMinecraftData"+"/GameData/world.json");
            fs.Close();
        }
       
        byte[] worldData=File.ReadAllBytes(gameWorldDataPath+"unityMinecraftData/GameData/world.json");
      //  List<WorldData> tmpList=new List<WorldData>();
       /* foreach(string s in worldData){
            WorldData tmp=JsonSerializer.Deserialize<WorldData>(s);
            tmpList.Add(tmp);
        }
        foreach(WorldData w in tmpList){
            chunkDataReadFromDisk.Add(new Vector2Int(w.posX,w.posZ),w);
        }
        if(worldData.Length>0){
        chunkDataReadFromDisk=MessagePackSerializer.Deserialize<ConcurrentDictionary<Vector2Int,WorldData>>(worldData,lz4Options);    
        }
        
        isJsonReadFromDisk=true;
        biomeNoiseGenerator.SetSeed(20000);
        biomeNoiseGenerator.SetFrequency(0.008f);
        biomeNoiseGenerator.SetNoiseType(FastNoise.NoiseType.Cellular);
        noiseGenerator.SetNoiseType(FastNoise.NoiseType.Perlin);
         noiseGenerator.SetFrequency(0.001f);
         noiseGenerator.SetFractalType(FastNoise.FractalType.FBM);
          noiseGenerator. SetFractalOctaves(1);
        RandomGenerator3D.InitNoiseGenerator();
       //    noiseGenerator.SetFractalLacunarity(100f);
       //    noiseGenerator. SetNoiseType(FastNoise.NoiseType.Value);
    }*/
 //   void Awake(){
      
 //   }
   



   public void ReInitData(){
    
      //  yield return new WaitUntil(()=>isJsonReadFromDisk==true); 

        chunkPos=new Vector2Int((int)transform.position.x,(int)transform.position.z);
    
        isChunkPosInited=true;
        //   lock(chunkLock){
        VoxelWorld.currentWorld.chunks.AddOrUpdate(chunkPos,(key)=>this,(key,value)=>this);
   /*      if(Chunks.ContainsKey(chunkPos)){
      //  if(GetChunk(chunkPos).gameObject!=null){
        //     ObjectPools.chunkPool.Remove(GetChunk(chunkPos).gameObject);
        //}
            Chunk c;
            Chunks.TryRemove(chunkPos,out c);  
            Chunks.TryAdd(chunkPos,this);   
       }else{
       Chunks.TryAdd(chunkPos,this);    
       }*/
    //   }
      
       

        if (VoxelWorld.currentWorld.chunkDataReadFromDisk.ContainsKey(chunkPos)){
            isSavedInDisk=true;
          //  Debug.Log(chunkPos);
        }
      //  StartLoadChunk();
   //   WorldManager.chunkLoadingQueue.Enqueue(this,(ushort)UnityEngine.Random.Range(0f,100f));
         VoxelWorld.currentWorld.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(this,true),(int)Mathf.Abs(transform.position.x-playerPosVec.x)+(int)Mathf.Abs(transform.position.z-playerPosVec.z));
          
    }

 /*   public void ReInitData(bool isStrongLoading){
    
      //  yield return new WaitUntil(()=>isJsonReadFromDisk==true); 

        chunkPos=new Vector2Int((int)transform.position.x,(int)transform.position.z);
    
        isChunkPosInited=true;
        //   lock(chunkLock){
        VoxelWorld.currentWorld.chunks.AddOrUpdate(chunkPos,(key)=>this,(key,value)=>this);
   /*      if(Chunks.ContainsKey(chunkPos)){
      //  if(GetChunk(chunkPos).gameObject!=null){
        //     ObjectPools.chunkPool.Remove(GetChunk(chunkPos).gameObject);
        //}
            Chunk c;
            Chunks.TryRemove(chunkPos,out c);  
            Chunks.TryAdd(chunkPos,this);   
       }else{
       Chunks.TryAdd(chunkPos,this);    
       }
    //   }
      
       

        if(chunkDataReadFromDisk.ContainsKey(chunkPos)){
            isSavedInDisk=true;
          //  Debug.Log(chunkPos);
        }
        //  StartLoadChunk();
        //   WorldManager.chunkLoadingQueue.Enqueue(this,(ushort)UnityEngine.Random.Range(0f,100f));
        VoxelWorld.currentWorld.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(this,isStrongLoading),(int)Mathf.Abs(transform.position.x-playerPosVec.x)+(int)Mathf.Abs(transform.position.z-playerPosVec.z));
          
    }*/
    public bool isStrongLoaded=false;
    //strongload: simulate chunk mesh collider
    public async void StrongLoadChunk(){
         JobHandle jh=new BakeJob{meshID=chunkMesh.GetInstanceID()}.Schedule();
        await UniTask.WaitUntil(()=>isTaskCompleted==true);
        jh.Complete();
        meshCollider.sharedMesh=chunkMesh;
        isStrongLoaded=true;
   //     Debug.DrawLine(transform.position+new Vector3(chunkWidth/2f,0f,chunkWidth/2f),transform.position+new Vector3(chunkWidth/2f,100f,chunkWidth/2f),Color.green,10f);
    }
    void OnDestroy(){
        
      
        map=null;
        thisHeightMap=null;
        DestroyImmediate(meshFilter.mesh, true);
        DestroyImmediate(meshFilterNS.mesh,true);
        DestroyImmediate(meshFilterWT.mesh,true);
        DestroyImmediate(chunkMesh, true);
        DestroyImmediate(chunkMeshLOD1, true);
        DestroyImmediate(chunkMeshLOD2, true);
        DestroyImmediate(chunkNonSolidMesh,true);
        DestroyImmediate(chunkWaterMesh,true);

    }
   
    public async void ChunkOnDisable(){
     //   taa=null;
    
        isRightChunkUnloaded=false;
        isFrontChunkUnloaded=false;
        isLeftChunkUnloaded=false;
        isBackChunkUnloaded=false;
        meshRenderer.enabled=false;
        meshRendererNS.enabled=false;
        meshRendererWT.enabled=false;
        meshRendererLOD1.enabled=false;
        meshRendererLOD2.enabled = false;
        isMapGenCompleted =false;
        isMeshBuildCompleted=false;
       
      //  meshCollider.sharedMesh=null;
         isChunkMapUpdated=false;
        if (VoxelWorld.currentWorld.chunkLoadingQueue.Contains(new ChunkLoadingQueueItem(this,true))){
            VoxelWorld.currentWorld.chunkLoadingQueue.Remove(new ChunkLoadingQueueItem(this,true)); 
         Debug.Log("remove");  
        }
        if (VoxelWorld.currentWorld.chunkLoadingQueue.Contains(new ChunkLoadingQueueItem(this,false))){
            VoxelWorld.currentWorld.chunkLoadingQueue.Remove(new ChunkLoadingQueueItem(this,false));   
        }
         SaveSingleChunk(VoxelWorld.currentWorld);
         isChunkPosInited=false;
        isStrongLoaded=false;
        isSavedInDisk=false;
        Chunk c;
        VoxelWorld.currentWorld.chunks.TryRemove(chunkPos,out c);
        c=null;
        isChunkMapUpdated=false;

        chunkPos=new Vector2Int(-10240000,-10240000);

        isModifiedInGame=false;
        
       map=new short[chunkWidth,chunkHeight,chunkWidth];
        
       //   map=new int[chunkWidth+2,chunkHeight+2,chunkWidth+2];
        
      
     
     //   isChunkPosInited=false;
    }


   // void Start(){
     
    //     chunkPos=new Vector2Int((int)transform.position.x,(int)transform.position.z);
      //    Chunks.Add(chunkPos,this); 
 /*       chunkPos=new Vector2Int((int)transform.position.x,(int)transform.position.z);
        Chunks.Add(chunkPos,this); 
        if(chunkDataReadFromDisk.ContainsKey(chunkPos)){
            isSavedInDisk=true;
        }
        meshRenderer = GetComponent<MeshRenderer>();
	    meshCollider = GetComponent<MeshCollider>();
	    meshFilter = GetComponent<MeshFilter>();
        meshRendererNS = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
	    meshColliderNS = transform.GetChild(0).gameObject.GetComponent<MeshCollider>();
	    meshFilterNS = transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
        StartLoadChunk();*/
     //   StartCoroutine(ReInitData());
   // }

    void Awake(){
     //   if(playerPos==null){
     //     playerPos=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();  
      //  }
          
       meshRenderer=GetComponent<MeshRenderer>();
	    meshCollider = GetComponent<MeshCollider>();
	    meshFilter = GetComponent<MeshFilter>();
        meshRendererNS = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
	  //  meshColliderNS = transform.GetChild(0).gameObject.GetComponent<MeshCollider>();
	    meshFilterNS = transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
        meshRendererWT = transform.GetChild(1).gameObject.GetComponent<MeshRenderer>();
	  //  meshColliderNS = transform.GetChild(0).gameObject.GetComponent<MeshCollider>();
	    meshFilterWT = transform.GetChild(1).gameObject.GetComponent<MeshFilter>();
        meshFilterLOD1=transform.GetChild(2).GetComponent<MeshFilter>(); 
        meshRendererLOD1=transform.GetChild(2).GetComponent<MeshRenderer>();
        meshFilterLOD2 = transform.GetChild(3).GetComponent<MeshFilter>();
        meshRendererLOD2 = transform.GetChild(3).GetComponent<MeshRenderer>();
        lodGroup =GetComponent<LODGroup>();
    }
 

    public void StartLoadChunk(){
        
      
    
           BuildChunk();
            
       
      //  Vector3 pos=transform.position;

   //     ThreadStart childref = new ThreadStart(() => InitMap(chunkPos));
      ///  Thread childThread=new Thread(childref);
     //   childThread.Start();
        
      
    }



 /*   public static int[,,] WorldDataTo3DMap(WorldData wd){
       //x->y->z
        int[,,] returnValue=new int[chunkWidth+2,chunkHeight+2,chunkWidth+2];
        //for(int i=0;i<wd.map.Length;i++){
       //     returnValue[i/(chunkHeight*chunkWidth),i%chunkHeight,(i/chunkHeight)%chunkWidth]=wd.map[i];
       // }
       int index=0;
       for(int x=0;x<chunkWidth;x++){
            for(int y=0;y<chunkHeight;y++){
                for(int z=0;z<chunkWidth;z++){
                    returnValue[x,y,z]=wd.map[index];
                    index++;
            }
        }
       }
       // Debug.Log(returnValue[2,2,2]);
        return returnValue;
    }
*/


  /*  public static int[] ThreeDMapToWorldData(int[,,] map){
        int[] returnValue=new int[chunkWidth*chunkHeight*chunkWidth+5];
        int index=0;
        for(int x=0;x<chunkWidth;x++){
            for(int y=0;y<chunkHeight;y++){
                for(int z=0;z<chunkWidth;z++){
                    returnValue[index]=map[x,y,z];
                    index++;
                }
            }

        }
      
        return returnValue;
    }*/
 /*   public void SaveSingleChunk(){
        if(!isChunkPosInited){
            return;
        }
        if(!isModifiedInGame){
            return;
        }
      //  lock(chunkDataLock){
          if(VoxelWorld.currentWorld.chunkDataReadFromDisk.ContainsKey(chunkPos)){
            

     //    int[,,] worldDataMap=map;
            WorldData wd=new WorldData(chunkPos.x,chunkPos.y,map);
          //  wd.map=worldDataMap;
          //  wd.posX=chunkPos.x;
          //  wd.posZ=chunkPos.y;
            WorldData wdtmp;
            VoxelWorld.currentWorld.chunkDataReadFromDisk.TryRemove(chunkPos,out wdtmp);
            VoxelWorld.currentWorld.chunkDataReadFromDisk.TryAdd(chunkPos,wd);
       //     wdtmp=null;
            
        }else{
     //       int[,,] worldDataMap=map;
            WorldData wd=new WorldData(chunkPos.x,chunkPos.y,map);
            VoxelWorld.currentWorld.chunkDataReadFromDisk.TryAdd(chunkPos,wd);
        }   

    //    }
       
    }*/


    public void SaveSingleChunk(VoxelWorld world)
    {
        if (!isChunkPosInited)
        {
            return;
        }
        if (!isModifiedInGame)
        {
            return;
        }
        //  lock(chunkDataLock){
        if (world.chunkDataReadFromDisk.ContainsKey(chunkPos))
        {


            //    int[,,] worldDataMap=map;
            WorldData wd = new WorldData(chunkPos.x, chunkPos.y, map);
            //  wd.map=worldDataMap;
            //  wd.posX=chunkPos.x;
            //  wd.posZ=chunkPos.y;
            WorldData wdtmp;
            world.chunkDataReadFromDisk.TryRemove(chunkPos, out wdtmp);
            world.chunkDataReadFromDisk.TryAdd(chunkPos, wd);
            //     wdtmp=null;

        }
        else
        {
            //       int[,,] worldDataMap=map;
            WorldData wd = new WorldData(chunkPos.x, chunkPos.y, map);
            world.chunkDataReadFromDisk.TryAdd(chunkPos, wd);
        }

        //    }

    }
 /*   public static void SaveWorldData(){
        
        FileStream fs;
        if (File.Exists(gameWorldDataPath+"unityMinecraftData/GameData/world.json"))
        {
                 fs = new FileStream(gameWorldDataPath+"unityMinecraftData/GameData/world.json", FileMode.Truncate, FileAccess.Write);//Truncate模式打开文件可以清空。
        }
        else
        {
                 fs = new FileStream(gameWorldDataPath+"unityMinecraftData/GameData/world.json", FileMode.Create, FileAccess.Write);
        }
        fs.Close();
        foreach(KeyValuePair<Vector2Int,Chunk> c in Chunks){
       // int[] worldDataMap=ThreeDMapToWorldData(c.Value.map);
     //   int x=(int)c.Value.transform.position.x;
      //  int z=(int)c.Value.transform.position.z;
     //   WorldData wd=new WorldData();
     //   wd.map=worldDataMap;
     //   wd.posX=x;
     //   wd.posZ=z;
     //   string tmpData=JsonMapper.ToJson(wd);
     //   File.AppendAllText(Application.dataPath+"/GameData/world.json",tmpData+"\n");
     //   c.Value.SaveSingleChunk();
        }
        Debug.Log(chunkDataReadFromDisk.Count);
   //    foreach(KeyValuePair<Vector2Int,WorldData> wd in chunkDataReadFromDisk){
      //  string tmpData=JsonSerializer.ToJsonString(wd.Value);
      //  File.AppendAllText(gameWorldDataPath+"unityMinecraftData/GameData/world.json",tmpData+"\n");
   //    }
        byte[] tmpData=MessagePackSerializer.Serialize(chunkDataReadFromDisk,lz4Options);
        File.WriteAllBytes(gameWorldDataPath+"unityMinecraftData/GameData/world.json",tmpData);
        isWorldDataSaved=true;
    }*/

 //0sea 1forest 2desert
    public static unsafe int[,] GenerateChunkBiomeMap(Vector2Int pos){
        //   float[,] biomeMap=new float[chunkWidth/8+2,chunkWidth/8+2];//插值算法
       //      int[,] chunkBiomeMap=GenerateChunkBiomeMap(pos);
            int[,] biomeMapInter=new int[chunkWidth/8+2,chunkWidth/8+2];//二维数组
            fixed(int* p=&biomeMapInter[0,0]){
                int* p1=p;
                for(int i=0;i<chunkWidth/8+2;i++){
                    for(int j=0;j<chunkWidth/8+2;j++){
         //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))

                /*biomeMapInter[i,j]*/*p1=(int)(1f+biomeNoiseGenerator.GetSimplex(pos.x+(i-1)*8,pos.y+(j-1)*8)*3f);
                    p1++;//正常运行
                }
            }//32,32
            }
            
          
          
           
  
        return biomeMapInter;
    }
    //int[,]多维数组
    //访问[x,y]元素*p=arr[0,0]+x*i+j
    //int[i][j]交错数组
    //指针访问[x][y]元素*p=&arr[0][0]+y*i+j
    public static float[,] GetRawChunkHeightmap(Vector2Int pos)
    {
        float[,] heightMap = new float[chunkWidth / 8 + 2, chunkWidth / 8 + 2];//插值算法
        int[,] chunkBiomeMap = GenerateChunkBiomeMap(pos);

        for (int i = 0; i < chunkWidth / 8 + 2; i++)
        {
            for (int j = 0; j < chunkWidth / 8 + 2; j++)
            {
                //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))

                heightMap[i, j] = chunkSeaLevel + noiseGenerator.GetSimplex(pos.x + (i - 1) * 8, pos.y + (j - 1) * 8) * 20f + chunkBiomeMap[i, j] * 15f;
                switch (chunkBiomeMap[i, j])
                {
                    case -1:
                        heightMap[i, j] += (float)RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(pos.x + (i - 1) * 8, 1, pos.y + (j - 1) * 8)) * 0.2f - 10f;
                        break;
                    case 3:
                        heightMap[i, j] += (float)RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(pos.x + (i - 1) * 8, 1, pos.y + (j - 1) * 8)) * 0.4f - 10f;
                        break;
                }
            }

        }
        return heightMap;
    }
     public static unsafe float[,] GenerateChunkHeightmap(Vector2Int pos){
        /*       float[,] heightMap=new float[chunkWidth/8+2,chunkWidth/8+2];//插值算法
               int[,] chunkBiomeMap=GenerateChunkBiomeMap(pos);

              for(int i=0;i<chunkWidth/8+2;i++){
                  for(int j=0;j<chunkWidth/8+2;j++){
           //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                  //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))

                  heightMap[i,j]=chunkSeaLevel+noiseGenerator.GetSimplex(pos.x+(i-1)*8,pos.y+(j-1)*8)*20f+chunkBiomeMap[i,j]*15f;
                  switch(chunkBiomeMap[i,j]){
                      case -1:heightMap[i,j]+=(float)RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(pos.x+(i-1)*8,1,pos.y+(j-1)*8))*0.2f-10f;
                      break;
                      case 3:heightMap[i,j]+=(float)RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(pos.x+(i-1)*8,1,pos.y+(j-1)*8))*0.4f-10f;
                      break;
                  }
                  }

              }//32,32*/
        float[,] heightMap = GetRawChunkHeightmap(pos);
            int interMultiplier=8;
            float[,] heightMapInterpolated=new float[(chunkWidth/8+2)*interMultiplier,(chunkWidth/8+2)*interMultiplier];
            fixed(float* p=&heightMapInterpolated[0,0]){
                float* p2=p;
              for(int i=0;i<(chunkWidth/8+2)*interMultiplier;++i){
                for(int j=0;j<(chunkWidth/8+2)*interMultiplier;++j){
                        int x=i;
                        int y=j;
                        float x1=(i/interMultiplier)*interMultiplier;
                        float x2=(i/interMultiplier)*interMultiplier+interMultiplier;
                        float y1=(j/interMultiplier)*interMultiplier;
                        float y2=(j/interMultiplier)*interMultiplier+interMultiplier;
                        int x1Ori=(i/interMultiplier);
                       // Debug.Log(x1Ori);
                        int x2Ori=(i/interMultiplier)+1;
                        x2Ori=Mathf.Clamp(x2Ori,0,(chunkWidth/8+2)-1);
                      //   Debug.Log(x2Ori);
                        int y1Ori=(j/interMultiplier);
                      //   Debug.Log(y1Ori);
                        int y2Ori=(j/interMultiplier)+1;
                         y2Ori=Mathf.Clamp(y2Ori,0,(chunkWidth/8+2)-1);
                    //     Debug.Log(y2Ori);
                    
                        float q11=heightMap[x1Ori,y1Ori];
                        float q12=heightMap[x1Ori,y2Ori];
                        float q21=heightMap[x2Ori,y1Ori];
                        float q22=heightMap[x2Ori,y2Ori];
                        float fxy1=(float)(x2-x)/(x2-x1)*q11+(float)(x-x1)/(x2-x1)*q21;
                        float fxy2=(float)(x2-x)/(x2-x1)*q12+(float)(x-x1)/(x2-x1)*q22;
                        float fxy=(float)(y2-y)/(y2-y1)*fxy1+(float)(y-y1)/(y2-y1)*fxy2;
                        *p2=fxy;
                        p2++;
                 //       Debug.Log(fxy);
                    //    Debug.Log(x1);
                      //  Debug.Log(x2);

                }
            }  
            }
        

        return heightMapInterpolated;
     }

    
    public static unsafe float[,,] GenerateChunk3DDensityMap(Vector2Int pos)
    {
               float[,,] densityMap=new float[chunkWidth/8+2, chunkHeight / 8,chunkWidth/8+2];//插值算法
              

              for(int i=0;i<chunkWidth/8+2;i++){
                  for(int j=0;j<chunkWidth/8+2;j++){
                for(int k = 0; k < chunkHeight / 8; k++)
                {
                    densityMap[i,k,j]=frequentNoiseGenerator.GetSimplex(pos.x+(i-1)*8,k*8,pos.y+(j-1)*8);

                  }
                }
           //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                  //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))

              

              }//32,32
   
        int interMultiplier = 8;
        float[,,] densityMapInterpolated = new float[(chunkWidth / 8 + 2) * interMultiplier,(chunkHeight / 8)*interMultiplier, (chunkWidth / 8 + 2) * interMultiplier];
        
         
            for (int i = 0; i < (chunkWidth / 8 + 2) * interMultiplier; ++i)
            {
                for (int j = 0; j < (chunkWidth / 8 + 2) * interMultiplier; ++j)
                {
                for (int k = 0; k < (chunkHeight / 8) * interMultiplier; k++)
                {
                        
                            int x = i;
                    int z = j;
                    int y = k;
                    float x1 = (i / interMultiplier) * interMultiplier;
                    float x2 = (i / interMultiplier) * interMultiplier + interMultiplier;
                    float z1 = (j / interMultiplier) * interMultiplier;
                    float z2 = (j / interMultiplier) * interMultiplier + interMultiplier;
                    float y1 = (k / interMultiplier) * interMultiplier;
                    float y2 = (k / interMultiplier) * interMultiplier + interMultiplier;
                    int x1Ori = (i / interMultiplier);
                    // Debug.Log(x1Ori);
                    int x2Ori = (i / interMultiplier) + 1;
                    x2Ori = Mathf.Clamp(x2Ori, 0, (chunkWidth / 8 + 2) - 1);
                    //   Debug.Log(x2Ori);
                    int z1Ori = (j / interMultiplier);
                    //   Debug.Log(y1Ori);
                    int z2Ori = (j / interMultiplier) + 1;
                    z2Ori = Mathf.Clamp(z2Ori, 0, (chunkWidth / 8 + 2) - 1);
                    //     Debug.Log(y2Ori);
                    int y1Ori = (k / interMultiplier);
                    //   Debug.Log(y1Ori);
                    int y2Ori = (k / interMultiplier) + 1;

                    y2Ori = Mathf.Clamp(y2Ori, 0, (chunkHeight / 8) - 1);
                    float q111 = densityMap[x1Ori,y1Ori, z1Ori];
                    float q112 = densityMap[x1Ori, y1Ori, z2Ori];
                    float q211 = densityMap[x2Ori, y1Ori, z1Ori];
                    float q212 = densityMap[x2Ori, y1Ori, z2Ori];
                    float fxz1d = (float)(x2 - x) / (x2 - x1) * q111 + (float)(x - x1) / (x2 - x1) * q211;
                    float fxz2d = (float)(x2 - x) / (x2 - x1) * q112 + (float)(x - x1) / (x2 - x1) * q212;
                    float fxzd = (float)(z2 - z) / (z2 - z1) * fxz1d + (float)(z - z1) / (z2 - z1) * fxz2d;


                    float q121 = densityMap[x1Ori, y2Ori, z1Ori];
                    float q122 = densityMap[x1Ori, y2Ori, z2Ori];
                    float q221 = densityMap[x2Ori, y2Ori, z1Ori];
                    float q222 = densityMap[x2Ori, y2Ori, z2Ori];
                    float fxz1u = (float)(x2 - x) / (x2 - x1) * q121 + (float)(x - x1) / (x2 - x1) * q221;
                    float fxz2u = (float)(x2 - x) / (x2 - x1) * q122 + (float)(x - x1) / (x2 - x1) * q222;
                    float fxzu = (float)(z2 - z) / (z2 - z1) * fxz1u + (float)(z - z1) / (z2 - z1) * fxz2u;

                    float fxyz= (float)(y2 - y) / (y2 - y1) * fxzd + (float)(y - y1) / (y2 - y1) * fxzu;
                    densityMapInterpolated[i,k,j]= fxyz;
                       
                }
                    
                    
                    //       Debug.Log(fxy);
                    //    Debug.Log(x1);
                    //  Debug.Log(x2);

                }
            }
            
            
        


        return densityMapInterpolated;
    }
    public bool isRightChunkUnloaded=false;
    public bool isFrontChunkUnloaded=false;
    public bool isLeftChunkUnloaded=false;
    public bool isBackChunkUnloaded=false;
     void  InitMap(Vector2Int pos,Mesh.MeshDataArray mda,Mesh.MeshDataArray mdaNS,Mesh.MeshDataArray mdaWT,Mesh.MeshDataArray mdaLOD1, Mesh.MeshDataArray mdaLOD2)
    {

        lock (taskLock)
        {
        
            //    Debug.Log("pos:"+pos+" "+"chunkPos:"+chunkPos);
      
        frontChunk=GetChunk(new Vector2Int(pos.x,pos.y+chunkWidth));
        frontLeftChunk=GetChunk(new Vector2Int(pos.x-chunkWidth,pos.y+chunkWidth));
        frontRightChunk=GetChunk(new Vector2Int(pos.x+chunkWidth,pos.y+chunkWidth));
        backLeftChunk=GetChunk(new Vector2Int(pos.x-chunkWidth,pos.y-chunkWidth));
        backRightChunk=GetChunk(new Vector2Int(pos.x+chunkWidth,pos.y-chunkWidth));
        backChunk=GetChunk(new Vector2Int(pos.x,pos.y-chunkWidth)); 
        leftChunk=GetChunk(new Vector2Int(pos.x-chunkWidth,pos.y));
        rightChunk=GetChunk(new Vector2Int(pos.x+chunkWidth,pos.y));


              if(frontChunk==null){isFrontChunkUnloaded=true;}
              if(backChunk==null){isBackChunkUnloaded=true;}
              if(leftChunk==null){isLeftChunkUnloaded=true;}
              if(rightChunk==null){isRightChunkUnloaded=true;}
            if (frontChunk != null&&frontChunk.isMapGenCompleted==false) { frontChunk = null; isFrontChunkUnloaded = true; };
            if (backChunk != null && backChunk.isMapGenCompleted == false) { backChunk = null; isBackChunkUnloaded = true; };
            if (leftChunk != null && leftChunk.isMapGenCompleted == false) { leftChunk = null; isLeftChunkUnloaded = true; };
            if (rightChunk != null && rightChunk.isMapGenCompleted == false) { rightChunk = null; isRightChunkUnloaded = true; };
            if (frontChunk != null && frontChunk.isMapGenCompleted == true) { isFrontChunkUnloaded = false; };
            if (backChunk != null && backChunk.isMapGenCompleted == true) {  isBackChunkUnloaded = false; };
            if (leftChunk != null && leftChunk.isMapGenCompleted == true) {  isLeftChunkUnloaded = false; };
            if (rightChunk != null && rightChunk.isMapGenCompleted == true) {  isRightChunkUnloaded = false; };
            //     leftHeightMap=GenerateChunkHeightmap(new Vector2Int(chunkPos.x-chunkWidth,chunkPos.y));
            //     rightHeightMap=GenerateChunkHeightmap(new Vector2Int(chunkPos.x+chunkWidth,chunkPos.y));
            //    frontHeightMap=GenerateChunkHeightmap(new Vector2Int(chunkPos.x,chunkPos.y+chunkWidth));
            //     backHeightMap=GenerateChunkHeightmap(new Vector2Int(chunkPos.x,chunkPos.y-chunkWidth));
            if (VoxelWorld.currentWorld.worldGenType == 0)
            {
            thisHeightMap =GenerateChunkHeightmap(new Vector2Int(pos.x,pos.y));
              
            }
            
           
        lightPoints=new List<Vector3>();
       // await Task.Run(()=>{while(frontChunk==null||backChunk==null||leftChunk==null||rightChunk==null){}});
        List<Vector3> opqVertsNL=new List<Vector3>();
        List<Vector3> opqNormsNL=new List<Vector3>();
        List<Vector2> opqUVsNL=new List<Vector2>();
        List<int> opqTrisNL=new List<int>();
        List<Vector3> NSVertsNL=new List<Vector3>();
        List<Vector3> NSNormsNL=new List<Vector3>();
        List<Vector2> NSUVsNL=new List<Vector2>();
        List<int> NSTrisNL=new List<int>();
        List<Vector3> WTVertsNL=new List<Vector3>();
        List<Vector3> WTNormsNL=new List<Vector3>();
        List<Vector2> WTUVsNL=new List<Vector2>();
        List<int> WTTrisNL=new List<int>();


            List<Vector3> opqVertsLOD = new List<Vector3>();
            List<Vector3> opqNormsLOD = new List<Vector3>();
            List<Vector2> opqUVsLOD = new List<Vector2>();
            List<int> opqTrisLOD = new List<int>();
            if (isMapGenCompleted==true){
          //  isModifiedInGame=true;
          //
      
            GenerateMeshOpqLOD(opqVertsLOD, opqUVsLOD, opqTrisLOD, opqNormsLOD, mdaLOD1, 2);

               opqVertsLOD = new List<Vector3>();
               opqNormsLOD = new List<Vector3>();
                 opqUVsLOD = new List<Vector2>();
               opqTrisLOD = new List<int>();
                GenerateMeshOpqLOD(opqVertsLOD, opqUVsLOD, opqTrisLOD, opqNormsLOD, mdaLOD2, 4);
               
                GenerateMesh(opqVertsNL,opqUVsNL,opqTrisNL,NSVertsNL,NSUVsNL,NSTrisNL,mda,mdaNS,opqNormsNL,NSNormsNL,mdaWT,WTVertsNL,WTUVsNL,WTTrisNL,WTNormsNL);
             
                return;
        }
         if(isChunkPosInited==false){
      /*       //   FreshGenMap(pos);
           //   Debug.Log("ReadF");
             map=(short[,,])chunkDataReadFromDisk[new Vector2Int(pos.x,pos.y)].map.Clone();
                 isMapGenCompleted=true;
                GenerateMesh(opqVertsNL,opqUVsNL,opqTrisNL,NSVertsNL,NSUVsNL,NSTrisNL,mda,mdaNS,opqNormsNL,NSNormsNL,mdaWT,WTVertsNL,WTUVsNL,WTTrisNL,WTNormsNL);
                return;*/
           //     WorldManager.chunkUnloadingQueue.Enqueue(this,0);
                return;
            }
        if(isSavedInDisk==true){
                 
                map =(short[,,])VoxelWorld.currentWorld.chunkDataReadFromDisk[chunkPos].map.Clone();
           
            
                 isMapGenCompleted=true;
                
                GenerateMeshOpqLOD(opqVertsLOD, opqUVsLOD, opqTrisLOD, opqNormsLOD, mdaLOD1, 2);
                opqVertsLOD = new List<Vector3>();
                opqNormsLOD = new List<Vector3>();
                opqUVsLOD = new List<Vector2>();
                opqTrisLOD = new List<int>();
                GenerateMeshOpqLOD(opqVertsLOD, opqUVsLOD, opqTrisLOD, opqNormsLOD, mdaLOD2, 4);
                
                GenerateMesh(opqVertsNL,opqUVsNL,opqTrisNL,NSVertsNL,NSUVsNL,NSTrisNL,mda,mdaNS,opqNormsNL,NSNormsNL,mdaWT,WTVertsNL,WTUVsNL,WTTrisNL,WTNormsNL);
              
            return;
            
            
         
        }
        FreshGenMap(pos);
            isMapGenCompleted = true;
           
            GenerateMeshOpqLOD(opqVertsLOD, opqUVsLOD, opqTrisLOD, opqNormsLOD, mdaLOD1, 2);
            opqVertsLOD = new List<Vector3>();
            opqNormsLOD = new List<Vector3>();
            opqUVsLOD = new List<Vector2>();
            opqTrisLOD = new List<int>();
            GenerateMeshOpqLOD(opqVertsLOD, opqUVsLOD, opqTrisLOD, opqNormsLOD, mdaLOD2, 4);
          
            GenerateMesh(opqVertsNL, opqUVsNL, opqTrisNL, NSVertsNL, NSUVsNL, NSTrisNL, mda, mdaNS, opqNormsNL, NSNormsNL, mdaWT, WTVertsNL, WTUVsNL, WTTrisNL, WTNormsNL);
            
        }
        
     
    
        
        void FreshGenMap(Vector2Int pos){
            //    Debug.Log("genMappos: "+pos);
            
            if (VoxelWorld.currentWorld.worldGenType==0){
 
    //    System.Random random=new System.Random(pos.x+pos.y);
 

      //      float[,] heightMapInterpolated=GenerateChunkHeightmap(chunkPos);//偏移4



              int[,] chunkBiomeMap=GenerateChunkBiomeMap(pos);

            int interMultiplier=8;
            int[,] chunkBiomeMapInterpolated=new int[(chunkWidth/8+2)*interMultiplier,(chunkWidth/8+2)*interMultiplier];
        for(int i=0;i<(chunkWidth/8+2)*interMultiplier;++i){
                for(int j=0;j<(chunkWidth/8+2)*interMultiplier;++j){
                        int x=i;
                        int y=j;
                        float x1=(i/interMultiplier)*interMultiplier;
                        float x2=(i/interMultiplier)*interMultiplier+interMultiplier;
                        float y1=(j/interMultiplier)*interMultiplier;
                        float y2=(j/interMultiplier)*interMultiplier+interMultiplier;
                        int x1Ori=(i/interMultiplier);
                       // Debug.Log(x1Ori);
                        int x2Ori=(i/interMultiplier)+1;
                        x2Ori=Mathf.Clamp(x2Ori,0,(chunkWidth/8+2)-1);
                      //   Debug.Log(x2Ori);
                        int y1Ori=(j/interMultiplier);
                      //   Debug.Log(y1Ori);
                        int y2Ori=(j/interMultiplier)+1;
                         y2Ori=Mathf.Clamp(y2Ori,0,(chunkWidth/8+2)-1);
                    //     Debug.Log(y2Ori);
                    
                        float q11=chunkBiomeMap[x1Ori,y1Ori];
                        float q12=chunkBiomeMap[x1Ori,y2Ori];
                        float q21=chunkBiomeMap[x2Ori,y1Ori];
                        float q22=chunkBiomeMap[x2Ori,y2Ori];
                        float fxy1=(float)(x2-x)/(x2-x1)*q11+(float)(x-x1)/(x2-x1)*q21;
                        float fxy2=(float)(x2-x)/(x2-x1)*q12+(float)(x-x1)/(x2-x1)*q22;
                        float fxyf=((y2-y)/(y2-y1)*fxy1+(y-y1)/(y2-y1)*fxy2);
                        if(fxyf-(int)fxyf>=0.5f){
                        chunkBiomeMapInterpolated[x,y]=(int)fxyf+1; 
                        }else{
                                chunkBiomeMapInterpolated[x,y]=(int)fxyf; 
                        }
                      
                 //       Debug.Log(fxy);
                    //    Debug.Log(x1);
                      //  Debug.Log(x2);

                }
            }
        //    int[,] biomeMap=GenerateChunkBiomeMap(chunkPos);
        for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
       //    float iscale=(i%4)/4f;
         //  float jscale=(j%4)/4f;
       //    Debug.Log(iscale);
              //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.y*0.01f+j*0.01f);
          //     float noiseValueX=(heightMap[i/4+1,j/4+1]*iscale+heightMap[i/4+2,j/4+1]*(1-iscale));
           //   float noiseValueY=(heightMap[i/4+1,j/4+1]*jscale+heightMap[i/4+1,j/4+2]*(1-jscale));
               float noiseValue=(thisHeightMap[i+8,j+8]);
                for(int k=0;k<chunkHeight;k++){
                    if(noiseValue>k+3){
                     map[i,k,j]=1;   
                    }else if(noiseValue>k){
                        if(chunkBiomeMapInterpolated[i,j]==3){
                            map[i,k,j]=1;
                        }else if(chunkBiomeMapInterpolated[i,j]==0){
                        map[i,k,j]=11;    
                            }else{
                        map[i,k,j]=3;    
                        }
                        
                    }
                    
                    
                }
            }
        }


        for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
       
                for(int k=chunkHeight-1;k>=0;k--){
                    
                    if(map[i,k,j]!=0&&map[i,k,j]!=9&&k>=chunkSeaLevel&&(chunkBiomeMapInterpolated[i,j]==2||chunkBiomeMapInterpolated[i,j]==1)){
                            map[i,k,j]=4;
                            break;
                    }
                 
                       
                }
                for(int k=chunkHeight-1;k>=0;k--){
                    
                   
                 
                    if(k>chunkSeaLevel&&map[i,k,j]==0&&map[i,k-1,j]==4&&map[i,k-1,j]!=100&&RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(pos.x,0,pos.y)+new Vector3Int(i,k,j))>70&&(chunkBiomeMapInterpolated[i,j]==2||chunkBiomeMapInterpolated[i,j]==1)){
                        map[i,k,j]=101;
                    }
                        if(k<chunkSeaLevel&&map[i,k,j]==0){
                                map[i,k,j]=100;
                        }
                       
                }
            }
        }

                List<Vector3Int> treePoints = new List<Vector3Int>();
                List<Vector3Int> treeLeafPoints = new List<Vector3Int>();
                for (int x = 0; x < 32; x++)
                {
                    for (int z = 0; z < 32; z++)
                    {
                        Vector3Int point = new Vector3Int(x, (int)thisHeightMap[x, z] + 1, z);
                        if (point.y < chunkSeaLevel)
                        {
                            continue;
                        }
                        Vector3Int pointTransformed2 = point - new Vector3Int(8, 0, 8);
                        if (chunkBiomeMapInterpolated[x, z] == 3 || chunkBiomeMapInterpolated[x, z]==0)
                        {
                            continue;
                        }
                        
                        if (pointTransformed2.x >= 0 && pointTransformed2.x < chunkWidth && pointTransformed2.y >= 0 && pointTransformed2.y < chunkHeight && pointTransformed2.z >= 0 && pointTransformed2.z < chunkWidth)
                        {
                            if (map[pointTransformed2.x, pointTransformed2.y - 1, pointTransformed2.z] == 1)
                            {
                                continue;
                            }
                        }
                        if (RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(point.x, point.y, point.z) + new Vector3Int(chunkPos.x, 0, chunkPos.y)) > 98.5f)
                        {
                            treePoints.Add(point);
                            treePoints.Add(point + new Vector3Int(0, 1, 0));
                            treePoints.Add(point + new Vector3Int(0, 2, 0));
                            treePoints.Add(point + new Vector3Int(0, 3, 0));
                            treePoints.Add(point + new Vector3Int(0, 4, 0));
                            treePoints.Add(point + new Vector3Int(0, 5, 0));
                            for (int i = -2; i < 3; i++)
                            {
                                for (int j = -2; j < 3; j++)
                                {
                                    for (int k = 3; k < 5; k++)
                                    {
                                        Vector3Int pointLeaf = point + new Vector3Int(i, k, j);
                                        treeLeafPoints.Add(pointLeaf);
                                    }
                                }
                            }
                            treeLeafPoints.Add(point + new Vector3Int(0, 5, 0));
                            treeLeafPoints.Add(point + new Vector3Int(0, 6, 0));
                            treeLeafPoints.Add(point + new Vector3Int(1, 5, 0));
                            treeLeafPoints.Add(point + new Vector3Int(1, 6, 0));
                            treeLeafPoints.Add(point + new Vector3Int(0, 5, 1));
                            treeLeafPoints.Add(point + new Vector3Int(0, 6, 1));
                            treeLeafPoints.Add(point + new Vector3Int(-1, 5, 0));
                            treeLeafPoints.Add(point + new Vector3Int(-1, 6, 0));
                            treeLeafPoints.Add(point + new Vector3Int(0, 5, -1));
                            treeLeafPoints.Add(point + new Vector3Int(0, 6, -1));
                        }
                    }
                }
                //  Debug.WriteLine(treePoints[0].x +" "+ treePoints[0].y+" " + treePoints[0].z);
                foreach (var point1 in treeLeafPoints)
                {
                    Vector3Int pointTransformed1 = point1 - new Vector3Int(8, 0, 8);
                    //   Debug.WriteLine(pointTransformed.x + " "+pointTransformed.y + " "+pointTransformed.z);
                    if (pointTransformed1.x >= 0 && pointTransformed1.x < chunkWidth && pointTransformed1.y >= 0 && pointTransformed1.y < chunkHeight && pointTransformed1.z >= 0 && pointTransformed1.z < chunkWidth)
                    {
                        if (map[pointTransformed1.x, pointTransformed1.y, pointTransformed1.z] == 0)
                        {
                            map[pointTransformed1.x, pointTransformed1.y, pointTransformed1.z] = 9;
                        }

                    }
                }
                foreach (var point in treePoints)
                {
                    Vector3Int pointTransformed = point - new Vector3Int(8, 0, 8);
                    //   Debug.WriteLine(pointTransformed.x + " "+pointTransformed.y + " "+pointTransformed.z);
                    if (pointTransformed.x >= 0 && pointTransformed.x < chunkWidth && pointTransformed.y >= 0 && pointTransformed.y < chunkHeight && pointTransformed.z >= 0 && pointTransformed.z < chunkWidth)
                    {
                        map[pointTransformed.x, pointTransformed.y, pointTransformed.z] = 7;
                    }
                }
                /*  bool leftChunkLoaded=(isLeftChunkUnloaded==false&&leftChunk!=null);
                  bool rightChunkLoaded=(isRightChunkUnloaded==false&&leftChunk!=null);
                  bool backChunkLoaded=(isFrontChunkUnloaded==false&&leftChunk!=null);
                  bool frontChunkLoaded=(isBackChunkUnloaded==false&&leftChunk!=null);
                  bool leftChunkUnLoaded=(isLeftChunkUnloaded==true&&leftChunk!=null);
                  bool rightChunkUnLoaded=(isRightChunkUnloaded==true&&rightChunk!=null);
                  bool backChunkUnLoaded=(isBackChunkUnloaded==true&&backChunk!=null);
                  bool frontChunkUnLoaded=(isFrontChunkUnloaded==true&&frontChunk!=null);*/
        /*        bool leftChunkNull=(leftChunk==null);
            bool rightChunkNull=(rightChunk==null);
            bool backChunkNull=(backChunk==null);
            bool frontChunkNull=(frontChunk==null);
                for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
       
                for(int k=chunkHeight-1;k>=0;k--){
              
                    if(k>chunkSeaLevel&&map[i,k,j]==0&&map[i,k-1,j]==4&&map[i,k-1,j]!=100){
                    if(treeCount>0){
                            if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(i,k,j))>98){
                           
                            
                               for(int x=-2;x<3;x++){
                                    for(int y=3;y<5;y++){
                                        for(int z=-2;z<3;z++){
                                            if(x+i<0||x+i>=chunkWidth||z+j<0||z+j>=chunkWidth){



                                            if(x+i<0){
                                                if(z+j>=0&&z+j<chunkWidth){
                                                   if(leftChunkNull==false){
                                                    if(isLeftChunkUnloaded==true){leftChunk.additiveMap[chunkWidth+(x+i),y+k,z+j]=9;}else{leftChunk.map[chunkWidth+(x+i),y+k,z+j]=9;}
                                                  
                                                
                                                        isLeftChunkUpdated=true;
                                                    
                                                //    WorldManager.chunkLoadingQueue.UpdatePriority(leftChunk,0);
                                           //         leftChunk.isChunkMapUpdated=true;
                                                } 
                                                }else if(z+j<0){
                                                    if(backLeftChunk!=null){
                                                        backLeftChunk.additiveMap[chunkWidth+(x+i),y+k,chunkWidth-1+(z+j)]=9;
                                                     
                                                        isBackLeftChunkUpdated=true;
                                                    
                                                  //    WorldManager.chunkLoadingQueue.UpdatePriority(backLeftChunk,0);
                                         //               backLeftChunk.isChunkMapUpdated=true;
                                                    }
                                                    
                                                }else if(z+j>=chunkWidth){
                                                    if(frontLeftChunk!=null){
                                                        frontLeftChunk.additiveMap[chunkWidth+(x+i),y+k,(z+j)-chunkWidth]=9;
                                                       
                                                        isFrontLeftChunkUpdated=true;
                                                   
                                                   //     WorldManager.chunkLoadingQueue.UpdatePriority(frontLeftChunk,0);
                                       //                 frontLeftChunk.isChunkMapUpdated=true;
                                                    } 
                                                }
                                                
                                            }else
                                            if(x+i>=chunkWidth){
                                                 if(z+j>=0&&z+j<chunkWidth){
                                                   if(rightChunkNull==false){
                                                    if(isRightChunkUnloaded==true){  rightChunk.additiveMap[(x+i)-chunkWidth,y+k,z+j]=9;}else{  rightChunk.map[(x+i)-chunkWidth,y+k,z+j]=9;}
                                                   // rightChunk.additiveMap[(x+i)-chunkWidth,y+k,z+j]=9;
                                                  
                                                        isRightChunkUpdated=true;
                                                    
                                                 //   WorldManager.chunkLoadingQueue.UpdatePriority(rightChunk,0);
                                              //      rightChunk.isChunkMapUpdated=true;
                                                } 
                                                }else if(z+j<0){
                                                    if(backRightChunk!=null){
                                                        backRightChunk.additiveMap[(x+i)-chunkWidth,y+k,chunkWidth+(z+j)]=9;
                                                    
                                                        isBackRightChunkUpdated=true;
                                                    
                                                  //    WorldManager.chunkLoadingQueue.UpdatePriority(backRightChunk,0);
                                               //         backRightChunk.isChunkMapUpdated=true;
                                                    }
                                                    
                                                }else if(z+j>=chunkWidth){
                                                    if(frontRightChunk!=null){
                                                        frontRightChunk.additiveMap[(x+i)-chunkWidth,y+k,(z+j)-chunkWidth]=9;
                                                     
                                                        isFrontRightChunkUpdated=true;
                                                    
                                                 //     WorldManager.chunkLoadingQueue.UpdatePriority(frontRightChunk,0);
                                              //          frontRightChunk.isChunkMapUpdated=true;
                                                    } 
                                                }
                                            }else
                                            if(z+j<0){

                                                 if(x+i>=0&&x+i<chunkWidth){
                                                   if(backChunkNull==false){
                                                    if(isBackChunkUnloaded==true){ backChunk.additiveMap[x+i,y+k,chunkWidth+(z+j)]=9;}else{
                                                         backChunk.map[x+i,y+k,chunkWidth+(z+j)]=9;
                                                    }
                                                  //  backChunk.additiveMap[x+i,y+k,chunkWidth+(z+j)]=9;
                                                   
                                                        isBackChunkUpdated=true;
                                                    
                                            //    WorldManager.chunkLoadingQueue.UpdatePriority(backChunk,0);
                                           //         backChunk.isChunkMapUpdated=true;
                                                } 
                                                }else if(x+i<0){
                                                    if(backLeftChunk!=null){
                                                        backLeftChunk.additiveMap[chunkWidth+(x+i),y+k,chunkWidth-1+(z+j)]=9;
                                                      
                                                        isBackLeftChunkUpdated=true;
                                                    
                                                 //    WorldManager.chunkLoadingQueue.UpdatePriority(backLeftChunk,0);
                                            //            backLeftChunk.isChunkMapUpdated=true;
                                                    }
                                                    
                                                }else if(x+i>=chunkWidth){
                                                    if(backRightChunk!=null){
                                                        backRightChunk.additiveMap[(x+i)-chunkWidth,y+k,chunkWidth-1+(z+j)]=9;
                                                     
                                                        isBackRightChunkUpdated=true;
                                                    
                                               //       WorldManager.chunkLoadingQueue.UpdatePriority(backRightChunk,0);
                                                  //      backRightChunk.isChunkMapUpdated=true;    
                                                    } 
                                                }

                                            }else
                                            if(z+j>=chunkWidth){

                                                if(x+i>=0&&x+i<chunkWidth){
                                                   if(frontChunkNull==false){
                                                          if(isFrontChunkUnloaded==true){ frontChunk.additiveMap[x+i,y+k,(z+j)-chunkWidth]=9;}else{
                                                             frontChunk.map[x+i,y+k,(z+j)-chunkWidth]=9;
                                                        }
                                              //      frontChunk.additiveMap[x+i,y+k,(z+j)-chunkWidth]=9;
                                               
                                                        isFrontChunkUpdated=true;
                                                    
                                              //    WorldManager.chunkLoadingQueue.UpdatePriority(frontChunk,0);
                                                 //   frontChunk.isChunkMapUpdated=true;
                                                } 
                                                }else if(x+i<0){
                                                    if(frontLeftChunk!=null){
                                                      
                                                        frontLeftChunk.additiveMap[chunkWidth+(x+i),y+k,(z+j)-chunkWidth]=9;
                                           
                                                    isBackLeftChunkUpdated=true;
                                                    
                                              //        WorldManager.chunkLoadingQueue.UpdatePriority(frontLeftChunk,0);
                                                    //    frontLeftChunk.isChunkMapUpdated=true;
                                                    }
                                                    
                                                }else if(x+i>=chunkWidth){
                                                    if(frontRightChunk!=null){
                                                        frontRightChunk.additiveMap[(x+i)-chunkWidth,y+k,(z+j)-chunkWidth]=9;
                                                     
                                                        isFrontRightChunkUpdated=true;
                                                    
                                                  //  WorldManager.chunkLoadingQueue.UpdatePriority(frontRightChunk,0);
                                                  //      frontRightChunk.isChunkMapUpdated=true;
                                                    } 
                                                }
                                            }


                                            }else{
                                                map[x+i,y+k,z+j]=9;
                                            }
                                        }
                                    }
                                }
                                  map[i,k,j]=7;
                                map[i,k+1,j]=7;
                               map[i,k+2,j]=7;
                                map[i,k+3,j]=7;
                                 map[i,k+4,j]=7;
                                 map[i,k+5,j]=9;
                                 map[i,k+6,j]=9;

                                if(i+1<chunkWidth){
                                map[i+1,k+5,j]=9;
                                 map[i+1,k+6,j]=9;
                           
                               }else{
                                if(rightChunkNull==false){
                                    if(isRightChunkUnloaded==true){rightChunk.additiveMap[0,k+5,j]=9;
                                 rightChunk.additiveMap[0,k+6,j]=9;}else{rightChunk.map[0,k+5,j]=9;rightChunk.map[0,k+6,j]=9;}
                                    
                               
                            //      rightChunk.isChunkMapUpdated=true;
                                }
                               }

                               if(i-1>=0){
                                map[i-1,k+5,j]=9;
                                map[i-1,k+6,j]=9;
                           
                               }else{
                                if(leftChunkNull==false){
                                    if(isLeftChunkUnloaded==true){leftChunk.additiveMap[chunkWidth-1,k+5,j]=9;
                                 leftChunk.additiveMap[chunkWidth-1,k+6,j]=9;}else{leftChunk.map[chunkWidth-1,k+5,j]=9;
                                 leftChunk.map[chunkWidth-1,k+6,j]=9;}
                                      
                            
                                // leftChunk.isChunkMapUpdated=true;
                                }
                               }
                               if(j+1<chunkWidth){
                                map[i,k+5,j+1]=9;
                                map[i,k+6,j+1]=9;
                           
                               }else{
                                if(frontChunkNull==false){
                                    if(isFrontChunkUnloaded==true){
                                        frontChunk.additiveMap[i,k+5,0]=9;
                                frontChunk.additiveMap[i,k+6,0]=9;
                                    }else{
                                        frontChunk.map[i,k+5,0]=9;
                                frontChunk.map[i,k+6,0]=9;
                                    }
                             //   frontChunk.additiveMap[i,k+5,0]=9;
                             //   frontChunk.additiveMap[i,k+6,0]=9;
                         
                             //   frontChunk.isChunkMapUpdated=true;
                                }
                               }

                               if(j-1>=0){
                                map[i,k+5,j-1]=9;
                                map[i,k+6,j-1]=9;
                      
                               }else{
                                if(backChunkNull==false){
                                    if(isBackChunkUnloaded==true){
                                         backChunk.additiveMap[i,k+5,chunkWidth-1]=9;
                                backChunk.additiveMap[i,k+6,chunkWidth-1]=9;
                                    }else{
                                         backChunk.map[i,k+5,chunkWidth-1]=9;
                                backChunk.map[i,k+6,chunkWidth-1]=9;
                                    }
                               
                             
                              //  backChunk.isChunkMapUpdated=true;
                                }
                               }


                         /*       
                                map[i,k,j]=7;
                                map[i,k+1,j]=7;
                               map[i,k+2,j]=7;
                                map[i,k+3,j]=7;
                                 map[i,k+4,j]=7;
                               map[i,k+5,j]=9;
                               
                               if(i+1<chunkWidth){
                                map[i+1,k+4,j]=9;
                                 map[i+1,k+3,j]=9;
                                  map[i+1,k+2,j]=9;
                               }else{
                                if(rightChunk!=null){
                                    rightChunk.additiveMap[0,k+4,j]=9;
                                 rightChunk.additiveMap[0,k+3,j]=9;
                                  rightChunk.additiveMap[0,k+2,j]=9;
                                   isRightChunkUpdated=true;
                            //      rightChunk.isChunkMapUpdated=true;
                                }
                               }
                               if(i-1>=0){
                                map[i-1,k+4,j]=9;
                                map[i-1,k+3,j]=9;
                                map[i-1,k+2,j]=9;
                               }else{
                                if(leftChunk!=null){
                                      leftChunk.additiveMap[chunkWidth-1,k+4,j]=9;
                                 leftChunk.additiveMap[chunkWidth-1,k+3,j]=9;
                                  leftChunk.additiveMap[chunkWidth-1,k+2,j]=9;
                                  isLeftChunkUpdated=true;
                           //       leftChunk.isChunkMapUpdated=true;
                                }
                               }
                               if(j+1<chunkWidth){
                                map[i,k+4,j+1]=9;
                                map[i,k+3,j+1]=9;
                                map[i,k+2,j+1]=9;
                               }else{
                                if(frontChunk!=null){
                                frontChunk.additiveMap[i,k+4,0]=9;
                                frontChunk.additiveMap[i,k+3,0]=9;
                                frontChunk.additiveMap[i,k+2,0]=9;
                                isFrontChunkUpdated=true;
                           //     frontChunk.isChunkMapUpdated=true;
                                }
                               }

                               

                               if(j-1>=0){
                                map[i,k+4,j-1]=9;
                                map[i,k+3,j-1]=9;
                                map[i,k+2,j-1]=9;
                               }else{
                                if(backChunk!=null){
                                backChunk.additiveMap[i,k+4,chunkWidth-1]=9;
                                backChunk.additiveMap[i,k+3,chunkWidth-1]=9;
                                backChunk.additiveMap[i,k+2,chunkWidth-1]=9;
                                isBackChunkUpdated=true;
                         //       backChunk.isChunkMapUpdated=true;
                                }
                               }*/
                               

                      //         treeCount--;
               /*             }
                        }
                    }
                    
                }
            }
        }*/
           for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
                 for(int k=0;k<chunkHeight/4;k++){
                    
                        if(0<k&&k<12){
                        if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(pos.x,0,pos.y)+new Vector3Int(i,k,j))>93){

                             map[i,k,j]=10;
                        
                        }
                        
                    }
               
            }
              
            }
        }
                for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
                map[i,0,j]=5;
            }
        }
                      /*          if(isLeftChunkUpdated==true){
                                   WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(leftChunk,false),0);
                               }
                               if(isRightChunkUpdated==true){
                                 WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(rightChunk,false),0);
                               }
                               if(isBackChunkUpdated==true){
                                 WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(backChunk,false),0);
                               }
                               if(isFrontChunkUpdated==true){
                                 WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(frontChunk,false),0);
                               }
                               if(isFrontLeftChunkUpdated==true){
                                 WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(frontLeftChunk,false),0);
                               }
                               if(isFrontRightChunkUpdated==true){
                                 WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(frontRightChunk,false),0);
                               }
                               if(isBackLeftChunkUpdated==true){
                                 WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(backLeftChunk,false),0);
                               }
                               if(isBackRightChunkUpdated==true){
                                 WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(backRightChunk,false),0);
                               }*/
        }else if(VoxelWorld.currentWorld.worldGenType== 1){
            for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
              //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.z*0.01f+j*0.01f);
                for(int k=0;k<chunkHeight/4;k++){
                  
                    map[i,k,j]=1;
                    
                }
            }
        }
        }else if(VoxelWorld.currentWorld.worldGenType == 2)
            {
             
                for (int i = 0; i < chunkWidth; i++)
                {
                    for (int j = 0; j < chunkWidth; j++)
                    {
                        //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.z*0.01f+j*0.01f);
                        for (int k = 0; k < chunkHeight / 2; k++)
                        {
                            float yLerpValue = Mathf.Lerp(-1, 1, (Mathf.Abs(k - chunkSeaLevel)) / 40f);
                            float xzLerpValue = Mathf.Lerp(-1, 1, (new Vector3(chunkPos.x + i, 0, chunkPos.y + j).magnitude /384f));
                            float xyzLerpValue =Mathf.Max( xzLerpValue,yLerpValue);
                            if (frequentNoiseGenerator.GetSimplex(i+chunkPos.x,k,j+chunkPos.y) > xyzLerpValue)
                            {
                            map[i, k, j] = 12;
                            }
                         

                        }
                    }
                }
               
             //   Debug.Log("original noise gen time:"+stopwatch.Elapsed.TotalMilliseconds);
            }
      
        isMapGenCompleted=true;    
                             
        
        }
        

       
       
        //Debug.Log(initMapThreads.Count);
    }
    public static int MaxIndex(int[] arr)
    {
        var i_Pos = 0;
        var value = arr[0];
        for (var i = 1; i < arr.Length; ++i)
        {
            var _value = arr[i];
            if (_value > value)
            {
                value = _value;
                i_Pos = i;
            }
        }
        return i_Pos;
    }
    public static Dictionary<int, int> blockIDWeightDic = new Dictionary<int, int>
    {
        {0,0 },
        {1,1},
        {4,2 },
        {9,2 },
        {11,2 }
    };
    public void GenerateMeshOpqLOD(List<Vector3> verts, List<Vector2> uvs, List<int> tris, List<Vector3> norms, Mesh.MeshDataArray mda, int lodBlockSkipCount = 2)
    {

     //   System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
      //  sw.Start();
      
        int[] typeIDs = new int[lodBlockSkipCount * 1 * lodBlockSkipCount];
        int[] typeIDWeights = new int[lodBlockSkipCount * 1 * lodBlockSkipCount];


        for (int x = 0; x < chunkWidth; x += lodBlockSkipCount)
        {
            for (int y = 0; y < chunkHeight; y += 1)
            {
                for (int z = 0; z < chunkWidth; z += lodBlockSkipCount)
                {//new int[chunkwidth,chunkheiight,chunkwidth]
                 //     BuildBlock(x, y, z, verts, uvs, tris, vertsNS, uvsNS, trisNS);


                    int typeid = this.map[x, y, z];
                    Array.Clear(typeIDs, 0, lodBlockSkipCount * 1 * lodBlockSkipCount);
                    Array.Clear(typeIDWeights, 0, lodBlockSkipCount * 1 * lodBlockSkipCount);
                    Vector3Int blockCheckPos = new Vector3Int(x, y, z);
               /*     if (lodBlockSkipCount > 1)
                    {
                        int indx = 0;
                        for (int x1 = 0; x1 < lodBlockSkipCount; x1++)
                        {
                           
                                for (int z1 = 0; z1 < lodBlockSkipCount; z1++)
                                {


                                    typeIDs[indx] = (this.map[x + x1, y , z + z1]);

                                     if (blockIDWeightDic.ContainsKey((this.map[x + x1, y , z + z1])))
                                    {
                                        typeIDWeights[indx] = (blockIDWeightDic[(this.map[x + x1, y, z + z1])]);
                                    }
                                    else
                                    {
                                        typeIDWeights[indx] = 1;
                                    } ;
                                    indx++;
                                }
                            
                        }

                        int maxIndex = MaxIndex(typeIDWeights);
                        typeid = typeIDs[maxIndex];
                     
                    }*/

                    if (typeid == 0) continue;
                    if (0 < typeid && typeid < 100)
                    {
                    
                            //Left
                            if (CheckNeedBuildFace((blockCheckPos.x - lodBlockSkipCount), blockCheckPos.y, blockCheckPos.z, lodBlockSkipCount))
                                BuildFace(typeid, new Vector3(x, y, z), Vector3.up * 1, Vector3.forward * lodBlockSkipCount, false, verts, uvs, tris, 0, norms);
                            //Right
                            if (CheckNeedBuildFace((blockCheckPos.x + lodBlockSkipCount), blockCheckPos.y, blockCheckPos.z, lodBlockSkipCount))
                                BuildFace(typeid, new Vector3(x + lodBlockSkipCount, y, z), Vector3.up * 1, Vector3.forward * lodBlockSkipCount, true, verts, uvs, tris, 1, norms);

                            //Bottom
                            if (CheckNeedBuildFace(blockCheckPos.x, blockCheckPos.y - 1, blockCheckPos.z, lodBlockSkipCount))
                                BuildFace(typeid, new Vector3(x, y, z), Vector3.forward * lodBlockSkipCount, Vector3.right * lodBlockSkipCount, false, verts, uvs, tris, 2, norms);
                            //Top
                            if (CheckNeedBuildFace(blockCheckPos.x, blockCheckPos.y + 1, blockCheckPos.z, lodBlockSkipCount))
                                BuildFace(typeid, new Vector3(x, y + 1, z), Vector3.forward * lodBlockSkipCount, Vector3.right * lodBlockSkipCount, true, verts, uvs, tris, 3, norms);

                            //Back
                            if (CheckNeedBuildFace(blockCheckPos.x, blockCheckPos.y, (blockCheckPos.z - lodBlockSkipCount) , lodBlockSkipCount))
                                BuildFace(typeid, new Vector3(x, y, z), Vector3.up * 1, Vector3.right * lodBlockSkipCount, true, verts, uvs, tris, 4, norms);
                            //Front
                            if (CheckNeedBuildFace(blockCheckPos.x, blockCheckPos.y,( blockCheckPos.z + lodBlockSkipCount), lodBlockSkipCount))
                                BuildFace(typeid, new Vector3(x, y, z + lodBlockSkipCount), Vector3.up * 1, Vector3.right * lodBlockSkipCount, false, verts, uvs, tris, 5, norms);

                        



                    }
                }
            }
        }


        NativeArray<VertexAttributeDescriptor> vertexAttributesDes = new NativeArray<VertexAttributeDescriptor>(new VertexAttributeDescriptor[]{new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)}, Allocator.Persistent);
        //    mda[0].SetVertexBufferParams(opqVertsNA.Length+1,vertexAttributesDes);
        //    mdaNS[0].SetVertexBufferParams(NSVertsNA.Length+1,vertexAttributesDes);
        //   mda[1].SetVertexBufferParams(NSVertsNA.Length+1,vertexAttributesDes);   
        var data = mda[0];
        data.SetVertexBufferParams(verts.Count, vertexAttributesDes);
        NativeArray<Vertex> pos = data.GetVertexData<Vertex>();

        for (int i = 0; i < verts.Count; i++)
        {
            /*    if(pos==null){
                    Debug.Log("null");
                    return;
                }*/
            pos[i] = new Vertex(verts[i], norms[i], uvs[i]);

        }
        data.SetIndexBufferParams((int)(pos.Length), IndexFormat.UInt32);
        var ib = data.GetIndexData<int>();
        for (int i = 0; i < ib.Length; ++i)
            ib[i] = tris[i];

        data.subMeshCount = 1;
        data.SetSubMesh(0, new SubMeshDescriptor(0, ib.Length, MeshTopology.Quads));

        pos.Dispose();
        ib.Dispose();
        vertexAttributesDes.Dispose();
     //   sw.Stop();
    //    Debug.Log(sw.ElapsedMilliseconds);
    }
    public void GenerateMesh(List<Vector3> verts, List<Vector2> uvs,List<int> tris,List<Vector3> vertsNS, List<Vector2> uvsNS,List<int> trisNS,Mesh.MeshDataArray mda,Mesh.MeshDataArray mdaNS,List<Vector3> norms,List<Vector3> normsNS,Mesh.MeshDataArray mdaWT,List<Vector3> vertsWT,List<Vector2> uvsWT,List<int> trisWT,List<Vector3> normsWT){
     //   Thread.Sleep(10);
    //     TmpCheckFace tmp=new TmpCheckFace(CheckNeedBuildFace);
    //    TmpBuildFace TmpBuildFace=new TmpBuildFace(BuildFace);
    
       /*
       fixed(int* p=&map[0,0,0]){
       int* p2=p;
        for (int x = 0; x < chunkWidth; x++){
            for (int y = 0; y < chunkHeight; y++){
                for (int z = 0; z < chunkWidth; z++){
                    ......
                    p2++;//三维数组指针无法正常运行
                    }
                }
            }
       }
        //循环无法停止
       */
      
     
        for (int x = 0; x < chunkWidth; x+= 1)
        {
            for (int y = 0; y < chunkHeight; y += 1)
            {
                for (int z = 0; z < chunkWidth; z += 1)
                {//new int[chunkwidth,chunkheiight,chunkwidth]
                   //     BuildBlock(x, y, z, verts, uvs, tris, vertsNS, uvsNS, trisNS);

        
                int typeid = this.map[x, y, z];
              
                   
                    if (typeid == 0) continue;
        if(0<typeid&&typeid<100){
            if(typeid==9){
            //Left
        if (CheckNeedBuildFace(x - 1, y, z)&&GetChunkBlockType(x- 1, y,z)!=9)
          BuildFace(typeid, new Vector3(x, y, z), Vector3.up* 1, Vector3.forward * 1, false, verts, uvs, tris,0,norms);
        //Right
        if (CheckNeedBuildFace(x + 1, y, z)&&GetChunkBlockType(x+ 1, y,z)!=9)
         BuildFace(typeid, new Vector3(x + 1, y, z), Vector3.up * 1, Vector3.forward * 1, true, verts, uvs, tris,1,norms);

        //Bottom
        if (CheckNeedBuildFace(x, y - 1, z)&&GetChunkBlockType(x,y- 1, z)!=9)
         BuildFace(typeid, new Vector3(x, y, z), Vector3.forward * 1, Vector3.right * 1, false, verts, uvs, tris,2,norms);
        //Top
        if (CheckNeedBuildFace(x, y + 1, z)&&GetChunkBlockType(x,y+ 1, z)!=9)
        BuildFace(typeid, new Vector3(x, y + 1, z), Vector3.forward * 1, Vector3.right * 1, true, verts, uvs, tris,3,norms);

        //Back
        if (CheckNeedBuildFace(x, y, z - 1) &&GetChunkBlockType(x,y,z- 1) !=9)
        BuildFace(typeid, new Vector3(x, y, z), Vector3.up * 1, Vector3.right * 1, true, verts, uvs, tris,4,norms);
        //Front
        if (CheckNeedBuildFace(x, y, z + 1) &&GetChunkBlockType(x,y,z+ 1) !=9)
        BuildFace(typeid, new Vector3(x, y, z + 1), Vector3.up * 1, Vector3.right * 1, false, verts, uvs, tris,5,norms); 
    
            }else{
             //Left
        if (CheckNeedBuildFace(x - 1, y, z))
          BuildFace(typeid, new Vector3(x, y, z), Vector3.up * 1, Vector3.forward * 1, false, verts, uvs, tris,0,norms);
        //Right
        if (CheckNeedBuildFace(x + 1, y, z))
         BuildFace(typeid, new Vector3(x + 1, y, z), Vector3.up * 1, Vector3.forward * 1, true, verts, uvs, tris,1,norms);

        //Bottom
        if (CheckNeedBuildFace(x, y - 1, z))
         BuildFace(typeid, new Vector3(x, y, z), Vector3.forward * 1, Vector3.right * 1, false, verts, uvs, tris,2,norms);
        //Top
        if (CheckNeedBuildFace(x, y + 1, z))
        BuildFace(typeid, new Vector3(x, y + 1, z), Vector3.forward * 1, Vector3.right * 1, true, verts, uvs, tris,3,norms);

        //Back
        if (CheckNeedBuildFace(x, y, z - 1))
        BuildFace(typeid, new Vector3(x, y, z), Vector3.up * 1, Vector3.right * 1, true, verts, uvs, tris,4,norms);
        //Front
        if (CheckNeedBuildFace(x, y, z + 1))
        BuildFace(typeid, new Vector3(x, y, z + 1), Vector3.up * 1, Vector3.right * 1, false, verts, uvs, tris,5,norms); 
    
            }
       


        }else if(100<=typeid&&typeid<200){

    if(typeid==100){



        //water
        //left
        if (CheckNeedBuildFace(x-1,y,z)&&GetChunkBlockType(x-1,y,z)!=100){
            if(GetChunkBlockType(x,y+1,z)!=100){
            BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, false, vertsWT, uvsWT, trisWT,0,normsWT); 




            }else{
     BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.forward, false, vertsWT, uvsWT, trisWT,0,normsWT); 





      
            }
           
        }
            
        //Right
        if (CheckNeedBuildFace(x+1,y,z)&&GetChunkBlockType(x+1,y,z)!=100){
                if(GetChunkBlockType(x,y+1,z)!=100){
     BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, true, vertsWT, uvsWT, trisWT,1,normsWT);



                }else{
         BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,1f,0f), Vector3.forward, true, vertsWT, uvsWT, trisWT,1,normsWT);



            }  

        }

            

        //Bottom
        if (CheckNeedBuildFace(x,y-1,z)&&GetChunkBlockType(x,y-1,z)!=100){
       BuildFace(typeid, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, vertsWT, uvsWT, trisWT,2,normsWT);




        }
            
        //Top
        if (CheckNeedBuildFace(x,y+1,z)&&GetChunkBlockType(x,y+1,z)!=100){
        BuildFace(typeid, new Vector3(x, y + 0.8f, z), Vector3.forward, Vector3.right, true, vertsWT, uvsWT, trisWT,3,normsWT);




        }
           



        //Back
        if (CheckNeedBuildFace(x,y,z-1)&&GetChunkBlockType(x,y,z-1)!=100){
            if(GetChunkBlockType(x,y+1,z)!=100){
            BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.right, true, vertsWT, uvsWT, trisWT,4,normsWT);



       
            }else{
            BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.right, true, vertsWT, uvsWT, trisWT,4,normsWT);






 
            }
            
        }

            
        //Front
        if (CheckNeedBuildFace(x,y,z+1)&&GetChunkBlockType(x,y,z+1)!=100){
            if(GetChunkBlockType(x,y+1,z)!=100){
            BuildFace(typeid, new Vector3(x, y, z + 1), new Vector3(0f,0.8f,0f), Vector3.right, false, vertsWT, uvsWT, trisWT,5,normsWT) ;


            }else{
            BuildFace(typeid, new Vector3(x, y, z+1), new Vector3(0f,1f,0f), Vector3.right, false, vertsWT, uvsWT, trisWT,4,normsWT);

            }
             
        }   
    }
            
        if(typeid>=101&&typeid<150){

            if(typeid==102){
            //torch

               
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0f,0.4375f), new Vector3(0f,0.625f,0f),new Vector3(0f,0f,0.125f),new Vector2(0.0078125f,0.0390625f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0f), false, vertsNS, uvsNS, trisNS,normsNS);
            //Right
     
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0f,0.4375f)+new Vector3(0.125f,0f,0f),  new Vector3(0f,0.625f,0f),new Vector3(0f,0f,0.125f),new Vector2(0.0078125f,0.0390625f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0f), true, vertsNS, uvsNS, trisNS,normsNS);

            //Bottom
     
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0f,0.4375f),new Vector3(0f,0f,0.125f),new Vector3(0.125f,0f,0f),new Vector2(0.0078125f,0.0078125f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0f), false, vertsNS, uvsNS, trisNS,normsNS);
                //Top
       
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0.625f,0.4375f), new Vector3(0f,0f,0.125f), new Vector3(0.125f,0f,0f),new Vector2(0.0078125f,0.0078125f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0.03125f), true, vertsNS, uvsNS, trisNS,normsNS);

            //Back
    
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0f,0.4375f), new Vector3(0f,0.625f,0f), new Vector3(0.125f,0f,0f),new Vector2(0.0078125f,0.0390625f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0f), true, vertsNS, uvsNS, trisNS,normsNS);
            //Front
    
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0f,0.4375f)+new Vector3(0f,0f,0.125f),  new Vector3(0f,0.625f,0f), new Vector3(0.125f,0f,0f),new Vector2(0.0078125f,0.0390625f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0f),false, vertsNS, uvsNS, trisNS,normsNS); 
            

            lightPoints.Add(new Vector3(x,y,z)+new Vector3(0.5f,0.635f,0.5f));
  
            }else{
                    Vector3 randomCrossModelOffset=new Vector3(0f,0f,0f);
                                BuildFace(typeid, new Vector3(x, y, z)+randomCrossModelOffset+new Vector3(-0.005f, 0f, 0.005f), new Vector3(0f,1f,0f)+randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), new Vector3(1f,0f,1f)+randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), false, vertsNS, uvsNS, trisNS,0,normsNS);

                                BuildFace(typeid, new Vector3(x, y, z) + randomCrossModelOffset- new Vector3(-0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), new Vector3(1f, 0f, 1f) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), true, vertsNS, uvsNS, trisNS, 0, normsNS);





                                BuildFace(typeid, new Vector3(x, y, z+1f)+randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), new Vector3(0f,1f,0f)+randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), new Vector3(1f,0f,-1f)+randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), false, vertsNS, uvsNS, trisNS,0,normsNS);

                                BuildFace(typeid, new Vector3(x, y, z + 1f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + -new Vector3(0.005f, 0f, 0.005f), new Vector3(1f, 0f, -1f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), true, vertsNS, uvsNS, trisNS, 0, normsNS);



                            }
      

        }

                        
                    }
                   
                }
            }
        }
    
     
      //  opqVertsNA=verts.AsArray();
      //  opqUVsNA=uvs.AsArray();
      //  opqTrisNA=tris.AsArray();
     //   NSVertsNA=vertsNS.AsArray();
      //  NSUVsNA=uvsNS.AsArray();
     //   NSTrisNA=trisNS.AsArray();

        
        NativeArray<VertexAttributeDescriptor> vertexAttributesDes=new NativeArray<VertexAttributeDescriptor>(new VertexAttributeDescriptor[]{new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)},Allocator.Persistent);
    //    mda[0].SetVertexBufferParams(opqVertsNA.Length+1,vertexAttributesDes);
    //    mdaNS[0].SetVertexBufferParams(NSVertsNA.Length+1,vertexAttributesDes);
     //   mda[1].SetVertexBufferParams(NSVertsNA.Length+1,vertexAttributesDes);   
        var data = mda[0];
        data.SetVertexBufferParams(verts.Count,vertexAttributesDes);
        NativeArray<Vertex> pos = data.GetVertexData<Vertex>();
     
        for(int i=0;i<verts.Count;i++){
        /*    if(pos==null){
                Debug.Log("null");
                return;
            }*/
            pos[i]=new Vertex(verts[i],norms[i],uvs[i]);
           
        }
       data.SetIndexBufferParams((int)(pos.Length), IndexFormat.UInt32);
        var ib = data.GetIndexData<int>();
        for (int i = 0; i < ib.Length; ++i)
            ib[i] = tris[i];

        data.subMeshCount = 1;
        data.SetSubMesh(0, new SubMeshDescriptor(0, ib.Length,MeshTopology.Quads));

           pos.Dispose();
           ib.Dispose();



        var dataNS = mdaNS[0];
        dataNS.SetVertexBufferParams(vertsNS.Count,vertexAttributesDes);
        NativeArray<Vertex> posNS = dataNS.GetVertexData<Vertex>();
      
        for(int i=0;i<vertsNS.Count;i++){
   /*         if(posNS==null){
                Debug.Log("null");
                return;
            }*/
            posNS[i]=new Vertex(vertsNS[i],normsNS[i],uvsNS[i]);
           
        }
       dataNS.SetIndexBufferParams((int)(posNS.Length), IndexFormat.UInt32);
        var ibNS = dataNS.GetIndexData<int>();
        for (int i = 0; i <ibNS.Length;++i){
           ibNS[i] = trisNS[i];  
        }
           

            dataNS.subMeshCount = 1;
            dataNS.SetSubMesh(0, new SubMeshDescriptor(0, ibNS.Length,MeshTopology.Quads));
           posNS.Dispose();
           ibNS.Dispose();






              var dataWT = mdaWT[0];
        dataWT.SetVertexBufferParams(vertsWT.Count,vertexAttributesDes);
        NativeArray<Vertex> posWT = dataWT.GetVertexData<Vertex>();
      
        for(int i=0;i<vertsWT.Count;i++){
        /*    if(posWT==null){
                Debug.Log("null");
                return;
            }*/
            posWT[i]=new Vertex(vertsWT[i],normsWT[i],uvsWT[i]);
           
        }
       dataWT.SetIndexBufferParams((int)(posWT.Length), IndexFormat.UInt32);
        var ibWT = dataWT.GetIndexData<int>();
        for (int i = 0; i <ibWT.Length;++i){
           ibWT[i] = trisWT[i];  
        }
           

            dataWT.subMeshCount = 1;
            dataWT.SetSubMesh(0, new SubMeshDescriptor(0, ibWT.Length,MeshTopology.Quads));
           posWT.Dispose();
           ibWT.Dispose();
        vertexAttributesDes.Dispose();
      /*  var data=mda[0];
           // Tetrahedron vertices with positions and normals.
        // 4 faces with 3 unique vertices in each -- the faces
        // don't share the vertices since normals have to be
        // different for each face.
        data.SetVertexBufferParams(12,
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1));
        // Four tetrahedron vertex positions:
        var sqrt075 = Mathf.Sqrt(0.75f);
        var p0 = new Vector3(0, 0, 0);
        var p1 = new Vector3(1, 0, 0);
        var p2 = new Vector3(0.5f, 0, sqrt075);
        var p3 = new Vector3(0.5f, sqrt075, sqrt075 / 3);
        // The first vertex buffer data stream is just positions;
        // fill them in.
        var pos = data.GetVertexData<Vector3>();
        pos[0] = p0; pos[1] = p1; pos[2] = p2;
        pos[3] = p0; pos[4] = p2; pos[5] = p3;
        pos[6] = p2; pos[7] = p1; pos[8] = p3;
        pos[9] = p0; pos[10] = p3; pos[11] = p1;
        // Note: normals will be calculated later in RecalculateNormals.
        // Tetrahedron index buffer: 4 triangles, 3 indices per triangle.
        // All vertices are unique so the index buffer is just a
        // 0,1,2,...,11 sequence.
        data.SetIndexBufferParams(12, IndexFormat.UInt16);
        var ib = data.GetIndexData<ushort>();
        for (ushort i = 0; i < ib.Length; ++i)
            ib[i] = i;
        // One sub-mesh with all the indices.
        data.subMeshCount = 1;
        data.SetSubMesh(0, new SubMeshDescriptor(0, ib.Length));*/


           

 
            isMeshBuildCompleted=true;    
            
        
    }



  
   public bool isTaskCompleted=false;
    public object taskLock=new object();
    public async void BuildChunk(){
        isTaskCompleted=false;
        try
        {
        if(!isChunkPosInited){
              WorldManager.chunkUnloadingQueue.Enqueue(this.chunkPos,0);  
              isTaskCompleted=true;
            return; 
        }
        isMeshBuildCompleted=false;
     
     
   //     t.Start();
    /*    ThreadStart childref = new ThreadStart(() => InitMap(chunkPos));
        Thread childThread=new Thread(childref);
        childThread.Start();
        childThread.Join();*/


      //  yield return new WaitUntil(()=>isMapGenCompleted==true&&isMeshBuildCompleted==true);
   // if(!isMapGenCompleted){
    ///    yield return 10;
   // }

   
        Mesh.MeshDataArray mbjMeshData=Mesh.AllocateWritableMeshData(1);
        Mesh.MeshDataArray mbjMeshDataNS=Mesh.AllocateWritableMeshData(1);
        Mesh.MeshDataArray mbjMeshDataWT=Mesh.AllocateWritableMeshData(1);
        Mesh.MeshDataArray mbjMeshDataLOD1=Mesh.AllocateWritableMeshData(1);
        Mesh.MeshDataArray mbjMeshDataLOD2 = Mesh.AllocateWritableMeshData(1);
            /*    NativeArray<VertexAttributeDescriptor> vertexAttributesDes=new NativeArray<VertexAttributeDescriptor>(new VertexAttributeDescriptor[]{new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)},Allocator.Persistent);
                 mbjMeshData[0].SetVertexBufferParams(opqVertsNA.Length,vertexAttributesDes);
                mbjMeshData[1].SetVertexBufferParams(NSVertsNA.Length,vertexAttributesDes);   */
            if (isChunkPosInited==true){
          //  isTaskShouldAbort=false;
            await Task.Run(() => InitMap(chunkPos,mbjMeshData,mbjMeshDataNS,mbjMeshDataWT, mbjMeshDataLOD1, mbjMeshDataLOD2));  
            
        }else{
            isTaskCompleted=true;
            return;
        }
      
      //  await UniTask.WaitUntil(() => isMeshBuildCompleted == true);
            if(chunkMesh==null){
             chunkMesh=new Mesh();        
            }
            if (chunkMeshLOD1 == null)
            {
                chunkMeshLOD1 = new Mesh();
            }
            if (chunkMeshLOD2 == null)
            {
                chunkMeshLOD2 = new Mesh();
            }

            if (chunkNonSolidMesh==null){
                chunkNonSolidMesh=new Mesh();      
                }
               
           
            
            chunkWaterMesh=new Mesh();
      

     
     //     frontChunk=GetChunk(new Vector2Int((int)transform.position.x,(int)transform.position.z+chunkWidth));
      //  backChunk=GetChunk(new Vector2Int((int)transform.position.x,(int)transform.position.z-chunkWidth));
      //  leftChunk=GetChunk(new Vector2Int((int)transform.position.x-chunkWidth,(int)transform.position.z));
      //  rightChunk=GetChunk(new Vector2Int((int)transform.position.x+chunkWidth,(int)transform.position.z));



        
      //  Thread childThread=new Thread(() => GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS));
     //   childThread.Start();
     // ThreadPool.QueueUserWorkItem(()=>GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS));
        //childThread.Join();
        //Task t1 = new Task(() => GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS));
       // ThreadPool.QueueUserWorkItem(new WaitCallback(GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS)));
       // Task.Run(()=>GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS));
       // t1.Wait();
     //   yield return new WaitUntil(()=>isMeshBuildCompleted==true); 
   
    
     
       
    
       
       
      
     /*   MeshBuildJob mbj=new MeshBuildJob{verts=opqVertsNA,tris=opqTrisNA,vertLen=opqVertsNA.Length,uvs=opqUVsNA,dataArray=mbjMeshData};
        JobHandle jh=mbj.Schedule();
        MeshBuildJob mbjNS=new MeshBuildJob{verts=NSVertsNA,tris=NSTrisNA,vertLen=NSVertsNA.Length,uvs=NSUVsNA,dataArray=mbjMeshDataNS};
        JobHandle jhNS=mbjNS.Schedule();
       

        JobHandle.CompleteAll(ref jh,ref jhNS);*/
        
        Mesh.ApplyAndDisposeWritableMeshData(mbjMeshData,chunkMesh);
        Mesh.ApplyAndDisposeWritableMeshData(mbjMeshDataLOD1, chunkMeshLOD1);
        Mesh.ApplyAndDisposeWritableMeshData(mbjMeshDataLOD2, chunkMeshLOD2);
        Mesh.ApplyAndDisposeWritableMeshData(mbjMeshDataNS,chunkNonSolidMesh);
        Mesh.ApplyAndDisposeWritableMeshData(mbjMeshDataWT,chunkWaterMesh);
        
         chunkMesh.RecalculateBounds();
         chunkMesh.RecalculateTangents();
         
            JobHandle jh=new BakeJob{meshID=chunkMesh.GetInstanceID()}.Schedule();
            chunkNonSolidMesh.RecalculateBounds();
            chunkWaterMesh.RecalculateBounds();

            chunkMeshLOD1.RecalculateBounds();
            chunkMeshLOD1.RecalculateTangents();
            chunkMeshLOD2.RecalculateBounds();
            chunkMeshLOD2.RecalculateTangents();
            // chunkWaterMesh=WeldVertices(chunkWaterMesh);
            //     chunkWaterMesh.RecalculateNormals();
            //    chunkNonSolidMesh.RecalculateNormals();


            // chunkMesh.indexFormat=IndexFormat.UInt32;
            /* chunkNonSolidMesh.indexFormat=IndexFormat.UInt32;
             chunkNonSolidMesh.SetVertices(NSVerts);
             chunkNonSolidMesh.SetUVs(0,NSUVs);
             chunkNonSolidMesh.triangles = NSTris;
             chunkMesh.SetVertices(opqVerts);
             chunkMesh.SetUVs(0,opqUVs);
             chunkMesh.triangles = opqTris;

             chunkNonSolidMesh.RecalculateNormals();

             chunkMesh.RecalculateNormals();*/


            //     var job=new BakeJob();
            //   job.meshes.Add(chunkMesh.GetInstanceID());
            //     job.Schedule();
            //     Graphics.DrawMeshNow(chunkMesh,transform.position,Quaternion.identity);
            // CombineInstance[] combine=new CombineInstance[1];
            // combine[0].mesh=chunkMesh;
            // WorldMeshManager.combine.Add(new CombineInstance{mesh=chunkMesh,transform=transform.localToWorldMatrix});
            //  WorldMeshManager.OnAllChunkMeshesChanged();

            meshFilter.mesh = chunkMesh;
        meshFilterNS.mesh = chunkNonSolidMesh;
        meshFilterWT.mesh=chunkWaterMesh;
        meshFilterLOD1.mesh = chunkMeshLOD1;
        meshFilterLOD2.mesh = chunkMeshLOD2;
            //   NativeArray<int> a=new NativeArray<int>(1,Allocator.TempJob);
            //  a[0]=chunkMesh.GetInstanceID();
            //   a[1]=chunkNonSolidMesh.GetInstanceID();

            //    yield return new WaitUntil(()=>handle.IsCompleted==true&&chunkMesh!=null);
            //   yield return new WaitForSeconds(0.01f);
            /*  NSVertsNA.Dispose();
              NSUVsNA.Dispose();
              NSTrisNA.Dispose();
              opqVertsNA.Dispose();
              opqUVsNA.Dispose();
              opqTrisNA.Dispose();
              NSVertsNL.Dispose();
              NSUVsNL.Dispose();
              NSTrisNL.Dispose();
              opqVertsNL.Dispose();
              opqUVsNL.Dispose();
              opqTrisNL.Dispose();*/
            //     a.Dispose();
            meshRenderer.enabled=true;
        meshRendererNS.enabled=true;
        meshRendererWT.enabled=true;
        meshRendererLOD1.enabled=true;
        meshRendererLOD2.enabled = true;
            foreach (var go in pointLightGameObjects){

     //        await UniTask.Yield();

        //    Destroy(pointLightGameObjects[0]);
            go.SetActive(false);
        }
      
        Vector3 worldSpacePos=new Vector3(chunkPos.x,0f,chunkPos.y);
        int pooledPointLightGameObjectsCount=pointLightGameObjects.Count;
       for(int i=0;i<Mathf.Min(lightPoints.Count,64);i++){
        
      //  await UniTask.NextFrame();
        if(pooledPointLightGameObjectsCount>0){
            int index=pointLightGameObjects.Count-pooledPointLightGameObjectsCount;

            GameObject b=pointLightGameObjects[index];
            b.transform.position=worldSpacePos+lightPoints[i];
            b.SetActive(true);
            pooledPointLightGameObjectsCount--;
        }else{
            GameObject a=Instantiate(VoxelWorld.pointLightPrefab,worldSpacePos+lightPoints[i],Quaternion.identity,transform);
            pointLightGameObjects.Add(a);   
        }
        
       }
           
    
            jh.Complete();
            if (chunkMesh.vertexCount > 0)
            {
            meshCollider.sharedMesh=chunkMesh;

            }
            else
            {
                meshCollider.sharedMesh = null;
            }
             isStrongLoaded=true;



            //[]  meshColliderNS.sharedMesh = chunkNonSolidMesh;

            //   sw.Stop();
            //   Debug.Log("Time used:"+sw.ElapsedMilliseconds);
            //       yield break;


            //  isStrongLoaded=false;
            lodGroup.fadeMode = LODFadeMode.CrossFade;
            
            LOD[] lod = new LOD[] { new LOD {fadeTransitionWidth=0.05f, screenRelativeTransitionHeight= 0.05f*Mathf.Max(chunkMesh.bounds.size.x, chunkMesh.bounds.size.y, chunkMesh.bounds.size.z, 1f) /16f, renderers=new Renderer[] { meshRenderer } } ,
                new LOD {fadeTransitionWidth=0.05f, screenRelativeTransitionHeight= 0.05f*Mathf.Max(chunkMesh.bounds.size.x, chunkMesh.bounds.size.y, chunkMesh.bounds.size.z, 1f) /32f, renderers=new Renderer[] { meshRendererLOD1 } } ,
                new LOD { fadeTransitionWidth = 0.05f, screenRelativeTransitionHeight = 0.001f * Mathf.Max(chunkMesh.bounds.size.x, chunkMesh.bounds.size.y, chunkMesh.bounds.size.z,1f) / 32f, renderers = new Renderer[] { meshRendererLOD2 } } };
            lodGroup.SetLODs(lod);
         
        }
        catch(Exception e)
        {

            Debug.Log(e);
            isTaskCompleted=true;
            //BuildChunk();
            return;
        }
        //   System.Diagnostics.Stopwatch sw=new System.Diagnostics.Stopwatch();
        //     sw.Start();

       
      
 
        isChunkMapUpdated=false;
        isTaskCompleted=true;
    }

    


   /* void BuildBlock(int x, int y, int z, List<Vector3> verts, List<Vector2> uvs, List<int> tris, List<Vector3> vertsNS, List<Vector2> uvsNS, List<int> trisNS){


        if (map[x, y, z] == 0) return;


        int typeid = map[x, y, z];
        if(0<typeid&&typeid<100){
           //Left
        if (CheckNeedBuildFace(x - 1, y, z))
            BuildFace(typeid, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris,0);
        //Right
        if (CheckNeedBuildFace(x + 1, y, z))
            BuildFace(typeid, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris,1);

        //Bottom
        if (CheckNeedBuildFace(x, y - 1, z))
            BuildFace(typeid, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris,2);
        //Top
        if (CheckNeedBuildFace(x, y + 1, z))
            BuildFace(typeid, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris,3);

        //Back
        if (CheckNeedBuildFace(x, y, z - 1))
            BuildFace(typeid, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris,4);
        //Front
        if (CheckNeedBuildFace(x, y, z + 1))
            BuildFace(typeid, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris,5); 
        }else if(100<=typeid&&typeid<200){
            if(typeid==100){


        //left
        if (CheckNeedBuildFace(x-1,y,z)&&GetChunkBlockType(x-1,y,z)!=100){
            if(GetChunkBlockType(x,y+1,z)!=100){
                    BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, false, vertsNS, uvsNS, trisNS,0); 
            }else{
                BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.forward, false, vertsNS, uvsNS, trisNS,0); 
            }
           
        }
            
        //Right
        if (CheckNeedBuildFace(x + 1, y, z)&&GetChunkBlockType(x+1,y,z)!=100){
                if(GetChunkBlockType(x,y+1,z)!=100){
                    BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, true, vertsNS, uvsNS, trisNS,1);
                }else{
                        BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,1f,0f), Vector3.forward, true, vertsNS, uvsNS, trisNS,1);
            }  

        }

            

        //Bottom
        if (CheckNeedBuildFace(x, y - 1, z)&&GetChunkBlockType(x,y-1,z)!=100)
            BuildFace(typeid, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, vertsNS, uvsNS, trisNS,2);
        //Top
        if (CheckNeedBuildFace(x, y + 1, z)&&GetChunkBlockType(x,y+1,z)!=100)
            BuildFace(typeid, new Vector3(x, y + 0.8f, z), Vector3.forward, Vector3.right, true, vertsNS, uvsNS, trisNS,3);



        //Back
        if (CheckNeedBuildFace(x, y, z - 1)&&GetChunkBlockType(x,y,z-1)!=100){
            if(GetChunkBlockType(x,y+1,z)!=100){
                BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.right, true, vertsNS, uvsNS, trisNS,4);
            }else{
                BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.right, true, vertsNS, uvsNS, trisNS,4);
            }
            
        }

            
        //Front
        if (CheckNeedBuildFace(x, y, z + 1)&&GetChunkBlockType(x,y,z+1)!=100){
            if(GetChunkBlockType(x,y+1,z)!=100){
                BuildFace(typeid, new Vector3(x, y, z + 1), new Vector3(0f,0.8f,0f), Vector3.right, false, vertsNS, uvsNS, trisNS,5); 
            }else{
                BuildFace(typeid, new Vector3(x, y, z+1), new Vector3(0f,1f,0f), Vector3.right, false, vertsNS, uvsNS, trisNS,4);
            }
             
        }
           

            }
        if(typeid>=101&&typeid<150){
            Vector3 randomCrossModelOffset=new Vector3(0f,0f,0f);
            BuildFace(typeid, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, false, vertsNS, uvsNS, trisNS,0);
            BuildFace(typeid, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, true, vertsNS, uvsNS, trisNS,0);
            BuildFace(typeid, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, false, vertsNS, uvsNS, trisNS,0);
            BuildFace(typeid, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, true, vertsNS, uvsNS, trisNS,0);
        }
        }
        
    }*/


    bool CheckNeedBuildFace(int x, int y, int z){
        if (y < 0) return false;
        var type = GetChunkBlockType(x, y, z);
        bool isNonSolid=false;
        if(type<200&&type>=100){
            isNonSolid=true;
        }
        switch(isNonSolid){
            case true:return true;
            case false:break;
        }
        switch (type)
        {
            
         
            case 0:
                return true;
            case 9:
            return true;
            default:
                return false;
        }
    }

    bool CheckNeedBuildFace(int x, int y, int z,int LODSkipBlockCount)
    {
        if (y < 0) return false;
        var type = GetChunkBlockTypeLOD(x, y, z, LODSkipBlockCount);
        bool isNonSolid = false;
        if (type < 200 && type >= 100)
        {
            isNonSolid = true;
        }
        switch (isNonSolid)
        {
            case true: return true;
            case false: break;
        }
        switch (type)
        {


            case 0:
                return true;
            case 9:
                return !(LODSkipBlockCount>1);
            default:
                return false;
        }
    }

    public static int PredictBlockType(float noiseValue,int y){  
        if(noiseValue>y){
            return 1;
        }else{
            if(y<chunkSeaLevel&&y>noiseValue){
                     return 100;       
            }
                return 0;
        }
       // return 0;
    }
    public static int PredictBlockType3D(int x,int y,int z)
    {
        float yLerpValue = Mathf.Lerp(-1, 1, (Mathf.Abs(y - chunkSeaLevel)) / 40f);
        float xzLerpValue = Mathf.Lerp(-1, 1, (new Vector3(x, 0, z).magnitude / 384f));
        float xyzLerpValue = Mathf.Max(xzLerpValue, yLerpValue);
        float noiseValue=frequentNoiseGenerator.GetSimplex(x,y,z);
        if (noiseValue > xyzLerpValue)
        {
            return 1;
        }
        else
        {
            
            return 0;
        }
        // return 0;
    }



    public static int PredictBlockType3DLOD(int x, int y, int z,int LODBlockSkipCount=4)
    {
        float yLerpValue = Mathf.Lerp(-1, 1, (Mathf.Abs(y - chunkSeaLevel)) / 40f);
        float xzLerpValue = Mathf.Lerp(-1, 1, (new Vector3(x, 0, z).magnitude / 384f));
        float xyzLerpValue = Mathf.Max(xzLerpValue, yLerpValue);
        float noiseValue = frequentNoiseGenerator.GetSimplex((int)(x/ LODBlockSkipCount) * LODBlockSkipCount, y, (int)(z / LODBlockSkipCount) * LODBlockSkipCount);
        if (noiseValue > xyzLerpValue)
        {
            return 1;
        }
        else
        {

            return 0;
        }
        // return 0;
    }
    public int GetChunkBlockType(int x, int y, int z){
        if (y < 0 || y > chunkHeight - 1)
        {
            return 0;
        }
        
        if ((x < 0) || (z < 0) || (x >= chunkWidth) || (z >= chunkWidth))
        {
            if (VoxelWorld.currentWorld.worldGenType == 0)
            {
        if(x>=chunkWidth){
                if(rightChunk!=null&&isRightChunkUnloaded==false){
                    if (rightChunk.isMapGenCompleted == true)
                    {

                        return rightChunk.map[0, y, z];
                    }
                    else return PredictBlockType(thisHeightMap[x - chunkWidth + 25, z + 8], y);

                }
                else return PredictBlockType(thisHeightMap[x-chunkWidth+25,z+8],y);
                
            }else if(z>=chunkWidth){
                if(frontChunk!=null&&isFrontChunkUnloaded==false){
                    if(frontChunk.isMapGenCompleted==true)
                    {

                    return frontChunk.map[x,y,0];
                    }
                    else return PredictBlockType(thisHeightMap[x + 8, z - chunkWidth + 25], y);



                }
                else return PredictBlockType(thisHeightMap[x+8,z-chunkWidth+25],y);
            }else if(x<0){
                if(leftChunk!=null&&isLeftChunkUnloaded==false){
                    if (leftChunk.isMapGenCompleted == true)
                    {

                        return leftChunk.map[chunkWidth - 1, y, z];
                    }
                    else return PredictBlockType(thisHeightMap[8 + x, z + 8], y);

                }
                else return PredictBlockType(thisHeightMap[8+x,z+8],y);
            }else if(z<0){
                if(backChunk!=null&&isBackChunkUnloaded==false){
                    if (backChunk.isMapGenCompleted == true)
                    {
                        return backChunk.map[x, y, chunkWidth - 1];
                    }
                    else return PredictBlockType(thisHeightMap[x + 8, 8 + z], y);

                }
                else return PredictBlockType(thisHeightMap[x+8,8+z],y);
            }
            }
            else if (VoxelWorld.currentWorld.worldGenType==2)
            {
                if (x >= chunkWidth)
                {
                    if (rightChunk != null && isRightChunkUnloaded == false)
                    {
                        if (rightChunk.isMapGenCompleted == true)
                        {

                            return rightChunk.map[0, y, z];
                        }
                        else return PredictBlockType3D(chunkPos.x+x,y,chunkPos.y+z);

                    }
                    else return PredictBlockType3D(chunkPos.x + x, y, chunkPos.y + z);

                }
                else if (z >= chunkWidth)
                {
                    if (frontChunk != null && isFrontChunkUnloaded == false)
                    {
                        if (frontChunk.isMapGenCompleted == true)
                        {

                            return frontChunk.map[x, y, 0];
                        }
                        else return PredictBlockType3D(chunkPos.x + x, y, chunkPos.y + z);



                    }
                    else return PredictBlockType3D(chunkPos.x + x, y, chunkPos.y + z);
                }
                else if (x < 0)
                {
                    if (leftChunk != null && isLeftChunkUnloaded == false)
                    {
                        if (leftChunk.isMapGenCompleted == true)
                        {

                            return leftChunk.map[chunkWidth - 1, y, z];
                        }
                        else return PredictBlockType3D(chunkPos.x + x, y, chunkPos.y + z);

                    }
                    else return PredictBlockType3D(chunkPos.x + x, y, chunkPos.y + z);
                }
                else if (z < 0)
                {
                    if (backChunk != null && isBackChunkUnloaded == false)
                    {
                        if (backChunk.isMapGenCompleted == true)
                        {
                            return backChunk.map[x, y, chunkWidth - 1];
                        }
                        else return PredictBlockType3D(chunkPos.x + x, y, chunkPos.y + z);

                    }
                    else return PredictBlockType3D(chunkPos.x + x, y, chunkPos.y + z);
                }
            }
            else
            {
                return 1;
            }
          
           
        }
        return map[x, y, z];
    }


    public int GetChunkBlockTypeLOD(int x, int y, int z,int LODSkipBlockCount=4)
    {
        if (y < 0 || y > chunkHeight - 1)
        {
            return 0;
        }

        if ((x < 0) || (z < 0) || (x >= chunkWidth) || (z >= chunkWidth))
        {
            if (VoxelWorld.currentWorld.worldGenType == 0)
            {
                if (x >= chunkWidth)
                {
                     return PredictBlockType(thisHeightMap[x - chunkWidth + 25, z + 8], y);

                }
                else if (z >= chunkWidth)
                {
                      return PredictBlockType(thisHeightMap[x + 8, z - chunkWidth + 25], y);
                }
                else if (x < 0)
                {
                     return PredictBlockType(thisHeightMap[8 + x, z + 8], y);
                }
                else if (z < 0)
                {
                    return PredictBlockType(thisHeightMap[x + 8, 8 + z], y);
                }
            }
            else if (VoxelWorld.currentWorld.worldGenType == 2)
            {
                if (x >= chunkWidth)
                {
                    if (rightChunk != null && isRightChunkUnloaded == false)
                    {
                        return PredictBlockType3DLOD(chunkPos.x + x, y, chunkPos.y + z, LODSkipBlockCount);

                    }
                    else return PredictBlockType3DLOD(chunkPos.x + x, y, chunkPos.y + z, LODSkipBlockCount);

                }
                else if (z >= chunkWidth)
                {
                    if (frontChunk != null && isFrontChunkUnloaded == false)
                    {
                         return PredictBlockType3DLOD(chunkPos.x + x, y, chunkPos.y + z, LODSkipBlockCount);



                    }
                    else return PredictBlockType3DLOD(chunkPos.x + x, y, chunkPos.y + z, LODSkipBlockCount);
                }
                else if (x < 0)
                {
                    if (leftChunk != null && isLeftChunkUnloaded == false)
                    {
                          return PredictBlockType3DLOD(chunkPos.x + x, y, chunkPos.y + z, LODSkipBlockCount);

                    }
                    else return PredictBlockType3DLOD(chunkPos.x + x, y, chunkPos.y + z, LODSkipBlockCount);
                }
                else if (z < 0)
                {
                    if (backChunk != null && isBackChunkUnloaded == false)
                    {
                         return PredictBlockType3DLOD(chunkPos.x + x, y, chunkPos.y + z, LODSkipBlockCount);

                    }
                    else return PredictBlockType3DLOD(chunkPos.x + x, y, chunkPos.y + z, LODSkipBlockCount);
                }
            }
            else
            {
                return 1;
            }


        }
        return map[x, y, z];
    }
    static void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int side,List<Vector3> norms){
        int index = verts.Count;
        Vector3 vert0=corner;
        Vector3 vert1=corner+up;
        Vector3 vert2=corner+up+right;
        Vector3 vert3=corner+right;
        verts.Add (vert0);
        verts.Add (vert1);
        verts.Add (vert2);
        verts.Add (vert3);

        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        if(blockInfo.ContainsKey(typeid)){
        uvCorner=blockInfo[typeid][side];    
        }
        
        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
    
        if (reversed)
            {
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);
        
        //    tris.Add(index + 2);
            tris.Add(index + 3);
            //   tris.Add(index + 0);
            Vector3 normal = Vector3.Cross(up, right);
            norms.Add(normal);
            norms.Add(normal);
            norms.Add(normal);
            norms.Add(normal);
            }
            else
            {
       //     tris.Add(index + 1);
       //     tris.Add(index + 0);
       //     tris.Add(index + 2);
            tris.Add(index+3);
            tris.Add(index+2);
            tris.Add(index+1);
            tris.Add(index+0);
            //     tris.Add(index + 3);
            //   tris.Add(index + 2);
            //   tris.Add(index + 0);
            Vector3 normal = -Vector3.Cross(up, right);
            norms.Add(normal);
            norms.Add(normal);
            norms.Add(normal);
            norms.Add(normal);
        }
    
    }



    static void BuildFaceComplex(Vector3 corner, Vector3 up, Vector3 right,Vector2 uvWidth,Vector2 uvCorner, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris,List<Vector3> norms){
        int index = verts.Count;
        Vector3 vert0=corner;
        Vector3 vert1=corner+up;
        Vector3 vert2=corner+up+right;
        Vector3 vert3=corner+right;
        verts.Add (vert0);
        verts.Add (vert1);
        verts.Add (vert2);
        verts.Add (vert3);

      //  Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
      //  Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
    //    uvCorner=blockInfo[typeid][side];
        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
    
        if (reversed)
            {
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 3);
            
             Vector3 v1 = Vector3.Cross(up, right);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            }
            else
            {
           
       
        
            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 1);
            tris.Add(index + 0);
            Vector3 v1 = -Vector3.Cross(up, right);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
        }
    
    }

   /* public static Chunk GetUnloadedChunk(Vector2Int chunkPos){
        if(Chunks.ContainsKey(chunkPos)&&Chunks[chunkPos].isMapGenCompleted==false){
            Chunk tmp=Chunks[chunkPos];
            return tmp;
        }else{
            return null;
        }
        
    }*/
    public static Chunk GetChunk(Vector2Int chunkPos){
 /*       if(Chunks.ContainsKey(chunkPos)){
            Chunk tmp=Chunks[chunkPos];
            return tmp;
        }else{
            return null;
        }*/
        return VoxelWorld.currentWorld.GetChunk(chunkPos);
    }
      
 
 /*   public static void TryUpdateChunkThread(){
   //     delegate void mainBuildChunk();
     //   mainBuildChunk callback;
     
        while(true){
            Thread.Sleep(5);
            if(WorldManager.isGoingToQuitGame==true){
                return;
            }
         
          
            foreach(var c in Chunks){
            if(c.Value.isChunkMapUpdated==true){
               c.Value.isModifiedInGame=true;  
            if(c.Value.isMeshBuildCompleted==true){
                 WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(c.Value,true),-50); 
            }else{
                continue;
            }
           //InitMap(chunkPos);
            c.Value.isChunkMapUpdated=false;
            }
                }
         
          
                 
        }
          
    }*/
 /* public static void TryReleaseChunkThread(){
    while(true){
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
       }
        Thread.Sleep(200);
            if(WorldManager.isGoingToQuitGame==true){
                return;
            } 
        foreach(var c in Chunks){
            if(!Chunks.ContainsKey(c.Key)){
            continue;
            }
        if(WorldManager.chunkUnloadingQueue.Contains(c.Key)){
            continue;
         }
           Vector2Int cPos=c.Key;
           bool isChunkNeededRemoving=true;
        foreach(var cl in ChunkLoaderBase.allChunkLoaders){
        if(!(Mathf.Abs(cPos.x-cl.chunkLoadingCenter.x)>cl.chunkLoadingRange+Chunk.chunkWidth+3||Mathf.Abs(cPos.y-cl.chunkLoadingCenter.y)>cl.chunkLoadingRange+Chunk.chunkWidth+3)){
            isChunkNeededRemoving=false;
           
      //    c.Value.isChunkPosInited=false;
        }       
        }
        if(isChunkNeededRemoving==true&&!WorldManager.chunkUnloadingQueue.Contains(c.Key)){
            WorldManager.chunkUnloadingQueue.Enqueue(c.Key,0);
        }
        }
        }
      
       
    }*/
  /*  void TryReleaseChunk(){
        
         if(!isChunkPosInited){
            return;
         }
       //  TransformAccessArray t=new TransformAccessArray(new Transform[]{playerPos},1);
         Vector3 pos=playerPos.position;
       if(Mathf.Abs(chunkPos.x-pos.x)>PlayerMove.viewRange+Chunk.chunkWidth+3||Mathf.Abs(chunkPos.y-pos.z)>PlayerMove.viewRange+Chunk.chunkWidth+3&&isMeshBuildCompleted==true&&!WorldManager.chunkUnloadingQueue.Contains(this)){

           WorldManager.chunkUnloadingQueue.Enqueue(this,1-((int)Mathf.Abs(transform.position.x-playerPos.position.x)+(int)Mathf.Abs(transform.position.z-playerPos.position.z)));
           isChunkPosInited=false;
        }
      
    }*/
    
  
    public void BFSInit(int x,int y,int z){
        BFSMapUpdate(x,y,z);
    }
    public void UpdateBlock(int x,int y,int z){
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==101&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y-1,chunkPos.y+z))==0){
        
            WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x+x,y,chunkPos.y+z));
        }
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==102&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y-1,chunkPos.y+z))==0){
           
            WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x+x,y,chunkPos.y+z));
        }
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==100&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y-1,chunkPos.y+z))==0){
           WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x+x,y-1,chunkPos.y+z),100);
        }
        
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==100&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x-1,y,chunkPos.y+z))==0){
           WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x+x-1,y,chunkPos.y+z),100);
        }
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==100&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x+1,y,chunkPos.y+z))==0){
           WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x+x+1,y,chunkPos.y+z),100);
        }
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==100&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z-1))==0){
           WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x+x,y,chunkPos.y+z-1),100);
        }
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==100&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z+1))==0){
           WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x+x,y,chunkPos.y+z+1),100);
        }
    }
    public void BFSMapUpdate(int x,int y,int z){
        //left right bottom top back front
        //left x-1 right x+1 top y+1 bottom y-1 back z-1 front z+1
      
       
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==101&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y-1,chunkPos.y+z))==0){
            WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x+x,y,chunkPos.y+z));
        }
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==102&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y-1,chunkPos.y+z))==0){
           
            WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x+x,y,chunkPos.y+z));
        }
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==100&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y-1,chunkPos.y+z))==0){
           WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x+x,y-1,chunkPos.y+z),100);
        }
        
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==100&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x-1,y,chunkPos.y+z))==0){
           WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x+x-1,y,chunkPos.y+z),100);
        }
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==100&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x+1,y,chunkPos.y+z))==0){
           WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x+x+1,y,chunkPos.y+z),100);
        }
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==100&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z-1))==0){
           WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x+x,y,chunkPos.y+z-1),100);
        }
        if(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z))==100&&WorldHelper.instance.GetBlock(new Vector3(chunkPos.x+x,y,chunkPos.y+z+1))==0){
           WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x+x,y,chunkPos.y+z+1),100);
        }
        UpdateBlock(x-1,y,z);
        UpdateBlock(x+1,y,z);
        UpdateBlock(x,y-1,z);
        UpdateBlock(x,y+1,z);
        UpdateBlock(x,y,z-1);
        UpdateBlock(x,y,z+1);
    }


    //   void UpdatePlayerDistance(){
    //        playerDistance = (chunkPos - new Vector2(playerPos.position.x,playerPos.position.z)).sqrMagnitude;
    //
    //  }
   /* public void Update()
    {
        if(BoundingBoxCullingHelper.IsBoundingBoxInOrIntersectsFrustum(meshRenderer.bounds,GeometryUtility.CalculateFrustumPlanes(Camera.main)))
        {
            Debug.DrawLine(meshRenderer.bounds.center + new Vector3(0, -100, 0), meshRenderer.bounds.center + new Vector3(0, 100, 0),Color.green);
        }
    }*/
}
