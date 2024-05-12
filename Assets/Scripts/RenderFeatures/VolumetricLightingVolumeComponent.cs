using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class VolumetricLightingVolumeComponent : VolumeComponent
{
    public IntParameter stepCount=new IntParameter(32);
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0.002f, 0f, 0.5f);
    public ColorParameter fogColor = new ColorParameter(new Color(253f/255f,251f / 255f, 182f / 255f));
}
