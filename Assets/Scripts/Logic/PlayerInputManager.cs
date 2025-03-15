using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
 
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager instance;
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
   
    public Button breakBlockButton;
    public Button placeBlockButton;
    public Button dropItemButton;
    public Button openInventoryButton;
   
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
        
        playerMoveAction.AddBinding(new InputBinding { isComposite = true });
       
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

        if (!PlayerUIMediator.instance.isCraftingUIOpened&& !PlayerUIMediator.instance.isPauseUIOpened&& !PlayerUIMediator.instance.isRespawnUIOpened)
        {
            if (WorldManager.platform == RuntimePlatform.Android || WorldManager.platform == RuntimePlatform.IPhonePlayer||MobileButtonHideBeh.isHidingButton==false)
            {
                EventSystem eventSystem = EventSystem.current;
                PointerEventData pointerEventData = new PointerEventData(eventSystem);
                for (int i = 0; i < UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count; i++)
                {
                   
                   
                    pointerEventData.position = Touch.activeTouches[i].screenPosition;
           //         Debug.Log("touch position:" + Touch.activeTouches[i].screenPosition);
                    List<RaycastResult> uiRaycastResultCache = new List<RaycastResult>();
                    eventSystem.RaycastAll(pointerEventData, uiRaycastResultCache);
                    if (uiRaycastResultCache.Count == 0 )
                    {
                        if(Touch.activeTouches[i].isInProgress)
                        {
                        mouseDelta = Touch.activeTouches[i].delta * GlobalGameOptions.inGameMouseSensitivity;
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
                mouseDelta = Mouse.current.delta.ReadValue() * GlobalGameOptions.inGameMouseSensitivity;
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

    void OnDestroy()
    {
        playerInputs.Disable();
    }
}
