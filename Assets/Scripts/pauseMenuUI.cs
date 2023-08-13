using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class pauseMenuUI : MonoBehaviour
{
    public Button rebuildAllChunksButton;
    public Text viewRangeText;
    public Slider viewRangeSlider;
    public PlayerMove player;
    void Start()
    {
        player=GameObject.Find("player").GetComponent<PlayerMove>();
        viewRangeSlider=GameObject.Find("viewrangeslider").GetComponent<Slider>();
        viewRangeText=GameObject.Find("viewrangetext").GetComponent<Text>();
        rebuildAllChunksButton=GameObject.Find("rebuildallchunksbutton").GetComponent<Button>();
        viewRangeSlider.onValueChanged.AddListener(ViewRangeSliderOnValueChanged);
        rebuildAllChunksButton.onClick.AddListener(RebuildAllChunksButtonOnClick);
    }
    void ViewRangeSliderOnValueChanged(float f){
        player.viewRange=viewRangeSlider.value;
        viewRangeText.text=viewRangeSlider.value.ToString();

    }
    void RebuildAllChunksButtonOnClick(){
        foreach(KeyValuePair<Vector2Int,Chunk> kvp in Chunk.Chunks){
            kvp.Value.isChunkMapUpdated=true;
        }
    }
}
