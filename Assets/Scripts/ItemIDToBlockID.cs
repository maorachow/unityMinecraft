using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemIDToBlockID 
{
    public static Dictionary<int,int> blockIDToItemIDDic=new Dictionary<int,int>();
    public static Dictionary<int,int> ItemIDToBlockIDDic=new Dictionary<int,int>();
    public static void InitDic(){
        blockIDToItemIDDic.Clear();
        blockIDToItemIDDic.TryAdd(0,-1);
        blockIDToItemIDDic.TryAdd(1,1);
        blockIDToItemIDDic.TryAdd(2,2);
        blockIDToItemIDDic.TryAdd(3,3);
        blockIDToItemIDDic.TryAdd(4,4);
        blockIDToItemIDDic.TryAdd(5,5);
        blockIDToItemIDDic.TryAdd(6,6);
        blockIDToItemIDDic.TryAdd(7,7);
        blockIDToItemIDDic.TryAdd(8,8);
        blockIDToItemIDDic.TryAdd(9,9);
        blockIDToItemIDDic.TryAdd(10,153);
        blockIDToItemIDDic.TryAdd(11,11);
        blockIDToItemIDDic.TryAdd(12, 12);
        blockIDToItemIDDic.TryAdd(13, 13);
        blockIDToItemIDDic.TryAdd(100,100);
        blockIDToItemIDDic.TryAdd(101,101);
        blockIDToItemIDDic.TryAdd(102,102);
        ItemIDToBlockIDDic.Clear();
        ItemIDToBlockIDDic.TryAdd(0,-1);
        ItemIDToBlockIDDic.TryAdd(1,1);
        ItemIDToBlockIDDic.TryAdd(2,2);
        ItemIDToBlockIDDic.TryAdd(3,3);
        ItemIDToBlockIDDic.TryAdd(4,4);
        ItemIDToBlockIDDic.TryAdd(5,5);
        ItemIDToBlockIDDic.TryAdd(6,6);
        ItemIDToBlockIDDic.TryAdd(7,7);
        ItemIDToBlockIDDic.TryAdd(8,8);
        ItemIDToBlockIDDic.TryAdd(9,9);
        ItemIDToBlockIDDic.TryAdd(10,10);
        ItemIDToBlockIDDic.TryAdd(11,11);
        ItemIDToBlockIDDic.TryAdd(12, 12);
        ItemIDToBlockIDDic.TryAdd(13, 13);
        ItemIDToBlockIDDic.TryAdd(153,-1);
        ItemIDToBlockIDDic.TryAdd(154,-1);
        ItemIDToBlockIDDic.TryAdd(152,-1);
        ItemIDToBlockIDDic.TryAdd(151,-1);
        ItemIDToBlockIDDic.TryAdd(100,100);
        ItemIDToBlockIDDic.TryAdd(101,101);
        ItemIDToBlockIDDic.TryAdd(102,102);
        ItemIDToBlockIDDic.TryAdd(155, -1);
        ItemIDToBlockIDDic.TryAdd(156, -1);
        ItemIDToBlockIDDic.TryAdd(157, -1);
        ItemIDToBlockIDDic.TryAdd(158, -1);
        //   ItemIDToBlockIDDic.TryAdd();

    }
}
