 using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Rendering;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.IO;
using Utf8Json;
public class WorldData{
    public int posX;
    public int posZ;
    public int[,,] map;
    
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
    public struct BakeJob:IJobParallelFor{
        public NativeArray<int> meshes;
        public void Execute(int index){
            Physics.BakeMesh(meshes[index],false);
        }
    }
    public struct MeshBuildJob:IJob{
        public NativeArray<Vector3> verts;
        public NativeArray<Vector2> uvs;
        public NativeArray<int> tris;
            public int vertLen;
            public int uvsLen;
            public int trisLen;
            public Mesh.MeshDataArray dataArray;
         public void Execute(){
             // Allocate mesh data for one mesh.
      //  dataArray = Mesh.AllocateWritableMeshData(1);
        var data = dataArray[0];
        // Tetrahedron vertices with positions and normals.
        // 4 faces with 3 unique vertices in each -- the faces
        // don't share the vertices since normals have to be
        // different for each face.
        data.SetVertexBufferParams(vertLen,
           new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
        // Four tetrahedron vertex positions:
   //     var sqrt075 = Mathf.Sqrt(0.75f);
   //     var p0 = new Vector3(0, 0, 0);
   //     var p1 = new Vector3(1, 0, 0);
   //     var p2 = new Vector3(0.5f, 0, sqrt075);
   //     var p3 = new Vector3(0.5f, sqrt075, sqrt075 / 3);
        // The first vertex buffer data stream is just positions;
        // fill them in.
        var pos = data.GetVertexData<Vertex>();
      //  pos=verts;
        for(int i=0;i<pos.Length;i++){
            pos[i]=new Vertex(verts[i],new Vector3(1f,1f,1f),uvs[i]);
           
        }
        // Note: normals will be calculated later in RecalculateNormals.
        // Tetrahedron index buffer: 4 triangles, 3 indices per triangle.
        // All vertices are unique so the index buffer is just a
        // 0,1,2,...,11 sequence.
    //    data.SetIndexBufferParams(verts.Length, IndexFormat.UInt16);
       data.SetIndexBufferParams((int)(pos.Length/2)*3, IndexFormat.UInt16);
        var ib = data.GetIndexData<ushort>();
        for (ushort i = 0; i < ib.Length; ++i)
            ib[i] = (ushort)tris[i];
        // One sub-mesh with all the indices.
        data.subMeshCount = 1;
        data.SetSubMesh(0, new SubMeshDescriptor(0, ib.Length));
        // Create the mesh and apply data to it:
     //   Debug.Log("job");
           
        }

    }
    //public enum BlockType
  //  {
    //    None = 0,
  //      Stone = 1,
   //     Grass = 2,
    //    Dirt=3,
  //  }
  //0None 1Stone 2Grass 3Dirt 4Side grass block 5Bedrock 6WoodX 7WoodY 8WoodZ 9Leaves 10Diamond Ore
  //100Water 101Grass
  //200Leaves
  //0-99solid blocks
  //100-199no hitbox blocks
  //200-299hitbox nonsolid blocks
    public static FastNoise noiseGenerator=new FastNoise();
    public static RuntimePlatform platform = Application.platform;
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
    public bool isStrongLoaded=false;
    public static Dictionary<int,List<Vector2>> itemBlockInfo=new Dictionary<int,List<Vector2>>();
    public static Dictionary<int,List<Vector2>> blockInfo=new Dictionary<int,List<Vector2>>();
    public Mesh chunkMesh;
    public Mesh chunkHitboxNonSolidMesh;
    public Mesh chunkNonSolidMesh;
    public static int worldGenType=0;
    //0Inf 1Superflat
    public static int chunkWidth=16;
    public static int chunkHeight=256;
    public static int chunkSeaLevel=63;

    public static Dictionary<Vector2Int,Chunk> Chunks=new Dictionary<Vector2Int,Chunk>();
    public static Dictionary<Vector2Int,WorldData> chunkDataReadFromDisk=new Dictionary<Vector2Int,WorldData>();

    public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public MeshCollider meshColliderNS;
    public MeshRenderer meshRendererNS;
    public MeshFilter meshFilterNS;
    public int[,,] map=new int[chunkWidth+3,chunkHeight+3,chunkWidth+3];
    public Chunk frontChunk;
    public Chunk backChunk;
    public Chunk leftChunk;
    public Chunk rightChunk;
    public Vector2Int chunkPos;
    public Transform playerPos;
    public float playerDistance;




    public Vector3[] opqVerts;
    public Vector2[] opqUVs;
    public int[] opqTris;
    public Vector3[] NSVerts;
    public Vector2[] NSUVs;
    public int[] NSTris;
    public static void AddBlockInfo(){
        //left right bottom top back front
        blockInfo.Add(1,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)});
        blockInfo.Add(2,new List<Vector2>{new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f)});
        blockInfo.Add(3,new List<Vector2>{new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f)});
        blockInfo.Add(4,new List<Vector2>{new Vector2(0.1875f,0f),new Vector2(0.1875f,0f),new Vector2(0.125f,0f),new Vector2(0.0625f,0f),new Vector2(0.1875f,0f),new Vector2(0.1875f,0f)});
        blockInfo.Add(100,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)});
        blockInfo.Add(101,new List<Vector2>{new Vector2(0f,0.0625f)});
        blockInfo.Add(5,new List<Vector2>{new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f)});
        blockInfo.Add(6,new List<Vector2>{new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f)});
        blockInfo.Add(7,new List<Vector2>{new Vector2(0.3125f,0f),new Vector2(0.3125f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.3125f,0f),new Vector2(0.3125f,0f)});
        blockInfo.Add(8,new List<Vector2>{new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f)});
        blockInfo.Add(9,new List<Vector2>{new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f)});


        itemBlockInfo.Add(1,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)});
        itemBlockInfo.Add(2,new List<Vector2>{new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f)});
        itemBlockInfo.Add(3,new List<Vector2>{new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f)});
        itemBlockInfo.Add(4,new List<Vector2>{new Vector2(0.1875f,0f),new Vector2(0.1875f,0f),new Vector2(0.125f,0f),new Vector2(0.0625f,0f),new Vector2(0.1875f,0f),new Vector2(0.1875f,0f)});
        itemBlockInfo.Add(100,new List<Vector2>{new Vector2(0.0625f,0.0625f),new Vector2(0.0625f,0.0625f),new Vector2(0.0625f,0.0625f),new Vector2(0.0625f,0.0625f),new Vector2(0.0625f,0.0625f),new Vector2(0.0625f,0.0625f)});
        itemBlockInfo.Add(101,new List<Vector2>{new Vector2(0f,0.0625f)});
        itemBlockInfo.Add(5,new List<Vector2>{new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f)});
        itemBlockInfo.Add(6,new List<Vector2>{new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f)});
        itemBlockInfo.Add(7,new List<Vector2>{new Vector2(0.3125f,0f),new Vector2(0.3125f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.3125f,0f),new Vector2(0.3125f,0f)});
        itemBlockInfo.Add(8,new List<Vector2>{new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f)});
        itemBlockInfo.Add(9,new List<Vector2>{new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f)});
        isBlockInfoAdded=true;

    }
    public static void ReadJson(){
      if(platform==RuntimePlatform.WindowsPlayer||platform==RuntimePlatform.WindowsEditor){
        gameWorldDataPath="C:/";
      }else{
        gameWorldDataPath=Application.persistentDataPath;
      }
         
         if (!Directory.Exists(gameWorldDataPath+"unityMinecraftData")){
                Directory.CreateDirectory(gameWorldDataPath+"unityMinecraftData");
               
            }
          if(!Directory.Exists(gameWorldDataPath+"unityMinecraftData/GameData")){
                    Directory.CreateDirectory(gameWorldDataPath+"unityMinecraftData/GameData");
                }
       
        if(!File.Exists(gameWorldDataPath+"unityMinecraftData"+"/GameData/world.json")){
            File.Create(gameWorldDataPath+"unityMinecraftData"+"/GameData/world.json");
        }
       
        string[] worldData=File.ReadAllLines(gameWorldDataPath+"unityMinecraftData/GameData/world.json");
        List<WorldData> tmpList=new List<WorldData>();
        foreach(string s in worldData){
            WorldData tmp=JsonSerializer.Deserialize<WorldData>(s);
            tmpList.Add(tmp);
        }
        foreach(WorldData w in tmpList){
            chunkDataReadFromDisk.Add(new Vector2Int(w.posX,w.posZ),w);
        }
        isJsonReadFromDisk=true;
    }
    void Awake(){
        if(isBlockInfoAdded==false){
          AddBlockInfo();  
        }
        if(isJsonReadFromDisk==false){
            ReadJson();
        }
    }




    void ReInitData(){

       chunkPos=new Vector2Int((int)transform.position.x,(int)transform.position.z);
       isChunkPosInited=true;
       Chunks.Add(chunkPos,this);  

        if(chunkDataReadFromDisk.ContainsKey(chunkPos)){
            isSavedInDisk=true;
           // Debug.Log(chunkPos);
        }
        StartLoadChunk();
    }
    //strongload: simulate chunk mesh collider
    void StrongLoadChunk(){
        isStrongLoaded=true;
    }
    void StopChunkFromStrongSim(){
        if(meshCollider.sharedMesh!=null||meshColliderNS.sharedMesh!=null){
            meshCollider.sharedMesh=null;
            meshColliderNS.sharedMesh=null;
        }
        isStrongLoaded=false;
    }
    void OnDisable(){
        SaveSingleChunk();
        Chunks.Remove(chunkPos);
        map=new int[chunkWidth+2,chunkHeight+2,chunkWidth+2];
        chunkPos=new Vector2Int(0,0);
        isChunkPosInited=false;

        isSavedInDisk=false;
        isMapGenCompleted=false;
        isMeshBuildCompleted=false;
        isStrongLoaded=false;
        isChunkMapUpdated=false;
	//    meshCollider.sharedMesh= null;
	//    meshFilter.mesh=null;
      
	 //   meshColliderNS.sharedMesh=null;
	 //   meshFilterNS.mesh=null;
      //  chunkMesh=null;
     //   chunkNonSolidMesh=null;
        isModifiedInGame=false;
     //   isChunkPosInited=false;
    }


    void Start(){
        playerPos=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
         meshRenderer = GetComponent<MeshRenderer>();
	    meshCollider = GetComponent<MeshCollider>();
	    meshFilter = GetComponent<MeshFilter>();
        meshRendererNS = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
	    meshColliderNS = transform.GetChild(0).gameObject.GetComponent<MeshCollider>();
	    meshFilterNS = transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
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
    }



    public void StartLoadChunk(){
      //  Vector3 pos=transform.position;

   //     ThreadStart childref = new ThreadStart(() => InitMap(chunkPos));
      ///  Thread childThread=new Thread(childref);
     //   childThread.Start();
        
        StartCoroutine(BuildChunk());
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
    public void SaveSingleChunk(){
        if(!isChunkPosInited){
            return;
        }
        if(!isModifiedInGame){

            return;
        }
        if(chunkDataReadFromDisk.ContainsKey(chunkPos)){
            chunkDataReadFromDisk.Remove(chunkPos);
         int[,,] worldDataMap=map;
            WorldData wd=new WorldData();
            wd.map=worldDataMap;
            wd.posX=chunkPos.x;
            wd.posZ=chunkPos.y;
            chunkDataReadFromDisk.Add(chunkPos,wd);
        }else{
            int[,,] worldDataMap=map;
            WorldData wd=new WorldData();
            wd.map=worldDataMap;
            wd.posX=chunkPos.x;
            wd.posZ=chunkPos.y;
            chunkDataReadFromDisk.Add(chunkPos,wd);
        }
    }
    public static void SaveWorldData(){
        
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
        c.Value.SaveSingleChunk();
        }
       foreach(KeyValuePair<Vector2Int,WorldData> wd in chunkDataReadFromDisk){
        string tmpData=JsonSerializer.ToJsonString(wd.Value);
        File.AppendAllText(gameWorldDataPath+"unityMinecraftData/GameData/world.json",tmpData+"\n");
       }
        isWorldDataSaved=true;
    }


    
    void InitMap(Vector2Int pos){
        frontChunk=GetChunk(new Vector2Int(chunkPos.x,chunkPos.y+chunkWidth));
        backChunk=GetChunk(new Vector2Int(chunkPos.x,chunkPos.y-chunkWidth));
        leftChunk=GetChunk(new Vector2Int(chunkPos.x-chunkWidth,chunkPos.y));
        rightChunk=GetChunk(new Vector2Int(chunkPos.x+chunkWidth,chunkPos.y));
        List<Vector3> vertsNS = new List<Vector3>();
        List<Vector2> uvsNS = new List<Vector2>();
        List<int> trisNS = new List<int>();
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        if(isMapGenCompleted==true){
        GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS);
            return;
        }
        if(isSavedInDisk==true){
            if(isChunkPosInited==false){
                FreshGenMap(pos);
                 isMapGenCompleted=true;
                GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS);
                return;
            }else{
             map=chunkDataReadFromDisk[new Vector2Int((int)pos.x,(int)pos.y)].map;
            isMapGenCompleted=true;
            GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS);   
             return;
            }
            
         
        }
        FreshGenMap(pos);
        void FreshGenMap(Vector2Int pos){
            if(worldGenType==0){
        System.Random random=new System.Random(pos.x+pos.y);
        int treeCount=10;

        for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
              //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.y*0.01f+j*0.01f);
               float noiseValue=chunkSeaLevel+noiseGenerator.GetSimplex(pos.x+i,pos.y+j)*20f;
                for(int k=0;k<chunkHeight;k++){
                    if(noiseValue>k+3){
                     map[i,k,j]=1;   
                    }else if(noiseValue>k){

                        map[i,k,j]=3;
                    }else{
                         map[i,k,j]=0;
                    }
                    
                }
            }
        }


        for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
       
                for(int k=chunkHeight-1;k>=0;k--){
                
                     if(map[i,k,j]!=0&&k>=chunkSeaLevel){
                            map[i,k,j]=4;
                            break;
                    }
                  
                    if(map[i,k,j]==0&&map[i,k-1,j]!=0&&map[i,k-1,j]!=100&&k>chunkSeaLevel&&random.Next(100)>80){
                        map[i,k,j]=101;
                    }
                        if(k<chunkSeaLevel&&map[i,k,j]==0){
                                map[i,k,j]=100;
                        }
                       
                }
            }
        }
        
                for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
       
                for(int k=chunkHeight-1;k>=0;k--){
              
                    if(map[i,k,j]==0&&map[i,k-1,j]==4&&map[i,k-1,j]!=100&&k>chunkSeaLevel){
                    if(treeCount>0){
                            if(random.Next(100)>98){
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
                                    rightChunk.map[0,k+4,j]=9;
                                 rightChunk.map[0,k+3,j]=9;
                                  rightChunk.map[0,k+2,j]=9;
                                  rightChunk.isChunkMapUpdated=true;
                                }
                               }
                               if(i-1>=0){
                                map[i-1,k+4,j]=9;
                                map[i-1,k+3,j]=9;
                                map[i-1,k+2,j]=9;
                               }else{
                                if(leftChunk!=null){
                                      leftChunk.map[chunkWidth-1,k+4,j]=9;
                                 leftChunk.map[chunkWidth-1,k+3,j]=9;
                                  leftChunk.map[chunkWidth-1,k+2,j]=9;
                                  leftChunk.isChunkMapUpdated=true;
                                }
                               }
                               if(j+1<chunkWidth){
                                map[i,k+4,j+1]=9;
                                map[i,k+3,j+1]=9;
                                map[i,k+2,j+1]=9;
                               }else{
                                if(frontChunk!=null){
                                frontChunk.map[i,k+4,0]=9;
                                frontChunk.map[i,k+3,0]=9;
                                frontChunk.map[i,k+2,0]=9;
                                frontChunk.isChunkMapUpdated=true;
                                }
                               }
                               if(j-1>=0){
                                map[i,k+4,j-1]=9;
                                map[i,k+3,j-1]=9;
                                map[i,k+2,j-1]=9;
                               }else{
                                if(backChunk!=null){
                                backChunk.map[i,k+4,chunkWidth-1]=9;
                                backChunk.map[i,k+3,chunkWidth-1]=9;
                                backChunk.map[i,k+2,chunkWidth-1]=9;
                                backChunk.isChunkMapUpdated=true;
                                }
                               }
                               treeCount--;
                            }
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
        
        }else if(worldGenType==1){
            for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
              //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.z*0.01f+j*0.01f);
                for(int k=0;k<chunkHeight/4;k++){
                  
                    map[i,k,j]=1;
                    
                }
            }
        }
        }
        isMapGenCompleted=true;
        }
        

        GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS);
    }



    public void GenerateMesh(List<Vector3> verts, List<Vector2> uvs, List<int> tris, List<Vector3> vertsNS, List<Vector2> uvsNS, List<int> trisNS){
       // Thread.Sleep(100);
         TmpCheckFace tmp=new TmpCheckFace(CheckNeedBuildFace);
        TmpBuildFace TmpBuildFace=new TmpBuildFace(BuildFace);
        for (int x = 0; x < chunkWidth; x++){
            for (int y = 0; y < chunkHeight; y++){
                for (int z = 0; z < chunkWidth; z++){
                   //     BuildBlock(x, y, z, verts, uvs, tris, vertsNS, uvsNS, trisNS);
        if (this.map[x, y, z] == 0) continue;
        int typeid = this.map[x, y, z];
        if(0<typeid&&typeid<100){
    //Left
        if (tmp(x - 1, y, z))
          TmpBuildFace(typeid, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris,0);
        //Right
        if (tmp(x + 1, y, z))
         TmpBuildFace(typeid, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris,1);

        //Bottom
        if (tmp(x, y - 1, z))
         TmpBuildFace(typeid, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris,2);
        //Top
        if (tmp(x, y + 1, z))
        TmpBuildFace(typeid, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris,3);

        //Back
        if (tmp(x, y, z - 1))
        TmpBuildFace(typeid, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris,4);
        //Front
        if (tmp(x, y, z + 1))
        TmpBuildFace(typeid, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris,5); 



        }else if(100<=typeid&&typeid<200){

    if(typeid==100){



        //water
        //left
        if (tmp(x-1,y,z)&&GetBlockType(x-1,y,z)!=100){
            if(GetBlockType(x,y+1,z)!=100){
            TmpBuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, false, vertsNS, uvsNS, trisNS,0); 




            }else{
     TmpBuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.forward, false, vertsNS, uvsNS, trisNS,0); 





      
            }
           
        }
            
        //Right
        if (tmp(x + 1, y, z)&&GetBlockType(x+1,y,z)!=100){
                if(GetBlockType(x,y+1,z)!=100){
     TmpBuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, true, vertsNS, uvsNS, trisNS,1);



                }else{
         TmpBuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,1f,0f), Vector3.forward, true, vertsNS, uvsNS, trisNS,1);



            }  

        }

            

        //Bottom
        if (tmp(x, y - 1, z)&&GetBlockType(x,y-1,z)!=100){
       TmpBuildFace(typeid, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, vertsNS, uvsNS, trisNS,2);




        }
            
        //Top
        if (tmp(x, y + 1, z)&&GetBlockType(x,y+1,z)!=100){
        TmpBuildFace(typeid, new Vector3(x, y + 0.8f, z), Vector3.forward, Vector3.right, true, vertsNS, uvsNS, trisNS,3);




        }
           



        //Back
        if (tmp(x, y, z - 1)&&GetBlockType(x,y,z-1)!=100){
            if(GetBlockType(x,y+1,z)!=100){
            TmpBuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.right, true, vertsNS, uvsNS, trisNS,4);



       
            }else{
            TmpBuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.right, true, vertsNS, uvsNS, trisNS,4);






 
            }
            
        }

            
        //Front
        if (tmp(x, y, z + 1)&&GetBlockType(x,y,z+1)!=100){
            if(GetBlockType(x,y+1,z)!=100){
            TmpBuildFace(typeid, new Vector3(x, y, z + 1), new Vector3(0f,0.8f,0f), Vector3.right, false, vertsNS, uvsNS, trisNS,5) ;


            }else{
            TmpBuildFace(typeid, new Vector3(x, y, z+1), new Vector3(0f,1f,0f), Vector3.right, false, vertsNS, uvsNS, trisNS,4);

            }
             
        }   
    }
            
        if(typeid>=101&&typeid<150){
            Vector3 randomCrossModelOffset=new Vector3(0f,0f,0f);
            TmpBuildFace(typeid, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, false, vertsNS, uvsNS, trisNS,0);
            


            TmpBuildFace(typeid, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, true, vertsNS, uvsNS, trisNS,0);



          TmpBuildFace(typeid, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, false, vertsNS, uvsNS, trisNS,0);



         TmpBuildFace(typeid, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, true, vertsNS, uvsNS, trisNS,0);


        }

                        
                    }
                }
            }
        }
        opqVerts=verts.ToArray();
        opqUVs=uvs.ToArray();
        opqTris=tris.ToArray();
        NSVerts=vertsNS.ToArray();
        NSUVs=uvsNS.ToArray();
        NSTris=trisNS.ToArray();
        
        isMeshBuildCompleted=true;
    }





    public IEnumerator BuildChunk(){
        isMeshBuildCompleted=false;

        ThreadStart childref = new ThreadStart(() => InitMap(chunkPos));
        Thread childThread=new Thread(childref);
        childThread.Start();



        yield return new WaitUntil(()=>isMapGenCompleted==true&&isMeshBuildCompleted==true);
   // if(!isMapGenCompleted){
    ///    yield return 10;
   // }


        chunkMesh=new Mesh();
        chunkNonSolidMesh=new Mesh();

     
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
        NativeArray<Vector3> vertsNative=new NativeArray<Vector3>(opqVerts,Allocator.TempJob);
        NativeArray<int> trisNative=new NativeArray<int>(opqTris,Allocator.TempJob);
        NativeArray<Vector2> uvsNative=new NativeArray<Vector2>(opqUVs,Allocator.TempJob);
        MeshBuildJob mbj=new MeshBuildJob{verts=vertsNative,tris=trisNative,vertLen=opqVerts.Length,trisLen=opqTris.Length,uvs=uvsNative,dataArray=Mesh.AllocateWritableMeshData(1)};
        mbj.Schedule().Complete();
        NativeArray<int> a=new NativeArray<int>(2,Allocator.TempJob);
        Mesh.ApplyAndDisposeWritableMeshData(mbj.dataArray,chunkMesh);
     //   chunkMesh.vertices = opqVerts;
    //    chunkMesh.uv = opqUVs;
     //  chunkMesh.triangles = opqTris;

        chunkMesh.RecalculateBounds();
        chunkMesh.RecalculateNormals();
        chunkNonSolidMesh.vertices = NSVerts;
        chunkNonSolidMesh.uv = NSUVs;
        chunkNonSolidMesh.triangles = NSTris;
        chunkNonSolidMesh.RecalculateNormals();
        
   //     var job=new BakeJob();
     //   job.meshes.Add(chunkMesh.GetInstanceID());
   //     job.Schedule();
        meshFilter.mesh = chunkMesh;
        
        
        
        meshFilterNS.mesh = chunkNonSolidMesh;
        a[0]=chunkMesh.GetInstanceID();
        a[1]=chunkNonSolidMesh.GetInstanceID();
        BakeJob job=new BakeJob();
        job.meshes=a;
        JobHandle handle = job.Schedule(a.Length, 1);
        handle.Complete();
        a.Dispose();
          meshCollider.sharedMesh = chunkMesh;
        meshColliderNS.sharedMesh = chunkNonSolidMesh;
        isChunkMapUpdated=false;
        yield break;
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
        if (CheckNeedBuildFace(x-1,y,z)&&GetBlockType(x-1,y,z)!=100){
            if(GetBlockType(x,y+1,z)!=100){
                    BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, false, vertsNS, uvsNS, trisNS,0); 
            }else{
                BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.forward, false, vertsNS, uvsNS, trisNS,0); 
            }
           
        }
            
        //Right
        if (CheckNeedBuildFace(x + 1, y, z)&&GetBlockType(x+1,y,z)!=100){
                if(GetBlockType(x,y+1,z)!=100){
                    BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, true, vertsNS, uvsNS, trisNS,1);
                }else{
                        BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,1f,0f), Vector3.forward, true, vertsNS, uvsNS, trisNS,1);
            }  

        }

            

        //Bottom
        if (CheckNeedBuildFace(x, y - 1, z)&&GetBlockType(x,y-1,z)!=100)
            BuildFace(typeid, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, vertsNS, uvsNS, trisNS,2);
        //Top
        if (CheckNeedBuildFace(x, y + 1, z)&&GetBlockType(x,y+1,z)!=100)
            BuildFace(typeid, new Vector3(x, y + 0.8f, z), Vector3.forward, Vector3.right, true, vertsNS, uvsNS, trisNS,3);



        //Back
        if (CheckNeedBuildFace(x, y, z - 1)&&GetBlockType(x,y,z-1)!=100){
            if(GetBlockType(x,y+1,z)!=100){
                BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.right, true, vertsNS, uvsNS, trisNS,4);
            }else{
                BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.right, true, vertsNS, uvsNS, trisNS,4);
            }
            
        }

            
        //Front
        if (CheckNeedBuildFace(x, y, z + 1)&&GetBlockType(x,y,z+1)!=100){
            if(GetBlockType(x,y+1,z)!=100){
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
        var type = GetBlockType(x, y, z);
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
            default:
                return false;
        }
    }
    public int GenerateBlockType(int x, int y, int z,Vector2Int pos){
        float noiseValue=chunkSeaLevel+noiseGenerator.GetSimplex(pos.x+x,pos.y+z)*20f;
        if(noiseValue>y){
            return 1;
        }else{
            if(y<chunkSeaLevel){
                     return 100;       
            }
                return 0;
        }
    }
    public int GetBlockType(int x, int y, int z){
        if (y < 0 || y > chunkHeight - 1)
        {
            return 0;
        }
        
        if ((x < 0) || (z < 0) || (x >= chunkWidth) || (z >= chunkWidth))
        {
            if(x>=chunkWidth){
                if(rightChunk!=null){
                return rightChunk.map[0,y,z];    
                }else return GenerateBlockType(x,y,z,chunkPos);
                
            }else if(z>=chunkWidth){
                if(frontChunk!=null){
                return frontChunk.map[x,y,0];
                 }else return GenerateBlockType(x,y,z,chunkPos);
            }else if(x<0){
                if(leftChunk!=null){
                return leftChunk.map[chunkWidth-1,y,z];
                 }else return GenerateBlockType(x,y,z,chunkPos);
            }else if(z<0){
                if(backChunk!=null){
                return backChunk.map[x,y,chunkWidth-1];
                 }else return GenerateBlockType(x,y,z,chunkPos);
            }
           
        }
        return map[x, y, z];
    }



     void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int side){
        int index = verts.Count;
    
        verts.Add (corner);
        verts.Add (corner + up);
        verts.Add (corner + up + right);
        verts.Add (corner + right);

        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        uvCorner=blockInfo[typeid][side];
        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
    
        if (reversed)
            {
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 0);
            }
            else
            {
            tris.Add(index + 1);
            tris.Add(index + 0);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 0);
        }
    
    }

    public static Chunk GetChunk(Vector2Int chunkPos){
        if(Chunks.ContainsKey(chunkPos)){
            Chunk tmp=Chunks[chunkPos];
            return tmp;
        }else{
            return null;
        }
        
    }

    public static Vector2Int Vec3ToChunkPos(Vector3 pos){
        Vector3 tmp=pos;
        tmp.x = Mathf.Floor(tmp.x / (float)chunkWidth) * chunkWidth;
        tmp.z = Mathf.Floor(tmp.z / (float)chunkWidth) * chunkWidth;
        Vector2Int value=new Vector2Int((int)tmp.x,(int)tmp.z);
        return value;
    }

    public static int FloatToInt(float f){
        if(f>=0){
            return (int)f;
        }else{
            return (int)f-1;
        }
    }
    public static Vector3Int Vec3ToBlockPos(Vector3 pos){
        Vector3Int intPos=new Vector3Int(FloatToInt(pos.x),FloatToInt(pos.y),FloatToInt(pos.z));
        return intPos;
    }
    public static void SetBlock(Vector3 pos,int blockID){

        Vector3Int intPos=new Vector3Int(FloatToInt(pos.x),FloatToInt(pos.y),FloatToInt(pos.z));
        Chunk chunkNeededUpdate=Chunk.GetChunk(Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos=intPos-new Vector3Int(FloatToInt(chunkNeededUpdate.transform.position.x),FloatToInt(chunkNeededUpdate.transform.position.y),FloatToInt(chunkNeededUpdate.transform.position.z));
        chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z]=blockID;
        chunkNeededUpdate.isChunkMapUpdated=true;
        if(chunkNeededUpdate.frontChunk!=null){
           chunkNeededUpdate.frontChunk.isChunkMapUpdated=true;
        }
        if(chunkNeededUpdate.backChunk!=null){
            chunkNeededUpdate.backChunk.isChunkMapUpdated=true;
        }
        if(chunkNeededUpdate.leftChunk!=null){
            chunkNeededUpdate.leftChunk.isChunkMapUpdated=true;
        }
        if(chunkNeededUpdate.rightChunk!=null){
            chunkNeededUpdate.rightChunk.isChunkMapUpdated=true;
        }
    }
    public static void SetBlockByHand(Vector3 pos,int blockID){

        Vector3Int intPos=new Vector3Int(FloatToInt(pos.x),FloatToInt(pos.y),FloatToInt(pos.z));
        Chunk chunkNeededUpdate=Chunk.GetChunk(Vec3ToChunkPos(pos));

        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(chunkNeededUpdate.transform.position);
        chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z]=blockID;
        chunkNeededUpdate.isChunkMapUpdated=true;
        if(chunkNeededUpdate.frontChunk!=null){
           chunkNeededUpdate.frontChunk.isChunkMapUpdated=true;
        }
        if(chunkNeededUpdate.backChunk!=null){
            chunkNeededUpdate.backChunk.isChunkMapUpdated=true;
        }
        if(chunkNeededUpdate.leftChunk!=null){
            chunkNeededUpdate.leftChunk.isChunkMapUpdated=true;
        }
        if(chunkNeededUpdate.rightChunk!=null){
            chunkNeededUpdate.rightChunk.isChunkMapUpdated=true;
        }
    }
    public static int GetChunkLandingPoint(float x, float z){
       Vector2Int intPos=new Vector2Int(FloatToInt(x),FloatToInt(z));

        Chunk locChunk=Chunk.GetChunk(Vec3ToChunkPos(new Vector3(x,0f,z)));
        if(locChunk==null){
            return 100;
        }
        Vector2Int chunkSpacePos=intPos-locChunk.chunkPos;
        for(int i=chunkHeight-1;i>=0;i--){
            if(locChunk.map[chunkSpacePos.x,i-1,chunkSpacePos.y]!=0){
                return i;
            }
        }
        return 100;
    }
    public static int GetBlock(Vector3 pos){
        Vector3Int intPos=new Vector3Int(FloatToInt(pos.x),FloatToInt(pos.y),FloatToInt(pos.z));
        Chunk chunkNeededUpdate=Chunk.GetChunk(Vec3ToChunkPos(pos));
        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(chunkNeededUpdate.transform.position);
        return chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z];
    }
    void Update(){
        if(isChunkMapUpdated==true){
            isModifiedInGame=true;
            StartCoroutine(BuildChunk());
           //InitMap(chunkPos);
            isChunkMapUpdated=false;
        }
      
    }
    void FixedUpdate(){
         TryReleaseChunk();
    }
    void TryReleaseChunk(){
        if(Mathf.Abs(chunkPos.x-playerPos.position.x)>PlayerMove.viewRange+Chunk.chunkWidth+3||Mathf.Abs(chunkPos.y-playerPos.position.z)>PlayerMove.viewRange+Chunk.chunkWidth+3){
            Chunk.Chunks.Remove(chunkPos);
            ObjectPools.chunkPool.Remove(gameObject);
           
        }
    }
    
    public int updateCount=0;
    public bool BFSIsWorking=false;
    public bool[,,] mapIsSearched;
    public void BFSInit(int x,int y,int z, int ignoreSide,int GainedUpdateCount){
        updateCount=GainedUpdateCount;
        mapIsSearched=new bool[chunkWidth+2,chunkHeight+2,chunkWidth+2];
        BFSIsWorking=true;
        StartCoroutine(BFSMapUpdate(x,y,z,ignoreSide));
    }
    IEnumerator BFSMapUpdate(int x,int y,int z, int ignoreSide){
        //left right bottom top back front
        //left x-1 right x+1 top y+1 bottom y-1 back z-1 front z+1
        yield return new WaitForSeconds(0.2f);
        if(!BFSIsWorking){
            yield break;
        }
        if(updateCount>256){
            BFSIsWorking=false;
            yield break;
        }
        mapIsSearched[x,y,z]=true;
        SetBlock(new Vector3(transform.position.x+x,y,transform.position.z+z),0);
        updateCount++;
        if(!(ignoreSide==0)&&x-1>=0){
            if(!mapIsSearched[x-1,y,z]&&map[x-1,y,z]!=0)
           StartCoroutine(BFSMapUpdate(x-1,y,z,ignoreSide));
        }else if(x-1<0){
            if(leftChunk!=null){
                leftChunk.BFSInit(chunkWidth-1,y,z,ignoreSide,updateCount);
            }
        }
        if(!(ignoreSide==1)&&x+1<chunkWidth){
             if(!mapIsSearched[x+1,y,z]&&map[x+1,y,z]!=0)
            StartCoroutine(BFSMapUpdate(x+1,y,z,ignoreSide));
        }else if(x+1>=chunkWidth){
            if(rightChunk!=null){
                rightChunk.BFSInit(0,y,z,ignoreSide,updateCount);
            }
        }
        if(!(ignoreSide==2)&&y-1>=0){
             if(!mapIsSearched[x,y-1,z]&&map[x,y-1,z]!=0)
            StartCoroutine(BFSMapUpdate(x,y-1,z,ignoreSide));
        }
        if(!(ignoreSide==3)&&y+1<chunkHeight){
             if(!mapIsSearched[x,y+1,z]&&map[x,y+1,z]!=0)
            StartCoroutine(BFSMapUpdate(x,y+1,z,ignoreSide));
        }
        if(!(ignoreSide==4)&&z-1>=0){
             if(!mapIsSearched[x,y,z-1]&&map[x,y,z-1]!=0)
            StartCoroutine(BFSMapUpdate(x,y,z-1,ignoreSide));
        }else if(z-1<0){
            if(backChunk!=null){
                backChunk.BFSInit(x,y,chunkWidth-1,ignoreSide,updateCount);
            }
        }
        if(!(ignoreSide==5)&&z+1<chunkWidth){
             if(!mapIsSearched[x,y,z+1]&&map[x,y,z+1]!=0)
            StartCoroutine(BFSMapUpdate(x,y,z+1,ignoreSide));
        }else if(z+1>=chunkWidth){
            if(frontChunk!=null){
                frontChunk.BFSInit(x,y,0,ignoreSide,updateCount);
            }
        }
        
    }
 //   void UpdatePlayerDistance(){
//        playerDistance = (chunkPos - new Vector2(playerPos.position.x,playerPos.position.z)).sqrMagnitude;
//
  //  }
}
