using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplashParticleBeh : MonoBehaviour
{
    public static WaterSplashParticleBeh instance;
    public ParticleSystem ps;

  
    void Start()
    {
        instance=this;
        ps=GetComponent<ParticleSystem>();

    }

    public void EmitParticleAtPosition(Vector3 pos){
        var emitParams = new ParticleSystem.EmitParams();
        for(int i=0;i<20;i++){
            emitParams.position = pos+new Vector3(Random.Range(-0.7f,0.7f),Random.Range(-0.5f,0.5f),Random.Range(-0.7f,0.7f));
     
        ps.Emit(emitParams,1);  
        }
      
    }
}
