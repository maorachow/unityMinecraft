using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using LitJson;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.IO;
public class WorldData{
    public int posX;
    public int posZ;
    public int[] map;
    
}
public class Chunk : MonoBehaviour
{   

    //public enum BlockType
  //  {
    //    None = 0,
  //      Stone = 1,
   //     Grass = 2,
    //    Dirt=3,
  //  }
  //0None 1Stone 2Grass 3Dirt 4Side grass block 5Bedrock 6WoodX 7WoodY 8WoodZ 9Leaves
  //100Water 101Grass
  //200Leaves
  //0-99solid blocks
  //100-199no hitbox blocks
  //200-299hitbox nonsolid blocks
    public delegate bool TmpCheckFace(int x,int y,int z);
    public delegate void TmpBuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int side);
    public static bool isBlockInfoAdded=false;
    public static bool isJsonReadFromDisk=false;
    public static bool isWorldDataSaved=false;
    public bool isMapGenCompleted=false;
    public bool isMeshBuildCompleted=false;
    public bool isChunkMapUpdated=false;
    public bool isSavedInDisk=false;
    public static Dictionary<int,List<Vector2>> blockInfo=new Dictionary<int,List<Vector2>>();
    public Mesh chunkMesh;
    public Mesh chunkHitboxNonSolidMesh;
    public Mesh chunkNonSolidMesh;
    public static int worldGenType=1;
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
        isBlockInfoAdded=true;

    }
    public static void ReadJson(){
        string[] worldData=File.ReadAllLines(Application.dataPath+"/GameData/world.json");
        List<WorldData> tmpList=new List<WorldData>();
        foreach(string s in worldData){
            WorldData tmp=JsonMapper.ToObject<WorldData>(s);
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
    void Start(){
        Chunks.Add(new Vector2Int((int)transform.position.x,(int)transform.position.z),this); 
        if(chunkDataReadFromDisk.ContainsKey(new Vector2Int((int)transform.position.x,(int)transform.position.z))){
            isSavedInDisk=true;
        }
        meshRenderer = GetComponent<MeshRenderer>();
	    meshCollider = GetComponent<MeshCollider>();
	    meshFilter = GetComponent<MeshFilter>();
        meshRendererNS = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
	    meshColliderNS = transform.GetChild(0).gameObject.GetComponent<MeshCollider>();
	    meshFilterNS = transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
        Vector3 pos=transform.position;
        ThreadStart childref = new ThreadStart(() => InitMap(pos));
        Thread childThread=new Thread(childref);
        childThread.Start();

         StartCoroutine(BuildChunk());
         //StartCoroutine(BuildChunk());
      //  InitMap(pos);
    
     
    }
    public static int[,,] WorldDataTo3DMap(WorldData wd){
       //x->y->z
        Debug.Log(wd.map[3]);
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

    public static int[] ThreeDMapToWorldData(int[,,] map){
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
        Debug.Log(returnValue);
        return returnValue;
    }

    public static void SaveWorldData(){
        
        FileStream fs;
             if (File.Exists(Application.dataPath+"/GameData/world.json"))
             {
                 fs = new FileStream(Application.dataPath+"/GameData/world.json", FileMode.Truncate, FileAccess.Write);//Truncate模式打开文件可以清空。
             }
             else
             {
                 fs = new FileStream(Application.dataPath+"/GameData/world.json", FileMode.Create, FileAccess.Write);
            }
             fs.Close();
        foreach(KeyValuePair<Vector2Int,Chunk> c in Chunks){
             int[] worldDataMap=ThreeDMapToWorldData(c.Value.map);
        int x=(int)c.Value.transform.position.x;
        int z=(int)c.Value.transform.position.z;
        WorldData wd=new WorldData();
        wd.map=worldDataMap;
        wd.posX=x;
        wd.posZ=z;
        string tmpData=JsonMapper.ToJson(wd);
        File.AppendAllText(Application.dataPath+"/GameData/world.json",tmpData+"\n");
        }
       
        isWorldDataSaved=true;
    }


    
    void InitMap(Vector3 pos){

        if(isSavedInDisk==true){
            map=WorldDataTo3DMap(chunkDataReadFromDisk[new Vector2Int((int)pos.x,(int)pos.z)]);
            isMapGenCompleted=true;
            return;
        }
        if(worldGenType==0){
        System.Random random=new System.Random();
        int treeCount=10;
        for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
                float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.z*0.01f+j*0.01f);
                for(int k=0;k<chunkHeight;k++){
                    if(noiseValue>k){
                     map[i,k,j]=(int)3;   
                    }else{
                        map[i,k,j]=0;
                    }
                    
                }
            }
        }


        for(int i=0;i<chunkWidth;i++){
            for(int j=0;j<chunkWidth;j++){
       
                for(int k=chunkHeight-1;k>=0;k--){
                
                     if(map[i,k,j]!=0&&k>chunkSeaLevel){
                            map[i,k,j]=4;
                            break;
                    }
                  
                    if(map[i,k,j]==0&&map[i,k-1,j]!=0&&map[i,k-1,j]!=100&&k>chunkSeaLevel&&random.Next(100)>70){
                        map[i,k,j]=101;
                    }
                        if(k<=chunkSeaLevel&&map[i,k,j]==0){
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
                            if(random.Next(100)>97){
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
                               }
                               if(i-1>=0){
                                map[i-1,k+4,j]=9;
                                map[i-1,k+3,j]=9;
                                map[i-1,k+2,j]=9;
                               }
                               if(j+1<chunkWidth){
                                map[i,k+4,j+1]=9;
                                map[i,k+3,j+1]=9;
                                map[i,k+2,j+1]=9;
                               }
                               if(j-1>=0){
                                map[i,k+4,j-1]=9;
                                map[i,k+3,j-1]=9;
                                map[i,k+2,j-1]=9;
                               }
                               treeCount--;
                            }
                        }
                    }
                       
                }
            }
        }}else if(worldGenType==1){
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



    public void GenerateMesh(List<Vector3> verts, List<Vector2> uvs, List<int> tris, List<Vector3> vertsNS, List<Vector2> uvsNS, List<int> trisNS){
        
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
            //        BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, false, vertsNS, uvsNS, trisNS,0); 




        var faceTypeID=typeid;
        var corner=new Vector3(x, y, z);
        var up=new Vector3(0f,0.8f,0f);
        var right=Vector3.forward;
        bool reversed=false;
        var side=0;
        int index = vertsNS.Count;
        vertsNS.Add (corner);
        vertsNS.Add (corner + up);
        vertsNS.Add (corner + up + right);
        vertsNS.Add (corner + right);
        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);
        uvCorner=blockInfo[typeid][side];
        uvsNS.Add(uvCorner);
        uvsNS.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        if (reversed){trisNS.Add(index + 0);trisNS.Add(index + 1);trisNS.Add(index + 2);trisNS.Add(index + 2);trisNS.Add(index + 3);trisNS.Add(index + 0);}else{trisNS.Add(index + 1);trisNS.Add(index + 0);trisNS.Add(index + 2);trisNS.Add(index + 3); trisNS.Add(index + 2);trisNS.Add(index + 0);}
            }else{
        //        BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.forward, false, vertsNS, uvsNS, trisNS,0); 





        var faceTypeID=typeid;
        var corner=new Vector3(x, y, z);
        var up=new Vector3(0f,1f,0f);
        var right=Vector3.forward;
        bool reversed=false;
        var side=0;
        int index = vertsNS.Count;
        vertsNS.Add (corner);
        vertsNS.Add (corner + up);
        vertsNS.Add (corner + up + right);
        vertsNS.Add (corner + right);
        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);
        uvCorner=blockInfo[typeid][side];
        uvsNS.Add(uvCorner);
        uvsNS.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        if (reversed){trisNS.Add(index + 0);trisNS.Add(index + 1);trisNS.Add(index + 2);trisNS.Add(index + 2);trisNS.Add(index + 3);trisNS.Add(index + 0);}else{trisNS.Add(index + 1);trisNS.Add(index + 0);trisNS.Add(index + 2);trisNS.Add(index + 3); trisNS.Add(index + 2);trisNS.Add(index + 0);}
            }
           
        }
            
        //Right
        if (tmp(x + 1, y, z)&&GetBlockType(x+1,y,z)!=100){
                if(GetBlockType(x,y+1,z)!=100){
      //              BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, true, vertsNS, uvsNS, trisNS,1);




        var faceTypeID=typeid;
        var corner=new Vector3(x + 1, y, z);
        var up= new Vector3(0f,0.8f,0f);
        var right=Vector3.forward;
        bool reversed=true;
        var side=0;
        int index = vertsNS.Count;
        vertsNS.Add (corner);
        vertsNS.Add (corner + up);
        vertsNS.Add (corner + up + right);
        vertsNS.Add (corner + right);
        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);
        uvCorner=blockInfo[typeid][side];
        uvsNS.Add(uvCorner);
        uvsNS.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        if (reversed){trisNS.Add(index + 0);trisNS.Add(index + 1);trisNS.Add(index + 2);trisNS.Add(index + 2);trisNS.Add(index + 3);trisNS.Add(index + 0);}else{trisNS.Add(index + 1);trisNS.Add(index + 0);trisNS.Add(index + 2);trisNS.Add(index + 3); trisNS.Add(index + 2);trisNS.Add(index + 0);}
                }else{
                //        BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,1f,0f), Vector3.forward, true, vertsNS, uvsNS, trisNS,1);





        var faceTypeID=typeid;
        var corner=new Vector3(x + 1, y, z);
        var up= new Vector3(0f,1f,0f);
        var right=Vector3.forward;
        bool reversed=true;
        var side=0;
        int index = vertsNS.Count;
        vertsNS.Add (corner);
        vertsNS.Add (corner + up);
        vertsNS.Add (corner + up + right);
        vertsNS.Add (corner + right);
        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);
        uvCorner=blockInfo[typeid][side];
        uvsNS.Add(uvCorner);
        uvsNS.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        if (reversed){trisNS.Add(index + 0);trisNS.Add(index + 1);trisNS.Add(index + 2);trisNS.Add(index + 2);trisNS.Add(index + 3);trisNS.Add(index + 0);}else{trisNS.Add(index + 1);trisNS.Add(index + 0);trisNS.Add(index + 2);trisNS.Add(index + 3); trisNS.Add(index + 2);trisNS.Add(index + 0);}
            }  

        }

            

        //Bottom
        if (tmp(x, y - 1, z)&&GetBlockType(x,y-1,z)!=100){
        //    BuildFace(typeid, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, vertsNS, uvsNS, trisNS,2);




            var faceTypeID=typeid;
        var corner=new Vector3(x, y, z);
        var up= Vector3.forward;
        var right=Vector3.right;
        bool reversed=false;
        var side=0;
        int index = vertsNS.Count;
        vertsNS.Add (corner);
        vertsNS.Add (corner + up);
        vertsNS.Add (corner + up + right);
        vertsNS.Add (corner + right);
        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);
        uvCorner=blockInfo[typeid][side];
        uvsNS.Add(uvCorner);
        uvsNS.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        if (reversed){trisNS.Add(index + 0);trisNS.Add(index + 1);trisNS.Add(index + 2);trisNS.Add(index + 2);trisNS.Add(index + 3);trisNS.Add(index + 0);}else{trisNS.Add(index + 1);trisNS.Add(index + 0);trisNS.Add(index + 2);trisNS.Add(index + 3); trisNS.Add(index + 2);trisNS.Add(index + 0);}
        }
            
        //Top
        if (tmp(x, y + 1, z)&&GetBlockType(x,y+1,z)!=100){
           //  BuildFace(typeid, new Vector3(x, y + 0.8f, z), Vector3.forward, Vector3.right, true, vertsNS, uvsNS, trisNS,3);




             var faceTypeID=typeid;
        var corner=new Vector3(x, y + 0.8f, z);
        var up= Vector3.forward;
        var right=Vector3.right;
        bool reversed=true;
        var side=0;
        int index = vertsNS.Count;
        vertsNS.Add (corner);
        vertsNS.Add (corner + up);
        vertsNS.Add (corner + up + right);
        vertsNS.Add (corner + right);
        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);
        uvCorner=blockInfo[typeid][side];
        uvsNS.Add(uvCorner);
        uvsNS.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        if (reversed){trisNS.Add(index + 0);trisNS.Add(index + 1);trisNS.Add(index + 2);trisNS.Add(index + 2);trisNS.Add(index + 3);trisNS.Add(index + 0);}else{trisNS.Add(index + 1);trisNS.Add(index + 0);trisNS.Add(index + 2);trisNS.Add(index + 3); tris.Add(index + 2);tris.Add(index + 0);}
        }
           



        //Back
        if (tmp(x, y, z - 1)&&GetBlockType(x,y,z-1)!=100){
            if(GetBlockType(x,y+1,z)!=100){
               // BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.right, true, vertsNS, uvsNS, trisNS,4);



               
               var faceTypeID=typeid;
        var corner=new Vector3(x, y, z);
        var up= new Vector3(0f,0.8f,0f);
        var right=Vector3.right;
        bool reversed=true;
        var side=0;
        int index = vertsNS.Count;
        vertsNS.Add (corner);
        vertsNS.Add (corner + up);
        vertsNS.Add (corner + up + right);
        vertsNS.Add (corner + right);
        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);
        uvCorner=blockInfo[typeid][side];
        uvsNS.Add(uvCorner);
        uvsNS.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        if (reversed){trisNS.Add(index + 0);trisNS.Add(index + 1);trisNS.Add(index + 2);trisNS.Add(index + 2);trisNS.Add(index + 3);trisNS.Add(index + 0);}else{trisNS.Add(index + 1);trisNS.Add(index + 0);trisNS.Add(index + 2);trisNS.Add(index + 3); trisNS.Add(index + 2);trisNS.Add(index + 0);}
            }else{
              //  BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.right, true, vertsNS, uvsNS, trisNS,4);






            var faceTypeID=typeid;
        var corner=new Vector3(x, y, z);
        var up= new Vector3(0f,1f,0f);
        var right=Vector3.right;
        bool reversed=true;
        var side=0;
        int index = vertsNS.Count;
        vertsNS.Add (corner);
        vertsNS.Add (corner + up);
        vertsNS.Add (corner + up + right);
        vertsNS.Add (corner + right);
        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);
        uvCorner=blockInfo[typeid][side];
        uvsNS.Add(uvCorner);
        uvsNS.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        if (reversed){trisNS.Add(index + 0);trisNS.Add(index + 1);trisNS.Add(index + 2);trisNS.Add(index + 2);trisNS.Add(index + 3);trisNS.Add(index + 0);}else{trisNS.Add(index + 1);trisNS.Add(index + 0);trisNS.Add(index + 2);trisNS.Add(index + 3); trisNS.Add(index + 2);trisNS.Add(index + 0);}
            }
            
        }

            
        //Front
        if (tmp(x, y, z + 1)&&GetBlockType(x,y,z+1)!=100){
            if(GetBlockType(x,y+1,z)!=100){
             //   BuildFace(typeid, new Vector3(x, y, z + 1), new Vector3(0f,0.8f,0f), Vector3.right, false, vertsNS, uvsNS, trisNS,5); 




             var faceTypeID=typeid;
        var corner=new Vector3(x, y, z + 1);
        var up= new Vector3(0f,0.8f,0f);
        var right=Vector3.right;
        bool reversed=false;
        var side=0;
        int index = vertsNS.Count;
        vertsNS.Add (corner);
        vertsNS.Add (corner + up);
        vertsNS.Add (corner + up + right);
        vertsNS.Add (corner + right);
        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);
        uvCorner=blockInfo[typeid][side];
        uvsNS.Add(uvCorner);
        uvsNS.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        if (reversed){trisNS.Add(index + 0);trisNS.Add(index + 1);trisNS.Add(index + 2);trisNS.Add(index + 2);trisNS.Add(index + 3);trisNS.Add(index + 0);}else{trisNS.Add(index + 1);trisNS.Add(index + 0);trisNS.Add(index + 2);trisNS.Add(index + 3); trisNS.Add(index + 2);trisNS.Add(index + 0);}
            }else{
               // BuildFace(typeid, new Vector3(x, y, z+1), new Vector3(0f,1f,0f), Vector3.right, false, vertsNS, uvsNS, trisNS,4);




               var faceTypeID=typeid;
        var corner=new Vector3(x, y, z+1);
        var up= new Vector3(0f,1f,0f);
        var right= Vector3.right;
        bool reversed=false;
        var side=0;
        int index = vertsNS.Count;
        vertsNS.Add (corner);
        vertsNS.Add (corner + up);
        vertsNS.Add (corner + up + right);
        vertsNS.Add (corner + right);
        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);
        uvCorner=blockInfo[typeid][side];
        uvsNS.Add(uvCorner);
        uvsNS.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvsNS.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        if (reversed){trisNS.Add(index + 0);trisNS.Add(index + 1);trisNS.Add(index + 2);trisNS.Add(index + 2);trisNS.Add(index + 3);trisNS.Add(index + 0);}else{trisNS.Add(index + 1);trisNS.Add(index + 0);trisNS.Add(index + 2);trisNS.Add(index + 3); trisNS.Add(index + 2);trisNS.Add(index + 0);}
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
        isMeshBuildCompleted=true;
    }





    public IEnumerator BuildChunk(){
        isMeshBuildCompleted=false;
        yield return new WaitUntil(()=>isMapGenCompleted==true);
   // if(!isMapGenCompleted){
    ///    yield return 10;
   // }
        chunkMesh=new Mesh();
        chunkNonSolidMesh=new Mesh();
        
        List<Vector3> vertsNS = new List<Vector3>();
        List<Vector2> uvsNS = new List<Vector2>();
        List<int> trisNS = new List<int>();
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        frontChunk=GetChunk(new Vector2Int((int)transform.position.x,(int)transform.position.z+chunkWidth));
        backChunk=GetChunk(new Vector2Int((int)transform.position.x,(int)transform.position.z-chunkWidth));
        leftChunk=GetChunk(new Vector2Int((int)transform.position.x-chunkWidth,(int)transform.position.z));
        rightChunk=GetChunk(new Vector2Int((int)transform.position.x+chunkWidth,(int)transform.position.z));

      //  yield return new WaitUntil(()=>frontChunk!=null&&backChunk!=null&&leftChunk!=null&&rightChunk!=null);
     //   for (int x = 0; x < chunkWidth; x++){
      //      for (int y = 0; y < chunkHeight; y++){
       //         for (int z = 0; z < chunkWidth; z++){
        //                BuildBlock(x, y, z, verts, uvs, tris, vertsNS, uvsNS, trisNS);
         //       }
       //     }
       // }
       // ThreadStart childref = new ThreadStart(() => GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS));
     //   Thread childThread=new Thread(childref);
       // childThread.Start();
        //childThread.Join();
        Task t1 = new Task(() => GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS));
        t1.Start();
       // t1.Wait();
        yield return new WaitUntil(()=>isMeshBuildCompleted==true);
        chunkMesh.vertices = verts.ToArray();
        chunkMesh.uv = uvs.ToArray();
        chunkMesh.triangles = tris.ToArray();
        chunkMesh.RecalculateBounds();
        chunkMesh.RecalculateNormals();
        meshFilter.mesh = chunkMesh;
        meshCollider.sharedMesh = chunkMesh;



        chunkNonSolidMesh.vertices = vertsNS.ToArray();
        chunkNonSolidMesh.uv = uvsNS.ToArray();
        chunkNonSolidMesh.triangles = trisNS.ToArray();
        chunkNonSolidMesh.RecalculateBounds();
        chunkNonSolidMesh.RecalculateNormals();
        meshFilterNS.mesh = chunkNonSolidMesh;
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
        if (y < 0) return true;
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
                }else return 1;
                
            }else if(z>=chunkWidth){
                if(frontChunk!=null){
                return frontChunk.map[x,y,0];
                 }else return 1;
            }else if(x<0){
                if(leftChunk!=null){
                return leftChunk.map[chunkWidth-1,y,z];
                 }else return 1;
            }else if(z<0){
                if(backChunk!=null){
                return backChunk.map[x,y,chunkWidth-1];
                 }else return 1;
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
    //   Debug.Log(chunkNeededUpdate.frontChunk);
     //   Debug.Log(chunkNeededUpdate.backChunk);
      //  Debug.Log(chunkNeededUpdate.leftChunk);
      //  Debug.Log(chunkNeededUpdate.rightChunk);
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

    public static int GetBlock(Vector3 pos){
        Vector3Int intPos=new Vector3Int(FloatToInt(pos.x),FloatToInt(pos.y),FloatToInt(pos.z));
        Chunk chunkNeededUpdate=Chunk.GetChunk(Vec3ToChunkPos(pos));
        Vector3Int chunkSpacePos=intPos-Vector3Int.FloorToInt(chunkNeededUpdate.transform.position);
        return chunkNeededUpdate.map[chunkSpacePos.x,chunkSpacePos.y,chunkSpacePos.z];
    }
    void Update(){
        if(isChunkMapUpdated==true){
            StartCoroutine(BuildChunk());
            isChunkMapUpdated=false;
        }
    }
    void OnDestroy(){
      //  SaveWorldData();
    }
}

