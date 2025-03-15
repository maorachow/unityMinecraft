using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;


//commit
public static partial class BlockMeshBuildingHelper
{

    public static void FillMeshData(ref Mesh.MeshDataArray mda, ref List<Vector3> positions, ref List<Vector2> uvs,
        ref List<Vector3> normals,ref List<Vector3> tangents,ref List<int> tris)
    {
        var data = mda[0];
        data.SetVertexBufferParams(positions.Count, ChunkVertex.vertexAttributesDes);
        NativeArray<ChunkVertex> pos = data.GetVertexData<ChunkVertex>();

        for (int i = 0; i < positions.Count; i++)
        {
           
            pos[i] = new ChunkVertex(positions[i], normals[i], tangents[i], uvs[i]);
        }

        data.SetIndexBufferParams((int)(pos.Length / 2 * 3), IndexFormat.UInt32);
        var ib = data.GetIndexData<int>();
        for (int i = 0; i < ib.Length; ++i)
            ib[i] = tris[i];

        data.subMeshCount = 1;
        data.SetSubMesh(0, new SubMeshDescriptor(0, ib.Length, MeshTopology.Triangles));
        
      //  pos.Dispose();
    //    ib.Dispose();
    }

    public static void FillMeshData(ref Mesh.MeshDataArray mda, ref NativeList<Vector3> positions, ref NativeList<Vector2> uvs,
        ref NativeList<Vector3> normals, ref NativeList<Vector3> tangents, ref NativeList<int> tris)
    {
        var data = mda[0];
        data.SetVertexBufferParams(positions.Length, ChunkVertex.vertexAttributesDes);
        NativeArray<ChunkVertex> pos = data.GetVertexData<ChunkVertex>();

        for (int i = 0; i < positions.Length; i++)
        {

            pos[i] = new ChunkVertex(positions[i], normals[i], tangents[i], uvs[i]);
        }

        data.SetIndexBufferParams((int)(pos.Length / 2 * 3), IndexFormat.UInt32);
        var ib = data.GetIndexData<int>();
        for (int i = 0; i < ib.Length; ++i)
            ib[i] = tris[i];

        data.subMeshCount = 1;
        data.SetSubMesh(0, new SubMeshDescriptor(0, ib.Length, MeshTopology.Triangles));

        //  pos.Dispose();
        //    ib.Dispose();
    }
    public static void BuildSingleBlock(IChunkFaceBuildingChecks curChunk, int x, int y, int z, BlockData blockData,
        ref NativeList<Vector3> OpqVerts, 
        ref NativeList<Vector2> OpqUVs,
        ref NativeList<int> OpqTris,
        ref NativeList<Vector3> OpqNorms,
        ref NativeList<Vector3> OpqTangents,


        ref NativeList<Vector3> NSVerts,
        ref NativeList<Vector2> NSUVs,
        ref NativeList<int> NSTris,
        ref NativeList<Vector3> NSNorms,
        ref NativeList<Vector3> NSTangents,

        ref NativeList<Vector3> WTVerts,
        ref NativeList<Vector2> WTUVs,
        ref NativeList<int> WTTris,
        ref NativeList<Vector3> WTNorms,
        ref NativeList<Vector3> WTTangents,

        ref NativeList<Vector3> TSVerts,
        ref NativeList<Vector2> TSUVs,
        ref NativeList<int> TSTris,
        ref NativeList<Vector3> TSNorms,
        ref NativeList<Vector3> TSTangents
        )
    {
        if (blockData.blockID == 0)
        {
            return;
        }
        if ( !Chunk.IsBlockIDValid(blockData))
        {
            if (curChunk.CheckNeedBuildFace(x - 1, y, z, blockData))
                BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.unknownBlockInfo.uvCorners[0], Chunk.unknownBlockInfo.uvSizes[0], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
            //Right
            if (curChunk.CheckNeedBuildFace(x + 1, y, z, blockData))
                BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.unknownBlockInfo.uvCorners[1], Chunk.unknownBlockInfo.uvSizes[1], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

            //Bottom
            if (curChunk.CheckNeedBuildFace(x, y - 1, z, blockData))
                BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.unknownBlockInfo.uvCorners[2], Chunk.unknownBlockInfo.uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
            //Top
            if (curChunk.CheckNeedBuildFace(x, y + 1, z, blockData))
                BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.unknownBlockInfo.uvCorners[3], Chunk.unknownBlockInfo.uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

            //Back
            if (curChunk.CheckNeedBuildFace(x, y, z - 1, blockData))
                BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.unknownBlockInfo.uvCorners[4], Chunk.unknownBlockInfo.uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
            //Front
            if (curChunk.CheckNeedBuildFace(x, y, z + 1, blockData))
                BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.unknownBlockInfo.uvCorners[5], Chunk.unknownBlockInfo.uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
            return;
        }
        short typeid = blockData.blockID;
        switch (Chunk.blockInfosNew[blockData.blockID].shape)
        {
            case BlockShape.Solid:
                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 6 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 6)
                {
                    return;
                }
                if (curChunk.CheckNeedBuildFace(x - 1, y, z, blockData))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                //Right
                if (curChunk.CheckNeedBuildFace(x + 1, y, z, blockData))
                    BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                //Bottom
                if (curChunk.CheckNeedBuildFace(x, y - 1, z, blockData))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                //Top
                if (curChunk.CheckNeedBuildFace(x, y + 1, z, blockData))
                    BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                //Back
                if (curChunk.CheckNeedBuildFace(x, y, z - 1, blockData))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                //Front
                if (curChunk.CheckNeedBuildFace(x, y, z + 1, blockData))
                    BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                break;
            case BlockShape.Water:

                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 6 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 6)
                {
                    return;
                }

                //water
                //left
                if (curChunk.CheckNeedBuildFace(x - 1, y, z, blockData))
                {
                    if (curChunk.GetChunkBlockType(x, y + 1, z) != 100)
                    {
                        BuildFaceComplex(new Vector3(x, y, z), new Vector3(0f, 0.8f, 0f), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,WTVerts,WTUVs,WTTris,WTNorms,WTTangents);




                    }
                    else
                    {
                        BuildFaceComplex(new Vector3(x, y, z), new Vector3(0f, 1f, 0f), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,WTVerts,WTUVs,WTTris,WTNorms,WTTangents);






                    }

                }

                //Right
                if (curChunk.CheckNeedBuildFace(x + 1, y, z, blockData))
                {
                    if (curChunk.GetChunkBlockType(x, y + 1, z) != 100)
                    {
                        BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0f, 0.8f, 0f), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true,WTVerts,WTUVs,WTTris,WTNorms,WTTangents);



                    }
                    else
                    {
                        BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0f, 1f, 0f), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true,WTVerts,WTUVs,WTTris,WTNorms,WTTangents);



                    }

                }



                //Bottom
                if (curChunk.CheckNeedBuildFace(x, y - 1, z, blockData))
                {
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false,WTVerts,WTUVs,WTTris,WTNorms,WTTangents);




                }

                //Top
                if (curChunk.CheckNeedBuildFace(x, y + 1, z, blockData))
                {
                    BuildFaceComplex(new Vector3(x, y + 0.8f, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true,WTVerts,WTUVs,WTTris,WTNorms,WTTangents);




                }




                //Back
                if (curChunk.CheckNeedBuildFace(x, y, z - 1, blockData))
                {
                    if (curChunk.GetChunkBlockType(x, y + 1, z) != 100)
                    {
                        BuildFaceComplex(new Vector3(x, y, z), new Vector3(0f, 0.8f, 0f), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true,WTVerts,WTUVs,WTTris,WTNorms,WTTangents);




                    }
                    else
                    {
                        BuildFaceComplex(new Vector3(x, y, z), new Vector3(0f, 1f, 0f), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true,WTVerts,WTUVs,WTTris,WTNorms,WTTangents);







                    }

                }


                //Front
                if (curChunk.CheckNeedBuildFace(x, y, z + 1, blockData))
                {
                    if (curChunk.GetChunkBlockType(x, y + 1, z) != 100)
                    {
                        BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0f, 0.8f, 0f), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false,WTVerts,WTUVs,WTTris,WTNorms,WTTangents);


                    }
                    else
                    {
                        BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0f, 1f, 0f), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false,WTVerts,WTUVs,WTTris,WTNorms,WTTangents);

                    }

                }
                break;
            case BlockShape.Torch:
                //datavalues: 0ground 1left 2right 3front 4back
                if (curChunk is Chunk)
                {
                    (curChunk as Chunk).lightPoints.Add(new Vector3(x, y, z) + new Vector3(0.5f, 0.725f, 0.5f));
                }

                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 6 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 6)
                {
                    return;
                }
                switch (blockData.optionalDataValue)
                {
                    case 0:
                        Matrix4x4 transformMat = Matrix4x4.identity;

                        BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Right

                        BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        //Bottom

                        BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Top

                        BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0.625f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        //Back

                        BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Front

                        BuildFaceComplex(new Vector3(x, y, z), transformMat, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0f, 0f, 0.125f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        break;
                    case 1:

                        Matrix4x4 transformMat1 = Matrix4x4.Rotate(Quaternion.Euler(0,0,25f)) * Matrix4x4.Translate(new Vector3(0.3f, 0.1f, 0f));

                        BuildFaceComplex(new Vector3(x, y, z), transformMat1, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Right

                        BuildFaceComplex(new Vector3(x, y, z), transformMat1, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        //Bottom

                        BuildFaceComplex(new Vector3(x, y, z), transformMat1, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Top

                        BuildFaceComplex(new Vector3(x, y, z), transformMat1, new Vector3(x, y, z) + new Vector3(0.4375f, 0.625f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        //Back

                        BuildFaceComplex(new Vector3(x, y, z), transformMat1, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Front

                        BuildFaceComplex(new Vector3(x, y, z), transformMat1, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0f, 0f, 0.125f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        break;
                    case 2:

                        Matrix4x4 transformMat2 = Matrix4x4.Rotate(Quaternion.Euler(0, 0, -25f)) * Matrix4x4.Translate(new Vector3(-0.3f, 0.1f, 0f));

                        BuildFaceComplex(new Vector3(x, y, z), transformMat2, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Right

                        BuildFaceComplex(new Vector3(x, y, z), transformMat2, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        //Bottom

                        BuildFaceComplex(new Vector3(x, y, z), transformMat2, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Top

                        BuildFaceComplex(new Vector3(x, y, z), transformMat2, new Vector3(x, y, z) + new Vector3(0.4375f, 0.625f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        //Back

                        BuildFaceComplex(new Vector3(x, y, z), transformMat2, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Front

                        BuildFaceComplex(new Vector3(x, y, z), transformMat2, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0f, 0f, 0.125f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        break;
                    case 3:

                        Matrix4x4 transformMat3 = Matrix4x4.Rotate(Quaternion.Euler(-25f, 0, 0)) * Matrix4x4.Translate(new Vector3(0f, 0.1f, 0.3f));

                        BuildFaceComplex(new Vector3(x, y, z), transformMat3, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Right

                        BuildFaceComplex(new Vector3(x, y, z), transformMat3, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        //Bottom

                        BuildFaceComplex(new Vector3(x, y, z), transformMat3, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Top

                        BuildFaceComplex(new Vector3(x, y, z), transformMat3, new Vector3(x, y, z) + new Vector3(0.4375f, 0.625f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        //Back

                        BuildFaceComplex(new Vector3(x, y, z), transformMat3, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Front

                        BuildFaceComplex(new Vector3(x, y, z), transformMat3, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0f, 0f, 0.125f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        break;
                    case 4:

                        Matrix4x4 transformMat4 = Matrix4x4.Rotate(Quaternion.Euler(25f, 0, 0)) * Matrix4x4.Translate(new Vector3(0f, 0.1f, -0.3f));

                        BuildFaceComplex(new Vector3(x, y, z), transformMat4, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Right

                        BuildFaceComplex(new Vector3(x, y, z), transformMat4, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0.625f, 0f), new Vector3(0f, 0f, 0.125f), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        //Bottom

                        BuildFaceComplex(new Vector3(x, y, z), transformMat4, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Top

                        BuildFaceComplex(new Vector3(x, y, z), transformMat4, new Vector3(x, y, z) + new Vector3(0.4375f, 0.625f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        //Back

                        BuildFaceComplex(new Vector3(x, y, z), transformMat4, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        //Front

                        BuildFaceComplex(new Vector3(x, y, z), transformMat4, new Vector3(x, y, z) + new Vector3(0.4375f, 0f, 0.4375f) + new Vector3(0f, 0f, 0.125f), new Vector3(0f, 0.625f, 0f), new Vector3(0.125f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                        break;
                }


                //   lightPoints.Add(new Vector3(x, y, z) + new Vector3(0.5f, 0.725f, 0.5f) + new Vector3(chunkPos.x, 0, chunkPos.y));
                break;

            case BlockShape.CrossModel:
                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 1 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 1)
                {
                    return;
                }
                Vector3 randomCrossModelOffset = new Vector3(0f, 0f, 0f);
                BuildFaceComplex(new Vector3(x, y, z) + randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), new Vector3(1f, 0f, 1f) + randomCrossModelOffset + new Vector3(-0.005f, 0f, 0.005f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                BuildFaceComplex(new Vector3(x, y, z) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), new Vector3(1f, 0f, 1f) + randomCrossModelOffset - new Vector3(-0.005f, 0f, 0.005f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);






                BuildFaceComplex(new Vector3(x, y, z + 1f) + randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), new Vector3(1f, 0f, -1f) + randomCrossModelOffset + new Vector3(0.005f, 0f, 0.005f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                BuildFaceComplex(new Vector3(x, y, z + 1f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), new Vector3(0f, 1f, 0f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), new Vector3(1f, 0f, -1f) + randomCrossModelOffset - new Vector3(0.005f, 0f, 0.005f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);

                break;

            case BlockShape.Slabs:
                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 6 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 6)
                {
                    return;
                }
                switch (blockData.optionalDataValue)
                {
                    case 0:

                        BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        //Right

                        BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                        //Bottom
                        if (curChunk.CheckNeedBuildFace(x, y - 1, z, blockData))
                            BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        //Top

                        BuildFaceComplex(new Vector3(x, y + 0.5f, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                        //Back

                        BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0.5f, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        //Front

                        BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0, 0.5f, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        break;

                    case 1:

                        BuildFaceComplex(new Vector3(x, y + 0.5f, z), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        //Right

                        BuildFaceComplex(new Vector3(x + 1, y + 0.5f, z), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                        //Bottom

                        BuildFaceComplex(new Vector3(x, y + 0.5f, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        //Top
                        if (curChunk.CheckNeedBuildFace(x, y + 1, z, blockData))
                            BuildFaceComplex(new Vector3(x, y + 1f, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                        //Back

                        BuildFaceComplex(new Vector3(x, y + 0.5f, z), new Vector3(0, 0.5f, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        //Front

                        BuildFaceComplex(new Vector3(x, y + 0.5f, z + 1), new Vector3(0, 0.5f, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        break;
                    case 2:
                        if (curChunk.CheckNeedBuildFace(x - 1, y, z, blockData))
                            BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        //Right
                        if (curChunk.CheckNeedBuildFace(x + 1, y, z, blockData))
                            BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                        //Bottom
                        if (curChunk.CheckNeedBuildFace(x, y - 1, z, blockData))
                            BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        //Top
                        if (curChunk.CheckNeedBuildFace(x, y + 1, z, blockData))
                            BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                        //Back
                        if (curChunk.CheckNeedBuildFace(x, y, z - 1, blockData))
                            BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        //Front
                        if (curChunk.CheckNeedBuildFace(x, y, z + 1, blockData))
                            BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                        break;
                }

                break;
            case BlockShape.Fence:


                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 6 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 6)
                {
                    return;
                }
                bool[] dataArray = MathUtility.GetBooleanArray(blockData.optionalDataValue);
                bool isLeftBuilt = dataArray[7];
                bool isRightBuilt = dataArray[6];
                bool isBackBuilt = dataArray[5];
                bool isFrontBuilt = dataArray[4];

                BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f), new Vector3(0f, 1, 0f), new Vector3(0f, 0f, 0.25f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                //Right

                BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f) + new Vector3(0.25f, 0f, 0f), new Vector3(0f, 1, 0f), new Vector3(0f, 0f, 0.25f), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                //Bottom

                BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f), new Vector3(0f, 0f, 0.25f), new Vector3(0.25f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                //Top

                BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 1f, 0.375f), new Vector3(0f, 0f, 0.25f), new Vector3(0.25f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                //Back

                BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f), new Vector3(0f, 1, 0f), new Vector3(0.25f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                //Front

                BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.375f, 0f, 0.375f) + new Vector3(0f, 0f, 0.25f), new Vector3(0f, 1, 0f), new Vector3(0.25f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);


                if (isLeftBuilt)
                {


                    //Bottom

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0f, 0.375f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0f, 0.375f + 0.1875f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0f, 0.375f, 0.4375f), new Vector3(0f, 0.1875f, 0f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0f, 0.375f, 0.4375f + 0.125f), new Vector3(0f, 0.1875f, 0f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);


                    //Bottom
                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0f, 0.375f + 0.375f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0f, 0.375f + 0.1875f + 0.375f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0f, 0.375f + 0.375f, 0.4375f), new Vector3(0f, 0.1875f, 0f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0f, 0.375f + 0.375f, 0.4375f + 0.125f), new Vector3(0f, 0.1875f, 0f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                }

                if (isRightBuilt)
                {


                    //Bottom

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.625f, 0.375f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.625f, 0.375f + 0.1875f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.625f, 0.375f, 0.4375f), new Vector3(0f, 0.1875f, 0f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.625f, 0.375f, 0.4375f + 0.125f), new Vector3(0f, 0.1875f, 0f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);


                    //Bottom
                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.625f, 0.375f + 0.375f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.625f, 0.375f + 0.1875f + 0.375f, 0.4375f), new Vector3(0f, 0f, 0.125f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.625f, 0.375f + 0.375f, 0.4375f), new Vector3(0f, 0.1875f, 0f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.625f, 0.375f + 0.375f, 0.4375f + 0.125f), new Vector3(0f, 0.1875f, 0f), new Vector3(0.375f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                }



                if (isBackBuilt)
                {


                    //Bottom

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f, 0f), new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f + 0.1875f, 0f), new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f, 0f), new Vector3(0f, 0.1875f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f + 0.125f, 0.375f, 0f), new Vector3(0f, 0.1875f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);


                    //Bottom

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f + 0.375f, 0f), new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f + 0.1875f + 0.375f, 0f), new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f + 0.375f, 0f), new Vector3(0f, 0.1875f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f + 0.125f, 0.375f + 0.375f, 0f), new Vector3(0f, 0.1875f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);


                }

                if (isFrontBuilt)
                {

                    //Bottom

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f, 0.625f), new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f + 0.1875f, 0.625f), new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f, 0.625f), new Vector3(0f, 0.1875f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f + 0.125f, 0.375f, 0.625f), new Vector3(0f, 0.1875f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);


                    //Bottom

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f + 0.375f, 0.625f), new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f + 0.1875f + 0.375f, 0.625f), new Vector3(0.125f, 0f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f, 0.375f + 0.375f, 0.625f), new Vector3(0f, 0.1875f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z) + new Vector3(0.4375f + 0.125f, 0.375f + 0.375f, 0.625f), new Vector3(0f, 0.1875f, 0f), new Vector3(0f, 0f, 0.375f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                }
                break;
            case BlockShape.Door:
                //additionaldata[6][7]
                //false false->left
                //false true->right
                //true false->back
                //true true->front
                //additionaldata[5]
                //false->bottom
                //true->up
                //[4]
                //false->close true->open

                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 12 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 12)
                {
                    return;
                }
                bool[] dataArray1 = MathUtility.GetBooleanArray(blockData.optionalDataValue);
                byte doorFaceID = 0;
                if (dataArray1[6] == false)
                {
                    if (dataArray1[7] == false)
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
                    if (dataArray1[7] == false)
                    {
                        doorFaceID = 2;
                    }
                    else
                    {
                        doorFaceID = 3;
                    }
                }

                Matrix4x4 rotationMat = Matrix4x4.identity;
                switch (doorFaceID)
                {
                    case 0:
                        rotationMat = Matrix4x4.Rotate(Quaternion.Euler(0,0f,0));
                        break;
                    case 1:
                        rotationMat = Matrix4x4.Rotate(Quaternion.Euler(0, -180f, 0));
                        break;
                    case 2:
                        rotationMat = Matrix4x4.Rotate(Quaternion.Euler(0, 270f, 0));
                        break;
                    case 3:
                        rotationMat = Matrix4x4.Rotate(Quaternion.Euler(0, 90f, 0));
                        break;
                }

                bool isOpen = dataArray1[4];
                bool isDown = dataArray1[5];
                if (isOpen)
                {
                    rotationMat *= Matrix4x4.Rotate(Quaternion.Euler(0, 90f, 0));
                }
                if (isDown == true)
                {

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z), new Vector3(0f, 1, 0f), new Vector3(0f, 0f, 1f), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Right

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z) + new Vector3(0.1875f, 0f, 0f), new Vector3(0f, 1, 0f), new Vector3(0f, 0f, 1f), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Bottom

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z), new Vector3(0f, 0f, 1f), new Vector3(0.1875f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z) + new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0.1875f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z), new Vector3(0f, 1, 0f), new Vector3(0.1875f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z) + new Vector3(0f, 0f, 1f), new Vector3(0f, 1, 0f), new Vector3(0.1875f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);



                }
                else
                {

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z), new Vector3(0f, 1, 0f), new Vector3(0f, 0f, 1f), Chunk.blockInfosNew[blockData.blockID].uvCorners[6], Chunk.blockInfosNew[blockData.blockID].uvSizes[6], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Right

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z) + new Vector3(0.1875f, 0f, 0f), new Vector3(0f, 1, 0f), new Vector3(0f, 0f, 1f), Chunk.blockInfosNew[blockData.blockID].uvCorners[7], Chunk.blockInfosNew[blockData.blockID].uvSizes[7], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Bottom

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z), new Vector3(0f, 0f, 1f), new Vector3(0.1875f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[8], Chunk.blockInfosNew[blockData.blockID].uvSizes[8], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Top

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z) + new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0.1875f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[9], Chunk.blockInfosNew[blockData.blockID].uvSizes[9], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);

                    //Back

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z), new Vector3(0f, 1, 0f), new Vector3(0.1875f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[10], Chunk.blockInfosNew[blockData.blockID].uvSizes[10], true, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);
                    //Front

                    BuildFaceComplex(new Vector3(x, y, z), rotationMat, new Vector3(x, y, z) + new Vector3(0f, 0f, 1f), new Vector3(0f, 1, 0f), new Vector3(0.1875f, 0f, 0f), Chunk.blockInfosNew[blockData.blockID].uvCorners[11], Chunk.blockInfosNew[blockData.blockID].uvSizes[11], false, OpqVerts, OpqUVs, OpqTris, OpqNorms,OpqTangents);


                }



                break;


            case BlockShape.WallAttachment:
                //0left 1right 2back 3front

                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 1 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 1)
                {
                    return;
                }

                switch (blockData.optionalDataValue)
                {
                    case 0:
                        BuildFaceComplex(new Vector3(x + 0.0625f, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        BuildFaceComplex(new Vector3(x + 0.0625f, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        break;
                    case 1:
                        BuildFaceComplex(new Vector3(x + 1 - 0.0625f, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        BuildFaceComplex(new Vector3(x + 1 - 0.0625f, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        break;
                    case 2:
                        BuildFaceComplex(new Vector3(x, y, z + 0.0625f), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        BuildFaceComplex(new Vector3(x, y, z + 0.0625f), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        break;
                    case 3:
                        BuildFaceComplex(new Vector3(x, y, z + 1f - 0.0625f), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], true,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        BuildFaceComplex(new Vector3(x, y, z + 1f - 0.0625f), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,NSVerts,NSUVs,NSTris,NSNorms,NSTangents);
                        break;
                    default:
                        break;
                }
                break;

            case BlockShape.SolidTransparent:
                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 6 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 6)
                {
                    return;
                }
                if (curChunk.CheckNeedBuildFace(x - 1, y, z, blockData))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false,TSVerts,TSUVs,TSTris,TSNorms,TSTangents);
                //Right
                if (curChunk.CheckNeedBuildFace(x + 1, y, z, blockData))
                    BuildFaceComplex(new Vector3(x + 1, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true,TSVerts,TSUVs,TSTris,TSNorms,TSTangents);

                //Bottom
                if (curChunk.CheckNeedBuildFace(x, y - 1, z, blockData))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false,TSVerts,TSUVs,TSTris,TSNorms,TSTangents);
                //Top
                if (curChunk.CheckNeedBuildFace(x, y + 1, z, blockData))
                    BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true,TSVerts,TSUVs,TSTris,TSNorms,TSTangents);

                //Back
                if (curChunk.CheckNeedBuildFace(x, y, z - 1, blockData))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true,TSVerts,TSUVs,TSTris,TSNorms,TSTangents);
                //Front
                if (curChunk.CheckNeedBuildFace(x, y, z + 1, blockData))
                    BuildFaceComplex(new Vector3(x, y, z + 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false,TSVerts,TSUVs,TSTris,TSNorms,TSTangents);
                break;

        }

    }




    public static void BuildSingleBlockLOD(int LODSkipBlockCount, IChunkFaceBuildingChecks curChunk, int x, int y, int z, BlockData blockData, ref List<Vector3> verts, ref List<Vector2> uvs, ref List<int> tris, ref List<Vector3> norms, ref List<Vector3> tangents)
    {
        if (blockData.blockID == 0)
        {
            return;
        }

        if (!Chunk.IsBlockIDValid(blockData))
        {
            if (curChunk.CheckNeedBuildFace(x - LODSkipBlockCount, y, z, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[0], Chunk.unknownBlockInfo.uvSizes[0], false, verts, uvs, tris, norms,tangents);
            //Right
            if (curChunk.CheckNeedBuildFace(x + LODSkipBlockCount, y, z, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x + 1 * LODSkipBlockCount, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[1], Chunk.unknownBlockInfo.uvSizes[1], true, verts, uvs, tris, norms,tangents);

            //Bottom
            if (curChunk.CheckNeedBuildFace(x, y - 1, z, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1) * LODSkipBlockCount, new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[2], Chunk.unknownBlockInfo.uvSizes[2], false, verts, uvs, tris, norms,tangents);
            //Top
            if (curChunk.CheckNeedBuildFace(x, y + 1, z, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1) * LODSkipBlockCount, new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[3], Chunk.unknownBlockInfo.uvSizes[3], true, verts, uvs, tris, norms,tangents);

            //Back
            if (curChunk.CheckNeedBuildFace(x, y, z - LODSkipBlockCount, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[4], Chunk.unknownBlockInfo.uvSizes[4], true, verts, uvs, tris, norms,tangents);
            //Front
            if (curChunk.CheckNeedBuildFace(x, y, z + LODSkipBlockCount, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x, y, z + 1 * LODSkipBlockCount), new Vector3(0, 1, 0), new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[5], Chunk.unknownBlockInfo.uvSizes[5], false, verts, uvs, tris, norms,tangents);
            return;
        }
        switch (Chunk.blockInfosNew[blockData.blockID].shape)
        {
            case BlockShape.Solid:
                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 6 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 6)
                {
                    return;
                }
                if (curChunk.CheckNeedBuildFace(x - LODSkipBlockCount, y, z, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false, verts, uvs,tris,norms, tangents);
                //Right
                if (curChunk.CheckNeedBuildFace(x + LODSkipBlockCount, y, z, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x + 1 * LODSkipBlockCount, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true, verts, uvs, tris, norms,tangents);

                //Bottom
                if (curChunk.CheckNeedBuildFace(x, y - 1, z, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1) * LODSkipBlockCount, new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, verts, uvs, tris, norms,tangents);
                //Top
                if (curChunk.CheckNeedBuildFace(x, y + 1, z, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1) * LODSkipBlockCount, new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, verts, uvs, tris, norms,tangents);

                //Back
                if (curChunk.CheckNeedBuildFace(x, y, z - LODSkipBlockCount, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, verts, uvs, tris, norms,tangents);
                //Front
                if (curChunk.CheckNeedBuildFace(x, y, z + LODSkipBlockCount, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x, y, z + 1 * LODSkipBlockCount), new Vector3(0, 1, 0), new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, verts, uvs, tris, norms,tangents);
                break;

        }
    }



    public static void BuildSingleBlockLOD(int LODSkipBlockCount, IChunkFaceBuildingChecks curChunk, int x, int y, int z, BlockData blockData, ref NativeList<Vector3> verts, ref NativeList<Vector2> uvs, ref NativeList<int> tris, ref NativeList<Vector3> norms, ref NativeList<Vector3> tangents)
    {
        if (blockData.blockID == 0)
        {
            return;
        }

        if (!Chunk.IsBlockIDValid(blockData))
        {
            if (curChunk.CheckNeedBuildFace(x - LODSkipBlockCount, y, z, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[0], Chunk.unknownBlockInfo.uvSizes[0], false, verts, uvs, tris, norms, tangents);
            //Right
            if (curChunk.CheckNeedBuildFace(x + LODSkipBlockCount, y, z, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x + 1 * LODSkipBlockCount, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[1], Chunk.unknownBlockInfo.uvSizes[1], true, verts, uvs, tris, norms, tangents);

            //Bottom
            if (curChunk.CheckNeedBuildFace(x, y - 1, z, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1) * LODSkipBlockCount, new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[2], Chunk.unknownBlockInfo.uvSizes[2], false, verts, uvs, tris, norms, tangents);
            //Top
            if (curChunk.CheckNeedBuildFace(x, y + 1, z, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1) * LODSkipBlockCount, new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[3], Chunk.unknownBlockInfo.uvSizes[3], true, verts, uvs, tris, norms, tangents);

            //Back
            if (curChunk.CheckNeedBuildFace(x, y, z - LODSkipBlockCount, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[4], Chunk.unknownBlockInfo.uvSizes[4], true, verts, uvs, tris, norms, tangents);
            //Front
            if (curChunk.CheckNeedBuildFace(x, y, z + LODSkipBlockCount, blockData, LODSkipBlockCount))
                BuildFaceComplex(new Vector3(x, y, z + 1 * LODSkipBlockCount), new Vector3(0, 1, 0), new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.unknownBlockInfo.uvCorners[5], Chunk.unknownBlockInfo.uvSizes[5], false, verts, uvs, tris, norms, tangents);
            return;
        }
        switch (Chunk.blockInfosNew[blockData.blockID].shape)
        {
            case BlockShape.Solid:
                if (Chunk.blockInfosNew[blockData.blockID].uvCorners.Count < 6 || Chunk.blockInfosNew[blockData.blockID].uvSizes.Count < 6)
                {
                    return;
                }
                if (curChunk.CheckNeedBuildFace(x - LODSkipBlockCount, y, z, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[0], Chunk.blockInfosNew[blockData.blockID].uvSizes[0], false, verts, uvs, tris, norms, tangents);
                //Right
                if (curChunk.CheckNeedBuildFace(x + LODSkipBlockCount, y, z, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x + 1 * LODSkipBlockCount, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[1], Chunk.blockInfosNew[blockData.blockID].uvSizes[1], true, verts, uvs, tris, norms, tangents);

                //Bottom
                if (curChunk.CheckNeedBuildFace(x, y - 1, z, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 0, 1) * LODSkipBlockCount, new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[2], Chunk.blockInfosNew[blockData.blockID].uvSizes[2], false, verts, uvs, tris, norms, tangents);
                //Top
                if (curChunk.CheckNeedBuildFace(x, y + 1, z, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x, y + 1, z), new Vector3(0, 0, 1) * LODSkipBlockCount, new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[3], Chunk.blockInfosNew[blockData.blockID].uvSizes[3], true, verts, uvs, tris, norms, tangents);

                //Back
                if (curChunk.CheckNeedBuildFace(x, y, z - LODSkipBlockCount, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[4], Chunk.blockInfosNew[blockData.blockID].uvSizes[4], true, verts, uvs, tris, norms, tangents);
                //Front
                if (curChunk.CheckNeedBuildFace(x, y, z + LODSkipBlockCount, blockData, LODSkipBlockCount))
                    BuildFaceComplex(new Vector3(x, y, z + 1 * LODSkipBlockCount), new Vector3(0, 1, 0), new Vector3(1, 0, 0) * LODSkipBlockCount, Chunk.blockInfosNew[blockData.blockID].uvCorners[5], Chunk.blockInfosNew[blockData.blockID].uvSizes[5], false, verts, uvs, tris, norms, tangents);
                break;

        }
    }

    /*static void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<VertexPositionNormalTangentTexture> verts, int side, List<ushort> indices)
    {
        VertexPositionNormalTangentTexture vert00 = new VertexPositionNormalTangentTexture();
        VertexPositionNormalTangentTexture vert01 = new VertexPositionNormalTangentTexture();
        VertexPositionNormalTangentTexture vert11 = new VertexPositionNormalTangentTexture();
        VertexPositionNormalTangentTexture vert10 = new VertexPositionNormalTangentTexture();
        short index = (short)verts.Count;
        vert00.Position = corner;
        vert01.Position = corner + up;
        vert11.Position = corner + up + right;
        vert10.Position = corner + right;
        //    verts.Add(vert0);
        //    verts.Add(vert1);
        //    verts.Add(vert2);
        //     verts.Add(vert3);

        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        if (Chunk.blockInfo.ContainsKey(typeid))
        {
            uvCorner = Chunk.blockInfo[typeid][side];
        }
        vert00.TextureCoordinate = uvCorner;
        vert01.TextureCoordinate = new Vector2(uvCorner.X, uvCorner.Y + uvWidth.Y);
        vert11.TextureCoordinate = new Vector2(uvCorner.X + uvWidth.X, uvCorner.Y + uvWidth.Y);
        vert10.TextureCoordinate = new Vector2(uvCorner.X + uvWidth.X, uvCorner.Y);
        //    uvs.Add(uvCorner);
        //    uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));


        if (!reversed)
        {


            Vector3 normal = -Vector3.Cross(up, right);
            Vector3 tangent = -right;
            vert00.Normal = normal;
            vert01.Normal = normal;
            vert11.Normal = normal;
            vert10.Normal = normal;

            vert00.Tangent = tangent;
            vert01.Tangent = tangent;
            vert11.Tangent = tangent;
            vert10.Tangent = tangent;
            /*  verts.Add(vert00);
               verts.Add(vert01);
               verts.Add(vert11);
               verts.Add(vert11);
               verts.Add(vert10);
               verts.Add(vert00);#1#
            indices.Add((ushort)(index + 0));
            indices.Add((ushort)(index + 1));
            indices.Add((ushort)(index + 2));
            indices.Add((ushort)(index + 2));
            indices.Add((ushort)(index + 3));
            indices.Add((ushort)(index + 0));
            //    tris.Add(index + 2);

            //   tris.Add(index + 0);

        }
        else
        {

            Vector3 normal = Vector3.Cross(up, right);
            Vector3 tangent = right;

            vert00.Normal = normal;
            vert01.Normal = normal;
            vert11.Normal = normal;
            vert10.Normal = normal;

            vert00.Tangent = tangent;
            vert01.Tangent = tangent;
            vert11.Tangent = tangent;
            vert10.Tangent = tangent;
            /*    verts.Add(vert01);
                verts.Add(vert00);
                verts.Add(vert11);
                verts.Add(vert10);
                verts.Add(vert11);
                verts.Add(vert00);#1#
            //     indices.Add()
            indices.Add((ushort)(index + 1));
            indices.Add((ushort)(index + 0));
            indices.Add((ushort)(index + 2));
            indices.Add((ushort)(index + 3));
            indices.Add((ushort)(index + 2));
            indices.Add((ushort)(index + 0));

        }

        verts.Add(vert00);
        verts.Add(vert01);
        verts.Add(vert11);
        verts.Add(vert10);



    }*/
    public static void BuildFaceComplex(Vector3 origin, Matrix4x4 transformMat, Vector3 corner, Vector3 up, Vector3 right, Vector2 uvCorner, Vector2 uvWidth, bool reversed, List<Vector3> verts,List<Vector2> uvs, List<int> tris, List<Vector3> norms, List<Vector3> tangents)
    { 

        int index = verts.Count;
        Vector3 corner1 = corner - (origin + new Vector3(0.5f, 0.5f, 0.5f));


        Vector3 vert00 = transformMat.MultiplyPoint(corner1) + origin + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 vert01 = transformMat.MultiplyPoint(corner1 + up) + origin + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 vert11 = transformMat.MultiplyPoint(corner1 + up + right) + origin + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 vert10 = transformMat.MultiplyPoint(corner1 + right) + origin + new Vector3(0.5f, 0.5f, 0.5f);
       
        //        Debug.WriteLine(vert00.Position + " " + vert01.Position+" "+vert11.Position+" "+vert10.Position);
        //    verts.Add(vert0);
        //    verts.Add(vert1);
        //    verts.Add(vert2);
        //     verts.Add(vert3);



        //uvCorner.x = (float)(typeid - 1) / 16;

      


        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        //    uvs.Add(uvCorner);
        //    uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));


        if (reversed)
        {


           /* Vector3 normal = -Vector3.Cross(Vector3.TransformNormal(up, transformMat), Vector3.TransformNormal(right, transformMat));
            Vector3 tangent = Vector3.TransformNormal(-right, transformMat);
            vert00.Normal = normal;
            vert01.Normal = normal;
            vert11.Normal = normal;
            vert10.Normal = normal;

            vert00.Tangent = tangent;
            vert01.Tangent = tangent;
            vert11.Tangent = tangent;
            vert10.Tangent = tangent;*/
            /*  verts.Add(vert00);
               verts.Add(vert01);
               verts.Add(vert11);
               verts.Add(vert11);
               verts.Add(vert10);
               verts.Add(vert00);*/
            tris.Add((index + 0));
            tris.Add((index + 1));
            tris.Add((index + 2));
            tris.Add((index + 2));
            tris.Add((index + 3));
            tris.Add((index + 0));
            //    tris.Add(index + 2);

            //   tris.Add(index + 0);
            Vector3 v1 = Vector3.Cross(transformMat.MultiplyVector(up), transformMat.MultiplyVector(right)).normalized;
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);

            Vector3 v2 = transformMat.MultiplyVector(right).normalized;
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
        }
        else
        {

       /*     Vector3 normal = Vector3.Cross(Vector3.TransformNormal(up, transformMat), Vector3.TransformNormal(right, transformMat));
            Vector3 tangent = Vector3.TransformNormal(right, transformMat);

            vert00.Normal = normal;
            vert01.Normal = normal;
            vert11.Normal = normal;
            vert10.Normal = normal;

            vert00.Tangent = tangent;
            vert01.Tangent = tangent;
            vert11.Tangent = tangent;
            vert10.Tangent = tangent;*/
            /*    verts.Add(vert01);
                verts.Add(vert00);
                verts.Add(vert11);
                verts.Add(vert10);
                verts.Add(vert11);
                verts.Add(vert00);*/
            //     indices.Add()
            tris.Add((index + 1));
            tris.Add((index + 0));
            tris.Add((index + 2));
            tris.Add((index + 3));
            tris.Add((index + 2));
            tris.Add((index + 0));
            Vector3 v1 = -Vector3.Cross(transformMat.MultiplyVector(up), transformMat.MultiplyVector(right)).normalized;
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);


            Vector3 v2 = -transformMat.MultiplyVector(right).normalized;
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
        }

        verts.Add(vert00);
        verts.Add(vert01);
        verts.Add(vert11);
        verts.Add(vert10);



    }


    public static void BuildFaceComplex(Vector3 origin, Matrix4x4 transformMat, Vector3 corner, Vector3 up, Vector3 right, Vector2 uvCorner, Vector2 uvWidth, bool reversed, NativeList<Vector3> verts, NativeList<Vector2> uvs, NativeList<int> tris, NativeList<Vector3> norms, NativeList<Vector3> tangents)
    {

        int index = verts.Length;
        Vector3 corner1 = corner - (origin + new Vector3(0.5f, 0.5f, 0.5f));


        Vector3 vert00 = transformMat.MultiplyPoint(corner1) + origin + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 vert01 = transformMat.MultiplyPoint(corner1 + up) + origin + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 vert11 = transformMat.MultiplyPoint(corner1 + up + right) + origin + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 vert10 = transformMat.MultiplyPoint(corner1 + right) + origin + new Vector3(0.5f, 0.5f, 0.5f);

        //        Debug.WriteLine(vert00.Position + " " + vert01.Position+" "+vert11.Position+" "+vert10.Position);
        //    verts.Add(vert0);
        //    verts.Add(vert1);
        //    verts.Add(vert2);
        //     verts.Add(vert3);



        //uvCorner.x = (float)(typeid - 1) / 16;




        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        //    uvs.Add(uvCorner);
        //    uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));


        if (reversed)
        {


            /* Vector3 normal = -Vector3.Cross(Vector3.TransformNormal(up, transformMat), Vector3.TransformNormal(right, transformMat));
             Vector3 tangent = Vector3.TransformNormal(-right, transformMat);
             vert00.Normal = normal;
             vert01.Normal = normal;
             vert11.Normal = normal;
             vert10.Normal = normal;

             vert00.Tangent = tangent;
             vert01.Tangent = tangent;
             vert11.Tangent = tangent;
             vert10.Tangent = tangent;*/
            /*  verts.Add(vert00);
               verts.Add(vert01);
               verts.Add(vert11);
               verts.Add(vert11);
               verts.Add(vert10);
               verts.Add(vert00);*/
            tris.Add((index + 0));
            tris.Add((index + 1));
            tris.Add((index + 2));
            tris.Add((index + 2));
            tris.Add((index + 3));
            tris.Add((index + 0));
            //    tris.Add(index + 2);

            //   tris.Add(index + 0);
            Vector3 v1 = Vector3.Cross(transformMat.MultiplyVector(up), transformMat.MultiplyVector(right)).normalized;
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);

            Vector3 v2 = transformMat.MultiplyVector(right).normalized;
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
        }
        else
        {

            /*     Vector3 normal = Vector3.Cross(Vector3.TransformNormal(up, transformMat), Vector3.TransformNormal(right, transformMat));
                 Vector3 tangent = Vector3.TransformNormal(right, transformMat);

                 vert00.Normal = normal;
                 vert01.Normal = normal;
                 vert11.Normal = normal;
                 vert10.Normal = normal;

                 vert00.Tangent = tangent;
                 vert01.Tangent = tangent;
                 vert11.Tangent = tangent;
                 vert10.Tangent = tangent;*/
            /*    verts.Add(vert01);
                verts.Add(vert00);
                verts.Add(vert11);
                verts.Add(vert10);
                verts.Add(vert11);
                verts.Add(vert00);*/
            //     indices.Add()
            tris.Add((index + 1));
            tris.Add((index + 0));
            tris.Add((index + 2));
            tris.Add((index + 3));
            tris.Add((index + 2));
            tris.Add((index + 0));
            Vector3 v1 = -Vector3.Cross(transformMat.MultiplyVector(up), transformMat.MultiplyVector(right)).normalized;
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);


            Vector3 v2 = -transformMat.MultiplyVector(right).normalized;
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
        }

        verts.Add(vert00);
        verts.Add(vert01);
        verts.Add(vert11);
        verts.Add(vert10);



    }

    public static void BuildFaceComplex(Vector3 origin, Matrix4x4 transformMat, Vector3 corner, Vector3 up, Vector3 right, Vector2 uvCorner, Vector2 uvWidth, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, List<Vector3> norms)
    {

        int index = verts.Count;
        Vector3 corner1 = corner - (origin + new Vector3(0.5f, 0.5f, 0.5f));


        Vector3 vert00 = transformMat.MultiplyPoint(corner1) + origin + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 vert01 = transformMat.MultiplyPoint(corner1 + up) + origin + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 vert11 = transformMat.MultiplyPoint(corner1 + up + right) + origin + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 vert10 = transformMat.MultiplyPoint(corner1 + right) + origin + new Vector3(0.5f, 0.5f, 0.5f);

        //        Debug.WriteLine(vert00.Position + " " + vert01.Position+" "+vert11.Position+" "+vert10.Position);
        //    verts.Add(vert0);
        //    verts.Add(vert1);
        //    verts.Add(vert2);
        //     verts.Add(vert3);



        //uvCorner.x = (float)(typeid - 1) / 16;




        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        //    uvs.Add(uvCorner);
        //    uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));


        if (reversed)
        {


            /* Vector3 normal = -Vector3.Cross(Vector3.TransformNormal(up, transformMat), Vector3.TransformNormal(right, transformMat));
             Vector3 tangent = Vector3.TransformNormal(-right, transformMat);
             vert00.Normal = normal;
             vert01.Normal = normal;
             vert11.Normal = normal;
             vert10.Normal = normal;

             vert00.Tangent = tangent;
             vert01.Tangent = tangent;
             vert11.Tangent = tangent;
             vert10.Tangent = tangent;*/
            /*  verts.Add(vert00);
               verts.Add(vert01);
               verts.Add(vert11);
               verts.Add(vert11);
               verts.Add(vert10);
               verts.Add(vert00);*/
            tris.Add((index + 0));
            tris.Add((index + 1));
            tris.Add((index + 2));
            tris.Add((index + 2));
            tris.Add((index + 3));
            tris.Add((index + 0));
            //    tris.Add(index + 2);

            //   tris.Add(index + 0);
            Vector3 v1 = Vector3.Cross(transformMat.MultiplyVector(up), transformMat.MultiplyVector(right)).normalized;
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);

        }
        else
        {

            /*     Vector3 normal = Vector3.Cross(Vector3.TransformNormal(up, transformMat), Vector3.TransformNormal(right, transformMat));
                 Vector3 tangent = Vector3.TransformNormal(right, transformMat);

                 vert00.Normal = normal;
                 vert01.Normal = normal;
                 vert11.Normal = normal;
                 vert10.Normal = normal;

                 vert00.Tangent = tangent;
                 vert01.Tangent = tangent;
                 vert11.Tangent = tangent;
                 vert10.Tangent = tangent;*/
            /*    verts.Add(vert01);
                verts.Add(vert00);
                verts.Add(vert11);
                verts.Add(vert10);
                verts.Add(vert11);
                verts.Add(vert00);*/
            //     indices.Add()
            tris.Add((index + 1));
            tris.Add((index + 0));
            tris.Add((index + 2));
            tris.Add((index + 3));
            tris.Add((index + 2));
            tris.Add((index + 0));
            Vector3 v1 = -Vector3.Cross(transformMat.MultiplyVector(up), transformMat.MultiplyVector(right)).normalized;
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);


          
        }

        verts.Add(vert00);
        verts.Add(vert01);
        verts.Add(vert11);
        verts.Add(vert10);



    }
    public static void BuildFaceComplex(Vector3 corner, Vector3 up, Vector3 right, Vector2 uvCorner, Vector2 uvWidth,
     bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, List<Vector3> norms, List<Vector3> tangents)
    {
        int index = verts.Count;
        Vector3 vert0 = corner;
        Vector3 vert1 = corner + up;
        Vector3 vert2 = corner + up + right;
        Vector3 vert3 = corner + right;
        verts.Add(vert0);
        verts.Add(vert1);
        verts.Add(vert2);
        verts.Add(vert3);

        //  Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        //  Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        //    uvCorner=blockInfo[typeid][side];
        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));

        if (reversed)
        {
            //     tris.Add(index + 0);
            //   tris.Add(index + 1);
            //   tris.Add(index + 2);
            //    tris.Add(index + 3);
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);

            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 0);
            Vector3 v1 = Vector3.Cross(up, right).normalized;
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);

            Vector3 v2 = right.normalized;
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
        }
        else
        {
            tris.Add(index + 1);
            tris.Add(index + 0);
            tris.Add(index + 2);

            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 0);
            //         tris.Add(index + 3);
            //        tris.Add(index + 2);
            //       tris.Add(index + 1);
            //       tris.Add(index + 0);
            Vector3 v1 = -Vector3.Cross(up, right).normalized;
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);


            Vector3 v2 = -right.normalized;
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
        }
    }

    public static void BuildFaceComplex(Vector3 corner, Vector3 up, Vector3 right, Vector2 uvCorner, Vector2 uvWidth,
    bool reversed, NativeList<Vector3> verts, NativeList<Vector2> uvs, NativeList<int> tris, NativeList<Vector3> norms, NativeList<Vector3> tangents)
    {
        int index = verts.Length;
        Vector3 vert0 = corner;
        Vector3 vert1 = corner + up;
        Vector3 vert2 = corner + up + right;
        Vector3 vert3 = corner + right;
        verts.Add(vert0);
        verts.Add(vert1);
        verts.Add(vert2);
        verts.Add(vert3);

        //  Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        //  Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        //    uvCorner=blockInfo[typeid][side];
        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));

        if (reversed)
        {
            //     tris.Add(index + 0);
            //   tris.Add(index + 1);
            //   tris.Add(index + 2);
            //    tris.Add(index + 3);
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);

            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 0);
            Vector3 v1 = Vector3.Cross(up, right).normalized;
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);

            Vector3 v2 = right.normalized;
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
        }
        else
        {
            tris.Add(index + 1);
            tris.Add(index + 0);
            tris.Add(index + 2);

            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 0);
            //         tris.Add(index + 3);
            //        tris.Add(index + 2);
            //       tris.Add(index + 1);
            //       tris.Add(index + 0);
            Vector3 v1 = -Vector3.Cross(up, right).normalized;
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);


            Vector3 v2 = -right.normalized;
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
            tangents.Add(v2);
        }
    }


    public static void BuildFaceComplex(Vector3 corner, Vector3 up, Vector3 right, Vector2 uvCorner, Vector2 uvWidth,
     bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, List<Vector3> norms)
    {
        int index = verts.Count;
        Vector3 vert0 = corner;
        Vector3 vert1 = corner + up;
        Vector3 vert2 = corner + up + right;
        Vector3 vert3 = corner + right;
        verts.Add(vert0);
        verts.Add(vert1);
        verts.Add(vert2);
        verts.Add(vert3);

        //  Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        //  Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        //    uvCorner=blockInfo[typeid][side];
        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));

        if (reversed)
        {
            //     tris.Add(index + 0);
            //   tris.Add(index + 1);
            //   tris.Add(index + 2);
            //    tris.Add(index + 3);
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
            //         tris.Add(index + 3);
            //        tris.Add(index + 2);
            //       tris.Add(index + 1);
            //       tris.Add(index + 0);
            Vector3 v1 = -Vector3.Cross(up, right);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);
            norms.Add(v1);


         
        }
    }
    /* static void BuildFaceComplex(Vector3 corner, Vector3 up, Vector3 right, Vector2 uvCorner, Vector2 uvWidth, bool reversed, List<VertexPositionNormalTangentTexture> verts, List<ushort> indices)
     {
         VertexPositionNormalTangentTexture vert00 = new VertexPositionNormalTangentTexture();
         VertexPositionNormalTangentTexture vert01 = new VertexPositionNormalTangentTexture();
         VertexPositionNormalTangentTexture vert11 = new VertexPositionNormalTangentTexture();
         VertexPositionNormalTangentTexture vert10 = new VertexPositionNormalTangentTexture();
         short index = (short)verts.Count;
         vert00.Position = corner;
         vert01.Position = corner + up;
         vert11.Position = corner + up + right;
         vert10.Position = corner + right;
         //    verts.Add(vert0);
         //    verts.Add(vert1);
         //    verts.Add(vert2);
         //     verts.Add(vert3);



         //uvCorner.x = (float)(typeid - 1) / 16;

         vert00.TextureCoordinate = uvCorner;
         vert01.TextureCoordinate = new Vector2(uvCorner.X, uvCorner.Y + uvWidth.Y);
         vert11.TextureCoordinate = new Vector2(uvCorner.X + uvWidth.X, uvCorner.Y + uvWidth.Y);
         vert10.TextureCoordinate = new Vector2(uvCorner.X + uvWidth.X, uvCorner.Y);
         //    uvs.Add(uvCorner);
         //    uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
         //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
         //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));


         if (!reversed)
         {


             Vector3 normal = -Vector3.Cross(up, right);
             Vector3 tangent = -right;
             vert00.Normal = normal;
             vert01.Normal = normal;
             vert11.Normal = normal;
             vert10.Normal = normal;

             vert00.Tangent = tangent;
             vert01.Tangent = tangent;
             vert11.Tangent = tangent;
             vert10.Tangent = tangent;
             /*  verts.Add(vert00);
                verts.Add(vert01);
                verts.Add(vert11);
                verts.Add(vert11);
                verts.Add(vert10);
                verts.Add(vert00);
             indices.Add((ushort)(index + 0));
             indices.Add((ushort)(index + 1));
             indices.Add((ushort)(index + 2));
             indices.Add((ushort)(index + 2));
             indices.Add((ushort)(index + 3));
             indices.Add((ushort)(index + 0));
             //    tris.Add(index + 2);

             //   tris.Add(index + 0);

         }
         else
         {

             Vector3 normal = Vector3.Cross(up, right);
             Vector3 tangent = right;

             vert00.Normal = normal;
             vert01.Normal = normal;
             vert11.Normal = normal;
             vert10.Normal = normal;

             vert00.Tangent = tangent;
             vert01.Tangent = tangent;
             vert11.Tangent = tangent;
             vert10.Tangent = tangent;
             /*    verts.Add(vert01);
                 verts.Add(vert00);
                 verts.Add(vert11);
                 verts.Add(vert10);
                 verts.Add(vert11);
                 verts.Add(vert00);
             //     indices.Add()
             indices.Add((ushort)(index + 1));
             indices.Add((ushort)(index + 0));
             indices.Add((ushort)(index + 2));
             indices.Add((ushort)(index + 3));
             indices.Add((ushort)(index + 2));
             indices.Add((ushort)(index + 0));

         }

         verts.Add(vert00);
         verts.Add(vert01);
         verts.Add(vert11);
         verts.Add(vert10);



     }*/
}