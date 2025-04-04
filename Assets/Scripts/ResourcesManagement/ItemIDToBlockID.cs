using System.Collections.Generic;

public class ItemIDToBlockIDMapper:IItemIDToBlockIDMapper
{
    public ItemIDToBlockIDMapper()
    {

    }

   
    public Dictionary<BlockData,int> blockIDToItemIDDic=new Dictionary<BlockData, int>();
    public Dictionary<int, BlockData> ItemIDToBlockIDDic=new Dictionary<int, BlockData>();

    public bool CanMapToBlockID(int itemID)
    {
        if (ItemIDToBlockIDDic.ContainsKey(itemID))
        {
            if (ItemIDToBlockIDDic[itemID] != -1)
            {
                return true;
            }
            else
            {
                return false;   
            }
           
        }
        else
        {
            return false;
        }
    }
    public BlockData ToBlockID(int itemID)
    {
        if (ItemIDToBlockIDDic.ContainsKey(itemID))
        {
            return ItemIDToBlockIDDic[itemID];
        }
        else
        {
            return -1;
        }
    }



    public bool CanMapToItemID(BlockData blockID)
    {
        if (blockIDToItemIDDic.ContainsKey(blockID.blockID))
        {
            if (blockIDToItemIDDic[blockID.blockID] != -1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        else
        {
            return false;
        }
    }
    public int ToItemID(BlockData blockID)
    {
        if (blockIDToItemIDDic.ContainsKey(blockID))
        {
            return blockIDToItemIDDic[blockID];
        }
        else
        {
            return -1;
        }
    }

    public void Initialize()
    {
        InitDic();
    }
    public void InitDic(){
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
        blockIDToItemIDDic.TryAdd(103, 103);

        blockIDToItemIDDic.TryAdd(104, 104);

        blockIDToItemIDDic.TryAdd(106, 106);
        blockIDToItemIDDic.TryAdd(107, 107);
        blockIDToItemIDDic.TryAdd(108, 108);
        blockIDToItemIDDic.TryAdd(109, 109);
        blockIDToItemIDDic.TryAdd(110, 110);
        blockIDToItemIDDic.TryAdd(111, 111);
        ItemIDToBlockIDDic.TryAdd(156, -1);
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
        ItemIDToBlockIDDic.TryAdd(103, 103);

        ItemIDToBlockIDDic.TryAdd(104, 104);

        ItemIDToBlockIDDic.TryAdd(106, 106);
        ItemIDToBlockIDDic.TryAdd(107, 107);
        ItemIDToBlockIDDic.TryAdd(108, 108);
        ItemIDToBlockIDDic.TryAdd(109, 109);
        ItemIDToBlockIDDic.TryAdd(110, 110);
        ItemIDToBlockIDDic.TryAdd(111, 111);

        ItemIDToBlockIDDic.TryAdd(155, -1);
        ItemIDToBlockIDDic.TryAdd(156, 156);
        ItemIDToBlockIDDic.TryAdd(157, -1);
        ItemIDToBlockIDDic.TryAdd(158, -1);
        //   ItemIDToBlockIDDic.TryAdd();

    }
}
