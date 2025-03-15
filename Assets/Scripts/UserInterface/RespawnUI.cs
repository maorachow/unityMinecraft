using UnityEngine;
using UnityEngine.UI;

public class RespawnUI : MonoBehaviour
{   
   
   
    public Button respawnButton;
    public Button mainMenuButton;
   
    void Start(){
     
       
        respawnButton=GameObject.Find("respawnbutton").GetComponent<Button>();
        mainMenuButton=GameObject.Find("mainmenubutton").GetComponent<Button>();
        respawnButton.onClick.AddListener(RespawnButtonOnClick);
         mainMenuButton.onClick.AddListener(MainMenuButtonOnClick);
    }
    
    public void RespawnButtonOnClick(){
        PlayerUIMediator.instance.PlayerRespawn();
    }   
    public void MainMenuButtonOnClick(){
        /*   ZombieBeh.isZombiePrefabLoaded=false;
           player.PlayerRespawn();
           player.SavePlayerData();
           Chunk.SaveWorldData();
           ItemEntityBeh.SaveWorldItemEntityData();
           EntityBeh.SaveWorldEntityData();
        //   Application.Quit();
             SceneManager.LoadScene(0);*/
        PlayerUIMediator.instance.QuitToMainMenuButtonOnClick();
     
    }
}
