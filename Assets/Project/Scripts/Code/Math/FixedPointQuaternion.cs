
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
    // 约定俗成：在 Unity 和大多数物理引擎中，乘法是从右往左生效的。即 A * B 是先做 B，再做 A。
    public static FixedPointQuaternion operator *(FixedPointQuaternion lhs, FixedPointQuaternion rhs)
    {

        return new FixedPointQuaternion();
    }

}
