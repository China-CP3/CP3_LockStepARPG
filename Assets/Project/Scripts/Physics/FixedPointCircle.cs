
using UnityEngine;

public readonly struct FixedPointCircle
{
    public readonly FixedPointVector3 center;
    public readonly FixedPoint radius;

    public FixedPointCircle(FixedPointVector3 center, FixedPoint radius)
    {
        this.center = new FixedPointVector3(center.x, FixedPoint.Zero, center.z);
        this.radius = radius;
    }

    public override string ToString()
    {
        return $"Circle(center={center}, radius={radius})";
    }
}
