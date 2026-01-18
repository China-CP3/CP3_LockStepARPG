
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

    // 四元数乘法：用于合并旋转 
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="angle01">0.1 度为单位的整数</param>
    /// <param name="axis">绕某条轴旋转</param>
    /// <returns></returns>
    public static FixedPointQuaternion AngleAxis(int angle01, FixedPointVector3 axis)
    {
        //取半角（这是四元数强制要求的数学结构）
        int halfAngle = angle01 / 2;//注意!传入的是 0.1 度为单位的整数

        FixedPoint s = FixedPoint.CreateByScaledValue(FixedPointMath.Sin(halfAngle));//xyz分别乘以sin(angle/2) 得到新的xyz 表示绕某条轴旋转
        FixedPoint c = FixedPoint.CreateByScaledValue(FixedPointMath.Cos(halfAngle));//cos(angle/2) 表示旋转的角度

        //旋转轴必须是单位向量（归一化）
        FixedPointVector3 normAxis = axis.normalized;

        return new FixedPointQuaternion(
            normAxis.x * s,
            normAxis.y * s,
            normAxis.z * s,
            c
        );
    }
}
