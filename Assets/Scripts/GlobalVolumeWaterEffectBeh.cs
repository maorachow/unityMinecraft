using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class GlobalVolumeWaterEffectBeh : MonoBehaviour
{
    Volume globalVolume;
    public static GlobalVolumeWaterEffectBeh instance;
    public VolumeComponent vigConfig;
    public VolumeComponent colorAdjustmentsConfig;
    
    void Start()
    {
        instance=this;
        globalVolume=GetComponent<Volume>();
       
        vigConfig=globalVolume.profile.components[0];
        colorAdjustmentsConfig=globalVolume.profile.components[1];
        vigConfig.active=false;
        colorAdjustmentsConfig.active=false;
     
    }
    public void SwitchEffects(bool isInWater){
       
      
            vigConfig.active=isInWater;
        colorAdjustmentsConfig.active=isInWater;
        
    }
}   
