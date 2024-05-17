using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakBlockParticleBeh : MonoBehaviour
{

public int blockID;
public Mesh particleMesh;
public ParticleSystemRenderer psr;
public ParticleSystem ps;
public List<Vector3> verts=new List<Vector3>();
public List<Vector2> uvs=new List<Vector2>();
public List<int> tris=new List<int>();
void OnDisable(){
    particleMesh=null;
     verts=null;
 uvs=null;
 tris=null;
 blockID=0;
}
void ReleaseGameObject(){
    if(gameObject.activeInHierarchy==true){
            VoxelWorld.currentWorld.particleEffectPool.Release(gameObject);    
    }
    
}
void OnEnable(){
   
 verts=new List<Vector3>();
 uvs=new List<Vector2>();
 tris=new List<int>();
     particleMesh=new Mesh();
    ps=GetComponent<ParticleSystem>();
    psr=GetComponent<ParticleSystemRenderer>();
}
public void EmitParticle(){
   //     Debug.Log(blockID);
    if(Chunk.blockAudioDic.ContainsKey(blockID)){
    AudioSource.PlayClipAtPoint(Chunk.blockAudioDic[blockID],transform.position,1f);    
    }else{
        Debug.Log("missing file");
    }
    
    particleMesh=new Mesh();
    int x=0;
    int y=0;
    int z=0;
     verts=new List<Vector3>();
 uvs=new List<Vector2>();
 tris=new List<int>();
    ps=GetComponent<ParticleSystem>();
    psr=GetComponent<ParticleSystemRenderer>();

    if(blockID>0&&blockID<100){
          BuildFace(blockID, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris,0);
        //Right
   
         BuildFace(blockID, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris,1);

        //Bottom
   
         BuildFace(blockID, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris,2);
        //Top
  
        BuildFace(blockID, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris,3);

        //Back
     
        BuildFace(blockID, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris,4);
        //Front
       
        BuildFace(blockID, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris,5); 
    }else{
        if(blockID>=101&&blockID<150){
            if(blockID==102){
                   BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0f,0.4375f), new Vector3(0f,0.625f,0f),new Vector3(0f,0f,0.125f),new Vector2(0.0078125f,0.0390625f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0f), false, verts, uvs, tris);
            //Right
     
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0f,0.4375f)+new Vector3(0.125f,0f,0f),  new Vector3(0f,0.625f,0f),new Vector3(0f,0f,0.125f),new Vector2(0.0078125f,0.0390625f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0f), true, verts, uvs, tris);

            //Bottom
     
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0f,0.4375f),new Vector3(0f,0f,0.125f),new Vector3(0.125f,0f,0f),new Vector2(0.0078125f,0.0078125f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0f), false, verts, uvs, tris);
                //Top
       
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0.625f,0.4375f), new Vector3(0f,0f,0.125f), new Vector3(0.125f,0f,0f),new Vector2(0.0078125f,0.0078125f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0.03125f), true, verts, uvs, tris);

            //Back
    
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0f,0.4375f), new Vector3(0f,0.625f,0f), new Vector3(0.125f,0f,0f),new Vector2(0.0078125f,0.0390625f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0f), true, verts, uvs, tris);
            //Front
    
            BuildFaceComplex(new Vector3(x, y, z)+new Vector3(0.4375f,0f,0.4375f)+new Vector3(0f,0f,0.125f),  new Vector3(0f,0.625f,0f), new Vector3(0.125f,0f,0f),new Vector2(0.0078125f,0.0390625f),new Vector2(0.0625f,0.0625f)+new Vector2(0.02734375f,0f),false, verts, uvs, tris); 
            
            }else{
                   Vector3 randomCrossModelOffset=new Vector3(0f,0f,0f);
            BuildFace(blockID, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, false, verts, uvs, tris,0);
            BuildFace(blockID, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, true, verts, uvs, tris,0);
            BuildFace(blockID, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, false, verts, uvs, tris,0);
            BuildFace(blockID, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, true, verts, uvs, tris,0);
            }
         
        }
    }

        particleMesh.vertices = verts.ToArray();
        particleMesh.uv = uvs.ToArray();
        particleMesh.triangles = tris.ToArray();
        particleMesh.RecalculateBounds();
        particleMesh.RecalculateNormals();
        particleMesh.RecalculateTangents();
        psr.mesh=particleMesh;
       ps.Play();
       Invoke("ReleaseGameObject",1.5f);
}
static void BuildFaceComplex(Vector3 corner, Vector3 up, Vector3 right,Vector2 uvWidth,Vector2 uvCorner, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris){
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
static void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int side){

        int index = verts.Count;
    
        verts.Add (corner);
        verts.Add (corner + up);
        verts.Add (corner + up + right);
        verts.Add (corner + right);

        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        uvCorner=Chunk.blockInfo[typeid][side];
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
}
