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
    //2个向量相除 没有意义 不需要 包括unity的vector也没有提供
    public static FixedPoint Dot(FixedPointVector2 a, FixedPointVector2 b)
    {
        //return a.x * b.x + a.y * b.y;

        //由于FixedPoint的乘法 会右移10位还原值 导致每次都会丢失小数 如果频繁的丢失 累计起来误差就大了 如果用第二种办法 只在最后结果右移一次 误差小很多
        //举例 假如放大位数为1位 A:15 * 15 = 225 对应缩小1.5 * 1.5 = 2.25  B:15 * 15  = 225 对应缩小 1.5 * 1.5 = 2.25 最终结果是2.25+2.25 = 4.5 
        //第一种办法是这样 A的225/10 = 2.2 B的225/10=2.2  2.2+2.2=4.4 丢掉了0.1 每次缩小都丢掉了0.05
        //第二种办法 先转换成int128再乘 别用定点数的乘法 再求和 最后右移1次 只丢掉1次0.05

        Int128 abX = Int128.Multiply(a.x.ScaledValue , b.x.ScaledValue);
        Int128 abY = Int128.Multiply(a.y.ScaledValue, b.y.ScaledValue);

        Int128 result = abX + abY;
        return FixedPoint.CreateByScaledValue((long)(result >> FixedPoint.ShiftBits));//先把int128右移再转long
    }

    public static FixedPoint Cross(FixedPointVector2 a, FixedPointVector2 b)
    {
        //return a.x * b.y - a.y * b.x;
        Int128 aXbY = Int128.Multiply(a.x.ScaledValue, b.y.ScaledValue);
        Int128 aYbX = Int128.Multiply(a.y.ScaledValue, b.x.ScaledValue);

        Int128 result = aXbY - aYbX;
        return FixedPoint.CreateByScaledValue((long)(result >> FixedPoint.ShiftBits));//先把int128右移再转long
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

    #region 几何属性

    /// <summary>
    /// 长度的平方 用来高效比较2个向量的长度 避免开发性能巨大消耗
    /// </summary>
    /// <returns></returns>
    public FixedPoint SqrMagnitude()
    {
        return Dot(this,this);
    }
    
    public FixedPoint Magnitude()
    {
        return FixedPoint.Sqrt(Dot(this, this));
    }

    public FixedPointVector2 normalized
    {
        get
        {
            FixedPoint magnitude = Magnitude();
            return magnitude.ScaledValue > 0 ? this / magnitude : Zero;
        }
    }
    #endregion
}
