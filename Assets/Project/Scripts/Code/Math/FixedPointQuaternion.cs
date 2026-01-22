
public readonly struct FixedPointQuaternion
{
    public readonly FixedPoint x;
    public readonly FixedPoint y;
    public readonly FixedPoint z;
    public readonly FixedPoint w;

    // 单位四元数，代表没有任何旋转
    public static readonly FixedPointQuaternion Identity = new FixedPointQuaternion(FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero, FixedPoint.One);

    public FixedPointQuaternion(FixedPoint x, FixedPoint y, FixedPoint z, FixedPoint w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    // 2个四元数相乘：用于合并旋转 
    // 逻辑：result = lhs * rhs (表示先进行 rhs 旋转，再进行 lhs 旋转)
    // 约定俗成：在 Unity 和大多数物理引擎中，乘法是从右往左生效的。即 A * B 是先执行B旋转，再执行A旋转。
    // 最终简化公式 哈密顿积公式：w表示标量 U表示向量xyz
    // newW：w1 * w2 - u1 * u2 (点乘) 得到新的旋转角度
    // newU: w1 * u2 + w2 * u1 + u1 * u2 (叉乘) 得到新的轴 表示绕这个轴旋转
    public static FixedPointQuaternion operator *(FixedPointQuaternion A, FixedPointQuaternion B)
    {
        long aX = A.x.ScaledValue;
        long aY = A.y.ScaledValue;
        long aZ = A.z.ScaledValue;
        long aW = A.w.ScaledValue;

        long bX = B.x.ScaledValue;
        long bY = B.y.ScaledValue;
        long bZ = B.z.ScaledValue;
        long bW = B.w.ScaledValue;

        // 计算公式：w_new = w1w2 - v1·v2,  v_new = w1v2 + w2v1 + v1 x v2
        //newW = a.w * b.w - (a.x * b.x + a.y * b.y + a.z * b.z);
        //newX = a.w * b.x + b.w * a.x + a.y * b.z - b.y * a.z
        //newY = a.w * b.y + b.w * a.y + a.z * b.x - a.x * b.z
        //newZ = a.w * b.z + b.w * a.z + a.x * b.y - a.y * b.x
        //想把先旋转B后旋转A改为先A后B 调换叉乘顺序即可
        Int128 x = Int128.Multiply(aW, bX) + Int128.Multiply(bW, aX) + Int128.Multiply(aY, bZ) - Int128.Multiply(aZ, bY);
        Int128 y = Int128.Multiply(aW, bY) + Int128.Multiply(bW, aY) + Int128.Multiply(aZ, bX) - Int128.Multiply(aX, bZ);
        Int128 z = Int128.Multiply(aW, bZ) + Int128.Multiply(bW, aZ) + Int128.Multiply(aX, bY) - Int128.Multiply(aY, bX);
        Int128 w = Int128.Multiply(aW, bW) - (Int128.Multiply(aX, bX) + Int128.Multiply(aY, bY) + Int128.Multiply(aZ, bZ));

        return new FixedPointQuaternion(
        FixedPoint.CreateByScaledValue((long)(x >> FixedPoint.ShiftBits)),
        FixedPoint.CreateByScaledValue((long)(y >> FixedPoint.ShiftBits)),
        FixedPoint.CreateByScaledValue((long)(z >> FixedPoint.ShiftBits)),
        FixedPoint.CreateByScaledValue((long)(w >> FixedPoint.ShiftBits))
        );

    }

    // 四元数 * 向量：实现向量 v 绕四元数 q 代表的轴旋转
    public static FixedPointVector3 operator *(FixedPointQuaternion q, FixedPointVector3 v)
    {
        //简单来说 分2步走 第一步先走直线 第二步修正 否则就成了直线移动 而不是绕某轴旋转 具体如何修正呢 用z轴叉乘y得到的向量 表示方向和长度进行修正即可

        //qV:这个四元素选择的方向和旋转量
        FixedPointVector3 qV = new FixedPointVector3(q.x, q.y, q.z);

        //v:需要旋转的点
        //*2是因为创建四元数的时候用了半角(angle/2) 所以*2还原回去
        //qV叉乘v得到新向量t t:同时垂直于qV和v的向量 或者说在qV和v相交点垂直的向量(想象v可以无限延伸) 表示v直线移动的方向
        FixedPointVector3 t = FixedPointVector3.Cross(qV, v) * FixedPoint.CreateByInt(2);

        // t * 2 * 标量 表示v直线移动的距离
        // 直线移动后 还有第二部修正 否则就成了直线移动
        // 用qV叉乘t 表示方向和长度即可
        // 计算 v' = v + q.w * t + Cross(qv, t)
        return v + (t * q.w) + FixedPointVector3.Cross(qV, t);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="angle01">0.1 度为单位的整数</param>
    /// <param name="axis">绕某条轴旋转</param>
    /// <returns></returns>
    public static FixedPointQuaternion AngleAxis(int angle01, FixedPointVector3 axis)
    {
        //取半角（这是四元数强制要求的数学结构）
        //因为四元数是通过 q * v * q的负一次方来旋转向量的
        //这个运算过程中，向量会被 q 乘一次，再被 q^-1 乘一次，合起来正好两次
        //所以我们在构造 q 的时候先除以2，最后旋转时‘两次旋转’加起来正好就是我们要的完整角度

        int halfAngle = angle01 / 2;//注意!传入的是 0.1 度为单位的整数

        FixedPoint s = FixedPoint.CreateByScaledValue(FixedPointMath.Sin(halfAngle));//xyz分别乘以sin(angle/2) 得到新的xyz 表示绕某条轴旋转
        FixedPoint c = FixedPoint.CreateByScaledValue(FixedPointMath.Cos(halfAngle));//cos(angle/2) 表示旋转的角度 标量其实就是cosx的值 -1表示旋转了360度 1表示旋转了0度
        FixedPointVector3 normAxis = axis.normalized;

        return new FixedPointQuaternion(
            normAxis.x * s,
            normAxis.y * s,
            normAxis.z * s,
            c
        );
    }

    public FixedPointQuaternion Normalized
    {
        get
        {
            long xS = x.ScaledValue;
            long yS = y.ScaledValue;
            long zS = z.ScaledValue;
            long wS = w.ScaledValue;

            // 计算 x^2 + y^2 + z^2 + w^2
            Int128 sum = Int128.Multiply(xS, xS) + Int128.Multiply(yS, yS) +
                         Int128.Multiply(zS, zS) + Int128.Multiply(wS, wS);

            FixedPoint magnitude = FixedPointMath.Sqrt(FixedPoint.CreateByScaledValue((long)(sum >> FixedPoint.ShiftBits)));
            if (magnitude <= FixedPoint.Zero) return Identity;
            return new FixedPointQuaternion(x / magnitude, y / magnitude, z / magnitude, w / magnitude);
        }
    }

    /// <summary>
    /// 欧拉角转四元数
    /// </summary>
    /// <param name="x01"></param>
    /// <param name="y01"></param>
    /// <param name="z01"></param>
    /// <returns></returns>
    public static FixedPointQuaternion EulerToQuaternion(int x01, int y01, int z01)
    {
        FixedPointQuaternion qZ = AngleAxis(z01, FixedPointVector3.Forward);
        FixedPointQuaternion qX = AngleAxis(x01, FixedPointVector3.Right);
        FixedPointQuaternion qY = AngleAxis(y01, FixedPointVector3.Up);

        return qY * qX * qZ;//约定成俗的规则 四元数a * b 先旋转b再选择a 这里真实顺序是zxy
    }

    public static FixedPointQuaternion Lerp(FixedPointQuaternion a, FixedPointQuaternion b, FixedPoint t)
    {
        // 公式：a + (b - a) * t
        FixedPoint oneMinusT = FixedPoint.One - t;
        return new FixedPointQuaternion(
            a.x * oneMinusT + b.x * t,
            a.y * oneMinusT + b.y * t,
            a.z * oneMinusT + b.z * t,
            a.w * oneMinusT + b.w * t
        );
    }

    /// <summary>
    /// 球形插值
    /// </summary>
    /// <param name="a">起点</param>
    /// <param name="b">终点</param>
    /// <param name="t">进度 0 - 1</param>
    /// <returns></returns>
    public static FixedPointQuaternion Slerp(FixedPointQuaternion a, FixedPointQuaternion b, FixedPoint t)
    {
        // 计算两个四元数的点积（夹角的余弦值）
        // dot = a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w
        long dotScaled = (long)(Int128.Multiply(a.x.ScaledValue, b.x.ScaledValue) +
                                     Int128.Multiply(a.y.ScaledValue, b.y.ScaledValue) +
                                     Int128.Multiply(a.z.ScaledValue, b.z.ScaledValue) +
                                     Int128.Multiply(a.w.ScaledValue, b.w.ScaledValue));

        //需要右移一次 因为上面是 a.x^16 * b.x^16 = x^32    x^32 + y^32 = (x+y)^32  右移一次即可 变成(x+y)^16 
        FixedPoint dot = FixedPoint.CreateByScaledValue(dotScaled >> FixedPoint.ShiftBits);

        // 避免绕远路 比如逆时针旋转10度 别变成了顺时针选择350度
        // 如果点乘为负，反转终点以确保走最短弧线
        FixedPointQuaternion end = b;
        if (dot < FixedPoint.Zero)
        {
            dot = -dot;
            end = new FixedPointQuaternion(-b.x, -b.y, -b.z, -b.w);
        }

        // 如果夹角极小，直接用 Lerp 以免除以 0
        if (dot > FixedPoint.CreateByScaledValue(65470)) // 约 0.999
        {
            return Lerp(a, end, t).Normalized;
        }

        // 1. 根据点乘结果（cosθ）反求出角度 theta
        int theta01 = FixedPointMath.Acos01(dot);

        // 2. 计算分母 sin(theta)
        long sinThetaScaled = FixedPointMath.Sin(theta01);
        if (sinThetaScaled == 0) return a; // 安全兜底

        FixedPoint sinThetaInv = FixedPoint.One / FixedPoint.CreateByScaledValue(sinThetaScaled);

        // 3. 计算 weightA = sin((1-t) * theta) / sin(theta)
        // t 是 0~1 的定点数，theta 是 0~1800 的整数
        int angleA = (int)((FixedPoint.One - t).ScaledValue * theta01 >> FixedPoint.ShiftBits);
        FixedPoint weightA = FixedPoint.CreateByScaledValue(FixedPointMath.Sin(angleA)) * sinThetaInv;

        // 4. 计算 weightB = sin(t * theta) / sin(theta)
        int angleB = (int)(t.ScaledValue * theta01 >> FixedPoint.ShiftBits);
        FixedPoint weightB = FixedPoint.CreateByScaledValue(FixedPointMath.Sin(angleB)) * sinThetaInv;

        // 5. 最终混合
        return new FixedPointQuaternion(
            a.x * weightA + end.x * weightB,
            a.y * weightA + end.y * weightB,
            a.z * weightA + end.z * weightB,
            a.w * weightA + end.w * weightB
        ).Normalized;
    }
}
