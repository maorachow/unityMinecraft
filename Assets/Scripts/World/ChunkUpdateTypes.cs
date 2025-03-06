using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace monogameMinecraftShared.World
{
    public interface IChunkUpdateOperation
    {
        public Vector3Int position { get; set; }
        public void Update();
    }

    public struct PlacingBlockOperation : IChunkUpdateOperation
    {
        public WorldUpdater worldUpdater;
        public Vector3Int position { get; set; }
        public BlockData placingBlockData;

        public PlacingBlockOperation(Vector3Int position, WorldUpdater worldUpdater, BlockData placingBlockData)
        {
            this.position = position;
            this.worldUpdater = worldUpdater;
            this.placingBlockData = placingBlockData;
        }

        public void Update()
        {
            BlockShape? shapeThis =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(0, 0, 0));

            BlockShape? shapeRight =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(1, 0, 0));
            BlockShape? shapeLeft =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(-1, 0, 0));
            BlockShape? shapeFront =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(0, 0, 1));
            BlockShape? shapeBack =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(0, 0, -1));

            WorldHelper.instance.SetBlockWithoutUpdateWithSaving(position, placingBlockData);

            BlockShape? shapePlaced = WorldHelper.instance.GetBlockShape(placingBlockData);
            if (shapeLeft != null && shapeLeft.Value == BlockShape.Fence)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(-1, 0, 0), worldUpdater, new Vector3Int(1, 0, 0), 1));
            }
            if (shapeRight != null && shapeRight.Value == BlockShape.Fence)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(1, 0, 0), worldUpdater, new Vector3Int(-1, 0, 0), 1));
            }
            if (shapeFront != null && shapeFront.Value == BlockShape.Fence)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(0, 0, 1), worldUpdater, new Vector3Int(0, 0, -1), 1));
            }
            if (shapeBack != null && shapeBack.Value == BlockShape.Fence)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(0, 0, -1), worldUpdater, new Vector3Int(0, 0, 1), 1));
            }

            if (shapePlaced is BlockShape.Fence)
            {
               worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position, worldUpdater, new Vector3Int(0, 0, 0), 0));
            }
            if (shapePlaced is BlockShape.Door&& WorldHelper.instance.GetBlock(position + new Vector3(0, 1, 0)) == 0)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new DoorUpperPartPlacingOperation(position));
            }
        }
    }
    public struct FenceUpdatingOperation : IChunkUpdateOperation
    {
        public WorldUpdater worldUpdater;
        public Vector3Int position { get; set; }
        public Vector3Int updateFromPoint;
        public int stackDepth;
        public FenceUpdatingOperation(Vector3Int position, WorldUpdater worldUpdater, Vector3Int updateFromPoint, int stackDepth)
        {
            this.position = position;
            this.worldUpdater = worldUpdater;
            this.updateFromPoint = updateFromPoint;
            this.stackDepth = stackDepth;
        }
        public void Update()
        {
            BlockShape? shapeThis =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(0, 0, 0));
            if (shapeThis is not BlockShape.Fence)
            {
                return;
            }
            BlockShape? shapeRight =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(1, 0, 0));
            BlockShape? shapeLeft =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(-1, 0, 0));
            BlockShape? shapeFront =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(0, 0, 1));
            BlockShape? shapeBack =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(0, 0, -1));
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
            WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(position, MathUtility.GetByte(shapes));

            if (stackDepth >= 2)
            {
                return;
            }
            if (shapeLeft != null && shapeLeft.Value == BlockShape.Fence && updateFromPoint.x != -1)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(-1, 0, 0), worldUpdater, new Vector3Int(1, 0, 0), stackDepth + 1));
            }
            if (shapeRight != null && shapeRight.Value == BlockShape.Fence && updateFromPoint.x != 1)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(1, 0, 0), worldUpdater, new Vector3Int(-1, 0, 0), stackDepth + 1));
            }
            if (shapeFront != null && shapeFront.Value == BlockShape.Fence && updateFromPoint.z != 1)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(0, 0, 1), worldUpdater, new Vector3Int(0, 0, -1), stackDepth + 1));
            }
            if (shapeBack != null && shapeBack.Value == BlockShape.Fence && updateFromPoint.z != -1)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(0, 0, -1), worldUpdater, new Vector3Int(0, 0, 1), stackDepth + 1));
            }
        }
    }

    public struct BreakBlockOperation : IChunkUpdateOperation
    {
        public Vector3Int position { get; set; }
        public WorldUpdater worldUpdater;
        public BlockData prevBlockData;
        public BreakBlockOperation(Vector3Int position, WorldUpdater worldUpdater, BlockData prevBlockData)
        {
            this.position = position;
            this.worldUpdater = worldUpdater;
            this.prevBlockData = prevBlockData;
        }
        public void Update()
        {
            Vector3Int tempPos = position;
            var key = prevBlockData;
            if (Chunk.blockInfosNew.ContainsKey(key.blockID))
            {
                Vector3Int effctPosition = position;
                BlockData effectPrevBlockData = prevBlockData;
                worldUpdater.onUpdatedOneShot += () =>
                {
                    ParticleEffectManagerBeh.instance.EmitBreakBlockParticleAtPosition(new Vector3(effctPosition.x + 0.5f, effctPosition.y + 0.5f, effctPosition.z + 0.5f), effectPrevBlockData);
                    ItemEntityBeh.SpawnNewItem(effctPosition.x + 0.5f, effctPosition.y + 0.5f, effctPosition.z + 0.5f, ItemIDToBlockID.blockIDToItemIDDic[effectPrevBlockData.blockID], new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f)));
                };
            }

           

            BlockShape? shapeRight =
                WorldHelper.instance.GetBlockShape(position + new Vector3Int(1, 0, 0));
            BlockShape? shapeLeft =
                WorldHelper.instance.GetBlockShape((position + new Vector3Int(-1, 0, 0)));
            BlockShape? shapeFront =
                WorldHelper.instance.GetBlockShape((position + new Vector3Int(0, 0, 1)));
            BlockShape? shapeBack =
                WorldHelper.instance.GetBlockShape((position + new Vector3Int(0, 0, -1)));

            BlockShape? shapeThis =
                WorldHelper.instance.GetBlockShape(prevBlockData);



            BlockShape? shapeUp =
                WorldHelper.instance.GetBlockShape((position + new Vector3Int(0, 1, 0)));

            if (shapeUp is BlockShape.CrossModel || shapeUp is BlockShape.Torch)
            {

                worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(new Vector3Int(position.x, position.y + 1, position.z), worldUpdater, WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 1, 0))));
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new PlacingBlockOperation(new Vector3Int(position.x, position.y + 1, position.z), worldUpdater, 0));
            }
            if (shapeThis is BlockShape.Door)
            {
                Debug.Log("break door");
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new DoorBreakingOperation(position, prevBlockData));
            }
            if (shapeLeft != null && shapeLeft.Value == BlockShape.Water)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new WaterFloodOperation(position + new Vector3Int(-1, 0, 0), worldUpdater, new Vector3Int(1, 0, 0), 1));
            }
            if (shapeRight != null && shapeRight.Value == BlockShape.Water)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new WaterFloodOperation(position + new Vector3Int(1, 0, 0), worldUpdater, new Vector3Int(-1, 0, 0), 1));
            }
            if (shapeFront != null && shapeFront.Value == BlockShape.Water)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new WaterFloodOperation(position + new Vector3Int(0, 0, 1), worldUpdater, new Vector3Int(0, 0, 1), 1));
            }
            if (shapeBack != null && shapeBack.Value == BlockShape.Water)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new WaterFloodOperation(position + new Vector3Int(0, 0, -1), worldUpdater, new Vector3Int(0, 0, -1), 1));
            }

            if (shapeLeft != null && shapeLeft.Value == BlockShape.Fence)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(-1, 0, 0), worldUpdater, new Vector3Int(1, 0, 0), 1));
            }
            if (shapeRight != null && shapeRight.Value == BlockShape.Fence)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(1, 0, 0), worldUpdater, new Vector3Int(-1, 0, 0), 1));
            }
            if (shapeFront != null && shapeFront.Value == BlockShape.Fence)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(0, 0, 1), worldUpdater, new Vector3Int(0, 0, -1), 1));
            }
            if (shapeBack != null && shapeBack.Value == BlockShape.Fence)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(0, 0, -1), worldUpdater, new Vector3Int(0, 0, 1), 1));
            }

            if (shapeLeft != null && shapeLeft.Value == BlockShape.WallAttachment)
            {
                if (WorldHelper.instance.GetBlockData(position + new Vector3Int(-1, 0, 0)).optionalDataValue == 1)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(-1, 0, 0), worldUpdater, WorldHelper.instance.GetBlockData(position + new Vector3Int(-1, 0, 0))));
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new PlacingBlockOperation(new Vector3Int(position.x - 1, position.y, position.z), worldUpdater, 0));
                }

            }
            if (shapeRight != null && shapeRight.Value == BlockShape.WallAttachment)
            {
                if (WorldHelper.instance.GetBlockData(position + new Vector3Int(1, 0, 0)).optionalDataValue == 0)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(1, 0, 0), worldUpdater, WorldHelper.instance.GetBlockData(position + new Vector3Int(1, 0, 0))));
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new PlacingBlockOperation(new Vector3Int(position.x + 1, position.y, position.z), worldUpdater, 0));
                }

            }
            if (shapeFront != null && shapeFront.Value == BlockShape.WallAttachment)
            {
                if (WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, 1)).optionalDataValue == 2)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(0, 0, 1), worldUpdater, WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, 1))));
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new PlacingBlockOperation(new Vector3Int(position.x, position.y, position.z + 1), worldUpdater, 0));
                }

            }
            if (shapeBack != null && shapeBack.Value == BlockShape.WallAttachment)
            {
                if (WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, -1)).optionalDataValue == 3)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(0, 0, -1), worldUpdater, WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, -1))));
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new PlacingBlockOperation(new Vector3Int(position.x, position.y, position.z - 1), worldUpdater, 0));
                }

            }

        }

    }

    public struct WaterFloodOperation : IChunkUpdateOperation
    {

        public WorldUpdater worldUpdater;
        public Vector3Int position { get; set; }

        public Vector3Int updateFromPoint;
        public int stackDepth;
        public WaterFloodOperation(Vector3Int position, WorldUpdater worldUpdater, Vector3Int updateFromPoint, int stackDepth)
        {
            this.position = position;
            this.worldUpdater = worldUpdater;
            this.updateFromPoint = updateFromPoint;
            this.stackDepth = stackDepth;
        }
        public void Update()
        {
            BlockShape? shapeThis =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, 0)));
            BlockData dataThis = WorldHelper.instance.GetBlockData(position);
            //   Debug.WriteLine("data this:"+dataThis.blockID);
            if (shapeThis is not BlockShape.Water)
            {
                return;
            }
            BlockShape? shapeRight =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(1, 0, 0)));
            BlockShape? shapeLeft =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(-1, 0, 0)));
            BlockShape? shapeFront =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, 1)));
            BlockShape? shapeBack =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, -1)));


            BlockShape? shapeBottom =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, -1, 0)));

            if (shapeLeft == null || shapeLeft.Value != BlockShape.Solid && shapeLeft.Value != BlockShape.Water && shapeLeft.Value != BlockShape.Fence && shapeLeft.Value != BlockShape.Slabs)
            {
                Debug.Log("left");
                WorldHelper.instance.SetBlockWithoutUpdate(position + new Vector3Int(-1, 0, 0), dataThis.blockID);
            }


            if (shapeRight == null || shapeRight.Value != BlockShape.Solid && shapeRight.Value != BlockShape.Water && shapeRight.Value != BlockShape.Fence && shapeRight.Value != BlockShape.Slabs)
            {
                Debug.Log("right");
                WorldHelper.instance.SetBlockWithoutUpdate(position + new Vector3Int(1, 0, 0), dataThis.blockID);
            }


            if (shapeBack == null || shapeBack.Value != BlockShape.Solid && shapeBack.Value != BlockShape.Water && shapeBack.Value != BlockShape.Fence && shapeBack.Value != BlockShape.Slabs)
            {
                Debug.Log("back");
                WorldHelper.instance.SetBlockWithoutUpdate(position + new Vector3Int(0, 0, -1), dataThis.blockID);
            }
            if (shapeFront == null || shapeFront.Value != BlockShape.Solid && shapeFront.Value != BlockShape.Water && shapeFront.Value != BlockShape.Fence && shapeFront.Value != BlockShape.Slabs)
            {
                Debug.Log("front");
                WorldHelper.instance.SetBlockWithoutUpdate(position + new Vector3Int(0, 0, 1), dataThis.blockID);
            }
            if (shapeBottom == null || shapeBottom.Value != BlockShape.Solid && shapeBottom.Value != BlockShape.Water && shapeBottom.Value != BlockShape.Fence && shapeBottom.Value != BlockShape.Slabs)
            {
                Debug.Log("bottom");
                WorldHelper.instance.SetBlockWithoutUpdate(position + new Vector3Int(0, -1, 0), dataThis.blockID);
            }
            shapeRight =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(1, 0, 0)));
            shapeLeft =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(-1, 0, 0)));
            shapeFront =
                  WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, 1)));
            shapeBack =
                  WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, -1)));

            shapeBottom =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, -1, 0)));



            if (stackDepth >= 2)
            {
                return;
            }
            /*     if (shapeLeft != null && (shapeLeft.Value == BlockShape.Water) && updateFromPoint.x != -1)
                 {
                     worldUpdater.queuedChunkUpdatePoints.Enqueue(new WaterFloodOperation(position + new Vector3Int(-1, 0, 0), this.worldUpdater, new Vector3Int(1, 0, 0), stackDepth+1));
                 }
                 if (shapeRight != null && (shapeRight.Value == BlockShape.Water) && updateFromPoint.x != 1)
                 {
                     worldUpdater.queuedChunkUpdatePoints.Enqueue(new WaterFloodOperation(position + new Vector3Int(1, 0, 0), this.worldUpdater, new Vector3Int(-1, 0, 0), stackDepth + 1));
                 }
                 if (shapeFront != null && (shapeFront.Value == BlockShape.Water) && updateFromPoint.z != 1)
                 {
                     worldUpdater.queuedChunkUpdatePoints.Enqueue(new WaterFloodOperation(position + new Vector3Int(0, 0, 1), this.worldUpdater, new Vector3Int(0, 0, -1), stackDepth + 1));
                 }
                 if (shapeBack != null && (shapeBack.Value == BlockShape.Water) && updateFromPoint.z !=- 1)
                 {
                     worldUpdater.queuedChunkUpdatePoints.Enqueue(new WaterFloodOperation(position + new Vector3Int(0, 0, -1), this.worldUpdater, new Vector3Int(0, 0, 1), stackDepth + 1));
                 }
                 if (shapeBottom != null && (shapeBottom.Value == BlockShape.Water) && updateFromPoint.y != -1)
                 {
                     worldUpdater.queuedChunkUpdatePoints.Enqueue(new WaterFloodOperation(position + new Vector3Int(0, -1, 0), this.worldUpdater, new Vector3Int(0, 1, 0), stackDepth + 1));
                 }*/
        }
    }

    public struct DoorInteractingOperation : IChunkUpdateOperation
    {
        public Vector3Int position { get; set; }

        public DoorInteractingOperation(Vector3Int position)
        {
            this.position = position;
        }
        public void Update()
        {

            BlockShape? shapeThis =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, 0)));
            //    Debug.WriteLine(shapeThis);
            if (shapeThis is not BlockShape.Door)
            {
                return;
            }

            BlockData thisData = (int)WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, 0));
        /*    if (Chunk.blockSoundInfo.ContainsKey(thisData.blockID))
            {
                SoundsUtility.PlaySound(MinecraftGameBase.gameposition, new Vector3(position.x + 0.5f, position.y + 0.5f, position.z + 0.5f), Chunk.blockSoundInfo[thisData.blockID], 20f);
            }*/
            BlockShape? shapeDown =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, -1, 0)));
            BlockShape? shapeUp =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 1, 0)));

            if (shapeThis is BlockShape.Door)
            {
                BlockData data = WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, 0));
                bool[] dataBinary = MathUtility.GetBooleanArray(data.optionalDataValue);
                dataBinary[4] = !dataBinary[4];
                WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(position + new Vector3Int(0, 0, 0), MathUtility.GetByte(dataBinary));
                //     Debug.WriteLine(dataBinary);
            }
            if (shapeDown is BlockShape.Door)
            {
                BlockData data = WorldHelper.instance.GetBlockData(position + new Vector3Int(0, -1, 0));
                bool[] dataBinary = MathUtility.GetBooleanArray(data.optionalDataValue);
                dataBinary[4] = !dataBinary[4];
                WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(position + new Vector3Int(0, -1, 0), MathUtility.GetByte(dataBinary));
                //          Debug.WriteLine(dataBinary);
            }

            if (shapeUp is BlockShape.Door)
            {
                BlockData data = WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 1, 0));
                bool[] dataBinary = MathUtility.GetBooleanArray(data.optionalDataValue);
                dataBinary[4] = !dataBinary[4];
                WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(position + new Vector3Int(0, 1, 0), MathUtility.GetByte(dataBinary));
                //         Debug.WriteLine(dataBinary);
            }

        }
    }

    public struct DoorUpperPartPlacingOperation : IChunkUpdateOperation
    {
        public Vector3Int position { get; set; }

        public DoorUpperPartPlacingOperation(Vector3Int position)
        {
            this.position = position;
        }
        public void Update()
        {

            BlockShape? shapeThis =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position));
            Debug.Log(shapeThis);
            if (shapeThis is not BlockShape.Door)
            {
                return;
            }


            BlockShape? shapeUp =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 1, 0)));



            if (shapeUp is not BlockShape.Door)
            {
                BlockData data = WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 0, 0));
                bool[] dataBinary = MathUtility.GetBooleanArray(data.optionalDataValue);
                dataBinary[5] = true;
                WorldHelper.instance.SetBlockWithoutUpdate(position + new Vector3Int(0, 1, 0), data.blockID);
                WorldHelper.instance.SetBlockOptionalDataWithoutUpdate(position + new Vector3Int(0, 1, 0), MathUtility.GetByte(dataBinary));
                //   Debug.WriteLine(dataBinary[7]);
            }

        }
    }


    public struct DoorBreakingOperation : IChunkUpdateOperation
    {
        public Vector3Int position { get; set; }
        public BlockData prevBlockData;

        public DoorBreakingOperation(Vector3Int position, BlockData prevBlockData)
        {
            this.position = position;
            this.prevBlockData = prevBlockData;
        }
        public void Update()
        {

            BlockShape? shapeThis =
                WorldHelper.instance.GetBlockShape(prevBlockData);
            Debug.Log(shapeThis);
            if (shapeThis is not BlockShape.Door)
            {
                return;
            }

            BlockData dataThis = prevBlockData;

            bool[] dataBinary = MathUtility.GetBooleanArray(dataThis.optionalDataValue);
            BlockShape? shapeUp =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, 1, 0)));
            BlockShape? shapeDown =
                WorldHelper.instance.GetBlockShape(WorldHelper.instance.GetBlockData(position + new Vector3Int(0, -1, 0)));


            //break door bottom
            if (dataBinary[5] == false)
            {
                if (shapeUp is BlockShape.Door)
                {

                    WorldHelper.instance.SetBlockWithoutUpdate(position + new Vector3Int(0, 1, 0), (short)0);

                    //   Debug.WriteLine(dataBinary[7]);
                }
            }
            else
            {

                if (shapeDown is BlockShape.Door)
                {

                    WorldHelper.instance.SetBlockWithoutUpdate(position + new Vector3Int(0, -1, 0), (short)0);

                    //   Debug.WriteLine(dataBinary[7]);
                }

            }



        }
    }
}
