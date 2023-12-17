using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AxisAlignedBB {

    public AxisAlignedBB setBounds(float minX, float minY, float minZ, float maxX, float maxY, float maxZ) {
        this.setMinX(minX);
        this.setMinY(minY);
        this.setMinZ(minZ);
        this.setMaxX(maxX);
        this.setMaxY(maxY);
        this.setMaxZ(maxZ);
        return this;
    }

    public AxisAlignedBB addCoord(float x, float y, float z) {
        float minX = this.getMinX();
        float minY = this.getMinY();
        float minZ = this.getMinZ();
        float maxX = this.getMaxX();
        float maxY = this.getMaxY();
        float maxZ = this.getMaxZ();

        if (x < 0) minX += x;
        if (x > 0) maxX += x;

        if (y < 0) minY += y;
        if (y > 0) maxY += y;

        if (z < 0) minZ += z;
        if (z > 0) maxZ += z;

        return new SimpleAxisAlignedBB(minX, minY, minZ, maxX, maxY, maxZ);
    }

    public AxisAlignedBB grow(float x, float y, float z) {
        return new SimpleAxisAlignedBB(this.getMinX() - x, this.getMinY() - y, this.getMinZ() - z, this.getMaxX() + x, this.getMaxY() + y, this.getMaxZ() + z);
    }

    public AxisAlignedBB expand(float x, float y, float z)

    {
        this.setMinX(this.getMinX() - x);
        this.setMinY(this.getMinY() - y);
        this.setMinZ(this.getMinZ() - z);
        this.setMaxX(this.getMaxX() + x);
        this.setMaxY(this.getMaxY() + y);
        this.setMaxZ(this.getMaxZ() + z);

        return this;
    }

    /*public AxisAlignedBB offset(float x, float y, float z) {
        this.setMinX(this.getMinX() + x);
        this.setMinY(this.getMinY() + y);
        this.setMinZ(this.getMinZ() + z);
        this.setMaxX(this.getMaxX() + x);
        this.setMaxY(this.getMaxY() + y);
        this.setMaxZ(this.getMaxZ() + z);
        Debug.Log("move"+x+" "+y+" "+z+" ");
        return this;
    }*/

    public AxisAlignedBB shrink(float x, float y, float z) {
        return new SimpleAxisAlignedBB(this.getMinX() + x, this.getMinY() + y, this.getMinZ() + z, this.getMaxX() - x, this.getMaxY() - y, this.getMaxZ() - z);
    }

    public AxisAlignedBB contract(float x, float y, float z) {
        this.setMinX(this.getMinX() + x);
        this.setMinY(this.getMinY() + y);
        this.setMinZ(this.getMinZ() + z);
        this.setMaxX(this.getMaxX() - x);
        this.setMaxY(this.getMaxY() - y);
        this.setMaxZ(this.getMaxZ() - z);

        return this;
    }

    public AxisAlignedBB setBB(AxisAlignedBB bb) {
        this.setMinX(bb.getMinX());
        this.setMinY(bb.getMinY());
        this.setMinZ(bb.getMinZ());
        this.setMaxX(bb.getMaxX());
        this.setMaxY(bb.getMaxY());
        this.setMaxZ(bb.getMaxZ());
        return this;
    }

    public AxisAlignedBB getOffsetBoundingBox(float x, float y, float z) {
        return new SimpleAxisAlignedBB(this.getMinX() + x, this.getMinY() + y, this.getMinZ() + z, this.getMaxX() + x, this.getMaxY() + y, this.getMaxZ() + z);
    }

    public float calculateXOffset(AxisAlignedBB bb, float x) {
        if (bb.getMaxY() <= this.getMinY() || bb.getMinY() >= this.getMaxY()) {
            return x;
        }
        if (bb.getMaxZ() <= this.getMinZ() || bb.getMinZ() >= this.getMaxZ()) {
            return x;
        }
        if (x > 0 && bb.getMaxX() <= this.getMinX()) {
            float x1 = this.getMinX() - bb.getMaxX();
            if (x1 < x) {
                x = x1;
            }
        }
        if (x < 0 && bb.getMinX() >= this.getMaxX()) {
            float x2 = this.getMaxX() - bb.getMinX();
            if (x2 > x) {
                x = x2;
            }
        }

        return x;
    }

    public float calculateYOffset(AxisAlignedBB bb, float y) {
        if (bb.getMaxX() <= this.getMinX() || bb.getMinX() >= this.getMaxX()) {
            return y;
        }
        if (bb.getMaxZ() <= this.getMinZ() || bb.getMinZ() >= this.getMaxZ()) {
            return y;
        }
        if (y > 0 && bb.getMaxY() <= this.getMinY()) {
            float y1 = this.getMinY() - bb.getMaxY();
            if (y1 < y) {
                y = y1;
            }
        }
        if (y < 0 && bb.getMinY() >= this.getMaxY()) {
            float y2 = this.getMaxY() - bb.getMinY();
            if (y2 > y) {
                y = y2;
            }
        }

        return y;
    }

    public float calculateZOffset(AxisAlignedBB bb, float z) {
        if (bb.getMaxX() <= this.getMinX() || bb.getMinX() >= this.getMaxX()) {
            return z;
        }
        if (bb.getMaxY() <= this.getMinY() || bb.getMinY() >= this.getMaxY()) {
            return z;
        }
        if (z > 0 && bb.getMaxZ() <= this.getMinZ()) {
            float z1 = this.getMinZ() - bb.getMaxZ();
            if (z1 < z) {
                z = z1;
            }
        }
        if (z < 0 && bb.getMinZ() >= this.getMaxZ()) {
            float z2 = this.getMaxZ() - bb.getMinZ();
            if (z2 > z) {
                z = z2;
            }
        }

        return z;
    }

    public bool intersectsWith(AxisAlignedBB bb) {
        if (bb.getMaxY() > this.getMinY() && bb.getMinY() < this.getMaxY()) {
            if (bb.getMaxX() > this.getMinX() && bb.getMinX() < this.getMaxX()) {
                return bb.getMaxZ() > this.getMinZ() && bb.getMinZ() < this.getMaxZ();
            }
        }

        return false;
    }

    public bool isVectorInside(Vector3 vector) {
        return vector.x >= this.getMinX() && vector.x <= this.getMaxX() && vector.y >= this.getMinY() && vector.y <= this.getMaxY() && vector.z >= this.getMinZ() && vector.z <= this.getMaxZ();

    }

    public float getAverageEdgeLength() {
        return (this.getMaxX() - this.getMinX() + this.getMaxY() - this.getMinY() + this.getMaxZ() - this.getMinZ()) / 3;
    }

    public bool isVectorInYZ(Vector3 vector) {
        return vector.y >= this.getMinY() && vector.y <= this.getMaxY() && vector.z >= this.getMinZ() && vector.z <= this.getMaxZ();
    }

    public bool isVectorInXZ(Vector3 vector) {
        return vector.x >= this.getMinX() && vector.x <= this.getMaxX() && vector.z >= this.getMinZ() && vector.z <= this.getMaxZ();
    }

    public bool isVectorInXY(Vector3 vector) {
        return vector.x >= this.getMinX() && vector.x <= this.getMaxX() && vector.y >= this.getMinY() && vector.y <= this.getMaxY();
    }



       public float getMinX() {
        return minX;
    }

     
    public void setMinX(float minX) {
        this.minX = minX;
    }

     
    public float getMinY() {
        return minY;
    }

     
    public void setMinY(float minY) {
        this.minY = minY;
    }

     
    public float getMinZ() {
        return minZ;
    }

     
    public void setMinZ(float minZ) {
        this.minZ = minZ;
    }

     
    public float getMaxX() {
        return maxX;
    }

     
    public void setMaxX(float maxX) {
        this.maxX = maxX;
    }

     
    public float getMaxY() {
        return maxY;
    }

     
    public void setMaxY(float maxY) {
        this.maxY = maxY;
    }

     
    public float getMaxZ() {
        return maxZ;
    }

     
    public void setMaxZ(float maxZ) {
        this.maxZ = maxZ;
    }

 //   AxisAlignedBB clone();

   public float minX;
    public float minY;
    public float minZ;
    public float maxX;
    public float maxY;
    public float maxZ;
}