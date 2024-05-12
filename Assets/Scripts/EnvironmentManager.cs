using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class EnvironmentManager : MonoBehaviour
{
    public Volume globalVolume;
    public VolumetricLightingVolumeComponent volumetricLightingVolumeComponent;
    public Transform mainLightTrans;
    [SerializeField]
    public Color dayFogColor;
    [SerializeField]
    public Color nightFogColor;
    void Start()
    {
        globalVolume = GetComponent<Volume>();
        volumetricLightingVolumeComponent = globalVolume.profile.components.Find(v => {  return v.GetType()== typeof(VolumetricLightingVolumeComponent); }) as VolumetricLightingVolumeComponent;
        mainLightTrans=GameObject.Find("Directional Light").transform;
    }

    
    void Update()
    {
        Vector3 mainLightDir= mainLightTrans.forward.normalized;
        float mainLightY = Mathf.Clamp(mainLightDir.y, -0.1f, 0.1f);
        if (volumetricLightingVolumeComponent != null)
        {
            volumetricLightingVolumeComponent.fogColor.overrideState = true;
            volumetricLightingVolumeComponent.fogColor.value = Color.Lerp(dayFogColor, nightFogColor, (mainLightY * 10) * 0.5f + 0.5f);
        }
      
    }
}
