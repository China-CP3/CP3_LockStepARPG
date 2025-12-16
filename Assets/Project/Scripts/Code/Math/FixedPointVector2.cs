using UnityEngine;

public readonly struct FixedPointVector2
{
    public readonly FixedPoint x;
    public readonly FixedPoint y;

    public static FixedPointVector2 Zero = new FixedPointVector2(FixedPoint.Zero, FixedPoint.Zero);
    public static FixedPointVector2 One = new FixedPointVector2(FixedPoint.One, FixedPoint.One);
    public static FixedPointVector2 Up = new FixedPointVector2(FixedPoint.Zero, FixedPoint.One);
    public static FixedPointVector2 Down = new FixedPointVector2(FixedPoint.Zero, -FixedPoint.One);
    public static FixedPointVector2 Left = new FixedPointVector2(-FixedPoint.One, FixedPoint.Zero);
    public static FixedPointVector2 Right = new FixedPointVector2(FixedPoint.One, FixedPoint.Zero);

    public FixedPointVector2(FixedPoint x,FixedPoint y)
    {
        this.x = x; 
        this.y = y;
    }

    #region 四则运算 + - * / 点乘叉乘
    public static FixedPointVector2 operator +(FixedPointVector2 a, FixedPointVector2 b)
    {
        return new FixedPointVector2(a.x + b.x, a.y + b.y);
    }

    public static FixedPointVector2 operator -(FixedPointVector2 a, FixedPointVector2 b)
    {
        return new FixedPointVector2(a.x - b.x, a.y - b.y);
    }

    //2个向量相乘 有2种情况 点乘叉乘 所以不用重载运算符 而是写函数
    //2哥向量相除 没有意义 不需要 包括unity的vector也没有提供
    public static FixedPoint Dot(FixedPointVector2 a, FixedPointVector2 b)
    {
        return a.x * b.x + a.y * b.y;
    }

    public static FixedPoint Cross(FixedPointVector2 a, FixedPointVector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    //向量乘以标量 类比 vector2 * int a
    public static FixedPointVector2 operator *(FixedPointVector2 a, FixedPoint b)
    {
        return new FixedPointVector2(a.x * b, a.y * b);
    }

    //向量除以标量 类比 vector2 * int a
    public static FixedPointVector2 operator /(FixedPointVector2 a, FixedPoint b)
    {
        return new FixedPointVector2(a.x / b, a.y / b);
    }

    #endregion
}
