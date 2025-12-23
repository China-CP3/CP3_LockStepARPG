using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public readonly struct Int128
{
    public readonly ulong low64;//ulong不会因为溢出变成负数
    public readonly long high64;//正负由最高位决定

    #region 常量定义
    public static readonly Int128 Zero = new Int128(0);
    public static readonly Int128 One = new Int128(1);
    public static readonly Int128 MaxValue = new Int128(long.MaxValue, ulong.MaxValue);
    public static readonly Int128 MinValue = new Int128(long.MinValue, 0);
    public static readonly Int128 MinusOne = new Int128(-1, ulong.MaxValue);
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


    #region 常用接口
    public override string ToString()
    {
        //用16进制方便看是否计算成功  不然数字太大了看着眼花
        return $"[High: 0x{high64:X16}, Low: 0x{low64:X16}]";
    }

    #endregion

    #region 四则运算 +-*/
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
        return a + (-b);
    }

    public static Int128 operator -(Int128 a)
    {
        return ~a + One;
    }

    public static Int128 operator *(Int128 a, Int128 b)
    {
        ulong lowResult;
        ulong highOfLowProduct = BigMul(a.low64, b.low64, out lowResult);

        // 步骤 2 & 3: 计算两个交叉项
        // 注意：这里需要将 ulong 转为 long 进行有符号乘法
        long crossProduct1 = (long)a.low64 * b.high64;
        long crossProduct2 = a.high64 * (long)b.low64;

        // 步骤 4: 将所有 high 部分加起来
        // 最终的 high = (a.low*b.low的高位) + (a.low*b.high) + (a.high*b.low)
        // 注意：highOfLowProduct 是 ulong，需要转为 long 才能和另外两个 long 相加
        long finalHigh = (long)highOfLowProduct + crossProduct1 + crossProduct2;

        return new Int128(finalHigh, lowResult);
    }

    private static ulong BigMul(ulong a, ulong b, out ulong low)
    {
        ulong aLow = a & 0xFFFFFFFF;//位掩码 左边32位全是0，右边32位全是1 为了得到a的低32位
        ulong aHigh = a >> 32;
        ulong bLow = b & 0xFFFFFFFF;
        ulong bHigh = b >> 32;

        //就像小学时乘法，要计算 12 * 34，把它拆成 (10+2) * (30+4)，然后计算 2*4, 2*30, 10*4, 10*30，最后加起来
        //可以把 12 看成(1 * 10) + 2，把 34 看成(3 * 10) + 4。
        //aHigh = 1, aLow = 2
        //bHigh = 3, bLow = 4
        //p1 = aLow * bLow; -> 2 * 4 = 8。（个位乘以个位）
        //p2 = aLow * bHigh; -> 2 * 3 = 6。（a的个位* b的十位）
        //p3 = aHigh * bLow; -> 1 * 4 = 4。（a的十位* b的个位）
        //p4 = aHigh * bHigh; -> 1 * 3 = 3。（十位乘以十位）

        //8 是个位。
        //6 和 4 都是十位上的，6 + 4 = 10 表示有 10 个“十”。
        //3 是百位上的。
        //所以结果是：3个百 + 10个十 + 8个一 = 300 + 100 + 8 = 408。
         
        //交叉相乘 a * b = (aHigh * bHigh * 2^64) + (aHigh * bLow * 2^32) + (aLow * bHigh * 2^32) + (aLow * bLow)
        //p4 * 2^64 + (p2+p3) * 2^32 + p1
        ulong p1 = aLow * bLow;
        ulong p2 = aLow * bHigh;
        ulong p3 = aHigh * bLow;
        ulong p4 = aHigh * bHigh;

        ulong middle = p2 + p3;
        low = p1 + (middle << 32);
        ulong high = p4 + (middle >> 32);
        if (low < p1)
        {
            high++;
        }
        if (middle < p2)
        {
            high += (1UL << 32); // 1UL << 32 就是 2^32
        }
        return high;
    }

    //public static Int128 operator -(Int128 a, Int128 b)
    //{

    //}
    #endregion

    #region 重载运算符 == != > < <= >= ~
    public static bool operator ==(Int128 a, Int128 b)
    {
        return a.high64 == b.high64 && a.low64 == b.low64;
    }

    public static bool operator !=(Int128 a, Int128 b)
    {
        return a.high64 != b.high64 || a.low64 != b.low64;
    }

    public static bool operator >(Int128 a, Int128 b)
    {
        return a.high64 > b.high64 || (a.high64 == b.high64 && a.low64 > b.low64);
    }

    public static bool operator <(Int128 a, Int128 b)
    {
        return a.high64 < b.high64 || (a.high64 == b.high64 && a.low64 < b.low64);
    }

    public static bool operator >=(Int128 a, Int128 b)
    {
        return a.high64 > b.high64 || (a.high64 == b.high64 && a.low64 >= b.low64);
    }

    public static bool operator <=(Int128 a, Int128 b)
    {
        return a.high64 < b.high64 || (a.high64 == b.high64 && a.low64 <= b.low64);
    }

    public static Int128 operator ~(Int128 a)
    {
        return new Int128(~a.high64, ~a.low64);
    }
    #endregion
}
