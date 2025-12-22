using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine; 

public readonly struct Int128
{
    public readonly ulong low64;//ulong不会因为溢出变成负数
    public readonly long high64;//正负由最高位决定

    #region 常量定义

    #endregion


    #region 构造函数
    //“垃圾进，垃圾出” (Garbage In, Garbage Out - GIGO)
    //构造函数本身不需要、也无法去阻止调用者传入一个已经“损坏”的 long 值。 它的职责就是做好本职的转换工作
    public Int128(long high = 0, ulong low = 0)
    {
        high64 = high;
        low64 = low;
    }

    public Int128(long value)
    {
        low64 = (ulong)value;
        // 关键：符号扩展。如果 value 是负数，比如 -1，
        // 它的二进制表示是全1。那么 High 部分也必须是全1，也就是 -1。
        // 如果 value 是正数，High 部分就是 0。
        high64 = value < 0 ? -1 : 0;
    }

    public Int128(ulong value)
    {
        low64 = value;
        high64 = 0; // ulong 永远是正数，所以 High 部分永远是 0
    }
    #endregion

    public override string ToString()
    {
        //用16进制方便看是否计算成功  不然数字太大了看着眼花
        return $"[High: 0x{high64:X16}, Low: 0x{low64:X16}]";
    }

    public static Int128 operator +(Int128 a, Int128 b)
    {
        ulong tempLow = a.low64 + b.low64;
        byte lowToHighCarry = 0;
        if (tempLow < a.low64)
        {
            lowToHighCarry = 1;
        }
        long tempHigh = a.high64 + b.high64 + lowToHighCarry;

        return new Int128(tempHigh, tempLow);
    }

    public static Int128 operator -(Int128 a, Int128 b)
    {
        ulong tempLow = a.low64 - b.low64;
        byte highToLowNum = 0;
        if(a.low64 < b.low64)
        {
            highToLowNum = 1;
        }

        long tempHigh = a.high64 - b.high64 - highToLowNum;

        return new Int128(tempHigh, tempLow);
    }
    
}
