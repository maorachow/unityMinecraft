using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MoblieControlUIBeh : MonoBehaviour
{
    public Button dropItemButton;
    public Button openInventoryButton;
    public Button leftClickButton;
    public Button rightClickButton;

    
    public Button pauseGameButton;
    public List<Button> setHotbarNumButtons;
    void Start()
    {
        dropItemButton = transform.Find("dropitembutton").GetComponent<Button>();

        leftClickButton = transform.Find("leftclickbutton").GetComponent<Button>();
        openInventoryButton = transform.Find("openinventorybutton").GetComponent<Button>();
        rightClickButton = transform.Find("rightclickbutton").GetComponent<Button>();
        
        pauseGameButton = transform.Find("pausebutton").GetComponent<Button>();

        dropItemButton.onClick.AddListener(PlayerUIMediator.instance.DropItemButtonOnClick);
        leftClickButton.onClick.AddListener(PlayerUIMediator.instance.BreakBlockButtonPress);
        openInventoryButton.onClick.AddListener(PlayerUIMediator.instance.OpenOrCloseCraftingUI);
      
        rightClickButton.onClick.AddListener(PlayerUIMediator.instance.PlaceBlockButtonPress);
       
        pauseGameButton.onClick.AddListener(PlayerUIMediator.instance.PauseOrResume);
        setHotbarNumButtons = new List<Button>();
        for (int i = 0; i < 9; i++)
        {
            Button setHotbarNumButton = transform.Find("hotbarItem"+(i+1).ToString()).GetComponent<Button>();
            var i1 = i;
            setHotbarNumButton.onClick.AddListener(new UnityAction(()=>PlayerUIMediator.instance.SetHotbarNum(num: i1 + 1)));
        }

    }

    void Update()
    {
        Debug.Log("listener count:" + dropItemButton.onClick.GetPersistentEventCount());
    }

    
}
