using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectManagerBeh : MonoBehaviour
{
    public static ParticleEffectManagerBeh instance;
    public ParticleSystem psWaterSplash;
    public ParticleSystem psEndermanParticle;
    public ParticleSystem psPlayerSweepParticle;
    public ParticleSystem psExplodeParticle;

    void Start()
    {
        instance=this;
        psWaterSplash = transform.GetChild(1).GetComponent<ParticleSystem>();
        psEndermanParticle= transform.GetChild(0).GetComponent<ParticleSystem>();
        psPlayerSweepParticle= transform.GetChild(2).GetComponent<ParticleSystem>();
        psExplodeParticle= transform.GetChild(3).GetComponent<ParticleSystem>();
    }

    public void EmitWaterSplashParticleAtPosition(Vector3 pos){
        var emitParams = new ParticleSystem.EmitParams();
        for(int i=0;i<20;i++){
            emitParams.position = pos+new Vector3(Random.Range(-0.7f,0.7f),Random.Range(-0.5f,0.5f),Random.Range(-0.7f,0.7f));

            psWaterSplash.Emit(emitParams,1);  
        }
      
    }

    public void EmitEndermanParticleAtPosition(Vector3 pos)
    {
        var emitParams = new ParticleSystem.EmitParams();
        for (int i = 0; i < 20; i++)
        {
            emitParams.position = pos + new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(-0.5f, 0.5f), Random.Range(-0.7f, 0.7f));
            emitParams.velocity = new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(-0.5f, 0.5f), Random.Range(-0.7f, 0.7f));
             psEndermanParticle.Emit(emitParams, 1);
        }

    }
    public void EmitPlayerSweepParticleAtPosition(Vector3 pos)
    {
        var emitParams = new ParticleSystem.EmitParams();

        emitParams.position = pos;

        psPlayerSweepParticle.Emit(emitParams, 1);
      

    }

    public void EmitExplodeParticleAtPosition(Vector3 pos)
    {
        var emitParams = new ParticleSystem.EmitParams();

        for (int i = 0; i < 30; i++)
        {
            emitParams.position = pos + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        
            psExplodeParticle.Emit(emitParams, 1);
        }

        


    }
    public void EmitBreakBlockParticleAtPosition(Vector3 pos,int blockID)
    {
        GameObject a = VoxelWorld.currentWorld.particleEffectPool.Get();
        a.transform.position = pos;
        a.GetComponent<BreakBlockParticleBeh>().blockID = blockID; 
        a.GetComponent<BreakBlockParticleBeh>().SendMessage("EmitParticle");
    }
}
