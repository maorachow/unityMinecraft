using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GlobalGameResourcesManager
{
    public static GlobalGameResourcesManager instance = new GlobalGameResourcesManager();

    public GlobalAudioResourcesManager audioResourcesManager;
    public TerrainTextureMaterialManager terrainTextureMaterialManager;

    public IItemIDToBlockIDMapper itemIDToBlockIDMapper;
    public IMeshBuildingInfoDataProvider meshBuildingInfoDataProvider;
    private GlobalGameResourcesManager()
    {
        audioResourcesManager = new GlobalAudioResourcesManager();
        terrainTextureMaterialManager= new TerrainTextureMaterialManager();
        itemIDToBlockIDMapper=new ItemIDToBlockIDMapper();
        meshBuildingInfoDataProvider= new MeshBuildingInfoDataProvider();
        LoadDefaultResources();
    }


    public void LoadDefaultResources()
    {
        audioResourcesManager.LoadDefaultBlockAudioResources();
        audioResourcesManager.LoadDefaultEntityAudioResources();
        terrainTextureMaterialManager.LoadAndSetDefaultTexMipmap();
        itemIDToBlockIDMapper.Initialize();
        meshBuildingInfoDataProvider.InitDefault();
    }
    
}
