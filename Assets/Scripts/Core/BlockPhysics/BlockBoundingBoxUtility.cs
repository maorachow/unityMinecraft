using UnityEngine;


public static class BlockBoundingBoxUtility
{

    public static bool IsBlockWithBoundingBox(BlockShape shape)
    {
        switch (shape)
        {
            case BlockShape.Fence:
                return true;
            case BlockShape.Door:
                return true;
            case BlockShape.Solid:
                return true;
            case BlockShape.SolidTransparent:
                return true;
            case BlockShape.Stairs:
                return true;
            case BlockShape.WallAttachment:
                return false;
            default:
                return false;
        }
    }
    public static bool IsBlockWithBoundingBox(BlockData data)
    {
      
        if (!GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.IsBlockDataValid(data))
        {
            return false;
        }
        BlockShape shape = GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.GetBlockInfo(data).shape;
        switch (shape)
        {
            case BlockShape.Fence:
                return true;
            case BlockShape.Door:
                return true;
            case BlockShape.Solid:
                return true;
            case BlockShape.SolidTransparent:
                return true;
            case BlockShape.Stairs:
                return true;
            case BlockShape.WallAttachment:
                return false;
            case BlockShape.Empty:
                return false;
            default:
                return false;
        }
    }
    public static SimpleAxisAlignedBB GetBoundingBox(int x, int y, int z, BlockData blockData)
    {
        if (!GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.IsBlockDataValid(blockData))
        {
            return new SimpleAxisAlignedBB();
        }
        BlockShape shape = GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.GetBlockInfo(blockData).shape;
        if (shape == BlockShape.Empty)
        {
            return new SimpleAxisAlignedBB();
        }

        if (shape == BlockShape.Solid)
        {
            return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));

        }
        if (shape == BlockShape.SolidTransparent)
        {
            return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));

        }
        if (shape == BlockShape.Slabs)
        {
            switch (blockData.optionalDataValue)
            {
                case 0:
                    return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1, y + 0.5f, z + 1));
                case 1:
                    return new SimpleAxisAlignedBB(new Vector3(x, y + 0.5f, z), new Vector3(x + 1, y + 1f, z + 1));
                case 2:
                    return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));
            }
        }

        if (shape == BlockShape.Fence)
        {
            bool[] fenceDatabools = MathUtility.GetBooleanArray(blockData.optionalDataValue);
            Vector3 boxMinPoint = new Vector3(x + 0.375f, y, z + 0.375f);
            Vector3 boxMaxPoint = new Vector3(x + 0.625f, y + 1.5f, z + 0.625f);
            bool isLeftBuilt = fenceDatabools[7];
            bool isRightBuilt = fenceDatabools[6];
            bool isBackBuilt = fenceDatabools[5];
            bool isFrontBuilt = fenceDatabools[4];
            if (isLeftBuilt)
            {
                boxMinPoint.x = x + 0f;
            }

            if (isRightBuilt)
            {
                boxMaxPoint.x = x + 1f;
            }

            if (isBackBuilt)
            {
                boxMinPoint.z = z + 0f;
            }

            if (isFrontBuilt)
            {
                boxMaxPoint.z = z + 1f;
            }
            return new SimpleAxisAlignedBB(boxMinPoint, boxMaxPoint);

        }

        if (shape == BlockShape.Door)
        {
            bool[] doorDataBools = MathUtility.GetBooleanArray(blockData.optionalDataValue);


            byte doorFaceID = 0;
            Vector3 boxMin = new Vector3(x, y, z);
            Vector3 boxMax = new Vector3(x + 1, y + 1, z + 1);
            if (doorDataBools[6] == false)
            {
                if (doorDataBools[7] == false)
                {
                    doorFaceID = 0;
                }
                else
                {
                    doorFaceID = 1;
                }
            }
            else
            {
                if (doorDataBools[7] == false)
                {
                    doorFaceID = 2;
                }
                else
                {
                    doorFaceID = 3;
                }
            }

            bool isOpen = doorDataBools[4];

            switch (doorFaceID)
            {
                case 0:
                    if (!isOpen)
                    {
                        boxMin = new Vector3(x, y, z);
                        boxMax = new Vector3(x + 0.1875f, y + 1, z + 1);
                    }
                    else
                    {
                        boxMin = new Vector3(x, y, z + 1 - 0.1875f);
                        boxMax = new Vector3(x + 1, y + 1, z + 1);
                    }

                    break;
                case 1:
                    if (!isOpen)
                    {
                        boxMin = new Vector3(x + 1 - 0.1875f, y, z);
                        boxMax = new Vector3(x + 1, y + 1, z + 1f);
                    }
                    else
                    {
                        boxMin = new Vector3(x, y, z);
                        boxMax = new Vector3(x + 1, y + 1, z + 0.1875f);
                    }

                    break;
                case 2:
                    if (!isOpen)
                    {
                        boxMin = new Vector3(x, y, z);
                        boxMax = new Vector3(x + 1, y + 1, z + 0.1875f);

                    }
                    else
                    {
                        boxMin = new Vector3(x, y, z);
                        boxMax = new Vector3(x + 0.1875f, y + 1, z + 1);
                    }

                    break;
                case 3:
                    if (!isOpen)
                    {
                        boxMin = new Vector3(x, y, z + 1 - 0.1875f);
                        boxMax = new Vector3(x + 1, y + 1, z + 1);
                    }
                    else
                    {
                        boxMin = new Vector3(x + 1 - 0.1875f, y, z);
                        boxMax = new Vector3(x + 1, y + 1, z + 1f);
                    }

                    break;
            }



            return new SimpleAxisAlignedBB(boxMin, boxMax);

        }

        if (shape == BlockShape.WallAttachment)
        {
            return new SimpleAxisAlignedBB();
        }

        return new SimpleAxisAlignedBB();

    }

    public static SimpleAxisAlignedBB GetBoundingBoxSelectable(int x, int y, int z, BlockData blockData)
    {

        if (!GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.IsBlockDataValid(blockData))
        {
            return new SimpleAxisAlignedBB();
        }
      
        BlockShape shape = GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.GetBlockInfo(blockData).shape;
        if (shape == BlockShape.Solid)
        {
            return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));

        }
        if (shape == BlockShape.SolidTransparent)
        {
            return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));

        }
        if (shape == BlockShape.Slabs)
        {
            switch (blockData.optionalDataValue)
            {
                case 0:
                    return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1, y + 0.5f, z + 1));
                case 1:
                    return new SimpleAxisAlignedBB(new Vector3(x, y + 0.5f, z), new Vector3(x + 1, y + 1f, z + 1));
                case 2:
                    return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));
            }
        }
        if (shape == BlockShape.Torch)
        {

            switch (blockData.optionalDataValue)
            {
                case 0:
                    return new SimpleAxisAlignedBB(new Vector3(x + 0.35f, y, z + 0.35f), new Vector3(x + 0.65f, y + 0.8f, z + +0.65f));
                case 1:
                    return new SimpleAxisAlignedBB(new Vector3(x + 0.35f + 0.35f, y, z + 0.35f), new Vector3(x + 0.65f + 0.35f, y + 0.8f, z + +0.65f));
                case 2:
                    return new SimpleAxisAlignedBB(new Vector3(x + 0.35f - 0.35f, y, z + 0.35f), new Vector3(x + 0.65f - 0.35f, y + 0.8f, z + +0.65f));
                case 3:
                    return new SimpleAxisAlignedBB(new Vector3(x + 0.35f, y, z + 0.35f + 0.35f), new Vector3(x + 0.65f, y + 0.8f, z + +0.65f + 0.35f));
                case 4:
                    return new SimpleAxisAlignedBB(new Vector3(x + 0.35f, y, z + 0.35f - 0.35f), new Vector3(x + 0.65f, y + 0.8f, z + +0.65f - 0.35f));
                default:
                    return new SimpleAxisAlignedBB(new Vector3(x + 0.35f, y, z + 0.35f), new Vector3(x + 0.65f, y + 0.8f, z + +0.65f));
            }
           


        }
        if (shape == BlockShape.CrossModel)
        {

            return new SimpleAxisAlignedBB(new Vector3(x + 0.25f, y, z + 0.25f), new Vector3(x + 0.75f, y + 0.75f, z + +0.75f));


        }
        if (shape == BlockShape.Water)
        {
            return new SimpleAxisAlignedBB();
        }

        if (shape == BlockShape.Fence)
        {

            float x1 = 0f;
            float y1 =0f;
            float z1 = 0f;
            bool[] fenceDatabools = MathUtility.GetBooleanArray(blockData.optionalDataValue);
            Vector3 boxMinPoint = new Vector3(x1 + 0.375f, y1, z1 + 0.375f);
            Vector3 boxMaxPoint = new Vector3(x1 + 0.625f, y1 + 1f, z1 + 0.625f);
            bool isLeftBuilt = fenceDatabools[7];
            bool isRightBuilt = fenceDatabools[6];
            bool isBackBuilt = fenceDatabools[5];
            bool isFrontBuilt = fenceDatabools[4];
            if (isLeftBuilt)
            {
                boxMinPoint.x = x1 + 0f;
            }

            if (isRightBuilt)
            {
                boxMaxPoint.x = x1 + 1f;
            }

            if (isBackBuilt)
            {
                boxMinPoint.z = z1 + 0f;
            }

            if (isFrontBuilt)
            {
                boxMaxPoint.z = z1 + 1f;
            }
         



            return new SimpleAxisAlignedBB(boxMinPoint+ new Vector3(x, y, z), boxMaxPoint+ new Vector3(x, y, z));
        }
        if (shape == BlockShape.Door)
        {
            bool[] doorDataBools = MathUtility.GetBooleanArray(blockData.optionalDataValue);


            byte doorFaceID = 0;
            Vector3 boxMin = new Vector3(x, y, z);
            Vector3 boxMax = new Vector3(x + 1, y + 1, z + 1);
            if (doorDataBools[6] == false)
            {
                if (doorDataBools[7] == false)
                {
                    doorFaceID = 0;
                }
                else
                {
                    doorFaceID = 1;
                }
            }
            else
            {
                if (doorDataBools[7] == false)
                {
                    doorFaceID = 2;
                }
                else
                {
                    doorFaceID = 3;
                }
            }

            bool isOpen = doorDataBools[4];

            switch (doorFaceID)
            {
                case 0:
                    if (!isOpen)
                    {
                        boxMin = new Vector3(x, y, z);
                        boxMax = new Vector3(x + 0.1875f, y + 1, z + 1);
                    }
                    else
                    {
                        boxMin = new Vector3(x, y, z + 1 - 0.1875f);
                        boxMax = new Vector3(x + 1, y + 1, z + 1);
                    }

                    break;
                case 1:
                    if (!isOpen)
                    {
                        boxMin = new Vector3(x + 1 - 0.1875f, y, z);
                        boxMax = new Vector3(x + 1, y + 1, z + 1f);
                    }
                    else
                    {
                        boxMin = new Vector3(x, y, z);
                        boxMax = new Vector3(x + 1, y + 1, z + 0.1875f);
                    }

                    break;
                case 2:
                    if (!isOpen)
                    {
                        boxMin = new Vector3(x, y, z);
                        boxMax = new Vector3(x + 1, y + 1, z + 0.1875f);

                    }
                    else
                    {
                        boxMin = new Vector3(x, y, z);
                        boxMax = new Vector3(x + 0.1875f, y + 1, z + 1);
                    }

                    break;
                case 3:
                    if (!isOpen)
                    {
                        boxMin = new Vector3(x, y, z + 1 - 0.1875f);
                        boxMax = new Vector3(x + 1, y + 1, z + 1);
                    }
                    else
                    {
                        boxMin = new Vector3(x + 1 - 0.1875f, y, z);
                        boxMax = new Vector3(x + 1, y + 1, z + 1f);
                    }

                    break;
            }
            return new SimpleAxisAlignedBB(boxMin, boxMax);
        }
        if (shape == BlockShape.WallAttachment)
        {
            return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1f, y + 1f, z + 1f));
        }

        
        return new SimpleAxisAlignedBB();

    }
}