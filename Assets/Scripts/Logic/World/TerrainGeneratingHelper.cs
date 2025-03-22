using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 

public class TerrainGeneratingHelper 
{

    public static FastNoiseLite noiseGenerator
    {
        get { return VoxelWorld.currentWorld.noiseGenerator; }
    }

    public static FastNoiseLite frequentNoiseGenerator
    {
        get { return VoxelWorld.currentWorld.frequentNoiseGenerator; }
    }

    public static FastNoiseLite biomeNoiseGenerator
    {
        get { return VoxelWorld.currentWorld.biomeNoiseGenerator; }
    }

    public static int chunkSeaLevel = 63;
    public static int[,] GenerateChunkBiomeMap(Vector2Int pos)
    {
        //   float[,] biomeMap=new float[Chunk.chunkWidth/8+2,Chunk.chunkWidth/8+2];//插值算法
        //      int[,] chunkBiomeMap=GenerateChunkBiomeMap(pos);
        int[,] biomeMapInter = new int[Chunk.chunkWidth / 8 + 2, Chunk.chunkWidth / 8 + 2];
        for (int i = 0; i < Chunk.chunkWidth / 8 + 2; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth / 8 + 2; j++)
            {
                //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))

                biomeMapInter[i, j] = (int)(1f + biomeNoiseGenerator.GetNoise(pos.x + (i - 1) * 8, pos.y + (j - 1) * 8) * 3f);
            }
        }//32,32



        return biomeMapInter;
    }
    public static int[,] GenerateChunkBiomeMapIterpolated(Vector2Int pos)
    {

        int[,] biomeMapInter = new int[Chunk.chunkWidth * 2, Chunk.chunkWidth * 2];
        for (int i = 0; i <Chunk.chunkWidth * 2; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth * 2; j++)
            {
                //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))

                biomeMapInter[i, j] = (int)(1f + biomeNoiseGenerator.GetNoise(pos.x + (i - Chunk.chunkWidth / 2), pos.y + (j - Chunk.chunkWidth / 2)) * 3f);
            }
        }//32,32



        return biomeMapInter;
    }

    public static float[,] GenerateFloatChunkHeightDetailMapIterpolated(Vector2Int pos)
    {

        float[,] biomeMapInter = new float[Chunk.chunkWidth * 2, Chunk.chunkWidth * 2];
        for (int i = 0; i < Chunk.chunkWidth * 2; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth * 2; j++)
            {
                //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))

                biomeMapInter[i, j] = (2f + biomeNoiseGenerator.GetNoise(pos.x + (i - Chunk.chunkWidth / 2), pos.y + (j - Chunk.chunkWidth / 2)) * 1f);
            }
        }//32,32



        return biomeMapInter;
    }
    public int[,] thisAccurateHeightMap;
    public static float[,] GenerateChunkHeightmap(Vector2Int pos)
    {
        float[,] heightMap = new float[Chunk.chunkWidth * 2, Chunk.chunkWidth * 2];//插值算法
        float[,] chunkHeightDetailMap = GenerateFloatChunkHeightDetailMapIterpolated(pos);

        for (int i = 0; i < Chunk.chunkWidth * 2; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth * 2; j++)
            {
                //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))
                float baseNoise =
                    noiseGenerator.GetNoise(pos.x + (i - Chunk.chunkWidth / 2), pos.y + (j - Chunk.chunkWidth / 2));
                float highlandFactor = MathF.Max(Mathf.Pow(MathF.Max(baseNoise - 0.35f, 0.02f) * 1.9f, 1.3f), 0.03f) * 50f;
                float adjustedHeight =
                    chunkSeaLevel + (baseNoise) * 15f + highlandFactor * chunkHeightDetailMap[i, j];
                if (adjustedHeight > chunkSeaLevel * 1.5f)
                {
                    adjustedHeight = UnityEngine.Mathf.Lerp(adjustedHeight, chunkSeaLevel * 1.5f, 0.1f);
                }
                heightMap[i, j] = adjustedHeight;


            }

        }//32,32
        /*  int interMultiplier = 8;
          float[,] heightMapInterpolated = new float[(Chunk.chunkWidth / 8 + 2) * interMultiplier, (Chunk.chunkWidth / 8 + 2) * interMultiplier];
          for (int i = 0; i < (Chunk.chunkWidth / 8 + 2) * interMultiplier; ++i)
          {
              for (int j = 0; j < (Chunk.chunkWidth / 8 + 2) * interMultiplier; ++j)
              {
                  int x = i;
                  int y = j;
                  float x1 = (i / interMultiplier) * interMultiplier;
                  float x2 = (i / interMultiplier) * interMultiplier + interMultiplier;
                  float y1 = (j / interMultiplier) * interMultiplier;
                  float y2 = (j / interMultiplier) * interMultiplier + interMultiplier;
                  int x1Ori = (i / interMultiplier);
                  // Debug.Log(x1Ori);
                  int x2Ori = (i / interMultiplier) + 1;
                  x2Ori = Math.Clamp(x2Ori, 0, (Chunk.chunkWidth / 8 + 2) - 1);
                  //   Debug.Log(x2Ori);
                  int y1Ori = (j / interMultiplier);
                  //   Debug.Log(y1Ori);
                  int y2Ori = (j / interMultiplier) + 1;
                  y2Ori = Math.Clamp(y2Ori, 0, (Chunk.chunkWidth / 8 + 2) - 1);
                  //     Debug.Log(y2Ori);

                  float q11 = heightMap[x1Ori, y1Ori];
                  float q12 = heightMap[x1Ori, y2Ori];
                  float q21 = heightMap[x2Ori, y1Ori];
                  float q22 = heightMap[x2Ori, y2Ori];
                  float fxy1 = (float)(x2 - x) / (x2 - x1) * q11 + (float)(x - x1) / (x2 - x1) * q21;
                  float fxy2 = (float)(x2 - x) / (x2 - x1) * q12 + (float)(x - x1) / (x2 - x1) * q22;
                  float fxy = (float)(y2 - y) / (y2 - y1) * fxy1 + (float)(y - y1) / (y2 - y1) * fxy2;
                  heightMapInterpolated[x, y] = fxy;
                  //       Debug.Log(fxy);
                  //    Debug.Log(x1);
                  //  Debug.Log(x2);

              }
          }*/

        return heightMap;
    }
    public static int[,] GenerateChunkBiomeMapUsingHeight(Vector2Int pos, ref float[,] chunkHeightmap)
    {

        int[,] biomeMapInter = new int[Chunk.chunkWidth * 2, Chunk.chunkWidth * 2];
        for (int i = 0; i < Chunk.chunkWidth * 2; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth * 2; j++)
            {
                //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))
                float noiseFactor =
                    (biomeNoiseGenerator.GetNoise(pos.x + (i - Chunk.chunkWidth / 2),
                        pos.y + (j - Chunk.chunkWidth / 2)) * 8f);
                float modifiedHeight = chunkHeightmap[i, j] + noiseFactor;
                if (modifiedHeight < chunkSeaLevel * 1.05f)
                {
                    biomeMapInter[i, j] = 0;
                }
                else if (modifiedHeight >= chunkSeaLevel * 1.05f && modifiedHeight < chunkSeaLevel * 1.8f)
                {
                    biomeMapInter[i, j] = 1;
                }
                else if (modifiedHeight >= chunkSeaLevel * 1.8f && modifiedHeight < chunkSeaLevel * 2.3f)
                {
                    biomeMapInter[i, j] = 2;
                }
                else if (modifiedHeight >= chunkSeaLevel * 2.3f)
                {
                    biomeMapInter[i, j] = 3;
                }


            }
        }//32,32



        return biomeMapInter;
    }


    public static BlockShape PredictBlockType3D(int x, int y, int z)
    {
        float yLerpValue = Mathf.Lerp(-1, 1, (Mathf.Abs(y - chunkSeaLevel)) / 40f);
        float xzLerpValue = Mathf.Lerp(-1, 1, (new Vector3(x, 0, z).magnitude / 384f));
        float xyzLerpValue = Mathf.Max(xzLerpValue, yLerpValue);
        float noiseValue = frequentNoiseGenerator.GetNoise(x, y, z);
        if (noiseValue > xyzLerpValue)
        {
            return BlockShape.Solid;
        }
        else
        {
            return BlockShape.Empty;
        }
        // return 0;
    }


    public static BlockShape PredictBlockType3DLOD(int x, int y, int z, int LODBlockSkipCount = 4)
    {
        float yLerpValue = Mathf.Lerp(-1, 1, (Mathf.Abs(y - chunkSeaLevel)) / 40f);
        float xzLerpValue = Mathf.Lerp(-1, 1, (new Vector3(x, 0, z).magnitude / 384f));
        float xyzLerpValue = Mathf.Max(xzLerpValue, yLerpValue);
        float noiseValue = frequentNoiseGenerator.GetNoise((int)(x / LODBlockSkipCount) * LODBlockSkipCount, y,
            (int)(z / LODBlockSkipCount) * LODBlockSkipCount);
        if (noiseValue > xyzLerpValue)
        {
            return BlockShape.Solid;
        }
        else
        {
            return BlockShape.Empty;
        }
        // return 0;
    }


    public static BlockShape PredictBlockTypeOverworld(float noiseValue, int y)
    {
        if (noiseValue > y)
        {
            return BlockShape.Solid;
        }
        else
        {
            if (y < chunkSeaLevel && y > noiseValue)
            {
                return BlockShape.Water;
            }

            return BlockShape.Empty;
        }
        // return 0;
    }
    public static void GenerateOverworldChunkMap(ref float[,] chunkHeightMap, ref UnsafeChunkMapData<BlockData> chunkMap, Vector2Int chunkPos)
    {
        if (chunkHeightMap == null)
        {
            chunkHeightMap = GenerateChunkHeightmap(chunkPos);
        }
        int[,] chunkBiomechunkMapInterpolated = GenerateChunkBiomeMapUsingHeight(chunkPos, ref chunkHeightMap);

        //    int[,] biomechunkMap=GenerateChunkBiomechunkMap(chunkPos);
        for (int i = 0; i < Chunk.chunkWidth; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth; j++)
            {
                //    float iscale=(i%4)/4f;
                //  float jscale=(j%4)/4f;
                //    Debug.Log(iscale);
                //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.y*0.01f+j*0.01f);
                //     float noiseValueX=(heightchunkMap[i/4+1,j/4+1]*iscale+heightchunkMap[i/4+2,j/4+1]*(1-iscale));
                //   float noiseValueY=(heightchunkMap[i/4+1,j/4+1]*jscale+heightchunkMap[i/4+1,j/4+2]*(1-jscale));
                float noiseValue = (chunkHeightMap[i + Chunk.chunkWidth / 2, j + Chunk.chunkWidth / 2]);
                for (int k = 0; k < Chunk.chunkHeight; k++)
                {
                    if (noiseValue > k + 3)
                    {
                        chunkMap[i, k, j] = 1;
                    }
                    else if (noiseValue > k)
                    {
                        if (chunkBiomechunkMapInterpolated[i + Chunk.chunkWidth / 2, j + Chunk.chunkWidth / 2] == 3)
                        {
                            chunkMap[i, k, j] = 1;
                        }
                        else if (chunkBiomechunkMapInterpolated[i + Chunk.chunkWidth / 2, j + Chunk.chunkWidth / 2] == 0)
                        {
                            chunkMap[i, k, j] = 11;
                        }
                        else
                        {
                            chunkMap[i, k, j] = 3;
                        }
                    }
                }
            }
        }


        for (int i = 0; i < Chunk.chunkWidth; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth; j++)
            {
                for (int k = Chunk.chunkHeight - 1; k >= 0; k--)
                {
                    if (chunkMap[i, k, j] != 0 && chunkMap[i, k, j] != 9 && k >= chunkSeaLevel &&
                        (chunkBiomechunkMapInterpolated[i + Chunk.chunkWidth / 2, j + Chunk.chunkWidth / 2] == 2 || chunkBiomechunkMapInterpolated[i + Chunk.chunkWidth / 2, j + Chunk.chunkWidth / 2] == 1))
                    {
                        chunkMap[i, k, j] = 4;
                        break;
                    }
                }

                for (int k = Chunk.chunkHeight - 1; k >= 0; k--)
                {
                    if (k > chunkSeaLevel && chunkMap[i, k, j] == 0 && chunkMap[i, k - 1, j] == 4 &&
                    chunkMap[i, k - 1, j] != 100 &&
                    RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(chunkPos.x, 0, chunkPos.y) +
                                                              new Vector3Int(i, k, j)) > 70 &&
                        (chunkBiomechunkMapInterpolated[i + Chunk.chunkWidth / 2, j + Chunk.chunkWidth / 2] == 2 || chunkBiomechunkMapInterpolated[i + Chunk.chunkWidth / 2, j + Chunk.chunkWidth / 2] == 1))
                    {
                        chunkMap[i, k, j] = 101;
                    }

                    if (k < chunkSeaLevel && chunkMap[i, k, j] == 0)
                    {
                        chunkMap[i, k, j] = 100;
                    }
                }
            }
        }

        List<Vector3Int> treePoints = new List<Vector3Int>();
        List<Vector3Int> treeLeafPoints = new List<Vector3Int>();
        for (int x = 0; x < 32; x++)
        {
            for (int z = 0; z < 32; z++)
            {
                Vector3Int point = new Vector3Int(x, (int)chunkHeightMap[x, z] + 1, z);
                if (point.y < chunkSeaLevel)
                {
                    continue;
                }

                Vector3Int pointTransformed2 = point - new Vector3Int(8, 0, 8);
                if (chunkBiomechunkMapInterpolated[x, z] == 3 || chunkBiomechunkMapInterpolated[x, z] == 0)
                {
                    continue;
                }

                if (pointTransformed2.x >= 0 && pointTransformed2.x < Chunk.chunkWidth && pointTransformed2.y >= 0 &&
                    pointTransformed2.y < Chunk.chunkHeight && pointTransformed2.z >= 0 &&
                    pointTransformed2.z < Chunk.chunkWidth)
                {
                    if (chunkMap[pointTransformed2.x, pointTransformed2.y - 1, pointTransformed2.z] == 1)
                    {
                        continue;
                    }
                }

                if (RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(point.x, point.y, point.z) +
                                                          new Vector3Int(chunkPos.x, 0, chunkPos.y)) > 98.5f)
                {
                    treePoints.Add(point);
                    treePoints.Add(point + new Vector3Int(0, 1, 0));
                    treePoints.Add(point + new Vector3Int(0, 2, 0));
                    treePoints.Add(point + new Vector3Int(0, 3, 0));
                    treePoints.Add(point + new Vector3Int(0, 4, 0));
                    treePoints.Add(point + new Vector3Int(0, 5, 0));
                    for (int i = -2; i < 3; i++)
                    {
                        for (int j = -2; j < 3; j++)
                        {
                            for (int k = 3; k < 5; k++)
                            {
                                Vector3Int pointLeaf = point + new Vector3Int(i, k, j);
                                treeLeafPoints.Add(pointLeaf);
                            }
                        }
                    }

                    treeLeafPoints.Add(point + new Vector3Int(0, 5, 0));
                    treeLeafPoints.Add(point + new Vector3Int(0, 6, 0));
                    treeLeafPoints.Add(point + new Vector3Int(1, 5, 0));
                    treeLeafPoints.Add(point + new Vector3Int(1, 6, 0));
                    treeLeafPoints.Add(point + new Vector3Int(0, 5, 1));
                    treeLeafPoints.Add(point + new Vector3Int(0, 6, 1));
                    treeLeafPoints.Add(point + new Vector3Int(-1, 5, 0));
                    treeLeafPoints.Add(point + new Vector3Int(-1, 6, 0));
                    treeLeafPoints.Add(point + new Vector3Int(0, 5, -1));
                    treeLeafPoints.Add(point + new Vector3Int(0, 6, -1));
                }
            }
        }

        //  Debug.WriteLine(treePoints[0].x +" "+ treePoints[0].y+" " + treePoints[0].z);
        foreach (var point1 in treeLeafPoints)
        {
            Vector3Int pointTransformed1 = point1 - new Vector3Int(8, 0, 8);
            //   Debug.WriteLine(pointTransformed.x + " "+pointTransformed.y + " "+pointTransformed.z);
            if (pointTransformed1.x >= 0 && pointTransformed1.x < Chunk.chunkWidth && pointTransformed1.y >= 0 &&
                pointTransformed1.y < Chunk.chunkHeight && pointTransformed1.z >= 0 &&
                pointTransformed1.z < Chunk.chunkWidth)
            {
                if (chunkMap[pointTransformed1.x, pointTransformed1.y, pointTransformed1.z] == 0)
                {
                    chunkMap[pointTransformed1.x, pointTransformed1.y, pointTransformed1.z] = 9;
                }
            }
        }

        foreach (var point in treePoints)
        {
            Vector3Int pointTransformed = point - new Vector3Int(8, 0, 8);
            //   Debug.WriteLine(pointTransformed.x + " "+pointTransformed.y + " "+pointTransformed.z);
            if (pointTransformed.x >= 0 && pointTransformed.x < Chunk.chunkWidth && pointTransformed.y >= 0 &&
                pointTransformed.y < Chunk.chunkHeight && pointTransformed.z >= 0 && pointTransformed.z < Chunk.chunkWidth)
            {
                chunkMap[pointTransformed.x, pointTransformed.y, pointTransformed.z] = 7;
            }
        }
        /*  bool leftChunkLoaded=(isLeftChunkUnloaded==false&&leftChunk!=null);
          bool rightChunkLoaded=(isRightChunkUnloaded==false&&leftChunk!=null);
          bool backChunkLoaded=(isFrontChunkUnloaded==false&&leftChunk!=null);
          bool frontChunkLoaded=(isBackChunkUnloaded==false&&leftChunk!=null);
          bool leftChunkUnLoaded=(isLeftChunkUnloaded==true&&leftChunk!=null);
          bool rightChunkUnLoaded=(isRightChunkUnloaded==true&&rightChunk!=null);
          bool backChunkUnLoaded=(isBackChunkUnloaded==true&&backChunk!=null);
          bool frontChunkUnLoaded=(isFrontChunkUnloaded==true&&frontChunk!=null);*/
        /*        bool leftChunkNull=(leftChunk==null);
            bool rightChunkNull=(rightChunk==null);
            bool backChunkNull=(backChunk==null);
            bool frontChunkNull=(frontChunk==null);
                for(int i=0;i<Chunk.chunkWidth;i++){
            for(int j=0;j<Chunk.chunkWidth;j++){

                for(int k=Chunk.chunkHeight-1;k>=0;k--){

                    if(k>chunkSeaLevel&&chunkMap[i,k,j]==0&&chunkMap[i,k-1,j]==4&&chunkMap[i,k-1,j]!=100){
                    if(treeCount>0){
                            if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(i,k,j))>98){


                               for(int x=-2;x<3;x++){
                                    for(int y=3;y<5;y++){
                                        for(int z=-2;z<3;z++){
                                            if(x+i<0||x+i>=Chunk.chunkWidth||z+j<0||z+j>=Chunk.chunkWidth){



                                            if(x+i<0){
                                                if(z+j>=0&&z+j<Chunk.chunkWidth){
                                                   if(leftChunkNull==false){
                                                    if(isLeftChunkUnloaded==true){leftChunk.additivechunkMap[Chunk.chunkWidth+(x+i),y+k,z+j]=9;}else{leftChunk.chunkMap[Chunk.chunkWidth+(x+i),y+k,z+j]=9;}


                                                        isLeftChunkUpdated=true;

                                                //    WorldManager.chunkLoadingQueue.UpdatePriority(leftChunk,0);
                                           //         leftChunk.isChunkchunkMapUpdated=true;
                                                }
                                                }else if(z+j<0){
                                                    if(backLeftChunk!=null){
                                                        backLeftChunk.additivechunkMap[Chunk.chunkWidth+(x+i),y+k,Chunk.chunkWidth-1+(z+j)]=9;

                                                        isBackLeftChunkUpdated=true;

                                                  //    WorldManager.chunkLoadingQueue.UpdatePriority(backLeftChunk,0);
                                         //               backLeftChunk.isChunkchunkMapUpdated=true;
                                                    }

                                                }else if(z+j>=Chunk.chunkWidth){
                                                    if(frontLeftChunk!=null){
                                                        frontLeftChunk.additivechunkMap[Chunk.chunkWidth+(x+i),y+k,(z+j)-Chunk.chunkWidth]=9;

                                                        isFrontLeftChunkUpdated=true;

                                                   //     WorldManager.chunkLoadingQueue.UpdatePriority(frontLeftChunk,0);
                                       //                 frontLeftChunk.isChunkchunkMapUpdated=true;
                                                    }
                                                }

                                            }else
                                            if(x+i>=Chunk.chunkWidth){
                                                 if(z+j>=0&&z+j<Chunk.chunkWidth){
                                                   if(rightChunkNull==false){
                                                    if(isRightChunkUnloaded==true){  rightChunk.additivechunkMap[(x+i)-Chunk.chunkWidth,y+k,z+j]=9;}else{  rightChunk.chunkMap[(x+i)-Chunk.chunkWidth,y+k,z+j]=9;}
                                                   // rightChunk.additivechunkMap[(x+i)-Chunk.chunkWidth,y+k,z+j]=9;

                                                        isRightChunkUpdated=true;

                                                 //   WorldManager.chunkLoadingQueue.UpdatePriority(rightChunk,0);
                                              //      rightChunk.isChunkchunkMapUpdated=true;
                                                }
                                                }else if(z+j<0){
                                                    if(backRightChunk!=null){
                                                        backRightChunk.additivechunkMap[(x+i)-Chunk.chunkWidth,y+k,Chunk.chunkWidth+(z+j)]=9;

                                                        isBackRightChunkUpdated=true;

                                                  //    WorldManager.chunkLoadingQueue.UpdatePriority(backRightChunk,0);
                                               //         backRightChunk.isChunkchunkMapUpdated=true;
                                                    }

                                                }else if(z+j>=Chunk.chunkWidth){
                                                    if(frontRightChunk!=null){
                                                        frontRightChunk.additivechunkMap[(x+i)-Chunk.chunkWidth,y+k,(z+j)-Chunk.chunkWidth]=9;

                                                        isFrontRightChunkUpdated=true;

                                                 //     WorldManager.chunkLoadingQueue.UpdatePriority(frontRightChunk,0);
                                              //          frontRightChunk.isChunkchunkMapUpdated=true;
                                                    }
                                                }
                                            }else
                                            if(z+j<0){

                                                 if(x+i>=0&&x+i<Chunk.chunkWidth){
                                                   if(backChunkNull==false){
                                                    if(isBackChunkUnloaded==true){ backChunk.additivechunkMap[x+i,y+k,Chunk.chunkWidth+(z+j)]=9;}else{
                                                         backChunk.chunkMap[x+i,y+k,Chunk.chunkWidth+(z+j)]=9;
                                                    }
                                                  //  backChunk.additivechunkMap[x+i,y+k,Chunk.chunkWidth+(z+j)]=9;

                                                        isBackChunkUpdated=true;

                                            //    WorldManager.chunkLoadingQueue.UpdatePriority(backChunk,0);
                                           //         backChunk.isChunkchunkMapUpdated=true;
                                                }
                                                }else if(x+i<0){
                                                    if(backLeftChunk!=null){
                                                        backLeftChunk.additivechunkMap[Chunk.chunkWidth+(x+i),y+k,Chunk.chunkWidth-1+(z+j)]=9;

                                                        isBackLeftChunkUpdated=true;

                                                 //    WorldManager.chunkLoadingQueue.UpdatePriority(backLeftChunk,0);
                                            //            backLeftChunk.isChunkchunkMapUpdated=true;
                                                    }

                                                }else if(x+i>=Chunk.chunkWidth){
                                                    if(backRightChunk!=null){
                                                        backRightChunk.additivechunkMap[(x+i)-Chunk.chunkWidth,y+k,Chunk.chunkWidth-1+(z+j)]=9;

                                                        isBackRightChunkUpdated=true;

                                               //       WorldManager.chunkLoadingQueue.UpdatePriority(backRightChunk,0);
                                                  //      backRightChunk.isChunkchunkMapUpdated=true;
                                                    }
                                                }

                                            }else
                                            if(z+j>=Chunk.chunkWidth){

                                                if(x+i>=0&&x+i<Chunk.chunkWidth){
                                                   if(frontChunkNull==false){
                                                          if(isFrontChunkUnloaded==true){ frontChunk.additivechunkMap[x+i,y+k,(z+j)-Chunk.chunkWidth]=9;}else{
                                                             frontChunk.chunkMap[x+i,y+k,(z+j)-Chunk.chunkWidth]=9;
                                                        }
                                              //      frontChunk.additivechunkMap[x+i,y+k,(z+j)-Chunk.chunkWidth]=9;

                                                        isFrontChunkUpdated=true;

                                              //    WorldManager.chunkLoadingQueue.UpdatePriority(frontChunk,0);
                                                 //   frontChunk.isChunkchunkMapUpdated=true;
                                                }
                                                }else if(x+i<0){
                                                    if(frontLeftChunk!=null){

                                                        frontLeftChunk.additivechunkMap[Chunk.chunkWidth+(x+i),y+k,(z+j)-Chunk.chunkWidth]=9;

                                                    isBackLeftChunkUpdated=true;

                                              //        WorldManager.chunkLoadingQueue.UpdatePriority(frontLeftChunk,0);
                                                    //    frontLeftChunk.isChunkchunkMapUpdated=true;
                                                    }

                                                }else if(x+i>=Chunk.chunkWidth){
                                                    if(frontRightChunk!=null){
                                                        frontRightChunk.additivechunkMap[(x+i)-Chunk.chunkWidth,y+k,(z+j)-Chunk.chunkWidth]=9;

                                                        isFrontRightChunkUpdated=true;

                                                  //  WorldManager.chunkLoadingQueue.UpdatePriority(frontRightChunk,0);
                                                  //      frontRightChunk.isChunkchunkMapUpdated=true;
                                                    }
                                                }
                                            }


                                            }else{
                                                chunkMap[x+i,y+k,z+j]=9;
                                            }
                                        }
                                    }
                                }
                                  chunkMap[i,k,j]=7;
                                chunkMap[i,k+1,j]=7;
                               chunkMap[i,k+2,j]=7;
                                chunkMap[i,k+3,j]=7;
                                 chunkMap[i,k+4,j]=7;
                                 chunkMap[i,k+5,j]=9;
                                 chunkMap[i,k+6,j]=9;

                                if(i+1<Chunk.chunkWidth){
                                chunkMap[i+1,k+5,j]=9;
                                 chunkMap[i+1,k+6,j]=9;

                               }else{
                                if(rightChunkNull==false){
                                    if(isRightChunkUnloaded==true){rightChunk.additivechunkMap[0,k+5,j]=9;
                                 rightChunk.additivechunkMap[0,k+6,j]=9;}else{rightChunk.chunkMap[0,k+5,j]=9;rightChunk.chunkMap[0,k+6,j]=9;}


                            //      rightChunk.isChunkchunkMapUpdated=true;
                                }
                               }

                               if(i-1>=0){
                                chunkMap[i-1,k+5,j]=9;
                                chunkMap[i-1,k+6,j]=9;

                               }else{
                                if(leftChunkNull==false){
                                    if(isLeftChunkUnloaded==true){leftChunk.additivechunkMap[Chunk.chunkWidth-1,k+5,j]=9;
                                 leftChunk.additivechunkMap[Chunk.chunkWidth-1,k+6,j]=9;}else{leftChunk.chunkMap[Chunk.chunkWidth-1,k+5,j]=9;
                                 leftChunk.chunkMap[Chunk.chunkWidth-1,k+6,j]=9;}


                                // leftChunk.isChunkchunkMapUpdated=true;
                                }
                               }
                               if(j+1<Chunk.chunkWidth){
                                chunkMap[i,k+5,j+1]=9;
                                chunkMap[i,k+6,j+1]=9;

                               }else{
                                if(frontChunkNull==false){
                                    if(isFrontChunkUnloaded==true){
                                        frontChunk.additivechunkMap[i,k+5,0]=9;
                                frontChunk.additivechunkMap[i,k+6,0]=9;
                                    }else{
                                        frontChunk.chunkMap[i,k+5,0]=9;
                                frontChunk.chunkMap[i,k+6,0]=9;
                                    }
                             //   frontChunk.additivechunkMap[i,k+5,0]=9;
                             //   frontChunk.additivechunkMap[i,k+6,0]=9;

                             //   frontChunk.isChunkchunkMapUpdated=true;
                                }
                               }

                               if(j-1>=0){
                                chunkMap[i,k+5,j-1]=9;
                                chunkMap[i,k+6,j-1]=9;

                               }else{
                                if(backChunkNull==false){
                                    if(isBackChunkUnloaded==true){
                                         backChunk.additivechunkMap[i,k+5,Chunk.chunkWidth-1]=9;
                                backChunk.additivechunkMap[i,k+6,Chunk.chunkWidth-1]=9;
                                    }else{
                                         backChunk.chunkMap[i,k+5,Chunk.chunkWidth-1]=9;
                                backChunk.chunkMap[i,k+6,Chunk.chunkWidth-1]=9;
                                    }


                              //  backChunk.isChunkchunkMapUpdated=true;
                                }
                               }


                         /*
                                chunkMap[i,k,j]=7;
                                chunkMap[i,k+1,j]=7;
                               chunkMap[i,k+2,j]=7;
                                chunkMap[i,k+3,j]=7;
                                 chunkMap[i,k+4,j]=7;
                               chunkMap[i,k+5,j]=9;

                               if(i+1<Chunk.chunkWidth){
                                chunkMap[i+1,k+4,j]=9;
                                 chunkMap[i+1,k+3,j]=9;
                                  chunkMap[i+1,k+2,j]=9;
                               }else{
                                if(rightChunk!=null){
                                    rightChunk.additivechunkMap[0,k+4,j]=9;
                                 rightChunk.additivechunkMap[0,k+3,j]=9;
                                  rightChunk.additivechunkMap[0,k+2,j]=9;
                                   isRightChunkUpdated=true;
                            //      rightChunk.isChunkchunkMapUpdated=true;
                                }
                               }
                               if(i-1>=0){
                                chunkMap[i-1,k+4,j]=9;
                                chunkMap[i-1,k+3,j]=9;
                                chunkMap[i-1,k+2,j]=9;
                               }else{
                                if(leftChunk!=null){
                                      leftChunk.additivechunkMap[Chunk.chunkWidth-1,k+4,j]=9;
                                 leftChunk.additivechunkMap[Chunk.chunkWidth-1,k+3,j]=9;
                                  leftChunk.additivechunkMap[Chunk.chunkWidth-1,k+2,j]=9;
                                  isLeftChunkUpdated=true;
                           //       leftChunk.isChunkchunkMapUpdated=true;
                                }
                               }
                               if(j+1<Chunk.chunkWidth){
                                chunkMap[i,k+4,j+1]=9;
                                chunkMap[i,k+3,j+1]=9;
                                chunkMap[i,k+2,j+1]=9;
                               }else{
                                if(frontChunk!=null){
                                frontChunk.additivechunkMap[i,k+4,0]=9;
                                frontChunk.additivechunkMap[i,k+3,0]=9;
                                frontChunk.additivechunkMap[i,k+2,0]=9;
                                isFrontChunkUpdated=true;
                           //     frontChunk.isChunkchunkMapUpdated=true;
                                }
                               }



                               if(j-1>=0){
                                chunkMap[i,k+4,j-1]=9;
                                chunkMap[i,k+3,j-1]=9;
                                chunkMap[i,k+2,j-1]=9;
                               }else{
                                if(backChunk!=null){
                                backChunk.additivechunkMap[i,k+4,Chunk.chunkWidth-1]=9;
                                backChunk.additivechunkMap[i,k+3,Chunk.chunkWidth-1]=9;
                                backChunk.additivechunkMap[i,k+2,Chunk.chunkWidth-1]=9;
                                isBackChunkUpdated=true;
                         //       backChunk.isChunkchunkMapUpdated=true;
                                }
                               }*/


        //         treeCount--;
        /*             }
                 }
             }

         }
     }
 }*/
        for (int i = 0; i < Chunk.chunkWidth; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth; j++)
            {
                for (int k = 0; k < Chunk.chunkHeight / 4; k++)
                {
                    if (0 < k && k < 12)
                    {
                        if (RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(chunkPos.x, 0, chunkPos.y) +
                                                                  new Vector3Int(i, k, j)) > 93)
                        {
                            chunkMap[i, k, j] = 10;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < Chunk.chunkWidth; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth; j++)
            {
                chunkMap[i, 0, j] = 5;
            }
        }
        /*          if(isLeftChunkUpdated==true){
                     WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(leftChunk,false),0);
                 }
                 if(isRightChunkUpdated==true){
                   WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(rightChunk,false),0);
                 }
                 if(isBackChunkUpdated==true){
                   WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(backChunk,false),0);
                 }
                 if(isFrontChunkUpdated==true){
                   WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(frontChunk,false),0);
                 }
                 if(isFrontLeftChunkUpdated==true){
                   WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(frontLeftChunk,false),0);
                 }
                 if(isFrontRightChunkUpdated==true){
                   WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(frontRightChunk,false),0);
                 }
                 if(isBackLeftChunkUpdated==true){
                   WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(backLeftChunk,false),0);
                 }
                 if(isBackRightChunkUpdated==true){
                   WorldManager.chunkLoadingQueue.Enqueue(new ChunkLoadingQueueItem(backRightChunk,false),0);
                 }*/
    }

    public static void GenerateSuperflatChunkMap(ref UnsafeChunkMapData<BlockData> chunkMap, Vector2Int chunkPos)
    {
        for (int i = 0; i <Chunk. chunkWidth; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth; j++)
            {
                //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.z*0.01f+j*0.01f);
                for (int k = 0; k < Chunk.chunkHeight / 4; k++)
                {
                    chunkMap[i, k, j] = 1;
                }
            }
        }
    }


    public static void GenerateEnderworldChunkMap(ref UnsafeChunkMapData<BlockData> chunkMap, Vector2Int chunkPos)
    {
        for (int i = 0; i < Chunk.chunkWidth; i++)
        {
            for (int j = 0; j < Chunk.chunkWidth; j++)
            {
                //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.z*0.01f+j*0.01f);
                for (int k = 0; k < Chunk.chunkHeight / 2; k++)
                {
                    float yLerpValue = Mathf.Lerp(-1, 1, (Mathf.Abs(k - chunkSeaLevel)) / 40f);
                    float xzLerpValue = Mathf.Lerp(-1, 1,
                        (new Vector3(chunkPos.x + i, 0, chunkPos.y + j).magnitude / 384f));
                    float xyzLerpValue = Mathf.Max(xzLerpValue, yLerpValue);
                    if (frequentNoiseGenerator.GetNoise(i + chunkPos.x, k, j + chunkPos.y) > xyzLerpValue)
                    {
                        chunkMap[i, k, j] = 12;
                    }
                }
            }
        }
    }
}
