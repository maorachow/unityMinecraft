using System.IO;
using UnityEngine;

public static class MathUtility
{

    public static bool[] GetBooleanArray(byte b)
    {
        bool[] array = new bool[8];
        for (int i = 7; i >= 0; i--)
        { //对于byte的每bit进行判定
            array[i] = (b & 1) == 1;   //判定byte的最后一位是否为1，若为1，则是true；否则是false
            b = (byte)(b >> 1);       //将byte右移一位
        }
        return array;
    }

    public static byte GetByte(bool[] array)
    {
        if (array != null && array.Length > 0)
        {
            byte b = 0;
            for (int i = 0; i <= 7; i++)
            {
                if (array[i])
                {
                    int nn = 1 << 7 - i;
                    b += (byte)nn;
                }
            }
            return b;
        }
        return 0;
    }

    public static byte[] CombineBytes(byte[] firstBytes, int firstIndex, int firstLength, byte[] secondBytes, int secondIndex, int secondLength)
    {
        byte[] bytes = null;
        MemoryStream ms = new MemoryStream();
        ms.Write(firstBytes, firstIndex, firstLength);
        ms.Write(secondBytes, secondIndex, secondLength);
        bytes = ms.ToArray();
        ms.Close();
        return (bytes);
    }
}
