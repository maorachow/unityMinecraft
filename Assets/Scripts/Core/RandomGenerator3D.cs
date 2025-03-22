using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RandomGenerator3D
{
    //  public System.Random rand=new System.Random(0);
    public static FastNoiseLite randomNoiseGenerator = new FastNoiseLite();
    public static bool initNoiseGenerator = InitNoiseGenerator();
    public static bool InitNoiseGenerator()
    {
        //  randomNoiseGenerator.SetSeed(0);
        randomNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Value);
        randomNoiseGenerator.SetFrequency(1);
        return true;
        // randomNoiseGenerator.SetFractalType(FastNoise.FractalType.None);
    }

    public static int GenerateIntFromVec3(Vector3Int pos)
    {
        float value = randomNoiseGenerator.GetNoise(pos.x, pos.y, pos.z);
        value += 1f;
        int finalValue = (int)(value * 50f);
        finalValue = Mathf.Clamp(finalValue, 0, 100);
        //   Debug.Log(finalValue);
        //   System.Random rand=new System.Random(pos.x*pos.y*pos.z*100);
        return finalValue;
    }
    public static int GenerateIntFromVec2(Vector2Int pos)
    {
        float value = randomNoiseGenerator.GetNoise(pos.x, pos.y);
        value += 1f;
        int finalValue = (int)(value * 50f);
        finalValue = Mathf.Clamp(finalValue, 0, 100);
        //   Debug.Log(finalValue);
        //   System.Random rand=new System.Random(pos.x*pos.y*pos.z*100);
        return finalValue;
    }
}
