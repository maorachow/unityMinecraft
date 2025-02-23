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
        if (data.blockID == 0)
        {
            return false;
        }
        if (!Chunk.blockInfosNew.ContainsKey(data.blockID))
        {
            return false;
        }
        BlockShape shape = Chunk.blockInfosNew[data.blockID].shape;
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
    public static SimpleAxisAlignedBB GetBoundingBox(int x, int y, int z, BlockData blockData)
    {
        if (blockData.blockID == 0)
        {
            return new SimpleAxisAlignedBB();
        }
        BlockShape shape = Chunk.blockInfosNew[blockData].shape;
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
        if (blockData.blockID == 0)
        {
            return new SimpleAxisAlignedBB();
        }
        BlockShape shape = Chunk.blockInfosNew[blockData].shape;
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

            return new SimpleAxisAlignedBB(new Vector3(x + 0.45f, y, z + 0.45f), new Vector3(x + 0.55f, y + 0.8f, z + +0.55f));


        }
        if (shape == BlockShape.CrossModel)
        {

            return new SimpleAxisAlignedBB(new Vector3(x + 0.25f, y, z + 0.25f), new Vector3(x + 0.75f, y + 0.75f, z + +0.75f));


        }
        if (shape == BlockShape.Water)
        {
            return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));
        }

        if (shape == BlockShape.Fence)
        {
            return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1f, y + 1f, z + 1f));
        }
        if (shape == BlockShape.Door)
        {
            return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1f, y + 1f, z + 1f));
        }
        if (shape == BlockShape.WallAttachment)
        {
            return new SimpleAxisAlignedBB(new Vector3(x, y, z), new Vector3(x + 1f, y + 1f, z + 1f));
        }
        return new SimpleAxisAlignedBB();

    }
}