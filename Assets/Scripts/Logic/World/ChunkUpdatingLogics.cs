using System;
using UnityEngine;

public enum ChunkUpdateTypes
{
    BlockPlacedUpdate,
    BlockBreakUpdate,
    BlockRefreshedUpdate,
    DoorInteractionUpdate
}
public partial class Chunk 
{
/*
    [Obsolete]
    public void BFSInit(int x, int y, int z,ChunkUpdateTypes updatingType= ChunkUpdateTypes.BlockRefreshedUpdate, BlockData? optionalPrevBlockData = null)
    {
        BFSMapUpdate(x, y, z, updatingType,optionalPrevBlockData);
    }
    [Obsolete]
    public void UpdateBlock(int x, int y, int z, ChunkUpdateTypes updatingType = ChunkUpdateTypes.BlockRefreshedUpdate,BlockData? optionalPrevBlockData=null)
    {

        if (updatingType == ChunkUpdateTypes.BlockPlacedUpdate)
        {

            if (blockInfosNew.ContainsKey(
                    WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z))))
            {

                if (blockInfosNew[WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z))]
                        .shape == BlockShape.Door)
                {
                    BlockShape? shapeUp =
                        WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(chunkPos.x + x, y+1, chunkPos.y + z));



                    if (shapeUp is not BlockShape.Door)
                    {
                        BlockData data = WorldUpdateablesMediator.instance.GetBlockData(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                        bool[] dataBinary = MathUtility.GetBooleanArray(data.optionalDataValue);
                        dataBinary[5] = true;
                        WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x, y+1, chunkPos.y + z), data.blockID);
                        WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(new Vector3Int(chunkPos.x + x, y + 1, chunkPos.y + z), MathUtility.GetByte(dataBinary));
                        //   Debug.WriteLine(dataBinary[7]);
                    }
                }
            }

        }

        if (updatingType == ChunkUpdateTypes.BlockBreakUpdate)
        {

          //  Debug.Log("break block update");


            if (optionalPrevBlockData != null)
            {
                BlockShape? shapeThis =
                    WorldUpdateablesMediator.instance.GetBlockShape(optionalPrevBlockData.Value);

                if (shapeThis is BlockShape.Door)
                {
               //     Debug.Log("break door update");
                    BlockData dataThis = optionalPrevBlockData.Value;

                    bool[] dataBinary = MathUtility.GetBooleanArray(dataThis.optionalDataValue);
                    BlockShape? shapeUp =
                        WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(chunkPos.x + x, y + 1, chunkPos.y + z));
                    BlockShape? shapeDown =
                        WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z));

                    Debug.Log("break door update:"+ dataBinary[5]);
                    //break door bottom
                    if (dataBinary[5] == false)
                    {
                        if (shapeUp is BlockShape.Door)
                        {
                            WorldHelper.instance.SetBlockWithoutUpdate(
                                new Vector3(chunkPos.x + x, y + 1, chunkPos.y + z), (short)0);

                            //   Debug.WriteLine(dataBinary[7]);
                        }
                    }
                    else
                    {
                        if (shapeDown is BlockShape.Door)
                        {
                            WorldHelper.instance.SetBlockWithoutUpdate(
                                new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z), (short)0);

                            //   Debug.WriteLine(dataBinary[7]);
                        }
                    }
                }
                
            }
            
        }


        if (updatingType == ChunkUpdateTypes.DoorInteractionUpdate)
        {
            if (blockInfosNew.ContainsKey(
                    WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z))))
            {

                if (blockInfosNew[WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z))]
                        .shape == BlockShape.Door)
                {
                   BlockData dataThis=
                        WorldUpdateablesMediator.instance.GetBlockData(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                    bool[] dataBinary = MathUtility.GetBooleanArray(dataThis.optionalDataValue);
                    BlockShape? shapeUp =
                        WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(chunkPos.x + x, y + 1, chunkPos.y + z));
                    BlockShape? shapeDown =
                        WorldUpdateablesMediator.instance.GetBlockShape(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z));

                    dataBinary[4] = !dataBinary[4];
                    byte thisDataByte = MathUtility.GetByte(dataBinary);
                    WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(
                        new Vector3Int(chunkPos.x + x, y, chunkPos.y + z), thisDataByte);

                    //break door bottom
                    if (dataBinary[5] == false)
                    {
                        if (shapeUp is BlockShape.Door)
                        {

                            BlockData dataUp =
                                WorldUpdateablesMediator.instance.GetBlockData(new Vector3(chunkPos.x + x, y+1, chunkPos.y + z));
                            bool[] dataBinaryUp = MathUtility.GetBooleanArray(dataUp.optionalDataValue);

                            dataBinaryUp[4] = !dataBinaryUp[4];
                            byte upDataByte = MathUtility.GetByte(dataBinaryUp);
                            WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(
                                new Vector3Int(chunkPos.x + x, y + 1, chunkPos.y + z), upDataByte);

                            //   Debug.WriteLine(dataBinary[7]);
                        }
                    }
                    else
                    {
                        if (shapeDown is BlockShape.Door)
                        {
                            BlockData dataDown =
                                WorldUpdateablesMediator.instance.GetBlockData(new Vector3(chunkPos.x + x, y -1, chunkPos.y + z));
                            bool[] dataBinaryDown = MathUtility.GetBooleanArray(dataDown.optionalDataValue);

                            dataBinaryDown[4] = !dataBinaryDown[4];
                            byte upDataByte = MathUtility.GetByte(dataBinaryDown);
                            WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(
                                new Vector3Int(chunkPos.x + x, y -1, chunkPos.y + z), upDataByte);
                            //   Debug.WriteLine(dataBinary[7]);
                        }
                    }


                }
            }
        }

        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 101 &&
            WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z)) == 0)
        {
            WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
        }

        if (blockInfosNew.ContainsKey(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z))))
        {
            if (blockInfosNew[WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z))].shape == BlockShape.Torch)
            {
                BlockData curBlockData = WorldUpdateablesMediator.instance.GetBlockData(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                switch (curBlockData.optionalDataValue)
                {
                    case 0://positiveY
                        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z)) == 0)
                        {
                            WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                        }
                        break;
                    case 1://negativeX
                        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x + 1, y, chunkPos.y + z)) == 0)
                        {
                            WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                        }
                        break;
                    case 2:
                        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x - 1, y, chunkPos.y + z)) == 0)
                        {
                            WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                        }
                        break;
                    case 3:
                        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z + 1)) == 0)
                        {
                            WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                        }
                        break;
                    case 4:
                        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z - 1)) == 0)
                        {
                            WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                        }
                        break;
                }

                //    WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
            }
            else if (blockInfosNew[WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z))].shape ==
                      BlockShape.Fence)
            {

                BlockShape? shapeThis =
                    WorldUpdateablesMediator.instance.GetBlockShape(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z) + new Vector3Int(0, 0, 0));
                if (shapeThis is not BlockShape.Fence)
                {
                    return;
                }
                BlockShape? shapeRight =
                    WorldUpdateablesMediator.instance.GetBlockShape(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z) + new Vector3Int(0, 0, 0) + new Vector3Int(1, 0, 0));
                BlockShape? shapeLeft =
                    WorldUpdateablesMediator.instance.GetBlockShape(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z) + new Vector3Int(0, 0, 0) + new Vector3Int(-1, 0, 0));
                BlockShape? shapeFront =
                    WorldUpdateablesMediator.instance.GetBlockShape(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z) + new Vector3Int(0, 0, 0) + new Vector3Int(0, 0, 1));
                BlockShape? shapeBack =
                    WorldUpdateablesMediator.instance.GetBlockShape(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z) + new Vector3Int(0, 0, 0) + new Vector3Int(0, 0, -1));
                bool[] shapes = new[] { false, false, false, false, false, false, false, false };
                if (shapeLeft != null && (shapeLeft.Value == BlockShape.Fence || shapeLeft.Value == BlockShape.Solid))
                {
                    shapes[7] = true;
                }
                else
                {
                    shapes[7] = false;
                }

                if (shapeRight != null && (shapeRight.Value == BlockShape.Fence || shapeRight.Value == BlockShape.Solid))
                {
                    shapes[6] = true;
                }
                else
                {
                    shapes[6] = false;
                }

                if (shapeBack != null && (shapeBack.Value == BlockShape.Fence || shapeBack.Value == BlockShape.Solid))
                {
                    shapes[5] = true;
                }
                else
                {
                    shapes[5] = false;
                }

                if (shapeFront != null && (shapeFront.Value == BlockShape.Fence || shapeFront.Value == BlockShape.Solid))
                {
                    shapes[4] = true;
                }
                else
                {
                    shapes[4] = false;
                }
                //     Debug.WriteLine("from::"+updateFromPoint);
                WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z), MathUtility.GetByte(shapes));

            }

        }

        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 &&
            WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z)) == 0)
        {
            WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z), 100);
        }

        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 &&
            WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x - 1, y, chunkPos.y + z)) == 0)
        {
            WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x - 1, y, chunkPos.y + z), 100);
        }

        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 &&
            WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x + 1, y, chunkPos.y + z)) == 0)
        {
            WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x + 1, y, chunkPos.y + z), 100);
        }

        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 &&
            WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z - 1)) == 0)
        {
            WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x, y, chunkPos.y + z - 1), 100);
        }

        if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 &&
            WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z + 1)) == 0)
        {
            WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x, y, chunkPos.y + z + 1), 100);
        }
    }
    [Obsolete]
    public void BFSMapUpdate(int x, int y, int z,ChunkUpdateTypes updatingType= ChunkUpdateTypes.BlockRefreshedUpdate, BlockData? optionalPrevBlockData = null)
    {
        //left right bottom top back front
        //left x-1 right x+1 top y+1 bottom y-1 back z-1 front z+1


        /*   if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 101 &&
               WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z)) == 0)
           {
               WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
           }

           if (blockInfosNew.ContainsKey(WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z))))
           {
               if (blockInfosNew[WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z))].shape == BlockShape.Torch)
               {
                   BlockData curBlockData = WorldUpdateablesMediator.instance.GetBlockData(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                   switch (curBlockData.optionalDataValue)
                   {
                       case 0:
                           if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z)) == 0)
                           {
                               WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                           }
                           break;
                       case 1:
                           if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x + 1, y, chunkPos.y + z)) == 0)
                           {
                               WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                           }
                           break;
                       case 2:
                           if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x - 1, y, chunkPos.y + z)) == 0)
                           {
                               WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                           }
                           break;
                       case 3:
                           if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z + 1)) == 0)
                           {
                               WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                           }
                           break;
                       case 4:
                           if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z - 1)) == 0)
                           {
                               WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                           }
                           break;
                   }

                   //    WorldHelper.instance.BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
               }


               if (blockInfosNew[WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z))].shape ==
                   BlockShape.Fence)
               {

                   BlockShape? shapeThis =
                       WorldUpdateablesMediator.instance.GetBlockShape(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z) + new Vector3Int(0, 0, 0));
                   if (shapeThis is not BlockShape.Fence)
                   {
                       return;
                   }
                   BlockShape? shapeRight =
                       WorldUpdateablesMediator.instance.GetBlockShape(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z) + new Vector3Int(0, 0, 0) + new Vector3Int(1, 0, 0));
                   BlockShape? shapeLeft =
                       WorldUpdateablesMediator.instance.GetBlockShape(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z) + new Vector3Int(0, 0, 0) + new Vector3Int(-1, 0, 0));
                   BlockShape? shapeFront =
                       WorldUpdateablesMediator.instance.GetBlockShape(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z) + new Vector3Int(0, 0, 0) + new Vector3Int(0, 0, 1));
                   BlockShape? shapeBack =
                       WorldUpdateablesMediator.instance.GetBlockShape(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z) + new Vector3Int(0, 0, 0) + new Vector3Int(0, 0, -1));
                   bool[] shapes = new[] { false, false, false, false, false, false, false, false };
                   if (shapeLeft != null && (shapeLeft.Value == BlockShape.Fence || shapeLeft.Value == BlockShape.Solid))
                   {
                       shapes[7] = true;
                   }
                   else
                   {
                       shapes[7] = false;
                   }

                   if (shapeRight != null && (shapeRight.Value == BlockShape.Fence || shapeRight.Value == BlockShape.Solid))
                   {
                       shapes[6] = true;
                   }
                   else
                   {
                       shapes[6] = false;
                   }

                   if (shapeBack != null && (shapeBack.Value == BlockShape.Fence || shapeBack.Value == BlockShape.Solid))
                   {
                       shapes[5] = true;
                   }
                   else
                   {
                       shapes[5] = false;
                   }

                   if (shapeFront != null && (shapeFront.Value == BlockShape.Fence || shapeFront.Value == BlockShape.Solid))
                   {
                       shapes[4] = true;
                   }
                   else
                   {
                       shapes[4] = false;
                   }
                   //     Debug.WriteLine("from::"+updateFromPoint);
                   WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(new Vector3Int(chunkPos.x + x, y, chunkPos.y + z), MathUtility.GetByte(shapes));

               }

           }






           if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 &&
               WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z)) == 0)
           {
               WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z), 100);
           }

           if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 &&
               WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x - 1, y, chunkPos.y + z)) == 0)
           {
               WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x - 1, y, chunkPos.y + z), 100);
           }

           if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 &&
               WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x + 1, y, chunkPos.y + z)) == 0)
           {
               WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x + 1, y, chunkPos.y + z), 100);
           }

           if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 &&
               WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z - 1)) == 0)
           {
               WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x, y, chunkPos.y + z - 1), 100);
           }

           if (WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 &&
               WorldHelper.instance.GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z + 1)) == 0)
           {
               WorldHelper.instance.SetBlockWithoutUpdate(new Vector3(chunkPos.x + x, y, chunkPos.y + z + 1), 100);
           }
        UpdateBlock(x, y, z, updatingType,optionalPrevBlockData);
        UpdateBlock(x - 1, y, z);
        UpdateBlock(x + 1, y, z);
        UpdateBlock(x, y - 1, z);
        UpdateBlock(x, y + 1, z);
        UpdateBlock(x, y, z - 1);
        UpdateBlock(x, y, z + 1);
    }
*/

}
