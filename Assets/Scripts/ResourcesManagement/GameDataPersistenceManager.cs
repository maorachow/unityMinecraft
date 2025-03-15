using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using UnityEngine;

public class GameDataPersistenceManager
{
   public static GameDataPersistenceManager instance =new GameDataPersistenceManager();
   public static RuntimePlatform platform;
   public string gameDataPath;
    private GameDataPersistenceManager()
    {
        platform = Application.platform;
       if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
       {
           gameDataPath = "C:/";
       }
       else
       {
           gameDataPath = Application.persistentDataPath;
           Debug.Log(gameDataPath);
       }
    }
    //每个world应该单独持有一份chunk/entity/itementity列表以进行管理，在进入世界时单独从对应文件加载数据以实现持久化，即每一个世界都有其单独的chunk/entity/itementity数据文件
    //而不应该如之前所写（不同世界entity/itementity仅由一件文件存储，每个entity/itementity具有世界标识数据，进入世界时如entity/itementity世界标识显示位于此世界则加载否则不加载）
    public Dictionary<int, ConcurrentDictionary<Vector2Int, ChunkData>> allWorldsDataReadFormFile =
        new Dictionary<int, ConcurrentDictionary<Vector2Int, ChunkData>>();
    

    public Dictionary<int, List<ItemEntityData>> itemEntityDataReadFromFile=new Dictionary<int, List<ItemEntityData>>();

    public bool IsItemEntityDataLoaded(int worldID)
    {
        return itemEntityDataReadFromFile.ContainsKey(worldID);
    }

    public Dictionary<int, List<EntityData>> entityDataReadFromFile = new Dictionary<int, List<EntityData>>();

    public bool IsEntityDataLoaded(int worldID)
    {
        return entityDataReadFromFile.ContainsKey(worldID);
    }

    public PlayerData playerDataReadFromFile;

    public bool isPlayerTeleportedThroughWorld=false;
    public bool isPlayerTeleportedThroughWorldSelfByPlayer = false;
    public bool isPlayerDataLoaded = false;
    //对于玩家只记录一份数据，保存退出游戏时记录其所在世界ID，开始游戏时进入（任意）世界后根据此ID跳转至上次所在世界

    public static MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

    public bool IsWorldDataLoaded(int worldID)
    {
        return allWorldsDataReadFormFile.ContainsKey(worldID);
    }

    private void CreateGameDataDirectoryIfNotExist(string path,bool createFoldersOnly=false)
    {
        if (!Directory.Exists(gameDataPath + "unityMinecraftData"))
        {
            Directory.CreateDirectory(gameDataPath + "unityMinecraftData");

        }
        if (!Directory.Exists(gameDataPath + "unityMinecraftData/GameData"))
        {
            Directory.CreateDirectory(gameDataPath + "unityMinecraftData/GameData");
        }

        if (createFoldersOnly == false)
        {
            if (!File.Exists(gameDataPath + "unityMinecraftData" + "/GameData/" + path))
            {
                FileStream fs = File.Create(gameDataPath + "unityMinecraftData" + "/GameData/" + path);
                fs.Close();
            }
        }
       
    }

    private ConcurrentDictionary<Vector2Int, ChunkData> TryLoadWorldDataDictionary(string dataPath)
    {
        CreateGameDataDirectoryIfNotExist(dataPath);

        try
        {
            byte[] data = File.ReadAllBytes(gameDataPath + "unityMinecraftData/GameData/" + dataPath);
        
            if (data.Length > 0)
            {
                return MessagePackSerializer.Deserialize<ConcurrentDictionary<Vector2Int, ChunkData>>(data, lz4Options);
            }
            else
            {
                return new ConcurrentDictionary<Vector2Int, ChunkData>();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("loading world data failed: "+ex.ToString());
            return null;
        }
    }

    private List<EntityData> TryLoadEntityData(string dataPath)
    {
        CreateGameDataDirectoryIfNotExist(dataPath);

        try
        {
            byte[] data = File.ReadAllBytes(gameDataPath + "unityMinecraftData/GameData/" + dataPath);
         
            if (data.Length > 0)
            {
                return MessagePackSerializer.Deserialize<List<EntityData>>(data, lz4Options);
            }
            else
            {
                return new List<EntityData>();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("loading entity data failed: " + ex.ToString());
            return null;
        }
    }

    private List<ItemEntityData> TryLoadItemEntityData(string dataPath)
    {
        CreateGameDataDirectoryIfNotExist(dataPath);

        try
        {
            byte[] data = File.ReadAllBytes(gameDataPath + "unityMinecraftData/GameData/" + dataPath);
         
            if (data.Length > 0)
            {
                return MessagePackSerializer.Deserialize<List<ItemEntityData>>(data, lz4Options);
            }
            else
            {
                return new List<ItemEntityData>();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("loading item entity data failed: " + ex.ToString());
            return null;
        }
    }


    private PlayerData TryLoadPlayerData(string dataPath)
    {
        CreateGameDataDirectoryIfNotExist(dataPath);

        try
        {
            byte[] data = File.ReadAllBytes(gameDataPath + "unityMinecraftData/GameData/" + dataPath);
          
            if (data.Length > 0)
            {
                return MessagePackSerializer.Deserialize<PlayerData>(data, lz4Options);
            }
            else
            {
                return new PlayerData(20,0,150,0,new int[1], new int[1],0,0);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("loading player data failed: " + ex.ToString());
            return null;
        }
    }

    private void TruncateGameDataIfExist(string path)
    {
        FileStream fs;
        if (File.Exists(gameDataPath + "unityMinecraftData/GameData/" + path))
        {
            fs = new FileStream(gameDataPath + "unityMinecraftData/GameData/" + path, FileMode.Truncate, FileAccess.Write);
        }
        else
        {
            fs = new FileStream(gameDataPath + "unityMinecraftData/GameData/" + path, FileMode.Create, FileAccess.Write);
        }
        fs.Close();
    }


    private void SaveWorldDataToPath(ref ConcurrentDictionary<Vector2Int,ChunkData> savingChunkDatas,string path)
    {

        TruncateGameDataIfExist(path);
       
        Debug.Log("saving chunks count:" + savingChunkDatas.Count);
      
        byte[] tmpData = MessagePackSerializer.Serialize(savingChunkDatas, lz4Options);
        File.WriteAllBytes(gameDataPath + "unityMinecraftData/GameData/" + path, tmpData);
        
    }

    private void SaveEntityDataToPath(ref List<EntityData> savingEntityDatas, string path)
    {

        TruncateGameDataIfExist(path);

        Debug.Log("saving entities count:" + savingEntityDatas.Count);
       
        byte[] tmpData = MessagePackSerializer.Serialize(savingEntityDatas, lz4Options);
        File.WriteAllBytes(gameDataPath + "unityMinecraftData/GameData/" + path, tmpData);

    }


    private void SaveItemEntityDataToPath(ref List<ItemEntityData> savingItemEntityDatas, string path)
    {

        TruncateGameDataIfExist(path);

        Debug.Log("saving item entities count:" + savingItemEntityDatas.Count);

        byte[] tmpData = MessagePackSerializer.Serialize(savingItemEntityDatas, lz4Options);
        File.WriteAllBytes(gameDataPath + "unityMinecraftData/GameData/" + path, tmpData);

    }


    private void SavePlayerDataToPath(ref PlayerData savingPlayerData, string path)
    {

        TruncateGameDataIfExist(path);

        byte[] tmpData = MessagePackSerializer.Serialize(savingPlayerData, lz4Options);
        File.WriteAllBytes(gameDataPath + "unityMinecraftData/GameData/" + path, tmpData);

    }


    public void LoadWorldDataDictionary(int worldID, string path)
    {
        var dictionaryFromFile = TryLoadWorldDataDictionary(path);
        if (dictionaryFromFile != null)
        {
            if (allWorldsDataReadFormFile.ContainsKey(worldID))
            {
                allWorldsDataReadFormFile.Remove(worldID);
            }
            allWorldsDataReadFormFile.Add(worldID, dictionaryFromFile);
        }
    }

    public void SaveWorldDataDictionary(int worldID, string path)
    {
        if (!IsWorldDataLoaded(worldID))
        {
            throw new Exception("data of world "+ worldID+" has not been loaded");
        }
        var worldDataDictionary = allWorldsDataReadFormFile[worldID];
        SaveWorldDataToPath(ref worldDataDictionary,path);
    }

    public void LoadEntityDataList(int worldID, string path)
    {
        var listFromFile = TryLoadEntityData(path);
        if (listFromFile != null)
        {

            if (entityDataReadFromFile.ContainsKey(worldID))
            {
                entityDataReadFromFile.Remove(worldID);
            }
            entityDataReadFromFile.Add(worldID,listFromFile);


        }
    }

    public void SaveEntityDataList(int worldID, string path)
    {
        if (!IsEntityDataLoaded(worldID))
        {
            throw new Exception("entity data has not been loaded");
        }
        var entityDataList = entityDataReadFromFile[worldID];
        SaveEntityDataToPath(ref entityDataList,path);
    }

    public void LoadItemEntityDataList(int worldID, string path)
    {
        var listFromFile = TryLoadItemEntityData(path);
        if (listFromFile != null)
        {
            if (itemEntityDataReadFromFile.ContainsKey(worldID))
            {
                itemEntityDataReadFromFile.Remove(worldID);
            }
            itemEntityDataReadFromFile.Add(worldID, listFromFile);

        }
    }


    public void SaveItemEntityDataList(int worldID, string path)
    {
        if (!IsItemEntityDataLoaded(worldID))
        {
            throw new Exception("item entity data has not been loaded");
        }
        var itemEntityDataList = itemEntityDataReadFromFile[worldID];
        SaveItemEntityDataToPath(ref itemEntityDataList, path);
    }

    public void LoadPlayerData(string path)
    {
        var itemFromFile = TryLoadPlayerData(path);
        if (itemFromFile != null)
        {

            playerDataReadFromFile = itemFromFile;
            isPlayerDataLoaded=true;
        }
    }


    public void SavePlayerData(string path)
    {
        if (isPlayerDataLoaded == false)
        {
            throw new Exception("player data has not been loaded");
        }
        var playerData = playerDataReadFromFile;
        SavePlayerDataToPath(ref playerData, path);
    }

    public void LoadPlayerData()
    {
        LoadPlayerData("playerdata.dat");
    }

    public void SavePlayerData()
    {
        SavePlayerData("playerdata.dat");
    }

    public void LoadAllDataOfSingleWorld(int worldID)
    {
        if (IsWorldDataLoaded(worldID))
        {
            Debug.Log("world " + worldID + " has been loaded into memory. truncating");
        }
        LoadWorldDataDictionary(worldID,"world"+worldID.ToString()+".dat");
        LoadEntityDataList(worldID, "world" + worldID.ToString() + "entities.dat");
        LoadItemEntityDataList(worldID, "world" + worldID.ToString() + "itementities.dat");
    }

    public void SaveAllDataOfSingleWorld(int worldID)
    {
        if (!IsWorldDataLoaded(worldID))
        {
            throw new Exception("world " + worldID + " has not been loaded");
        }
        SaveWorldDataDictionary(worldID, "world" + worldID.ToString() + ".dat");
        SaveEntityDataList(worldID, "world" + worldID.ToString() + "entities.dat");
        SaveItemEntityDataList(worldID, "world" + worldID.ToString() + "itementities.dat");
    }
    public void SaveAllDataOfSingleWorld(int worldID,bool releaseResourcesAfterSaving)
    {
        if (!IsWorldDataLoaded(worldID))
        {
            throw new Exception("world " + worldID + " has not been loaded");
        }
        SaveWorldDataDictionary(worldID, "world" + worldID.ToString() + ".dat");
        SaveEntityDataList(worldID, "world" + worldID.ToString() + "entities.dat");
        SaveItemEntityDataList(worldID, "world" + worldID.ToString() + "itementities.dat");
        if (releaseResourcesAfterSaving == true)
        {
            allWorldsDataReadFormFile[worldID] = new ConcurrentDictionary<Vector2Int, ChunkData>();
            entityDataReadFromFile[worldID] = new List<EntityData>();
            itemEntityDataReadFromFile[worldID] = new List<ItemEntityData>();
        }

    }
    public void ClearAllSavedDatasInFile()
    {
        CreateGameDataDirectoryIfNotExist("",true);
        foreach (string d in Directory.GetFileSystemEntries(gameDataPath + "unityMinecraftData" + "/GameData/"))
        {
            if (File.Exists(d))
            {
                File.Delete(d);
            }
        }

    }
}
