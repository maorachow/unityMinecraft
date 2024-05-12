using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;
using System.IO;
using MessagePack;
public class AssetBundlePacker : Editor
{
    [MenuItem("Tools/CreatAssetBundle for Android")]
  
    static void CreatAssetBundle()
    {

        string path = Application.dataPath+"/AssetBundles";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.Android);
        UnityEngine.Debug.Log("Android Finish!");
    }

    [MenuItem("Tools/CreatAssetBundle for IOS")]
    static void BuildAllAssetBundlesForIOS()
    {
        string dirName =  Application.dataPath+"AssetBundles/IOS";
        if (!Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName);
        }
        BuildPipeline.BuildAssetBundles(dirName, BuildAssetBundleOptions.None, BuildTarget.iOS);
        UnityEngine.Debug.Log("IOS Finish!");

    }


    [MenuItem("Tools/CreatAssetBundle for Win")]
    static void CreatPCAssetBundleForwINDOWS()
    {
        string path =  Application.dataPath+"/AssetBundles/Win";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        UnityEngine.Debug.Log("Windows Finish!");
    }
       [MenuItem("Tools/GenerateBlockInfoDicJson")]
    static void GenerateBlockInfoDicJson(){
          string path =  Application.dataPath+"/AssetBundles/jsonData";
          FileStream fs=File.Create(path+"/blockterraininfo.dat");
          fs.Close();
          File.WriteAllBytes(path+"/blockterraininfo.dat",MessagePackSerializer.Serialize(Chunk.blockInfo));/*new  Dictionary<int,List<Vector2>>{
    {  1,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)}},
    { 2,new List<Vector2>{new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f)}},
    {     3,new List<Vector2>{new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f)}},
    {  4,new List<Vector2>{new Vector2(0.1875f,0f),new Vector2(0.1875f,0f),new Vector2(0.125f,0f),new Vector2(0.0625f,0f),new Vector2(0.1875f,0f),new Vector2(0.1875f,0f)}},
    {  100,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)}},
    {  101,new List<Vector2>{new Vector2(0f,0.0625f)}},
    { 5,  new List<Vector2>{new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f)}},
    {   6,new List<Vector2>{new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f)}},
    { 7,new List<Vector2>{new Vector2(0.3125f,0f),new Vector2(0.3125f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.3125f,0f),new Vector2(0.3125f,0f)}},
   {    8,new List<Vector2>{new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f)}},
   {  9,new List<Vector2>{new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f)}},
  { 10,new List<Vector2>{new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f)}},
  { 11,new List<Vector2>{new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f)}}
     
          })*/
    }
      [MenuItem("Tools/GenerateItemBlockDicJson")]
    static void GenerateItemBlockDicJson(){
         string path =  Application.dataPath+"/AssetBundles/jsonData"; 
           FileStream fs=File.Create(path+"/itemblockinfo.dat");
          fs.Close();
        File.WriteAllBytes(path+"/itemblockinfo.dat",MessagePackSerializer.Serialize(Chunk.itemBlockInfo/*new Dictionary<int,List<Vector2>>{
    {1,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)}},
       {2,new List<Vector2>{new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f)}},
       {3,new List<Vector2>{new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f)}},
       {4,new List<Vector2>{new Vector2(0.1875f,0f),new Vector2(0.1875f,0f),new Vector2(0.125f,0f),new Vector2(0.0625f,0f),new Vector2(0.1875f,0f),new Vector2(0.1875f,0f)}},
       {100,new List<Vector2>{new Vector2(0.125f,0.0625f),new Vector2(0.125f,0.0625f),new Vector2(0.125f,0.0625f),new Vector2(0.125f,0.0625f),new Vector2(0.125f,0.0625f),new Vector2(0.125f,0.0625f)}},
       {101,new List<Vector2>{new Vector2(0f,0.0625f)}},
       {5,new List<Vector2>{new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f)}},
      {6,new List<Vector2>{new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f)}},
      {7,new List<Vector2>{new Vector2(0.3125f,0f),new Vector2(0.3125f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.3125f,0f),new Vector2(0.3125f,0f)}},
      {8,new List<Vector2>{new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f)}},
       {11,new List<Vector2>{new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f),new Vector2(0.625f,0f)}},
       {9,new List<Vector2>{new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f)}}
       
        }*/));
    }
      [MenuItem("Tools/GenerateBlockNameDicJson")]
    static void GenerateBlockNameDicJson(){
          string path =  Application.dataPath+"/AssetBundles/jsonData";
            FileStream fs=File.Create(path+"/blockname.dat");
          fs.Close();
          File.WriteAllBytes(path+"/blockname.dat",MessagePackSerializer.Serialize(GameUIBeh.blockNameDic/*new Dictionary<int,string>{
          {0,"None"},
       {1,"Stone"},
      {2,"Grass"},
       {3,"Dirt"},
       {4,"Side Grass Block"},
       {5,"Bedrock"},
       {6,"WoodX"},
     {7,"WoodY"},
      {8,"WoodZ"},
     {9,"Leaves"},
      {11,"Sand"},
     {100,"Water"},
     {101,"Grass Crop"},
      {102,"Torch"},
   {151,"Diamond Pickaxe"},
    {152,"Diamond Sword"},
   {153,"Diamond"},
          }*/));
    }
}
