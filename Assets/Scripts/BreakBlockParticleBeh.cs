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
public List<Vector3> norms = new List<Vector3>();
    public List<Vector2> uvs=new List<Vector2>();
public List<int> tris=new List<int>();
void OnDisable(){
    particleMesh=null;
             verts=null;
         uvs=null;
         tris=null;
         norms=null;
 blockID=0;
}
void ReleaseGameObject(){
    if(gameObject.activeInHierarchy==true){
            VoxelWorld.currentWorld.particleEffectPool.Release(gameObject);    
    }
    
}
void OnEnable(){
   
 verts=new List<Vector3>();
 norms = new List<Vector3>();
        uvs =new List<Vector2>();
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
        ItemEntityMeshBuildingHelper.BuildItemMesh(blockID,ref verts,ref uvs,ref tris,ref norms);
  /*  if(blockID>0&&blockID<100){
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
    }*/

        particleMesh.vertices = verts.ToArray();
        particleMesh.normals = norms.ToArray();
        particleMesh.uv = uvs.ToArray();
        particleMesh.triangles = tris.ToArray();
        particleMesh.RecalculateBounds();
        
        particleMesh.RecalculateTangents();
        psr.mesh=particleMesh;
       ps.Play();
       Invoke("ReleaseGameObject",1.5f);
} 
}
