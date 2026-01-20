
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
        FixedPointQuaternion qX = AngleAxis(x01, FixedPointVector3.Right); // (1, 0, 0)
        FixedPointQuaternion qY = AngleAxis(y01, FixedPointVector3.Up);    // (0, 1, 0)
        FixedPointQuaternion qZ = AngleAxis(z01, FixedPointVector3.Forward); // (0, 0, 1)

        return qY * qX * qZ;
    }


}
