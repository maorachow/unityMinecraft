using System;
using NUnit.Framework.Constraints;
using UnityEditor.Build.Content;
using UnityEngine;

public class PlayerUIMediator
{
    public static PlayerUIMediator instance=new PlayerUIMediator();

    public float playerHealth;
    public float playerArmorPoints;
    public int[] playerInventoryItemTypeSlots= Array.Empty<int>();
    public int[] playerInventoryItemNumberSlots= Array.Empty<int>();
    public int playerInventoryCurSelectedHotbarIndex;
    public int playerInventorySlotCapacity;
    public bool isPlayerDied=false;
    public PlayerMove player;
    public GameUIBeh gameUI;

    public bool isRespawnUIOpened;
    public bool isPauseUIOpened;
    public bool isCraftingUIOpened;

    public bool isGoingToQuitGame=false;
    
    static PlayerUIMediator()
    {
         


    }

    public void UpdateUIStats()
    {
        
        isRespawnUIOpened = gameUI.isRespawnUIOpened;
        isCraftingUIOpened = gameUI.isCraftingMenuOpened;
        isPauseUIOpened = gameUI.isPauseMenuOpened;
        if (isGoingToQuitGame == true)
        {
            isGoingToQuitGame=false;
            SceneManagementHelper.QuitToMainMenu();

        }
    }
    
    public void UpdatePlayerStats(bool forceUpdateUI=false)
    {
        if (Math.Abs(playerHealth - player.playerHealth) > 1e-7f||forceUpdateUI==true)
        {
            if (gameUI != null)
            {
                gameUI.PlayerHealthSliderOnValueChanged(player.playerHealth);
            }
        }
        if (Math.Abs(playerArmorPoints - player.playerArmorPoints) > 1e-7f || forceUpdateUI == true)
        {
            if (gameUI != null)
            {
                gameUI.PlayerArmorPointsSliderOnValueChanged(player.playerHealth);
            }
        }
        if (isPlayerDied!=player.isDied || forceUpdateUI == true)
        {
            if (gameUI != null)
            {
                if (player.isDied == true)
                {
                    gameUI.OpenRespawnUI();
                }
                else
                {
                    gameUI.CloseRespawnUI();
                }
            }
        }
        isPlayerDied = player.isDied;
        playerHealth = player.playerHealth;
        playerArmorPoints = player.playerArmorPoints;
        playerInventoryCurSelectedHotbarIndex = player.currentSelectedHotbar;
        player.TryGetInventoryData(out playerInventoryItemTypeSlots, out playerInventoryItemNumberSlots);
        playerInventorySlotCapacity = player.inventory.inventorySlotCount;
    }

    public void PauseOrResume()
    {
        if (gameUI == null)
        {
            return;
        }
        //     Debug.Log("Pause");

        if (GlobalGameOptions.isGamePaused == false)
        {
           gameUI.PauseGame();
            return;
        }
        else
        {

            gameUI.Resume();
            if (gameUI.isCraftingMenuOpened == true)
            {
                gameUI.CloseCraftingUI();
            }
            return;
        }
    }

    public void MouseLock()
    {
        if (gameUI!=null)
        {
            gameUI.MouseLock();
        }
    }
    public void OpenOrCloseCraftingUI()
    {
        if (gameUI == null)
        {
            return;
        }

        if (gameUI.isCraftingMenuOpened == true)
        {
            gameUI.CloseCraftingUI();
        }
        else
        {
            if (gameUI.isPauseMenuOpened == true)
            {
                return;
            }
            gameUI.OpenCraftingUI();
        }
    }

    public void PlayerSwitchCameraPosAction()
    {
        player.SwitchCameraPosAction();
    }
    public void PlayerRespawn()
    {
        if (player != null)
        {
            player.PlayerRespawn();
        }

        if (gameUI != null)
        {
            gameUI.CloseRespawnUI();
        }

    }

    public void BreakBlockButtonPress()
    {
        if (player != null)
        {
            player.BreakBlockButtonPress();
        }
    }

    public void PlaceBlockButtonPress()
    {
        if (player != null)
        {
            player.PlaceBlockButtonPress();
        }
    }

    public void DropItemButtonOnClick()
    {
        if (player != null)
        {
            player.DropItemButtonOnClick();
        }
    }

    public void SetHotbarNum(int num)
    {
        if (player != null)
        {
            player.SetHotbarNum(num);
        }
    }
    public void PlayerInventorySwapSlot(int slotSrc,int slotDst)
    {
        if (player != null)
        {
            player.inventory.SwapItemSlot(slotSrc, slotDst);
        }
         
    }

    public void QuitToMainMenuButtonOnClick()
    {
        if (player != null)
        {
            player.SavePlayerDataToPersistenceManager();
        }
        isGoingToQuitGame = true;
    }
    

}
