using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBoxCullingHelper 
{
    public static Vector3[] GetPointsOfBounds(Bounds bounds)
    {
        Vector3[] points = new Vector3[8];
        points[0] = bounds.min;
        points[1] = bounds.min+new Vector3(bounds.size.x,0,0);
        points[2] = bounds.min + new Vector3(0, bounds.size.y, 0);
        points[3] = bounds.min + new Vector3(0, 0, bounds.size.z);
        points[4] = bounds.min + new Vector3(bounds.size.x, 0, bounds.size.z);
        points[5] = bounds.min + new Vector3(bounds.size.x, bounds.size.y, 0);
        points[6] = bounds.min + new Vector3(0, bounds.size.y, bounds.size.z);
        points[7] = bounds.max;
        return points;
    }
    public static bool IsBoundingBoxInOrIntersectsPlane(Bounds bounds,Plane plane)
    {
        Vector3[] points= GetPointsOfBounds(bounds);
        bool result = false ;
        foreach (Vector3 point in points)
        {
            if (plane.GetSide(point) == true)
            {
                result= true; break;
            }
        }
        return result;
    }
    public static bool IsBoundingBoxInOrIntersectsFrustum(Bounds bounds,Plane[] planes)
    {

        foreach(var plane in planes)
        {
            if (IsBoundingBoxInOrIntersectsPlane(bounds, plane) == false)
            {
                return false;
            }
        }
        return true;
    }
}
