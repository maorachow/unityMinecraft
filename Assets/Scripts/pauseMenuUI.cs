using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class pauseMenuUI : MonoBehaviour
{
    public Button rebuildAllChunksButton;
    public Button SaveWorldButton;
    public Text viewRangeText;
    public Slider viewRangeSlider;
    public PlayerMove player;
    public Button returnToMainMenuButton;
    void Start()
    {
        
        player=GameObject.Find("player").GetComponent<PlayerMove>();
        viewRangeSlider=GameObject.Find("viewrangeslider").GetComponent<Slider>();
        viewRangeText=GameObject.Find("viewrangetext").GetComponent<Text>();
        rebuildAllChunksButton=GameObject.Find("rebuildallchunksbutton").GetComponent<Button>();
        viewRangeSlider.onValueChanged.AddListener(ViewRangeSliderOnValueChanged);
        rebuildAllChunksButton.onClick.AddListener(RebuildAllChunksButtonOnClick);
        SaveWorldButton=GameObject.Find("saveworldbutton").GetComponent<Button>();
        SaveWorldButton.onClick.AddListener(SaveWorldButtonOnClick);
        returnToMainMenuButton=GameObject.Find("pausemainmenubutton").GetComponent<Button>();
        returnToMainMenuButton.onClick.AddListener(ReturnToMainMenuButtonOnClick);
    }
    void ViewRangeSliderOnValueChanged(float f){
        PlayerMove.viewRange=viewRangeSlider.value;
        viewRangeText.text=viewRangeSlider.value.ToString();

    }
    void ReturnToMainMenuButtonOnClick(){
        SaveWorldButtonOnClick();
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
}
