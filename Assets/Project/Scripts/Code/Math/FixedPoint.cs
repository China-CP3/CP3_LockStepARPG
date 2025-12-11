using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct FixedPoint
{
    private long scaledValue;//放大后的数
    public long ScaledValue
    {
        get 
        {
            return scaledValue;
        }
    }

    private const int ShiftBits = 32;//位移数
    private const long ScaleFactor = 1L << ShiftBits;//乘法放大的倍数
    private const long RoundAdd = 1L << (ShiftBits - 1);//用来处理四舍五入的加量

    #region 构造函数
    public FixedPoint(float value)
    {
        scaledValue = (long)(value * ScaleFactor + (value >= 0 ? RoundAdd : -RoundAdd));
    }

    public FixedPoint(double value)
    {
        scaledValue = (long)(value * ScaleFactor + (value >= 0 ? RoundAdd : -RoundAdd));
    }

    public FixedPoint(int value)
    {
        scaledValue = (long)value << ShiftBits;//C#规则 先位运算的值 决定容器大小 假如这里不(long)的话 value就是int 容器大小是32位 后位运算的值位移数会按32取模 导致long的值丢失
    }

    public FixedPoint(long value)
    {
        scaledValue = value << ShiftBits;
    }
    #endregion

    #region 常用接口
    /// <summary>
    /// 从一个已经放大的 scaled value 创建一个 FixedPoint 实例。
    /// </summary>
    public static FixedPoint FixedPointFactory(long scaledValue)
    {
        FixedPoint fp;
        fp.scaledValue = scaledValue;
        return fp;
    }
    #endregion

    #region 四则运算 重载+ - * /
    public static FixedPoint operator +(FixedPoint a, FixedPoint b)
    {
        return FixedPointFactory(a.scaledValue + b.scaledValue);
    }

    public static FixedPoint operator -(FixedPoint a, FixedPoint b)
    {
        return FixedPointFactory(a.scaledValue - b.scaledValue);
    }

    
    //* A* B
    //= (A_real * ScaleFactor) * (B_real * ScaleFactor)
    //= (A_real * B_real) * ScaleFactor * ScaleFactor

    //结果被放大了 两次 ScaleFactor！
    //正确结果应该是(A_real* B_real) * ScaleFactor。
    //所以，在计算完 a.scaledValue * b.scaledValue 之后，我们必须除以一个 ScaleFactor 来把它“拉回”到正确的放大倍数。
    //也就是 a * b / c 但是2个long相乘容易溢出 调换顺序 a / c * b 先缩小后放大也不行 会丢失精度 哪怕它俩数学上相等

    public static FixedPoint operator *(FixedPoint a, FixedPoint b)
    {
        return FixedPointFactory(a.scaledValue * b.scaledValue >> ShiftBits);
    }

    //A / B
    //= (A_real* ScaleFactor) / (B_real* ScaleFactor)
    //= A_real / B_real
    //两个 ScaleFactor 公倍数直接被约掉了 所以需要乘以1个ScaleFactor
    //A_real / B_real * ScaleFactor 
    //先除法的话 long会丢失精度 比如7/2=3 所以调换顺序  A_real * ScaleFactor / B_real //小心也会有乘法溢出问题
    //A_real * ScaleFactor / B_real 乘法以后再除法 会不会产生小数？会的 但是毫不影响 举个例子
    //10 / 3 * 10 = 3 * 10 = 30 丢失了0.3
    // 10 * 10 / 3 = 33.3 这时候33就是10/3放大10倍后的结果 0.3是产生的小数 因为是long会自动丢失 完全没影响 
    /*
     * 10 * 10 / 3 = 33.3 为什么要乘以10  假设规则定为保留1位小数  不管结果在小数点后面有多少位 只放大10倍 只保留1位 
     * 对分子乘以10等于是把结果乘以10倍 
     * 原本是 10/3 = 3.3 会丢失0.3变成3
     * 放大10倍 10 * 10 / 3 = 33.3 原本3.3变成了33.3 小数的0.3已经变成了整数 此时小数点后面的数 全是之前本身就要摒弃 不需要的  
     * 只不过我们实际是左移32位 把原本存在于小数点后面32位的小数变到了整数，后续小数点后面如果还有数 完全不用管 丢弃即可
     */
    public static FixedPoint operator /(FixedPoint a, FixedPoint b)
    {   
        if(b.scaledValue == 0)//分母不能为0
        {
            Debug.LogError("FixedPoint a/b b.scaledValue is 0!");
            return new FixedPoint(0);
        }
        return FixedPointFactory((a.scaledValue << ShiftBits) / b.scaledValue);
    }
    
    #endregion
}
