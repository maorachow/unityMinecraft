using UnityEngine.SceneManagement;

public class SceneManagementHelper 
{
    public static void QuitToMainMenu()
    {
       

        VoxelWorld.isWorldChanged = false;
        
        VoxelWorld.currentWorld.SaveAndQuitWorld();
      
        VoxelWorld.currentWorld = VoxelWorld.worlds[0];
        GameDataPersistenceManager.instance.isPlayerDataLoaded=false;
        GameDataPersistenceManager.instance.isPlayerTeleportedThroughWorld = false;
        GameDataPersistenceManager.instance.isPlayerTeleportedThroughWorldSelfByPlayer=false;
        SceneManager.LoadScene(0);
    }
 
    public static void SwitchToWorldWithSceneChanged(int worldID,int SceneID)
    {
        VoxelWorld.SwitchToWorld(worldID);
        SceneManager.LoadScene(SceneID);
    }
    public static void SwitchToWorldWithoutSavingWithSceneChanged(int worldID, int SceneID)
    {
        VoxelWorld.SwitchToWorldWithoutSaving(worldID);
        SceneManager.LoadScene(SceneID);
    }
}
