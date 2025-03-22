using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuildingInfoDataProvider : IMeshBuildingInfoDataProvider
{
    public ItemMeshBuildingInfo defaultItemInfo=new ItemMeshBuildingInfo(true);
   
    public BlockInfo defaultBlockInfo = new BlockInfo(

        new List<Vector2>
        {
            new Vector2(0, 1-0.0625f), new Vector2(0, 1-0.0625f),
            new Vector2(0, 1-0.0625f),new Vector2(0, 1-0.0625f),
            new Vector2(0, 1-0.0625f),new Vector2(0, 1-0.0625f),
        },
        new List<Vector2>
        {
            new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
            new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
        }, BlockShape.Solid
    );

    public Dictionary<int, ItemMeshBuildingInfo> itemMaterialInfo = new Dictionary<int, ItemMeshBuildingInfo>();

    public Dictionary<int, BlockInfo> blockInfos = new Dictionary<int, BlockInfo>();

    public void InitDefaultItemMaterialInfo()
    {
        itemMaterialInfo.Clear();
        itemMaterialInfo.TryAdd(154, new ItemMeshBuildingInfo(false, new Vector2(0.1875f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(153, new ItemMeshBuildingInfo(false, new Vector2(0.125f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(151, new ItemMeshBuildingInfo(false, new Vector2(0.0625f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(152, new ItemMeshBuildingInfo(false, new Vector2(0.0f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(155, new ItemMeshBuildingInfo(false, new Vector2(0.25f, 0.25f), new Vector2(0.0625f, 0.0625f)));

        itemMaterialInfo.TryAdd(157, new ItemMeshBuildingInfo(false, new Vector2(0.5f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(158, new ItemMeshBuildingInfo(false, new Vector2(0.5625f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(104, new ItemMeshBuildingInfo(false, new Vector2(0.625f, 0.25f), new Vector2(0.0625f, 0.0625f)));
        itemMaterialInfo.TryAdd(1, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(2, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(3, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(4, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(5, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(6, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(7, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(8, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(9, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(10, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(11, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(12, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(13, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(14, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(15, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(16, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(17, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(18, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(19, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(20, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(21, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(22, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(23, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(100, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(101, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(102, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(103, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(104, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(105, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(106, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(107, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(108, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(109, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(110, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(111, new ItemMeshBuildingInfo(true));
        itemMaterialInfo.TryAdd(156, new ItemMeshBuildingInfo(true));
    }

    public void InitDefaultBlockMaterialInfo()
    {
        blockInfos.Clear();
        blockInfos = new Dictionary<int, BlockInfo>
        {
            {
                0,
                new BlockInfo(new List<Vector2>(),new List<Vector2>(),BlockShape.Empty)
            },
            {
                1,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                        new Vector2(0f, 0f), new Vector2(0f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                2,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0f), new Vector2(0.0625f, 0f), new Vector2(0.0625f, 0f),
                        new Vector2(0.0625f, 0f), new Vector2(0.0625f, 0f), new Vector2(0.0625f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                3,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.125f, 0f), new Vector2(0.125f, 0f), new Vector2(0.125f, 0f),
                        new Vector2(0.125f, 0f),
                        new Vector2(0.125f, 0f), new Vector2(0.125f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                4,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.1875f, 0f), new Vector2(0.1875f, 0f), new Vector2(0.125f, 0f),
                        new Vector2(0.0625f, 0f), new Vector2(0.1875f, 0f), new Vector2(0.1875f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                5,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.375f, 0f), new Vector2(0.375f, 0f), new Vector2(0.375f, 0f),
                        new Vector2(0.375f, 0f),
                        new Vector2(0.375f, 0f), new Vector2(0.375f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                6,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                        new Vector2(0.5f, 0f), new Vector2(0.5f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                7,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.3125f, 0f), new Vector2(0.3125f, 0f), new Vector2(0.25f, 0f),
                        new Vector2(0.25f, 0f),
                        new Vector2(0.3125f, 0f), new Vector2(0.3125f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                8,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                        new Vector2(0.25f, 0f), new Vector2(0.25f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                9,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.4375f, 0f), new Vector2(0.4375f, 0f), new Vector2(0.4375f, 0f),
                        new Vector2(0.4375f, 0f), new Vector2(0.4375f, 0f), new Vector2(0.4375f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                10,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.5625f, 0f), new Vector2(0.5625f, 0f), new Vector2(0.5625f, 0f),
                        new Vector2(0.5625f, 0f), new Vector2(0.5625f, 0f), new Vector2(0.5625f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },


            {
                11,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.625f, 0f), new Vector2(0.625f, 0f), new Vector2(0.625f, 0f),
                        new Vector2(0.625f, 0f),
                        new Vector2(0.625f, 0f), new Vector2(0.625f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                12,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f),
                        new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f), new Vector2(0.6875f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                13,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.6875f, 0f),
                        new Vector2(0.8125f, 0f),
                        new Vector2(0.75f, 0f), new Vector2(0.75f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                14,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.1875f, 0.0625f), new Vector2(0.1875f, 0.0625f), new Vector2(0.1875f, 0.0625f),
                        new Vector2(0.1875f, 0.0625f), new Vector2(0.1875f, 0.0625f), new Vector2(0.1875f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                15,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.875f, 0f), new Vector2(0.875f, 0f), new Vector2(0.875f, 0f),
                        new Vector2(0.875f, 0f),
                        new Vector2(0.875f, 0f), new Vector2(0.875f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                16,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.9375f, 0f), new Vector2(0.9375f, 0f), new Vector2(0.9375f, 0f),
                        new Vector2(0.9375f, 0f), new Vector2(0.9375f, 0f), new Vector2(0.9375f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                17,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f),
                        new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                18,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.3125f, 0.0625f), new Vector2(0.3125f, 0.0625f), new Vector2(0.3125f, 0.0625f),
                        new Vector2(0.3125f, 0.0625f), new Vector2(0.3125f, 0.0625f), new Vector2(0.3125f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                19,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.375f, 0.0625f), new Vector2(0.375f, 0.0625f), new Vector2(0.375f, 0.0625f),
                        new Vector2(0.375f, 0.0625f), new Vector2(0.375f, 0.0625f), new Vector2(0.375f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                20,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.4375f, 0.0625f), new Vector2(0.4375f, 0.0625f), new Vector2(0.4375f, 0.0625f),
                        new Vector2(0.4375f, 0.0625f), new Vector2(0.4375f, 0.0625f), new Vector2(0.4375f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },


            {
                23,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.5f, 0.0625f), new Vector2(0.5f, 0.0625f), new Vector2(0.5f, 0.0625f),
                        new Vector2(0.5f, 0.0625f), new Vector2(0.5f, 0.0625f), new Vector2(0.5f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },


            {
                22,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.5625f, 0.0625f), new Vector2(0.5625f, 0.0625f), new Vector2(0.5625f, 0.0625f),
                        new Vector2(0.5625f, 0.0625f), new Vector2(0.5625f, 0.0625f), new Vector2(0.5625f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
            {
                21,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f),
                        new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Slabs)
            },
            {
                100,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0f, 0.0625f), new Vector2(0f, 0.0625f), new Vector2(0f, 0.0625f),
                        new Vector2(0f, 0.0625f), new Vector2(0f, 0.0625f), new Vector2(0f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Water)
            },

            {
                101,
                new BlockInfo(new List<Vector2> { new Vector2(0.0625f, 0.0625f) },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.CrossModel)
            },
            {
                102, new BlockInfo(new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0f) + new Vector2(0.0625f, 0f),
                        new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0f) + new Vector2(0.0625f, 0f),
                        new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0f) + new Vector2(0.0625f, 0f),
                        new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0.03125f) + new Vector2(0.0625f, 0f),
                        new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0f) + new Vector2(0.0625f, 0f),
                        new Vector2(0.0625f, 0.0625f) + new Vector2(0.02734375f, 0f) + new Vector2(0.0625f, 0f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0078125f, 0.0390625f),
                        new Vector2(0.0078125f, 0.0390625f),
                        new Vector2(0.0078125f, 0.0078125f),
                        new Vector2(0.0078125f, 0.0078125f),
                        new Vector2(0.0078125f, 0.0390625f),
                        new Vector2(0.0078125f, 0.0390625f)
                    }, BlockShape.Torch)
            },
            {
                103,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f),
                        new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f), new Vector2(0.25f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Fence)
            },
            {
                104,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.9375f, 0.125f), new Vector2(0.9375f, 0.125f), new Vector2(0.9375f, 0.125f),
                        new Vector2(0.9375f, 0.125f), new Vector2(0.9375f, 0.125f), new Vector2(0.9375f, 0.125f),
                        new Vector2(0.9375f, 0.0625f), new Vector2(0.9375f, 0.0625f), new Vector2(0.9375f, 0.0625f),
                        new Vector2(0.9375f, 0.0625f), new Vector2(0.9375f, 0.0625f), new Vector2(0.9375f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.01171875f, 0.0625f),
                        new Vector2(0.01171875f, 0.0625f), new Vector2(0.01171875f, 0.0625f),
                        new Vector2(0.01171875f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.01171875f, 0.0625f),
                        new Vector2(0.01171875f, 0.0625f), new Vector2(0.01171875f, 0.0625f),
                        new Vector2(0.01171875f, 0.0625f)
                    }, BlockShape.Door)
            },
            {
                105,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.875f, 0.0625f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.WallAttachment)
            },
            {
                106,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0f, 0.125f), new Vector2(0f, 0.125f), new Vector2(0f, 0.125f),
                        new Vector2(0f, 0.125f), new Vector2(0f, 0.125f), new Vector2(0f, 0.125f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.SolidTransparent)
            },

            {
                107,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.125f), new Vector2(0.0625f, 0.125f), new Vector2(0.0625f, 0.125f),
                        new Vector2(0.0625f, 0.125f), new Vector2(0.0625f, 0.125f), new Vector2(0.0625f, 0.125f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.SolidTransparent)
            },
            {
                108,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.125f, 0.125f), new Vector2(0.125f, 0.125f), new Vector2(0.125f, 0.125f),
                        new Vector2(0.125f, 0.125f), new Vector2(0.125f, 0.125f), new Vector2(0.125f, 0.125f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.SolidTransparent)
            },

            {
                109,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.1875f, 0.125f), new Vector2(0.1875f, 0.125f), new Vector2(0.1875f, 0.125f),
                        new Vector2(0.1875f, 0.125f), new Vector2(0.1875f, 0.125f), new Vector2(0.1875f, 0.125f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.SolidTransparent)
            },
            {
                110,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.25f, 0.125f), new Vector2(0.25f, 0.125f), new Vector2(0.25f, 0.125f),
                        new Vector2(0.25f, 0.125f), new Vector2(0.25f, 0.125f), new Vector2(0.25f, 0.125f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.SolidTransparent)
            },
            {
                111,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(0.3125f, 0.125f), new Vector2(0.3125f, 0.125f), new Vector2(0.3125f, 0.125f),
                        new Vector2(0.3125f, 0.125f), new Vector2(0.3125f, 0.125f), new Vector2(0.3125f, 0.125f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.SolidTransparent)
            },
            {
                156,
                new BlockInfo(
                    new List<Vector2>
                    {
                        new Vector2(320f / 1024f, 256f / 1024f), new Vector2(320f / 1024f, 256f / 1024f),
                        new Vector2(448f / 1024f, 256f / 1024f), new Vector2(384f / 1024f, 256f / 1024f),
                        new Vector2(320f / 1024f, 256f / 1024f), new Vector2(320f / 1024f, 256f / 1024f)
                    },
                    new List<Vector2>
                    {
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f),
                        new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f), new Vector2(0.0625f, 0.0625f)
                    }, BlockShape.Solid)
            },
        };
    }
    public void InitDefault()
    {
        InitDefaultItemMaterialInfo();
        InitDefaultBlockMaterialInfo();



    }
   
    public void Init()
   {
        
   }

   public BlockInfo GetBlockInfo(BlockData blockID)
   {
       if (IsBlockDataValid(blockID))
       {
           return blockInfos[blockID.blockID];
       }
       else
       {
           return defaultBlockInfo;
       }
   }

   public bool IsBlockDataValid(BlockData blockID)
   {
       return blockInfos.ContainsKey(blockID.blockID);
   }

   public ItemMeshBuildingInfo GetItemMeshBuildingInfo(int itemID)
   {
       if (IsItemIDValid(itemID))
       {
           return itemMaterialInfo[itemID];
       }
       else
       {
           return defaultItemInfo;
       }
    }

   public bool IsItemIDValid(int itemID)
   {
       return itemMaterialInfo.ContainsKey(itemID);
   }
}
