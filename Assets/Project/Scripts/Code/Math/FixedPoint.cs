using System;
using UnityEngine; // 为了 Debug.LogError

/// <summary>
/// 一个64位定点数结构体 (Q32.32)。
/// 32位整数部分，32位小数部分。
/// </summary>
public struct FixedPoint
{
    private long scaledValue; // 放大后的整数值
    public long ScaledValue => scaledValue;

    // --- 常量定义 ---
    private const int ShiftBits = 32; // 小数部分占用的位数
    private const long ScaleFactor = 1L << ShiftBits; // 放大/缩小因子
    private const long RoundAdd = 1L << (ShiftBits - 1); // 用于浮点数构造时四舍五入的加数


    #region 构造函数

    public FixedPoint(float value)
    {
        // [健壮性修正] 增加溢出检查
        if (value > (float)long.MaxValue / ScaleFactor || value < (float)long.MinValue / ScaleFactor)
        {
            throw new OverflowException("Float value is too large or small to be represented by FixedPoint.");
        }
        scaledValue = (long)((value * ScaleFactor) + (value >= 0 ? RoundAdd: -RoundAdd));
    }

    public FixedPoint(double value)
    {
        // [健壮性修正] 增加溢出检查
        if (value > (double)long.MaxValue / ScaleFactor || value < (double)long.MinValue / ScaleFactor)
        {
            throw new OverflowException("Float value is too large or small to be represented by FixedPoint.");
        }
        scaledValue = (long)((value * ScaleFactor) + (value >= 0 ? RoundAdd : -RoundAdd));
    }

    public FixedPoint(int value)
    {
        scaledValue = (long)value << ShiftBits;
    }

    public FixedPoint(long value,bool isShift)
    {
        // [健壮性修正] 检查long值是否超出Q31.32的整数部分表示范围
        const long maxIntPart = long.MaxValue >> ShiftBits; // 等效于 int.MaxValue
        const long minIntPart = long.MinValue >> ShiftBits; // 等效于 int.MinValue
        if (value > maxIntPart || value < minIntPart)
        {
            throw new OverflowException("Long value is too large or small for the integer part of FixedPoint.");
        }
        scaledValue = value << ShiftBits;
    }

    #endregion

    #region 四则运算 重载+ - * /

    public static FixedPoint operator +(FixedPoint a, FixedPoint b)
    {
        return new FixedPoint(a.scaledValue + b.scaledValue);
    }

    public static FixedPoint operator -(FixedPoint a, FixedPoint b)
    {
        return new FixedPoint(a.scaledValue - b.scaledValue);
    }

    // --- 修正后的乘法和除法 ---

    public static FixedPoint operator *(FixedPoint a, FixedPoint b)
    {
        // 将 scaledValue 提升到 128位的 decimal 类型进行计算，以防止溢出
        decimal valA = a.scaledValue;
        decimal valB = b.scaledValue;

        // (A * B) / ScaleFactor
        decimal result = (valA * valB) / ScaleFactor;

        return new FixedPoint((long)result);
    }

    public static FixedPoint operator /(FixedPoint a, FixedPoint b)
    {
        if (b.scaledValue == 0)
        {
            Debug.LogError("FixedPoint division by zero!");
            return new FixedPoint(0);
        }

        return new FixedPoint((long)((a.scaledValue << ShiftBits) / b.ScaledValue));
    }

    #endregion
}