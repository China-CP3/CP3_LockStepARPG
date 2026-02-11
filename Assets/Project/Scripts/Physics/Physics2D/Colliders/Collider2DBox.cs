using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider2DBox : Collider2DBase
{
    public FixedPointVector2 Size { get; private set; }
    public FixedPoint HalfWidth { get; private set; }
    public FixedPoint HalfHeight { get; private set; }

    public Collider2DBox(FixedPointVector2 CenterPos, FixedPointVector2 LogicPos, FixedPointVector2 Size) :base(CenterPos, LogicPos)
    {
        this.Size = Size;
        HalfWidth = Size.x / FixedPoint.CreateByInt(2);
        HalfHeight = Size.y / FixedPoint.CreateByInt(2);
    }


}
