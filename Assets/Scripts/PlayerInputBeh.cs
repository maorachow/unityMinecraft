using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
 
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
public class PlayerInputBeh : MonoBehaviour
{
    public static PlayerInputBeh instance;
    public Action switchCameraPosAction;
    public Action pauseOrResumeAction;
    public Action dropItemButtonAction;
    public Action openInventoryButtonAction;
    public List<Action> playerLeftClickActions=new List<Action>();
    public List<Action> playerRightClickActions = new List<Action>();
    public Vector2 mouseDelta;
    public Vector2 playerMoveVec;
    public bool isJumping = false;
    public bool isPlayerSpeededUp = false;
    public static float mouseSens = 1f;
    public Button breakBlockButton;
    public Button placeBlockButton;
    public Button dropItemButton;
    public Button openInventoryButton;
    public List<Button> selectHotbarButtons;
    public UnityAction<int> selectHotbarButtonAction;
    public float switchItemSlotAxis = 0f;
    public InputAction playerMoveAction = new InputAction(type: InputActionType.Value);
    public PlayerInputs playerInputs;  
    void Awake()
    {
       instance = this;
        EnhancedTouchSupport.Enable();
        playerInputs = new PlayerInputs();
        playerInputs.Enable();
        breakBlockButton =GameObject.Find("leftclickbutton").GetComponent<Button>();
        openInventoryButton=GameObject.Find("openinventorybutton").GetComponent<Button> ();
        placeBlockButton = GameObject.Find("rightclickbutton").GetComponent<Button>();
        dropItemButton = GameObject.Find("dropitembutton").GetComponent<Button>();
        playerMoveAction.AddBinding(new InputBinding { isComposite = true });
        for (int i = 0; i < 9; i++)
        {
            Button hotbarButton = GameObject.Find("hotbarItem" + (i + 1).ToString()).GetComponent<Button>();
            selectHotbarButtons.Add(hotbarButton);
        }
    }
    public void AddButtonActions()
    {
        if (PlayerInputBeh.instance.breakBlockButton != null)
        {
            foreach (var action in PlayerInputBeh.instance.playerLeftClickActions)
            {
                PlayerInputBeh.instance.breakBlockButton.onClick.AddListener(new UnityAction(action));
            }

        }
        if (PlayerInputBeh.instance.placeBlockButton != null)
        {
            foreach (var action in PlayerInputBeh.instance.playerRightClickActions)
            {
                PlayerInputBeh.instance.placeBlockButton.onClick.AddListener(new UnityAction(action));
            }

        }
        if(dropItemButton != null)
        {
            dropItemButton.onClick.AddListener(new UnityAction(dropItemButtonAction));
        }
        for(int i = 0;i< selectHotbarButtons.Count; i++)
        {
            int j = i+1;
             
            UnityAction action = new UnityAction(() => {   selectHotbarButtonAction(j); });
            if (selectHotbarButtons[i] != null)
            {
            selectHotbarButtons[i].onClick.AddListener(action);
            }
          
        }
  /*      if (openInventoryButton != null)
        {
            openInventoryButton.onClick.AddListener(new UnityAction(openInventoryButtonAction));
        }*/
    }
    float timePressed = 0f;
    
    void Update()
    {
        if(playerInputs.PlayerInput.SwitchCameraPos.WasPressedThisFrame()==true)
        {
            if(switchCameraPosAction!=null)
            {
            switchCameraPosAction();
            }
           
        }
    
        if (playerInputs.PlayerInput.PauseGame.WasPressedThisFrame() == true)
        {
 
            if (pauseOrResumeAction != null)
            {
                pauseOrResumeAction();
            }

        }
        if (playerInputs.PlayerInput.OpenInventory.WasPressedThisFrame() == true)
        {
            
            if (openInventoryButtonAction != null)
            {
                openInventoryButtonAction();
            }

        }

        if (!GameUIBeh.instance.isCraftingMenuOpened)
        {
            if (WorldManager.platform == RuntimePlatform.Android || WorldManager.platform == RuntimePlatform.IPhonePlayer||MobileButtonHideBeh.isHidingButton==false)
            {
                for (int i = 0; i < UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count; i++)
                {
                    EventSystem eventSystem = EventSystem.current;
                    PointerEventData pointerEventData = new PointerEventData(eventSystem);
                    pointerEventData.position = Touch.activeTouches[i].screenPosition;
           //         Debug.Log("touch position:" + Touch.activeTouches[i].screenPosition);
                    List<RaycastResult> uiRaycastResultCache = new List<RaycastResult>();
                    eventSystem.RaycastAll(pointerEventData, uiRaycastResultCache);
                    if (uiRaycastResultCache.Count == 0 )
                    {
                        if(Touch.activeTouches[i].isInProgress)
                        {
                        mouseDelta = Touch.activeTouches[i].delta * mouseSens;
                            timePressed += Time.deltaTime;
                        }
                        else
                        {
                            mouseDelta =new Vector2(0,0);
                            timePressed = 0f;
                        }
                      
                    }
                }
            }
            else
            {
                mouseDelta = Mouse.current.delta.ReadValue() * mouseSens;
            }
        }
        
        if(Mouse.current!=null)
        {
    if (Mouse.current.leftButton.isPressed)
        {

            foreach(var action in playerLeftClickActions)
            {
                if(action != null)
                {
                    action();
                }
               
            }
        }

        if (Mouse.current.rightButton.isPressed)
        {

            foreach (var action in playerRightClickActions)
            {
                if (action != null)
                {
                    action();
                }

            }
        }
        }

       
        if (playerInputs.PlayerInput.DropItem.WasPressedThisFrame())
        {
            dropItemButtonAction();
        }
         
        playerMoveVec=playerInputs.PlayerInput.Move.ReadValue<Vector2>();
        isJumping = playerInputs.PlayerInput.Jump.IsInProgress();
        isPlayerSpeededUp=playerInputs.PlayerInput.SpeedUp.IsInProgress();
        switchItemSlotAxis= playerInputs.PlayerInput.SwitchItemSlot.ReadValue<float>()/120f;
       
    }
}
