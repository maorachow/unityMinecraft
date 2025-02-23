using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class ItemEntityMeshBuildingHelper
{
    public static bool IsItemBlockShaped(int itemID)
    {
        switch (itemID)
        {
            case 151:
                return false;
            case 152:
                return false;
            case 153:
                return false;
            case 154:
                return false;
            case 155:
                return false;
            case 156:
                return true;
            case 157:
                return false;
            case 158:
                return false;
            default:
                return true;
        }
    }

    public static void BuildItemMesh(int itemID, ref List<Vector3> verts, ref List<Vector2> uvs, ref List<int> tris, ref List<Vector3> norms,float  modelScale=1f,float itemOptionalScale=1f)
    {
        if (itemID == -1)
        {
            return;
        }
        if (IsItemBlockShaped(itemID))
        {
            try
            {
               
                short blockID = ItemIDToBlockID.ItemIDToBlockIDDic[itemID];
                if (blockID == -1)
                {
                    if (itemID == 156)
                    {
                        blockID = 156;
                    }
                    else
                    {

                        return;
                    }
                   
                }
                float x = -0.5f;
                float y = -0.5f;
                float z = -0.5f;
                switch (Chunk.blockInfosNew[blockID].shape)
                {

                    case BlockShape.Solid:

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockID].uvCorners[0], Chunk.blockInfosNew[blockID].uvSizes[0], false, verts, uvs, tris, norms);
                        //Right
                            
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockID].uvCorners[1], Chunk.blockInfosNew[blockID].uvSizes[1], true, verts, uvs, tris, norms);

                        //Bottom
                     
                            BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[2], Chunk.blockInfosNew[blockID].uvSizes[2], false, verts, uvs, tris, norms);
                        //Top

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[3], Chunk.blockInfosNew[blockID].uvSizes[3], true, verts, uvs, tris, norms);

                        //Back

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[4], Chunk.blockInfosNew[blockID].uvSizes[4], true, verts, uvs, tris, norms);
                        //Front

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[5], Chunk.blockInfosNew[blockID].uvSizes[5], false, verts, uvs, tris, norms);
                        break;

                    case BlockShape.SolidTransparent:

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockID].uvCorners[0], Chunk.blockInfosNew[blockID].uvSizes[0], false, verts, uvs, tris, norms);
                        //Right

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockID].uvCorners[1], Chunk.blockInfosNew[blockID].uvSizes[1], true, verts, uvs, tris, norms);

                        //Bottom

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[2], Chunk.blockInfosNew[blockID].uvSizes[2], false, verts, uvs, tris, norms);
                        //Top

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[3], Chunk.blockInfosNew[blockID].uvSizes[3], true, verts, uvs, tris, norms);

                        //Back

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[4], Chunk.blockInfosNew[blockID].uvSizes[4], true, verts, uvs, tris, norms);
                        //Front

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[5], Chunk.blockInfosNew[blockID].uvSizes[5], false, verts, uvs, tris, norms);
                        break;
                    case BlockShape.CrossModel:
                        Vector3 randomCrossModelOffset = new Vector3(0f, 0f, 0f);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), new Vector3(1f, 0f, 1f) + randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), Chunk.blockInfosNew[blockID].uvCorners[0], Chunk.blockInfosNew[blockID].uvSizes[0], false, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), new Vector3(1f, 0f, 1f) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), Chunk.blockInfosNew[blockID].uvCorners[0], Chunk.blockInfosNew[blockID].uvSizes[0], true, verts, uvs, tris, norms);






                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z + 1f) + randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), new Vector3(1f, 0f, -1f) + randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), Chunk.blockInfosNew[blockID].uvCorners[0], Chunk.blockInfosNew[blockID].uvSizes[0], false, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z + 1f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), new Vector3(1f, 0f, -1f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), Chunk.blockInfosNew[blockID].uvCorners[0], Chunk.blockInfosNew[blockID].uvSizes[0], true, verts, uvs, tris, norms);

                        break;
                    case BlockShape.Torch:
                        Matrix4x4 transformMat = Matrix4x4.identity;

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockID].uvCorners[0], Chunk.blockInfosNew[blockID].uvSizes[0], false, verts, uvs, tris, norms);
                        //Right

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockID].uvCorners[1], Chunk.blockInfosNew[blockID].uvSizes[1], true, verts, uvs, tris, norms);

                        //Bottom

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockID].uvCorners[2], Chunk.blockInfosNew[blockID].uvSizes[2], false, verts, uvs, tris, norms);
                        //Top

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0.625f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockID].uvCorners[3], Chunk.blockInfosNew[blockID].uvSizes[3], true, verts, uvs, tris, norms);

                        //Back
                            
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockID].uvCorners[4], Chunk.blockInfosNew[blockID].uvSizes[4], true, verts, uvs, tris, norms);
                        //Front

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0f, 0f, 0.125f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockID].uvCorners[5], Chunk.blockInfosNew[blockID].uvSizes[5], false, verts, uvs, tris, norms);

                        break;

                    case BlockShape.Water:
                        BlockMeshBuildingHelper. BuildFaceComplex(new Vector3(x, y, z), new Vector3(0f, 0.8f, 0f), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockID].uvCorners[0], Chunk.blockInfosNew[blockID].uvSizes[0], false, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0f, 0.8f, 0f), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockID].uvCorners[1], Chunk.blockInfosNew[blockID].uvSizes[1], true, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[2], Chunk.blockInfosNew[blockID].uvSizes[2], false, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y + 0.8f, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[3], Chunk.blockInfosNew[blockID].uvSizes[3], true, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0f, 0.8f, 0f), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[4], Chunk.blockInfosNew[blockID].uvSizes[4], true, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0f, 1f, 0f), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockID].uvCorners[4], Chunk.blockInfosNew[blockID].uvSizes[4], true, verts, uvs, tris, norms);
                        break;
                    case BlockShape.Fence:
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f), new Vector3(0f, 1, 0f), new Vector3(0f, 0f, 0.25f), Chunk.blockInfosNew[blockID].uvCorners[0], Chunk.blockInfosNew[blockID].uvSizes[0], false, verts, uvs, tris, norms);
                        //Right

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f) + new Vector3(0.25f, 0f, 0f), new Vector3(0f, 1, 0f), new Vector3(0f, 0f, 0.25f), Chunk.blockInfosNew[blockID].uvCorners[1], Chunk.blockInfosNew[blockID].uvSizes[1], true, verts, uvs, tris, norms);

                        //Bottom

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f), new Vector3(0f, 0f, 0.25f), new Vector3(0.25f, 0f, 0f), Chunk.blockInfosNew[blockID].uvCorners[2], Chunk.blockInfosNew[blockID].uvSizes[2], false, verts, uvs, tris, norms);
                        //Top

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 1f, 0.375f), new Vector3(0f, 0f, 0.25f), new Vector3(0.25f, 0f, 0f), Chunk.blockInfosNew[blockID].uvCorners[3], Chunk.blockInfosNew[blockID].uvSizes[3], true, verts, uvs, tris, norms);

                        //Back

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f), new Vector3(0f, 1, 0f), new Vector3(0.25f, 0f, 0f), Chunk.blockInfosNew[blockID].uvCorners[4], Chunk.blockInfosNew[blockID].uvSizes[4], true, verts, uvs, tris, norms);
                        //Front

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f) + new Vector3(0f, 0f, 0.25f), new Vector3(0f, 1, 0f), new Vector3(0.25f, 0f, 0f), Chunk.blockInfosNew[blockID].uvCorners[5], Chunk.blockInfosNew[blockID].uvSizes[5], false, verts, uvs, tris, norms);
                        break;
                }

            }
            catch (Exception e)
            {
                Debug.Log(e);
                // ignored
            }

          

        }
        else
        {
            BuildFlatItemModel(itemID, ref verts, ref uvs, ref tris, ref norms, itemOptionalScale);
        }
    }



    public static void BuildFlatItemModel(int itemID, ref List<Vector3> verts, ref List<Vector2> uvs, ref List<int> tris, ref List<Vector3> norms,float optionalScale=1f)
    {
        float x = 0f;
        float y = 0f;
        float z = 0f;
        BuildFlatItemFace(ItemEntityBeh.itemMaterialInfo[itemID].x, ItemEntityBeh.itemMaterialInfo[itemID].y, 0.0625f, new Vector3(x, y, z) / 16 * optionalScale, Vector3.forward * ItemEntityBeh.textureXSize / 4 / 16* optionalScale, Vector3.right * ItemEntityBeh.textureYSize / 4 / 16 * optionalScale, false, verts, uvs, tris, norms);
        BuildFlatItemFace(ItemEntityBeh.itemMaterialInfo[itemID].x, ItemEntityBeh.itemMaterialInfo[itemID].y, 0.0625f, new Vector3(x, y + 1f, z) / 16 * optionalScale, Vector3.forward * ItemEntityBeh.textureXSize / 4 / 16 * optionalScale, Vector3.right * ItemEntityBeh.textureYSize / 4 / 16 * optionalScale, true, verts, uvs, tris, norms);
        for (int i = 0; i < ItemEntityBeh.textureXSize; i++)
        {
            for (int j = 0; j < ItemEntityBeh.textureYSize; j++)
            {


                BuildModelPixel(ItemEntityBeh.itemTexturePosInfo[itemID].x + i, ItemEntityBeh.itemTexturePosInfo[itemID].y + j, ItemEntityBeh.itemTexturePosInfo[itemID].x, ItemEntityBeh.itemTexturePosInfo[itemID].y, 1f * optionalScale, verts, uvs, tris, norms);
                //           Debug.Log((float)(itemTexturePosInfo[itemID].x + i) / textureXSize * 0.0625f);



            }
        }
        float offsetX = -0.5f;
        float offsetY = -0.5f;
        float offsetZ = -0.5f;
        
            for (int i = 0; i < verts.Count; i++)
            {
                verts[i] = new Vector3(verts[i].x + offsetX, verts[i].y + offsetY, verts[i].z + offsetZ);

            }
         

    }
    public static void BuildModelPixel(int x, int y, int originX, int originY, float scale,  List<Vector3> verts,  List<Vector2> uvs,  List<int> tris,  List<Vector3> norms)
    {


        if (ItemEntityBeh.itemTexture.GetPixel(x, y).a != 0f && ItemEntityBeh.itemTexture.GetPixel(x + 1, y).a == 0f)
        {
            //right
            BuildFlatItemFace((float)x / (float)ItemEntityBeh.textureXSize * 0.0625f, (float)y / (float)ItemEntityBeh.textureYSize * 0.0625f, (float)1f / 64f / 16f, new Vector3(x - originX + 1, 0, y - originY) / 4f / 16f * scale, Vector3.up / 16f * scale, Vector3.forward / 4f / 16f * scale, true, verts, uvs, tris, norms);

        }
        if (ItemEntityBeh.itemTexture.GetPixel(x, y).a != 0f && ItemEntityBeh.itemTexture.GetPixel(x - 1, y).a == 0f)
        {
            //left
            BuildFlatItemFace((float)x / (float)ItemEntityBeh.textureXSize * 0.0625f, (float)y / (float)ItemEntityBeh.textureYSize * 0.0625f, (float)1f / 64f / 16f, new Vector3(x - originX, 0, y - originY) / 4f / 16f * scale, Vector3.up / 16 * scale, Vector3.forward / 4 / 16 * scale, false, verts, uvs, tris,norms);

        }
        if (ItemEntityBeh.itemTexture.GetPixel(x, y).a != 0f && ItemEntityBeh.itemTexture.GetPixel(x, y + 1).a == 0f)
        {
            //front
            BuildFlatItemFace((float)x / (float)ItemEntityBeh.textureXSize * 0.0625f, (float)y / (float)ItemEntityBeh.textureYSize * 0.0625f, (float)1f / 64f / 16f, new Vector3(x - originX, 0, y - originY + 1) / 4f / 16f * scale, Vector3.up / 16 * scale, Vector3.right / 4 / 16 * scale, false, verts, uvs, tris, norms);

        }
        if (ItemEntityBeh.itemTexture.GetPixel(x, y).a != 0f && ItemEntityBeh.itemTexture.GetPixel(x, y - 1).a == 0f)
        {
            //back
            BuildFlatItemFace((float)x / (float)ItemEntityBeh.textureXSize * 0.0625f, (float)y / (float)ItemEntityBeh.textureYSize * 0.0625f, (float)1f / 64f / 16f, new Vector3(x - originX, 0, y - originY) / 4f / 16f * scale, Vector3.up / 16 * scale, Vector3.right / 4 / 16 * scale, true, verts, uvs, tris, norms);

        }

    }
    static void BuildFlatItemFace(float uvX, float uvY, float uvWidthXY, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, List<Vector3> norms)
    {
        Vector2 uvCorner = new Vector2(uvX, uvY);

        int index = verts.Count;

        verts.Add(corner);
        verts.Add(corner + up);
        verts.Add(corner + up + right);
        verts.Add(corner + right);



        Vector2 uvWidth = new Vector2(uvWidthXY, uvWidthXY);


        //uvCorner.x = (float)(typeid - 1) / 16;


        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));

        if (reversed)
        {
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 0);
            Vector3 v1 = Vector3.Cross(up, right);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
        }
        else
        {
            tris.Add(index + 1);
            tris.Add(index + 0);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 0);
            Vector3 v1 = -Vector3.Cross(up, right);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
        }

    }
}
