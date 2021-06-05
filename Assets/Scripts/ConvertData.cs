using System;

public abstract class ConvertData
{
    public static float[] ConvertByteToFloat(byte[] array)
    {
        var floatArr = new float[array.Length / 4];
        for (var i = 0; i < floatArr.Length; i++)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array, i * 4, 4);
            }
            floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
        }
        return floatArr;
    }
}
