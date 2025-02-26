using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class BlockOutlineBeh : MonoBehaviour
{
    public bool isCollidingWithPlayer;
    public bool isCollidingWithEntity;

    public BlockData curBlockData;
    public BlockData prevBlockData;
    public Transform outlineRendererTransform;
    public Vector3Int prevPosInt;

    void Start()
    {
        outlineRendererTransform = transform.GetChild(0);
    }
    void FixedUpdate()
    {
        curBlockData = WorldHelper.instance.GetBlockData(transform.position);
        Vector3Int curPosInt = ChunkCoordsHelper.Vec3ToBlockPos(transform.position);
      
        if (curBlockData != prevBlockData||prevPosInt!=curPosInt)
        {
            OnCurrentBlockChanged(curBlockData, prevBlockData);
        }
        prevPosInt=curPosInt;
        prevBlockData= curBlockData;
    }

    public void ManualOnCurrentBlockChanged()
    {
        OnCurrentBlockChanged(curBlockData, prevBlockData);
    }
    public void OnCurrentBlockChanged(BlockData curBlockData1, BlockData prevBlockData1)
    {
        BlockShape? curBlockShape;
        if (Chunk.blockInfosNew.ContainsKey(curBlockData1.blockID))
        {
            curBlockShape= Chunk.blockInfosNew[curBlockData1.blockID].shape;
        }
        else
        {
            curBlockShape= null;
        }

        switch (curBlockShape)
        {
            case null:
                outlineRendererTransform.localScale = new Vector3(0,0, 0);
                return;
            case BlockShape.Solid:
                outlineRendererTransform.localPosition = new Vector3(0, 0, 0);
                outlineRendererTransform.localScale =new Vector3(1,1,1);
                return;
            case BlockShape.SolidTransparent:
                outlineRendererTransform.localPosition = new Vector3(0, 0, 0);
                outlineRendererTransform.localScale = new Vector3(1, 1, 1);
                return;
            case BlockShape.Fence:
                float x1 = -0.5f;
                float y1 = -0.5f;
                float z1 = -0.5f;
                bool[] fenceDatabools = MathUtility.GetBooleanArray(curBlockData1.optionalDataValue);
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
                Vector3 boxExtents1 = boxMaxPoint- boxMinPoint;
                Vector3 boxCenter1 = (boxMaxPoint + boxMinPoint) / 2f;
                outlineRendererTransform.localPosition = boxCenter1;
                outlineRendererTransform.localScale = boxExtents1;
                return;
            case BlockShape.CrossModel:
                outlineRendererTransform.localPosition = new Vector3(0, -0.125f, 0);
                outlineRendererTransform.localScale = new Vector3(0.5f, 0.75f, 0.5f);
              
                return;
          
            case BlockShape.Torch:

                switch (curBlockData1.optionalDataValue)
                {
                    case 0:
                        outlineRendererTransform.localPosition = new Vector3(0, -0.1f, 0);
                        outlineRendererTransform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
                       break;
                    case 1:
                        outlineRendererTransform.localPosition = new Vector3(0.35f, -0.1f, 0);
                        outlineRendererTransform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
                        break;
                    case 2:
                        outlineRendererTransform.localPosition = new Vector3(-0.35f, -0.1f, 0);
                        outlineRendererTransform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
                        break;
                    case 3:
                        outlineRendererTransform.localPosition = new Vector3(0f, -0.1f, 0.35f);
                        outlineRendererTransform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
                        break;
                    case 4:
                        outlineRendererTransform.localPosition = new Vector3(0f, -0.1f, -0.35f);
                        outlineRendererTransform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
                        break;
                    default:
                        outlineRendererTransform.localPosition = new Vector3(0, -0.1f, 0);
                        outlineRendererTransform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
                        break;
                }
                return;

            case BlockShape.Door:
                bool[] doorDataBools = MathUtility.GetBooleanArray(curBlockData1.optionalDataValue);
                float x = -0.5f;
                float y = -0.5f;
                float z = -0.5f;


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

                Vector3 boxExtents = boxMax - boxMin;
                Vector3 boxCenter = (boxMax + boxMin) / 2f;
                outlineRendererTransform.localPosition = boxCenter;
                outlineRendererTransform.localScale = boxExtents;
                return;
        }
    }
    void OnTriggerStay(Collider other){
        if(other.gameObject.tag=="Player"){
            isCollidingWithPlayer=true;

        }else{
            isCollidingWithPlayer=false;
        }
        if(other.gameObject.tag=="Entity"){
            isCollidingWithEntity=true;
        }else{
            isCollidingWithEntity=false;
        }
    }
    void OnDisable(){
        isCollidingWithPlayer=false;
        isCollidingWithEntity=false;
    }
    void OnTriggerExit(Collider other){
        isCollidingWithPlayer=false;
        isCollidingWithEntity=false;
    }
}
