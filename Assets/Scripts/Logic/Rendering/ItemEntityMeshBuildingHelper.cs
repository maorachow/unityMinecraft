using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static UnityEditor.Progress;

public static class ItemEntityMeshBuildingHelper
{
    public static bool IsItemBlockShaped(int itemID)
    {
        if (itemID == -1 || itemID == 0)
        {
            return true;
        }
        ItemMeshBuildingInfo info = ItemEntityBeh.itemMaterialInfo[itemID];
        return info.isItemBlockShaped;
    }

    public static void BuildItemMesh(int itemID, ref NativeList<Vector3> verts, ref NativeList<Vector2> uvs, ref NativeList<int> tris, ref NativeList<Vector3> norms,float  modelScale=1f,float itemOptionalScale=1f,bool forceBlockShaped=false)
    {
        if (itemID == -1||itemID==0)
        {
            return;
        }

        if (!ItemEntityBeh.itemMaterialInfo.ContainsKey(itemID))
        {
            return;
        }
        if (IsItemBlockShaped(itemID)|| forceBlockShaped==true)
        {
            try
            {
               
                short blockID = forceBlockShaped? (short)itemID : GlobalGameResourcesManager.instance.itemIDToBlockIDMapper.ToBlockID(itemID);
                if (!GlobalGameResourcesManager.instance.itemIDToBlockIDMapper.CanMapToBlockID(itemID))
                {
                   
                        return;
                    
                   
                }
                float x = -0.5f;
                float y = -0.5f;
                float z = -0.5f;
                BlockInfo blockShapeThis =
                    GlobalGameResourcesManager.instance.meshBuildingInfoDataProvider.GetBlockInfo(blockID);

                switch (blockShapeThis.shape)
                {

                    case BlockShape.Solid:

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), blockShapeThis.uvCorners[0], blockShapeThis.uvSizes[0], false, verts, uvs, tris, norms);
                        //Right
                            
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), blockShapeThis.uvCorners[1], blockShapeThis.uvSizes[1], true, verts, uvs, tris, norms);

                        //Bottom
                     
                            BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), blockShapeThis.uvCorners[2], blockShapeThis.uvSizes[2], false, verts, uvs, tris, norms);
                        //Top

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), blockShapeThis.uvCorners[3], blockShapeThis.uvSizes[3], true, verts, uvs, tris, norms);

                        //Back

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), blockShapeThis.uvCorners[4], blockShapeThis.uvSizes[4], true, verts, uvs, tris, norms);
                        //Front

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), blockShapeThis.uvCorners[5], blockShapeThis.uvSizes[5], false, verts, uvs, tris, norms);
                        break;

                    case BlockShape.SolidTransparent:

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), blockShapeThis.uvCorners[0], blockShapeThis.uvSizes[0], false, verts, uvs, tris, norms);
                        //Right

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), blockShapeThis.uvCorners[1], blockShapeThis.uvSizes[1], true, verts, uvs, tris, norms);

                        //Bottom

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), blockShapeThis.uvCorners[2], blockShapeThis.uvSizes[2], false, verts, uvs, tris, norms);
                        //Top

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), blockShapeThis.uvCorners[3], blockShapeThis.uvSizes[3], true, verts, uvs, tris, norms);

                        //Back

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), blockShapeThis.uvCorners[4], blockShapeThis.uvSizes[4], true, verts, uvs, tris, norms);
                        //Front

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), blockShapeThis.uvCorners[5], blockShapeThis.uvSizes[5], false, verts, uvs, tris, norms);
                        break;
                    case BlockShape.CrossModel:
                        Vector3 randomCrossModelOffset = new Vector3(0f, 0f, 0f);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), new Vector3(1f, 0f, 1f) + randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), blockShapeThis.uvCorners[0], blockShapeThis.uvSizes[0], false, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), new Vector3(1f, 0f, 1f) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), blockShapeThis.uvCorners[0], blockShapeThis.uvSizes[0], true, verts, uvs, tris, norms);






                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z + 1f) + randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), new Vector3(1f, 0f, -1f) + randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), blockShapeThis.uvCorners[0], blockShapeThis.uvSizes[0], false, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z + 1f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), new Vector3(1f, 0f, -1f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), blockShapeThis.uvCorners[0], blockShapeThis.uvSizes[0], true, verts, uvs, tris, norms);

                        break;
                    case BlockShape.Torch:
                        Matrix4x4 transformMat = Matrix4x4.identity;

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), blockShapeThis.uvCorners[0], blockShapeThis.uvSizes[0], true, verts, uvs, tris, norms);
                        //Right

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), blockShapeThis.uvCorners[1], blockShapeThis.uvSizes[1], false, verts, uvs, tris, norms);

                        //Bottom

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), blockShapeThis.uvCorners[2], blockShapeThis.uvSizes[2], true, verts, uvs, tris, norms);
                        //Top

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0.625f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), blockShapeThis.uvCorners[3], blockShapeThis.uvSizes[3], false, verts, uvs, tris, norms);

                        //Back
                            
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), blockShapeThis.uvCorners[4], blockShapeThis.uvSizes[4], false, verts, uvs, tris, norms);
                        //Front

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0f, 0f, 0.125f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), blockShapeThis.uvCorners[5], blockShapeThis.uvSizes[5], true, verts, uvs, tris, norms);

                        break;

                    case BlockShape.Water:
                        BlockMeshBuildingHelper. BuildFaceComplex(new Vector3(x, y, z), new Vector3(0f, 0.8f, 0f), new Vector3(0, 0, 1), blockShapeThis.uvCorners[0], blockShapeThis.uvSizes[0], false, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0f, 0.8f, 0f), new Vector3(0, 0, 1), blockShapeThis.uvCorners[1], blockShapeThis.uvSizes[1], true, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), blockShapeThis.uvCorners[2], blockShapeThis.uvSizes[2], false, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y + 0.8f, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), blockShapeThis.uvCorners[3], blockShapeThis.uvSizes[3], true, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z), new Vector3(0f, 0.8f, 0f), new Vector3(1, 0, 0), blockShapeThis.uvCorners[4], blockShapeThis.uvSizes[4], true, verts, uvs, tris, norms);
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0f, 0.8f, 0f), new Vector3(1, 0, 0), blockShapeThis.uvCorners[5], blockShapeThis.uvSizes[5], false, verts, uvs, tris, norms);
                        break;
                    case BlockShape.Fence:
                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f), new Vector3(0f, 1, 0f), new Vector3(0f, 0f, 0.25f), blockShapeThis.uvCorners[0], blockShapeThis.uvSizes[0], false, verts, uvs, tris, norms);
                        //Right

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f) + new Vector3(0.25f, 0f, 0f), new Vector3(0f, 1, 0f), new Vector3(0f, 0f, 0.25f), blockShapeThis.uvCorners[1], blockShapeThis.uvSizes[1], true, verts, uvs, tris, norms);

                        //Bottom

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f), new Vector3(0f, 0f, 0.25f), new Vector3(0.25f, 0f, 0f), blockShapeThis.uvCorners[2], blockShapeThis.uvSizes[2], false, verts, uvs, tris, norms);
                        //Top

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 1f, 0.375f), new Vector3(0f, 0f, 0.25f), new Vector3(0.25f, 0f, 0f), blockShapeThis.uvCorners[3], blockShapeThis.uvSizes[3], true, verts, uvs, tris, norms);

                        //Back

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f), new Vector3(0f, 1, 0f), new Vector3(0.25f, 0f, 0f), blockShapeThis.uvCorners[4], blockShapeThis.uvSizes[4], true, verts, uvs, tris, norms);
                        //Front

                        BlockMeshBuildingHelper.BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f) + new Vector3(0f, 0f, 0.25f), new Vector3(0f, 1, 0f), new Vector3(0.25f, 0f, 0f), blockShapeThis.uvCorners[5], blockShapeThis.uvSizes[5], false, verts, uvs, tris, norms);
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


    public static void BuildItemMesh(ref Mesh targetMesh,int itemID,float optionalScale=1f,bool forceBlockShaped=false)
    {
        NativeList<Vector3> verts = new NativeList<Vector3>(Allocator.Temp);
        NativeList<Vector2> uvs = new NativeList<Vector2>(Allocator.Temp);
        NativeList<int> tris = new NativeList<int>(Allocator.Temp);
        NativeList<Vector3> norms = new NativeList<Vector3>(Allocator.Temp);


        ItemEntityMeshBuildingHelper.BuildItemMesh(itemID, ref verts, ref uvs, ref tris, ref norms, 1, optionalScale, forceBlockShaped);
        NativeArray<Vector3> vertsArray = verts.ToArray(Allocator.Temp);
        NativeArray<Vector2> uvsArray = uvs.ToArray(Allocator.Temp);
        NativeArray<int> trisArray = tris.ToArray(Allocator.Temp);
        NativeArray<Vector3> normsArray = norms.ToArray(Allocator.Temp);
        targetMesh.SetVertices(vertsArray);
        targetMesh.SetNormals(normsArray);
        targetMesh.SetUVs(0, uvsArray);
        targetMesh.SetIndices(trisArray, MeshTopology.Triangles, 0);
        targetMesh.RecalculateBounds();

        targetMesh.RecalculateTangents();
        vertsArray.Dispose();
        uvsArray.Dispose();
        trisArray.Dispose();
        normsArray.Dispose();
        verts.Dispose();
        norms.Dispose();
        tris.Dispose();
        uvs.Dispose();
    } 
     static void BuildFlatItemModel(int itemID, ref NativeList<Vector3> verts, ref NativeList<Vector2> uvs, ref NativeList<int> tris, ref NativeList<Vector3> norms,float optionalScale=1f)
    {
        float x = 0f;
        float y = 0f;
        float z = 0f;
        BuildFlatItemFace(ItemEntityBeh.itemMaterialInfo[itemID].uvCorner.x,
            ItemEntityBeh.itemMaterialInfo[itemID].uvCorner.y,
            new Vector2(ItemEntityBeh.itemMaterialInfo[itemID].uvSize.x,
                ItemEntityBeh.itemMaterialInfo[itemID].uvSize.y), new Vector3(x, y, z) * optionalScale,
            Vector3.forward * optionalScale, Vector3.right * optionalScale, false, ref verts, ref uvs, ref tris,
            ref norms);


        BuildFlatItemFace(ItemEntityBeh.itemMaterialInfo[itemID].uvCorner.x,
            ItemEntityBeh.itemMaterialInfo[itemID].uvCorner.y,
            new Vector2(ItemEntityBeh.itemMaterialInfo[itemID].uvSize.x,
                ItemEntityBeh.itemMaterialInfo[itemID].uvSize.y), new Vector3(x, y + 1f / 16f, z) * optionalScale,
            Vector3.forward * optionalScale, Vector3.right * optionalScale, true, ref verts, ref uvs, ref tris,
            ref norms);

 
        float deltaPixelX = 1f / ItemEntityBeh.fullTextureXSize;
        float deltaPixelY = 1f / ItemEntityBeh.fullTextureYSize;

        float itemPixelSizeX = ItemEntityBeh.itemMaterialInfo[itemID].uvSize.x * ItemEntityBeh.fullTextureXSize;
        float itemPixelSizeY = ItemEntityBeh.itemMaterialInfo[itemID].uvSize.y * ItemEntityBeh.fullTextureYSize;
        float deltaFaceX = 1f / itemPixelSizeX;
        float deltaFaceY = 1f / itemPixelSizeY;
        for (int i = 0; i < (int)itemPixelSizeX; i++)
        {
            for (int j = 0; j < (int)itemPixelSizeY; j++)
            {
             

                BuildModelPixel((float)i * deltaFaceX, (float)j * deltaFaceY,
                    ItemEntityBeh.itemMaterialInfo[itemID].uvCorner + new Vector2(deltaPixelX * i, deltaPixelY * j),
                    new Vector2(deltaPixelX, deltaPixelY), 1f * optionalScale, ref verts, ref uvs, ref tris, ref norms, deltaFaceX,
                    deltaFaceY, deltaPixelX, deltaPixelY);
                //           Debug.Log((float)(itemTexturePosInfo[itemID].x + i) / textureXSize * 0.0625f);



            }
        }
        float offsetX = -0.5f;
        float offsetY = -0.5f / 16f;
        float offsetZ = -0.5f;
        
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = new Vector3(verts[i].x + offsetX, verts[i].y + offsetY, verts[i].z + offsetZ);

            }
         

    }
     static void BuildModelPixel(float x, float y, Vector2 uvCorner, Vector2 uvWidth, float scale,  ref NativeList<Vector3> verts, ref NativeList<Vector2> uvs, ref NativeList<int> tris, ref NativeList<Vector3> norms, float deltaFaceX, float deltaFaceY, float deltaPixelX, float detaPixelY)
    {

        int pixelX =(int) ((float)ItemEntityBeh.fullTextureXSize* uvCorner.x);
        int pixelY = (int)((float)ItemEntityBeh.fullTextureYSize * uvCorner.y);
      
        if (ItemEntityBeh.itemTexture.GetPixel(pixelX, pixelY).a != 0f && ItemEntityBeh.itemTexture.GetPixel(pixelX + 1, pixelY).a == 0f)
        {
            //right
            BuildFlatItemFace(uvCorner.x, uvCorner.y , uvWidth, new Vector3(x + deltaFaceX, 0, y )  * scale, Vector3.up / 16f * scale, Vector3.forward * deltaFaceX * scale, true, ref verts, ref uvs, ref tris, ref norms);

        }
        if (ItemEntityBeh.itemTexture.GetPixel(pixelX, pixelY).a != 0f && ItemEntityBeh.itemTexture.GetPixel(pixelX - 1, pixelY).a == 0f)
        {
            //left
            BuildFlatItemFace(uvCorner.x , uvCorner.y , uvWidth, new Vector3(x, 0, y )  * scale, Vector3.up / 16 * scale, Vector3.forward * deltaFaceX * scale, false, ref verts, ref uvs, ref tris, ref norms);

        }
        if (ItemEntityBeh.itemTexture.GetPixel(pixelX, pixelY).a != 0f && ItemEntityBeh.itemTexture.GetPixel(pixelX, pixelY + 1).a == 0f)
        {
            //front
            BuildFlatItemFace(uvCorner.x , uvCorner.y , uvWidth, new Vector3(x, 0, y + deltaFaceY)  * scale, Vector3.up / 16 * scale, Vector3.right * deltaFaceX * scale, false, ref verts, ref uvs, ref tris, ref norms);

        }
        if (ItemEntityBeh.itemTexture.GetPixel(pixelX, pixelY).a != 0f && ItemEntityBeh.itemTexture.GetPixel(pixelX, pixelY - 1).a == 0f)
        {
            //back
            BuildFlatItemFace(uvCorner.x , uvCorner.y, uvWidth, new Vector3(x , 0, y )* scale, Vector3.up / 16 * scale, Vector3.right * deltaFaceX * scale, true, ref verts, ref uvs, ref tris, ref norms);

        }

    }
    static void BuildFlatItemFace(float uvX, float uvY, Vector2 uvWidthXY, Vector3 corner, Vector3 up, Vector3 right, bool reversed, ref NativeList<Vector3> verts, ref NativeList<Vector2> uvs, ref NativeList<int> tris, ref NativeList<Vector3> norms)
    {
        Vector2 uvCorner = new Vector2(uvX, uvY);

        int index = verts.Length;

        verts.Add(corner);
        verts.Add(corner + up);
        verts.Add(corner + up + right);
        verts.Add(corner + right);



        Vector2 uvWidth = new Vector2(uvWidthXY.x, uvWidthXY.y);


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
