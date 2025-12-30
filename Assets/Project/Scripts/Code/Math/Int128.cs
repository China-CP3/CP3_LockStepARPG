using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
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
        ulong carry = BigMul(a.low64, b.low64, out lowResult);//carry是溢出的进位 表示有N个low.max+1

        long crossProduct1 = (long)a.low64 * b.high64;
        long crossProduct2 = a.high64 * (long)b.low64;

        //a * b = a.high * b.high << 128 + 交叉相乘 << 64 + a.low * b.low //交叉相乘位移位数取决于low的位数

        // 步骤 4: 将所有 high 部分加起来   a.high * b.high溢出128位 直接丢弃
        // 最终的 high = (a.low*b.low的高位) + (a.low*b.high) + (a.high*b.low)
        long finalHigh = (long)carry + crossProduct1 + crossProduct2;//2个高位相乘 已经溢出128位了 直接不需要了

        return new Int128(finalHigh, lowResult);
    }

    private static ulong BigMul(ulong a, ulong b, out ulong low)
    {
        ulong aLow = a & 0xFFFFFFFF;//位掩码 左边32位全是0，右边32位全是1 为了得到a的低32位
        ulong aHigh = a >> 32;
        ulong bLow = b & 0xFFFFFFFF;
        ulong bHigh = b >> 32;

        //计算 12 * 34，把它拆成 (10+2) * (30+4)，然后计算 2*4, 2*30, 10*4, 10*30，最后加起来
        //aHigh = 1, aLow = 2
        //bHigh = 3, bLow = 4
        //p1 = aLow * bLow; -> 2 * 4 = 8
        //p2 = aLow * bHigh; -> 2 * 3 = 6
        //p3 = aHigh * bLow; -> 1 * 4 = 4
        //p4 = aHigh * bHigh; -> 1 * 3 = 3

        //8 是个位。
        //6 和 4 都是十位上的，6 + 4 = 10 表示有 10 个“十”
        //3 是百位上的。
        //所以结果是：3 * 10 * 10 + 10 * 10 + 8 = 300 + 100 + 8 = 408

        //交叉相乘 a * b = (aHigh * bHigh * 2^64) + (aHigh * bLow * 2^32) + (aLow * bHigh * 2^32) + (aLow * bLow)
        //p4 * 2^64 + (p2+p3) * 2^32 + p1
        //分段乘法中，交叉项的放大倍数 = N进制的X次方  X =「低位的位数」 从上面的10进制例子就可以得出 不信的再多举一些例子自己看
        //低位的位数」是人为拆分的结果：
        //比如十进制 123×456，可以拆低位段 1 位（个位），也可以拆 2 位（十位 + 个位），只要确定了低位段位数，规则就不变：
        //拆 2 位 放大倍数 = 10²= 100，交叉项就要 ×100；

        //二进制场景：低位段位数 = 32  放大倍数 = 2³²
        //十进制场景：低位段位数 = 1（个位）放大倍数 = 10¹
        //八进制 + 低位段 2 位  放大倍数 = 8²= 64
        //十六进制 + 低位段 4 位  放大倍数 = 16⁴= 65536

        ulong p1 = aLow * bLow;//这里为什么不需要处理进位  因为p1是64位 就算是2个32位相乘 也装得下
        ulong p2 = aLow * bHigh;
        ulong p3 = aHigh * bLow;
        ulong p4 = aHigh * bHigh;

        ulong carry = 0;
        ulong middle = p2 + p3;//注意看上面的例子和公式 交叉相乘 注定天生就要左移32位 例子中的6和4 天生就是60 40 放大了10倍
        if (middle < p2)//对于128位来说 middle自身是64位但是已经处于在32-95这个位置 当它溢出时 等于是在high的第31位溢出 需要在32位+1 所以是加上1UL << 32 而不是简单的末尾+1
        {
            carry = 1UL << 32;
        }

        low = p1 + (middle << 32);
        if (low < p1)
        {
            carry += 1;
        }

        ulong high = p4 + (middle >> 32);
        high += carry;
        return high;
    }

    //每一轮 余数左移1位 加上新加入的值 商左移一位 为本次计算结果腾出空间  如果够除 商+1
    //余数 - 除数 =余数 也就是 去掉用掉的数 比如十进制 13/4 用掉了12 剩下1  不能整除就开始下一轮循环
    
    public static Int128 operator /(Int128 a, Int128 b)
    {
        if(b == Zero)
        {
            throw new System.DivideByZeroException("Int128 a/b b is 0 !!!");
        }

        // C# 内置的 Int128 在这种情况下会抛出 OverflowException。
        if (a == MinValue && b == MinusOne)
        {
            return MinValue;
        }

        //10 / 2 = 5 -10 / 2 = -5 10 / -2 = -5 -10 / -2 = 5 很明显 双方符号不同时才是负数 相同时是正数
        //一开始就可以取得符号 然后双方按正数处理 最后结果加上符号 这样更方便
        bool isPlus = (a.high64 >= 0 && b.high64 >= 0) || (a.high64 < 0 && b.high64 < 0);

        Int128 aAbs = a.high64 < 0 ? -a : a;
        Int128 bAbs = b.high64 < 0 ? -b : b;

        Int128 quotient = UnsignedDivide(aAbs, bAbs);

        return isPlus ? quotient:-quotient;
    }

    //取模运算
    //public static Int128 UnsignedDivRem()
    //{

    //}

    /// <summary>
    /// 无符号128位除法 (dividend / divisor)。
    /// 除法和取模运算的核心。
    /// </summary>
    /// <param name="a">被除数 (必须为正数)</param>
    /// <param name="b">除数 (必须为正数)</param>
    /// <returns>商</returns>
    private static Int128 UnsignedDivide(Int128 a, Int128 b)
    {
        if (UnsignedCompareTo(b, a) > 0)
        {
            return Zero;
        }

        if (b == a)
        {
            return One;
        }

        Int128 quotient = Zero;//商
        Int128 remainder = Zero;//余数

        //每一轮 余数左移1位 加上新加入的值 商左移一位 为本次计算结果腾出空间  如果够除 商+1
        //余数 - 除数 =余数 也就是 去掉用掉的数 比如十进制 13/4 用掉了12 剩下1  不能整除就开始下一轮循环
        for (int i = 0; i < 128; i++)
        {
            quotient = quotient << 1;
            remainder = remainder << 1;

            if (((ulong)a.high64 & 0x8000000000000000) != 0)//掩码 是1000...0000 这样和a.high逻辑与运算 只会得到a.high的最高位是1或0
            {
                remainder = new Int128(remainder.high64, remainder.low64 | 1);//不需要考虑进位 开头把remainder左移了1位 末尾必定是0  和1逻辑或运算 只会让末尾变成0或1 不影响其他位
            }

            a = a << 1;

            if(UnsignedCompareTo(remainder, b) >= 0)
            {
                remainder = remainder - b;
                quotient = new Int128(quotient.high64, quotient.low64 | 1);
            }
        }

        return quotient;
    }

    /// <summary>
    /// 对两个 Int128 值进行无符号比较。
    /// </summary>
    /// <returns>-1 a < b; 0 a == b; 1 a > b</returns>
    private static int UnsignedCompareTo(Int128 a, Int128 b)
    {
        // 关键：将 high64 强制转换为 ulong 进行无符号比较
        int highCompare = ((ulong)a.high64).CompareTo((ulong)b.high64);
        if (highCompare != 0)
        {
            return highCompare;
        }
        return a.low64.CompareTo(b.low64);
    }
    #endregion

    #region 重载运算符 == != > < <= >= ~ << >>
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

    public static Int128 operator <<(Int128 a, int shift)
    {
        if (shift == 0) return a;
        if (shift >= 128) return Zero;
        if (shift == 64) return new Int128((long)a.low64,0);
        if (shift < 64)
        {
            ulong carry = a.low64 >> (64 - shift);//low本身就是ulong 这里是为了计算出需要进位多少 如果改成long 最高位万一是1就会被识别成负数 然后右移 就会在最高位添加1  导致值彻底错了
            long newHigh = a.high64 << shift;
            newHigh = newHigh | (long)carry;
            return new Int128(newHigh,a.low64 << shift);
        }
        if(shift > 64)
        {
            long newHigh = (long)(a.low64 << (shift - 64));//移动64位刚好把low变成high  shift-64剩下的 才是新high需要位移的数
            return new Int128(newHigh, 0);
        }
        return Zero;
    }

    public static Int128 operator >>(Int128 a, int shift)
    {
        bool isPlus = !(a.high64 < 0);
        if (shift == 0) return a;
        if (shift >= 128) return isPlus ? Zero:MinusOne;
        if (shift == 64) return new Int128(isPlus ? 0L:-1L, (ulong)a.high64);
        if (shift < 64)
        {
            ulong carry = (ulong)a.high64 << (64 - shift);
            ulong newLow = a.low64 >> shift | carry;
            return new Int128(a.high64 >> shift, newLow);
        }
        if (shift > 64)
        {
            ulong newLow = (ulong)(a.high64 >> (shift - 64));
            return new Int128(isPlus ? 0L : -1L, newLow);
        }
        return Zero;
    }
    #endregion

    #region 位逻辑运算 & | ^
    public static Int128 operator &(Int128 a, Int128 b)
    {
        return new Int128(a.high64 & b.high64, a.low64 & b.low64);
    }

    public static Int128 operator |(Int128 a, Int128 b)
    {
        return new Int128(a.high64 | b.high64, a.low64 | b.low64);
    }

    public static Int128 operator ^(Int128 a, Int128 b)
    {
        return new Int128(a.high64 ^ b.high64, a.low64 ^ b.low64);
    }
    #endregion

    #region 隐式转换
    public static implicit operator Int128(long value) => new Int128(value);
    public static implicit operator Int128(int value) => new Int128(value);
    #endregion
}
