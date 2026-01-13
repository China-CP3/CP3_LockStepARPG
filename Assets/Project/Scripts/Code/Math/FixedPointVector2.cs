using System;

public readonly struct FixedPointVector2:IEquatable<FixedPointVector2>
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
    public bool Equals(FixedPointVector2 other)
    {
        return this.x == other.x && this.y == other.y;
    }

    public override bool Equals(object obj)
    {
        return obj is FixedPointVector2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked // 允许溢出，不检查
        {
            int hash = 17;
            hash = hash * 31 + x.ScaledValue.GetHashCode();
            hash = hash * 31 + y.ScaledValue.GetHashCode();
            return hash;
        }

        /*
         * 为什么选 17 和 31？这纯粹是数学经验和前辈们的性能总结：它们都是质数 在乘法运算中能让结果分布得更均匀，减少重复。
         * 为什么是 31？因为 31 * i 可以被编译器优化为 (i << 5) - i，这是一个位移和减法操作，CPU 运行速度极快。
         */
    }

    public static bool operator ==(FixedPointVector2 a, FixedPointVector2 b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(FixedPointVector2 a, FixedPointVector2 b)
    {
        return !a.Equals(b);
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
    /// 固定匀速向目标移动
    /// </summary>
    /// <param name="curPos"></param>
    /// <param name="targetPos"></param>
    /// <param name="speed">每次移动多少距离</param>
    /// <returns></returns>
    public static FixedPointVector2 MoveTowards(FixedPointVector2 curPos,FixedPointVector2 targetPos,FixedPoint speed)
    {
        FixedPointVector2 dir = targetPos - curPos;
        FixedPoint distanceSqr = dir.SqrMagnitude();

        // 如果距离已经小于速度，直接到达，防止抖动
        if (distanceSqr == FixedPoint.Zero || (speed > FixedPoint.Zero && distanceSqr <= speed * speed))
        {
            return targetPos;
        }

        //return curPos + dir.normalized * speed;//优化   normalized会再调用SqrMagenitude 上面已经调用过了 重复调用浪费性能了
        FixedPoint magnitude = FixedPoint.Sqrt(distanceSqr);
        return curPos + dir * speed / magnitude;
    }

    //想要匀速插值还是先快后慢插值 取决于使用方式
    //匀速插值：起点和终点固定不变 外部调用时 每帧传入匀速放大的T 得到的自然就是匀速推进的向量
    //先快后慢插值：其实是每一帧 用新的当前位置 推进t进度 t是个百分比 t填1 1帧就到达位置了 但不会完全到达 会无比接近
    //假如t是0.1 a到b总共100米 那么第一次就走了 10米 新起点是10 剩下距离是90
    //第二步走9米 新起点是19米 剩下81米 第三步走8.1米 新起点是27.1米 剩下72.9米
    //第四步走7.29米。。。以此类推
    //会发现越走越慢 越走越接近终点 无限逼近 理论上无限接近 但代码里可以判断<x米时 视为接近
    public static FixedPointVector2 Lerp(FixedPointVector2 a, FixedPointVector2 b,FixedPoint t)
    {
        if (t <= FixedPoint.Zero) return a;
        if (t >= FixedPoint.One) return b;

        return a + (b - a) * t;
    }

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
        return FixedPoint.Sqrt(SqrMagnitude());
    }

    

    /// <summary>
    /// 归一化 求方向
    /// </summary>
    public FixedPointVector2 normalized
    {
        //让向量的每个分量都除以它自己的长度 得到新向量 新的向量长度变为 1，但方向保持不变
        //(x/Magnitude, y/Magnitude) = normalized

        get
        {
            FixedPoint sqrMag = SqrMagnitude();

            // 2. 检查是否为零向量，避免开方和除零错误
            if (sqrMag.ScaledValue <= 0)
            {
                return Zero;
            }

            FixedPoint magnitude = FixedPoint.Sqrt(sqrMag);
            //return mag.ScaledValue > 0 ? this / mag : Zero;原始版本 用了2次除法 下面优化了 使用1次除法 两次更快的乘法来完成归一化
            FixedPoint invMagnitude = FixedPoint.One / magnitude;
            return new FixedPointVector2(this.x * invMagnitude, this.y * invMagnitude);
        }
    }
    #endregion
}
