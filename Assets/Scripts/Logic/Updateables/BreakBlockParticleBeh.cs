using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using Unity.Collections;
using UnityEngine;

public class BreakBlockParticleBeh : MonoBehaviour
{

public int blockID;
public Mesh particleMesh;
public ParticleSystemRenderer psr;
public ParticleSystem ps;
 
void OnDisable(){
    particleMesh=null;
     
 blockID=0;
}
void ReleaseGameObject(){
    if(gameObject.activeInHierarchy==true){
            VoxelWorld.currentWorld.particleEffectPool.Release(gameObject);    
    }
    
}
void OnEnable(){
   
 
     particleMesh=new Mesh();
    ps=GetComponent<ParticleSystem>();
    psr=GetComponent<ParticleSystemRenderer>();
}
public void EmitParticle(){
   //     Debug.Log(blockID);
   
    GlobalAudioResourcesManager.PlayClipAtPointCustomRollOff(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetBlockAudioClip(blockID),transform.position,1f,40f);    
    
    
    particleMesh=new Mesh();
    int x=0;
    int y=0;
    int z=0;
    
        ps =GetComponent<ParticleSystem>();
        psr=GetComponent<ParticleSystemRenderer>();
        
        ItemEntityMeshBuildingHelper.BuildItemMesh(ref particleMesh, blockID,1f,true);

        psr.mesh=particleMesh;
       ps.Play();
       Invoke("ReleaseGameObject",1.5f);
} 
}
