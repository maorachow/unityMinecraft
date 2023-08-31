using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class RespawnUI : MonoBehaviour
{   
    public PlayerMove player;
    public static RespawnUI instance;
    public Button respawnButton;
    public Button mainMenuButton;
    void Start(){
        player=GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        instance=this;
        respawnButton=GameObject.Find("respawnbutton").GetComponent<Button>();
        mainMenuButton=GameObject.Find("mainmenubutton").GetComponent<Button>();
        respawnButton.onClick.AddListener(RespawnButtonOnClick);
         mainMenuButton.onClick.AddListener(MainMenuButtonOnClick);
    }
    
    public void RespawnButtonOnClick(){
        player.PlayerRespawn();
    }   
    public void MainMenuButtonOnClick(){
        player.PlayerRespawn();
        player.SavePlayerData();
        Chunk.SaveWorldData();
        ItemEntityBeh.SaveWorldItemEntityData();
        EntityBeh.SaveWorldEntityData();
     //   Application.Quit();
          SceneManager.LoadScene(0);
    }
}
