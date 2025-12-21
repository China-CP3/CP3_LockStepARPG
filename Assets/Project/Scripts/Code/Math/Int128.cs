using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine; 

public struct Int128
{
    public long low64;
    public long high64;

    #region 常量定义
    /// <summary>最小值：-2^127</summary>
    public static readonly Int128 MinValue = new Int128(unchecked((long)0x8000000000000000), 0);
    /// <summary>最大值：2^127 - 1</summary>
    public static readonly Int128 MaxValue = new Int128(unchecked((long)0x7FFFFFFFFFFFFFFF), unchecked((long)0xFFFFFFFFFFFFFFFF));
    /// <summary>零</summary>
    public static readonly Int128 Zero = new Int128(0, 0);
    /// <summary>一</summary>
    public static readonly Int128 One = new Int128(0, 1);
    /// <summary>负一</summary>
    public static readonly Int128 NegativeOne = new Int128(-1, -1);
    #endregion

    public Int128(long high64, long low64)
    {
        this.high64 = high64;
        this.low64 = low64;
    }

    public Int128(long value)
    {
        this.low64 = (long)value;
        // (long)value >> 63 会得到 0 (如果value为正) 或 -1 (如果value为负)
        // -1 的 ulong 形式就是 0xFFFFFFFFFFFFFFFF
        this.high64 = (long)((long)value >> 63);

    }

    public static implicit operator Int128(long value)
    {
        return new Int128(value);
    }

    public override string ToString()
    {
        //用16进制方便看是否计算成功  不然数字太大了看着眼花
        return $"[High: 0x{high64:X16}, Low: 0x{low64:X16}]";
    }

    public static Int128 operator +(Int128 a, Int128 b)
    {
        long tempLow = a.low64 + b.low64;
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
        long tempLow = a.low64 - b.low64;
        byte highToLowNum = 0;
        if(a.low64 < b.low64)
        {
            highToLowNum = 1;
        }

        long tempHigh = a.high64 - b.high64 - highToLowNum;

        return new Int128(tempHigh, tempLow);
    }
    
}
