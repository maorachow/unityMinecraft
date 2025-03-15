using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GlobalGameResourcesManager
{
    public static GlobalGameResourcesManager instance = new GlobalGameResourcesManager();

    public GlobalAudioResourcesManager audioResourcesManager;
    public TerrainTextureMaterialManager terrainTextureMaterialManager;

    private GlobalGameResourcesManager()
    {
        audioResourcesManager = new GlobalAudioResourcesManager();
        terrainTextureMaterialManager= new TerrainTextureMaterialManager();
        LoadDefaultResources();
    }


    public void LoadDefaultResources()
    {
        audioResourcesManager.LoadDefaultBlockAudioResources();
        audioResourcesManager.LoadDefaultEntityAudioResources();
        terrainTextureMaterialManager.LoadAndSetDefaultTexMipmap();
    }
    
}
