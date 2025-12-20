using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct MyInt8
{
    public byte low4Bit;//表示0-15
    public byte high4Bits;

    public MyInt8(byte value)
    {
        low4Bit = (byte)(value % 16);//看看低位还剩多少
        high4Bits = (byte)((value - low4Bit) / 16);//计算高位是多少

    }

    public byte GetValue
    {
        get { return (byte)(high4Bits << 4 | low4Bit); }
    }

    public static MyInt8 operator +(MyInt8 a, MyInt8 b)
    {
        MyInt8 result = new MyInt8();

        int tempLow = a.low4Bit + b.low4Bit;
        result.low4Bit = (byte)(tempLow % 16);
        byte caryy = (byte)((tempLow - result.low4Bit) / 16);

        int tempHigh = a.high4Bits + b.high4Bits + caryy;
        result.high4Bits = (byte)tempHigh;   

        return result;
    }

    public override string ToString()
    {
        return $"[High: {high4Bits}, Low: {low4Bit}]  (Value: {GetValue})";
    }
}
