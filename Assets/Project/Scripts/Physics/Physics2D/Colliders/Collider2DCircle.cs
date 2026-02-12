using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider2DCircle : Collider2DBase
{
    public FixedPoint radius;
    public Collider2DCircle(FixedPoint radius,FixedPointVector2 CenterPos, FixedPointVector2 LogicPos) :base(CenterPos, LogicPos)
    {
        this.radius = radius;
        Collider2DType = Collider2DEnum.Circle;
    }

    public override void UpdateLogicRadius(FixedPoint radius)
    {
        this.radius = radius;
    }
}
