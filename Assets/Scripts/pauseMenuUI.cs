using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
public class pauseMenuUI : MonoBehaviour
{
    public static pauseMenuUI instance;
    public Texture2D terrainNormal;
    public Button rebuildAllChunksButton;
    public Button SaveWorldButton;
    public Text viewRangeText;
    public Slider viewRangeSlider;
    public Slider graphicsQualitySlider;
    public Text graphicsQualityText;
    public PlayerMove player;
    public Button returnToMainMenuButton;
    public InputField resourcesDirectoryField;
    public Button loadResourceButton;
    public Text resourcePackLoadingStatsText;
    void Start()
    {
        instance=this;
        resourcePackLoadingStatsText=GameObject.Find("resourcepackloadingstatstext").GetComponent<Text>();
        resourcesDirectoryField=GameObject.Find("resourcedirectoryfield").GetComponent<InputField>();
        loadResourceButton=GameObject.Find("loadresourcepackbutton").GetComponent<Button>();
        terrainNormal=Resources.Load<Texture2D>("Textures/terrainnormal");
        TerrainTextureMipmapAdjusting.SetTerrainNormalMipmap(terrainNormal);
        player =GameObject.Find("player").GetComponent<PlayerMove>();
        graphicsQualitySlider=GameObject.Find("graphicsqualityslider").GetComponent<Slider>();
        graphicsQualityText=GameObject.Find("graphicsqualitytext").GetComponent<Text>();
        viewRangeSlider=GameObject.Find("viewrangeslider").GetComponent<Slider>();
        viewRangeText=GameObject.Find("viewrangetext").GetComponent<Text>();
        rebuildAllChunksButton=GameObject.Find("rebuildallchunksbutton").GetComponent<Button>();
        viewRangeSlider.onValueChanged.AddListener(ViewRangeSliderOnValueChanged);
        rebuildAllChunksButton.onClick.AddListener(RebuildAllChunksButtonOnClick);
        SaveWorldButton=GameObject.Find("saveworldbutton").GetComponent<Button>();
        SaveWorldButton.onClick.AddListener(SaveWorldButtonOnClick);
        returnToMainMenuButton=GameObject.Find("pausemainmenubutton").GetComponent<Button>();
        returnToMainMenuButton.onClick.AddListener(ReturnToMainMenuButtonOnClick);
        graphicsQualitySlider.onValueChanged.AddListener(GraphicsQualitySliderOnValueChanged);
        loadResourceButton.onClick.AddListener(LoadResourceButtonOnClick);
    }
    void GraphicsQualitySliderOnValueChanged(float f){
        switch((int)graphicsQualitySlider.value){
            case 0:
            graphicsQualityText.text="Very Low";
            QualitySettings.SetQualityLevel(0, true);
              ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BumpMap",null);
            break;
            case 1:
             graphicsQualityText.text="Low";
            QualitySettings.SetQualityLevel(1, true);
               ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BumpMap",null);
            break;
            case 2:
             graphicsQualityText.text="Medium";
            QualitySettings.SetQualityLevel(2, true);
                ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BumpMap",null);
            break;
            case 3:
             graphicsQualityText.text="High";
            QualitySettings.SetQualityLevel(3, true);
               ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BumpMap",null);
            break;
            case 4:
             graphicsQualityText.text="Very High";
            QualitySettings.SetQualityLevel(4, true);
              ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BumpMap", terrainNormal);
            break;
            case 5:
             graphicsQualityText.text="Ultra";
            QualitySettings.SetQualityLevel(5, true);
           ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BumpMap", terrainNormal);
            break;
        }
    }
    void ViewRangeSliderOnValueChanged(float f){
        PlayerMove.viewRange=viewRangeSlider.value;
        viewRangeText.text=viewRangeSlider.value.ToString();

    }
    void ReturnToMainMenuButtonOnClick(){
        FileAssetLoaderBeh.instance.UnloadAndResetResouces();
          ItemEntityBeh.AddFlatItemInfo();
        terrainNormal=Resources.Load<Texture2D>("Textures/terrainnormal");
        ObjectPools.chunkPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BumpMap",terrainNormal);
        ObjectPools.itemPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>("Textures/itemterrain"));
        ZombieBeh.isZombiePrefabLoaded=false;
        WorldManager.isGoingToQuitGame=true;
        SaveWorldButtonOnClick();
         WorldManager.DestroyAllChunks();
         SceneManager.LoadScene(0);


    }
     void SaveWorldButtonOnClick(){
        player.SavePlayerData();
        Chunk.SaveWorldData();
        EntityBeh.SaveWorldEntityData();
        ItemEntityBeh.SaveWorldItemEntityData();
    }
    void RebuildAllChunksButtonOnClick(){
        foreach(KeyValuePair<Vector2Int,Chunk> kvp in Chunk.Chunks){
            kvp.Value.isChunkMapUpdated=true;
        }
    }
    void LoadResourceButtonOnClick(){
        string resourcePackRootPath=resourcesDirectoryField.text;
        bool isLoadingSuccessful=FileAssetLoaderBeh.instance.LoadBlockNameDic(resourcePackRootPath+"/blockname.dat");
        isLoadingSuccessful=FileAssetLoaderBeh.instance.LoadChunkBlockInfo(resourcePackRootPath+"/blockterraininfo.dat");
        isLoadingSuccessful=FileAssetLoaderBeh.instance.LoadItemBlockInfo(resourcePackRootPath+"/itemblockinfo.dat");
        isLoadingSuccessful=FileAssetLoaderBeh.instance.LoadBlockAudio(resourcePackRootPath+"/audioab.dat");
        isLoadingSuccessful=FileAssetLoaderBeh.instance.LoadBlockTexture(resourcePackRootPath+"/textureab.dat");
        if(isLoadingSuccessful==true){
            resourcePackLoadingStatsText.text="Successfully loaded a resources pack";
        }else{
            resourcePackLoadingStatsText.text="Resources pack loading failed";
        }
    }
    void OnApplicationQuit(){
           ObjectPools.itemPrefab.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>("Textures/itemterrain"));
             ItemEntityBeh.AddFlatItemInfo();
          terrainNormal=Resources.Load<Texture2D>("Textures/terrainnormal");
            player.curChunk.meshRenderer.sharedMaterial.SetTexture("_BumpMap",terrainNormal);
                FileAssetLoaderBeh.instance.UnloadAndResetResouces();
    }
}
