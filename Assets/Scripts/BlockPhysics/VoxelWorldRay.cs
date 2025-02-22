using System;
using UnityEngine;

public enum BlockFaces
{
    PositiveX,
    PositiveY,
    PositiveZ,
    NegativeX,
    NegativeY,
    NegativeZ,

}
public struct VoxelWorldRay
{
    public Vector3 origin;
    public Vector3 direction;

    public VoxelWorldRay(Vector3 origin, Vector3 direction)
    {
        this.origin = origin;
        this.direction = direction;
    }


    public float? Intersects(SimpleAxisAlignedBB box, out BlockFaces blockFaceID)
    {
        const float Epsilon = 1e-9f;


        float tMaxTmp = 0;
        blockFaceID = BlockFaces.PositiveY;
        Vector3 maxT = new Vector3(-1.0f, -1.0f, -1.0f);


        if (origin.x >= box.Min.x
            && origin.x <= box.Max.x
            && origin.y >= box.Min.y
            && origin.y <= box.Max.y
            && origin.z >= box.Min.z
            && origin.z <= box.Max.z)
            return 0.0f;

        if (Math.Abs(direction.x) < Epsilon)
        {
            return null;
        }
        else
        {
            if (origin.x < box.Min.x)
            {
                maxT.x = (box.Min.x - origin.x) / direction.x;
            }
            else if (origin.x > box.Max.x)
            {
                maxT.x = (box.Max.x - origin.x) / direction.x;
            }
        }

        if (Math.Abs(direction.y) < Epsilon)
        {
            return null;
        }
        else
        {
            if (origin.y < box.Min.y)
            {
                maxT.y = (box.Min.y - origin.y) / direction.y;
            }
            else if (origin.y > box.Max.y)
            {
                maxT.y = (box.Max.y - origin.y) / direction.y;
            }
        }

        if (Math.Abs(direction.z) < Epsilon)
        {
            return null;
        }
        else
        {
            if (origin.z < box.Min.z)
            {
                maxT.z = (box.Min.z - origin.z) / direction.z;
            }
            else if (origin.z > box.Max.z)
            {
                maxT.z = (box.Max.z - origin.z) / direction.z;
            }
        }


        if (maxT.x > maxT.y && maxT.x > maxT.z)
        {
            if (maxT.x < 0f)
            {
                return null;
            }

            float intersectPointZ = origin.z + maxT.x * direction.z;
            if (intersectPointZ < box.Min.z || intersectPointZ > box.Max.z)
                return null;

            float intersectPointY = origin.y + maxT.x * direction.y;
            if (intersectPointY < box.Min.y || intersectPointY > box.Max.y)
                return null;


            if (origin.x < box.Min.x)
                blockFaceID = BlockFaces.NegativeX;
            else if (origin.x > box.Max.x)
                blockFaceID = BlockFaces.PositiveX;


            return maxT.x;
        }


        if (maxT.y > maxT.x && maxT.y > maxT.z)
        {
            if (maxT.y < 0f)
            {
                return null;
            }

            float intersectPointZ = origin.z + maxT.y * direction.z;
            if (intersectPointZ < box.Min.z || intersectPointZ > box.Max.z)
                return null;

            float intersectPointX = origin.x + maxT.y * direction.x;
            if (intersectPointX < box.Min.x || intersectPointX > box.Max.x)
                return null;


            if (origin.y < box.Min.y)
                blockFaceID = BlockFaces.NegativeY;
            else if (origin.y > box.Max.y)
                blockFaceID = BlockFaces.PositiveY;

            return maxT.y;
        }
        else

        {
            if (maxT.z < 0f)
            {
                return null;
            }

            float intersectPointY = origin.y + maxT.z * direction.y;
            if (intersectPointY < box.Min.y || intersectPointY > box.Max.y)
                return null;

            float intersectPointX = origin.x + maxT.z * direction.x;
            if (intersectPointX < box.Min.x || intersectPointX > box.Max.x)
                return null;


            if (origin.z < box.Min.z)
                blockFaceID = BlockFaces.NegativeZ;
            else if (origin.z > box.Max.z)
                blockFaceID = BlockFaces.PositiveZ;

            return maxT.z;
        }
    }
}
