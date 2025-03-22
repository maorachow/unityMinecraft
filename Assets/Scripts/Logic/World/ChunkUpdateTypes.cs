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
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(0, 0, 0));

            BlockShape? shapeRight =
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(1, 0, 0));
            BlockShape? shapeLeft =
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(-1, 0, 0));
            BlockShape? shapeFront =
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(0, 0, 1));
            BlockShape? shapeBack =
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(0, 0, -1));

            worldUpdater.worldAccessor.SetBlockWithoutUpdate(position, placingBlockData);

            BlockShape? shapePlaced = worldUpdater.worldAccessor.GetBlockShape(placingBlockData);
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
            if (shapePlaced is BlockShape.Door && worldUpdater.worldAccessor.GetBlockData(position + new Vector3(0, 1, 0)).blockID == 0)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new DoorUpperPartPlacingOperation(position,worldUpdater));
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
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(0, 0, 0));
            if (shapeThis is not BlockShape.Fence)
            {
                return;
            }
            BlockShape shapeRight =
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(1, 0, 0));
            BlockShape shapeLeft =
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(-1, 0, 0));
            BlockShape shapeFront =
               worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(0, 0, 1));
            BlockShape shapeBack =
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(0, 0, -1));
            bool[] shapes = new[] { false, false, false, false, false, false, false, false };
            if ((shapeLeft == BlockShape.Fence || shapeLeft == BlockShape.Solid))
            {
                shapes[7] = true;
            }
            else
            {
                shapes[7] = false;
            }

            if ((shapeRight == BlockShape.Fence || shapeRight == BlockShape.Solid))
            {
                shapes[6] = true;
            }
            else
            {
                shapes[6] = false;
            }

            if ((shapeBack == BlockShape.Fence || shapeBack == BlockShape.Solid))
            {
                shapes[5] = true;
            }
            else
            {
                shapes[5] = false;
            }

            if ((shapeFront == BlockShape.Fence || shapeFront == BlockShape.Solid))
            {
                shapes[4] = true;
            }
            else
            {
                shapes[4] = false;
            }
            //     Debug.WriteLine("from::"+updateFromPoint);
           worldUpdater.worldAccessor.SetBlockOptionalDataWithoutUpdate(position, MathUtility.GetByte(shapes));

            if (stackDepth >= 2)
            {
                return;
            }
            if (shapeLeft == BlockShape.Fence && updateFromPoint.x != -1)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(-1, 0, 0), worldUpdater, new Vector3Int(1, 0, 0), stackDepth + 1));
            }
            if (shapeRight == BlockShape.Fence && updateFromPoint.x != 1)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(1, 0, 0), worldUpdater, new Vector3Int(-1, 0, 0), stackDepth + 1));
            }
            if (shapeFront == BlockShape.Fence && updateFromPoint.z != 1)
            {
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new FenceUpdatingOperation(position + new Vector3Int(0, 0, 1), worldUpdater, new Vector3Int(0, 0, -1), stackDepth + 1));
            }
            if (shapeBack == BlockShape.Fence && updateFromPoint.z != -1)
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
            worldUpdater.worldAccessor.SetBlockWithoutUpdate(position, 0);
           
                Vector3Int effctPosition = position;
                BlockData effectPrevBlockData = prevBlockData;

                worldUpdater.onUpdatedOneShot += () =>
                {
                  WorldUpdateablesMediator.instance.TrySpawnBlockBreakingParticle(new Vector3(effctPosition.x + 0.5f, effctPosition.y + 0.5f, effctPosition.z + 0.5f), effectPrevBlockData);

                  WorldUpdateablesMediator.instance.TrySpawnItemEntityFromBlockID(new Vector3(effctPosition.x + 0.5f, effctPosition.y + 0.5f, effctPosition.z + 0.5f) , key.blockID, new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f)));
                    
                   
                };
            
        

            BlockShape? shapeRight =
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(1, 0, 0));
            BlockShape? shapeLeft =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(-1, 0, 0)));
            BlockShape? shapeFront =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, 0, 1)));
            BlockShape? shapeBack =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, 0, -1)));

            BlockShape? shapeThis =
                worldUpdater.worldAccessor.GetBlockShape(prevBlockData);



            BlockShape? shapeUp =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, 1, 0)));

           
            if (shapeThis is BlockShape.Door)
            {
                Debug.Log("break door");
                worldUpdater.queuedChunkUpdatePoints.Enqueue(new DoorBreakingOperation(position, prevBlockData,worldUpdater));
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
                if (worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(-1, 0, 0)).optionalDataValue == 1)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(-1, 0, 0), worldUpdater, worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(-1, 0, 0))));
                   
                }

            }
            if (shapeRight != null && shapeRight.Value == BlockShape.WallAttachment)
            {
                if (worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(1, 0, 0)).optionalDataValue == 0)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(1, 0, 0), worldUpdater, worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(1, 0, 0))));
                  
                }

            }
            if (shapeFront != null && shapeFront.Value == BlockShape.WallAttachment)
            {
                if (worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, 1)).optionalDataValue == 2)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(0, 0, 1), worldUpdater, worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, 1))));
                    
                }

            }
            if (shapeBack != null && shapeBack.Value == BlockShape.WallAttachment)
            {
                if (worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, -1)).optionalDataValue == 3)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(0, 0, -1), worldUpdater, worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, -1))));
                   
                }

            }


            if (shapeUp is BlockShape.CrossModel || shapeUp is BlockShape.Torch)
            {

                worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(new Vector3Int(position.x, position.y + 1, position.z), worldUpdater, worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 1, 0))));
               
            }

            if (shapeLeft is BlockShape.Torch)
            {
                if (worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(-1, 0, 0)).optionalDataValue == 1)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(-1, 0, 0), worldUpdater, worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(-1, 0, 0))));
                  
                }

            }
            if (shapeRight is BlockShape.Torch)
            {
                if (worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(1, 0, 0)).optionalDataValue == 2)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(1, 0, 0), worldUpdater, worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(1, 0, 0))));
                
                }

            }
            if (shapeFront is BlockShape.Torch)
            {
                if (worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, 1)).optionalDataValue == 4)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(0, 0, 1), worldUpdater, worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, 1))));
                }

            }
            if (shapeBack is BlockShape.Torch)
            {
                if (worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, -1)).optionalDataValue == 3)
                {
                    worldUpdater.queuedChunkUpdatePoints.Enqueue(new BreakBlockOperation(position + new Vector3Int(0, 0, -1), worldUpdater, worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, -1))));
                   
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
                worldUpdater.worldAccessor.GetBlockShape(position + new Vector3Int(0, 0, 0));
            BlockData dataThis = worldUpdater.worldAccessor.GetBlockData(position);
            //   Debug.WriteLine("data this:"+dataThis.blockID);
            if (shapeThis is not BlockShape.Water)
            {
                return;
            }
            BlockShape shapeRight =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(1, 0, 0)));
            BlockShape shapeLeft =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(-1, 0, 0)));
            BlockShape shapeFront =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, 0, 1)));
            BlockShape shapeBack =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, 0, -1)));


            BlockShape? shapeBottom =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, -1, 0)));

            if (shapeLeft == BlockShape.Empty)
            {
                Debug.Log("left");
                worldUpdater.worldAccessor.SetBlockWithoutUpdate(position + new Vector3Int(-1, 0, 0), dataThis.blockID);
            }


            if (shapeRight == BlockShape.Empty)
            {
                Debug.Log("right");
                worldUpdater.worldAccessor.SetBlockWithoutUpdate(position + new Vector3Int(1, 0, 0), dataThis.blockID);
            }


            if (shapeBack ==   BlockShape.Empty)
            {
                Debug.Log("back");
                worldUpdater.worldAccessor.SetBlockWithoutUpdate(position + new Vector3Int(0, 0, -1), dataThis.blockID);
            }
            if (shapeFront ==  BlockShape.Empty)
            {
                Debug.Log("front");
                worldUpdater.worldAccessor.SetBlockWithoutUpdate(position + new Vector3Int(0, 0, 1), dataThis.blockID);
            }
            if (shapeBottom == BlockShape.Empty)
            {
                Debug.Log("bottom");
                worldUpdater.worldAccessor.SetBlockWithoutUpdate(position + new Vector3Int(0, -1, 0), dataThis.blockID);
            }
        /*    shapeRight =
                WorldUpdateablesMediator.instance.GetBlockShape(WorldUpdateablesMediator.instance.GetBlockData(position + new Vector3Int(1, 0, 0)));
            shapeLeft =
                WorldUpdateablesMediator.instance.GetBlockShape(WorldUpdateablesMediator.instance.GetBlockData(position + new Vector3Int(-1, 0, 0)));
            shapeFront =
                  WorldUpdateablesMediator.instance.GetBlockShape(WorldUpdateablesMediator.instance.GetBlockData(position + new Vector3Int(0, 0, 1)));
            shapeBack =
                  WorldUpdateablesMediator.instance.GetBlockShape(WorldUpdateablesMediator.instance.GetBlockData(position + new Vector3Int(0, 0, -1)));

            shapeBottom =
                WorldUpdateablesMediator.instance.GetBlockShape(WorldUpdateablesMediator.instance.GetBlockData(position + new Vector3Int(0, -1, 0)));
        */


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

        public WorldUpdater worldUpdater;
        public DoorInteractingOperation(Vector3Int position,WorldUpdater worldUpdater)
        {
            this.position = position;
            this.worldUpdater = worldUpdater;
        }
        public void Update()
        {

            BlockShape shapeThis =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, 0, 0)));
            //    Debug.WriteLine(shapeThis);
            if (shapeThis is not BlockShape.Door)
            {
                return;
            }

            BlockData thisData = (int)worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, 0));
         
            BlockShape shapeDown =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, -1, 0)));
            BlockShape shapeUp =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, 1, 0)));

            {
                BlockData data = worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, 0));
                bool[] dataBinary = MathUtility.GetBooleanArray(data.optionalDataValue);
                dataBinary[4] = !dataBinary[4];
                worldUpdater.worldAccessor.SetBlockOptionalDataWithoutUpdate(position + new Vector3Int(0, 0, 0), MathUtility.GetByte(dataBinary));
                //     Debug.WriteLine(dataBinary);
            }
            if (shapeDown is BlockShape.Door)
            {
                BlockData data = worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, -1, 0));
                bool[] dataBinary = MathUtility.GetBooleanArray(data.optionalDataValue);
                dataBinary[4] = !dataBinary[4];
                worldUpdater.worldAccessor.SetBlockOptionalDataWithoutUpdate(position + new Vector3Int(0, -1, 0), MathUtility.GetByte(dataBinary));
                //          Debug.WriteLine(dataBinary);
            }

            if (shapeUp is BlockShape.Door)
            {
                BlockData data = worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 1, 0));
                bool[] dataBinary = MathUtility.GetBooleanArray(data.optionalDataValue);
                dataBinary[4] = !dataBinary[4];
                worldUpdater.worldAccessor.SetBlockOptionalDataWithoutUpdate(position + new Vector3Int(0, 1, 0), MathUtility.GetByte(dataBinary));
                //         Debug.WriteLine(dataBinary);
            }

        }
    }

    public struct DoorUpperPartPlacingOperation : IChunkUpdateOperation
    {
        public Vector3Int position { get; set; }
        public WorldUpdater worldUpdater;
        public DoorUpperPartPlacingOperation(Vector3Int position,WorldUpdater worldUpdater)
        {
            this.position = position;
            this.worldUpdater = worldUpdater;
        }
        public void Update()
        {

            BlockShape shapeThis =
                worldUpdater.worldAccessor.GetBlockShape((position));
         
            if (shapeThis is not BlockShape.Door)
            {
                return;
            }


            BlockShape shapeUp =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, 1, 0)));



            if (shapeUp is not BlockShape.Door)
            {
                BlockData data = worldUpdater.worldAccessor.GetBlockData(position + new Vector3Int(0, 0, 0));
                bool[] dataBinary = MathUtility.GetBooleanArray(data.optionalDataValue);
                dataBinary[5] = true;
                worldUpdater.worldAccessor.SetBlockWithoutUpdate(position + new Vector3Int(0, 1, 0), data.blockID);
                worldUpdater.worldAccessor.SetBlockOptionalDataWithoutUpdate(position + new Vector3Int(0, 1, 0), MathUtility.GetByte(dataBinary));
                //   Debug.WriteLine(dataBinary[7]);
            }

        }
    }


    public struct DoorBreakingOperation : IChunkUpdateOperation
    {
        public Vector3Int position { get; set; }
        public BlockData prevBlockData;
        public WorldUpdater worldUpdater;
        public DoorBreakingOperation(Vector3Int position, BlockData prevBlockData,WorldUpdater worldUpdater)
        {
            this.position = position;
            this.prevBlockData = prevBlockData;
            this.worldUpdater = worldUpdater;
        }
        public void Update()
        {

            BlockShape shapeThis =
                worldUpdater.worldAccessor.GetBlockShape(prevBlockData);
            Debug.Log(shapeThis);
            if (shapeThis is not BlockShape.Door)
            {
                return;
            }

            BlockData dataThis = prevBlockData;

            bool[] dataBinary = MathUtility.GetBooleanArray(dataThis.optionalDataValue);
            BlockShape? shapeUp =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, 1, 0)));
            BlockShape? shapeDown =
                worldUpdater.worldAccessor.GetBlockShape((position + new Vector3Int(0, -1, 0)));


            //break door bottom
            if (dataBinary[5] == false)
            {
                if (shapeUp is BlockShape.Door)
                {

                    worldUpdater.worldAccessor.SetBlockWithoutUpdate(position + new Vector3Int(0, 1, 0), (short)0);

                    //   Debug.WriteLine(dataBinary[7]);
                }
            }
            else
            {

                if (shapeDown is BlockShape.Door)
                {

                    worldUpdater.worldAccessor.SetBlockWithoutUpdate(position + new Vector3Int(0, -1, 0), (short)0);

                    //   Debug.WriteLine(dataBinary[7]);
                }

            }



        }
    }
}
