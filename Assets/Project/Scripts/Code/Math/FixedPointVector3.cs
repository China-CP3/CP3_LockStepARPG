using System;

public readonly struct FixedPointVector3:IEquatable<FixedPointVector3>
{
    public readonly FixedPoint x;
    public readonly FixedPoint y;
    public readonly FixedPoint z;

    public static FixedPointVector3 Zero = new FixedPointVector3(FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero);
    public static FixedPointVector3 One = new FixedPointVector3(FixedPoint.One, FixedPoint.One, FixedPoint.One);
    public static FixedPointVector3 Up = new FixedPointVector3(FixedPoint.Zero, FixedPoint.One, FixedPoint.Zero);
    public static FixedPointVector3 Down = new FixedPointVector3(FixedPoint.Zero, -FixedPoint.One, FixedPoint.Zero);
    public static FixedPointVector3 Left = new FixedPointVector3(-FixedPoint.One, FixedPoint.Zero, FixedPoint.Zero);
    public static FixedPointVector3 Right = new FixedPointVector3(FixedPoint.One, FixedPoint.Zero, FixedPoint.Zero);
    public static FixedPointVector3 Forward = new FixedPointVector3(FixedPoint.Zero, FixedPoint.Zero, FixedPoint.One);

    public FixedPointVector3(FixedPoint x, FixedPoint y, FixedPoint z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public bool Equals(FixedPointVector3 other)
    {
        return this.x == other.x && this.y == other.y && this.z == other.z;
    }

    public override bool Equals(object obj)
    {
        return obj is FixedPointVector3 other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked // 允许溢出，不检查
        {
            int hash = 17;
            hash = hash * 31 + x.GetHashCode();
            hash = hash * 31 + y.GetHashCode();
            hash = hash * 31 + z.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(FixedPointVector3 a, FixedPointVector3 b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(FixedPointVector3 a, FixedPointVector3 b)
    {
        return !a.Equals(b);
    }

    #region 四则运算 + - * / 点乘叉乘
    public static FixedPointVector3 operator +(FixedPointVector3 a, FixedPointVector3 b)
    {
        return new FixedPointVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static FixedPointVector3 operator -(FixedPointVector3 a, FixedPointVector3 b)
    {
        return new FixedPointVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    //2个向量相乘 有2种情况 点乘叉乘 所以不用重载运算符 而是写函数
    //2个向量相除 没有意义 不需要 包括unity的vector也没有提供
    public static FixedPoint Dot(FixedPointVector3 a, FixedPointVector3 b)
    {
        //return a.x * b.x + a.y * b.y;

        //由于FixedPoint的乘法 会右移10位还原值 导致每次都会丢失小数 如果频繁的丢失 累计起来误差就大了 如果用第二种办法 只在最后结果右移一次 误差小很多
        //举例 假如放大位数为1位 A:15 * 15 = 225 对应缩小1.5 * 1.5 = 2.25  B:15 * 15  = 225 对应缩小 1.5 * 1.5 = 2.25 最终结果是2.25+2.25 = 4.5 
        //第一种办法是这样 A的225/10 = 2.2 B的225/10=2.2  2.2+2.2=4.4 丢掉了0.1 每次缩小都丢掉了0.05
        //第二种办法 先转换成int128再乘 别用定点数的乘法 再求和 最后右移1次 只丢掉1次0.05

        Int128 abX = Int128.Multiply(a.x.ScaledValue, b.x.ScaledValue);
        Int128 abY = Int128.Multiply(a.y.ScaledValue, b.y.ScaledValue);
        Int128 abZ = Int128.Multiply(a.z.ScaledValue, b.z.ScaledValue);

        Int128 result = abX + abY + abZ;
        return FixedPoint.CreateByScaledValue((long)(result >> FixedPoint.ShiftBits));//先把int128右移再转long
    }

    public static FixedPointVector3 Cross(FixedPointVector3 a, FixedPointVector3 b)
    {
        //return a.x * b.y - a.y * b.x;
        Int128 aXbY = Int128.Multiply(a.x.ScaledValue, b.y.ScaledValue);
        Int128 aXbZ = Int128.Multiply(a.x.ScaledValue, b.z.ScaledValue);
        Int128 aYbX = Int128.Multiply(a.y.ScaledValue, b.x.ScaledValue);
        Int128 aYbZ = Int128.Multiply(a.y.ScaledValue, b.z.ScaledValue);
        Int128 aZbX = Int128.Multiply(a.z.ScaledValue, b.x.ScaledValue);
        Int128 aZbY = Int128.Multiply(a.z.ScaledValue, b.y.ScaledValue);

        FixedPoint resultX = FixedPoint.CreateByScaledValue((long)((aYbZ - aZbY) >> FixedPoint.ShiftBits));
        FixedPoint resultY = FixedPoint.CreateByScaledValue((long)((aZbX - aXbZ) >> FixedPoint.ShiftBits));
        FixedPoint resultZ = FixedPoint.CreateByScaledValue((long)((aXbY - aYbX) >> FixedPoint.ShiftBits));
        return new FixedPointVector3(resultX, resultY, resultZ);

        //Int128 result = aXbY - aYbX;
        //return FixedPoint.CreateByScaledValue((long)(result >> FixedPoint.ShiftBits));//先把int128右移再转long
    }

    //向量乘以标量 类比 vector2 * int a
    public static FixedPointVector3 operator *(FixedPointVector3 a, FixedPoint b)
    {
        return new FixedPointVector3(a.x * b, a.y * b, a.z * b);
    }

    //向量除以标量 类比 vector2 * int a
    public static FixedPointVector3 operator /(FixedPointVector3 a, FixedPoint b)
    {
        return new FixedPointVector3(a.x / b, a.y / b, a.z / b);
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
    public static FixedPointVector3 MoveTowards(FixedPointVector3 curPos, FixedPointVector3 targetPos, FixedPoint speed)
    {
        FixedPointVector3 dir = targetPos - curPos;
        long distSqrScaled = dir.SqrMagnitude().ScaledValue;

        // 如果距离已经小于速度，直接到达，防止抖动
        // 使用 Int128 处理 speed 的平方判定
        Int128 speedScaled = speed.ScaledValue;
        Int128 speedSqrFull = (speedScaled * speedScaled) >> FixedPoint.ShiftBits;
        if (distSqrScaled == 0 || (speed > FixedPoint.Zero && distSqrScaled <= speedSqrFull))
        {
            return targetPos;
        }

        //return curPos + dir.normalized * speed;//优化   normalized会再调用SqrMagenitude 上面已经调用过了 重复调用浪费性能了
        FixedPoint magnitude = FixedPointMath.Sqrt(FixedPoint.CreateByScaledValue(distSqrScaled));
        return curPos + dir * speed / magnitude;
    }

    //想要匀速插值还是先快后慢插值 取决于使用方式
    //匀速插值：起点和终点固定不变 外部调用时 每帧传入匀速放大的T 得到的自然就是匀速推进的向量
    //先快后慢插值：其实是每一帧 用新的当前位置 推进t进度 t是个百分比 t填1 1帧就到达位置了 但不会完全到达 会无比接近
    //假如t是0.1 a到b总共100米 那么第一次就走了 10米 新起点是10 剩下距离是90
    //第二步走9米 新起点是19米 剩下81米 第三步走8.1米 新起点是27.1米 剩下72.9米
    //第四步走7.29米。。。以此类推
    //会发现越走越慢 越走越接近终点 无限逼近 理论上无限接近 但代码里可以判断<x米时 视为接近
    public static FixedPointVector3 Lerp(FixedPointVector3 a, FixedPointVector3 b, FixedPoint t)
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
        return Dot(this, this);
    }

    public FixedPoint Magnitude()
    {
        return FixedPointMath.Sqrt(SqrMagnitude());
    }

    /// <summary>
    /// 归一化 求方向
    /// </summary>
    public FixedPointVector3 normalized
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

            FixedPoint magnitude = FixedPointMath.Sqrt(sqrMag);
            return new FixedPointVector3(this.x / magnitude, this.y / magnitude, this.z / magnitude);
        }
    }
    #endregion

    /// <summary>
    /// 投影到 XZ 平面 丢弃 Y 轴
    /// </summary>
    public FixedPointVector2 ToVector2XZ()
    {
        return new FixedPointVector2(this.x, this.z);
    }
}
