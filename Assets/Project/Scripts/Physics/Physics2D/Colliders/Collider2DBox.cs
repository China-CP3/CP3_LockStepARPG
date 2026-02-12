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
        UpdateLogicSize(Size);
        this.Collider2DType = Collider2DEnum.Box;
    }

    public override void UpdateLogicSize(FixedPointVector2 logicSize)
    {
        base.UpdateLogicSize(logicSize);
        this.Size = logicSize;
        HalfWidth = Size.x / FixedPoint.CreateByInt(2);
        HalfHeight = Size.y / FixedPoint.CreateByInt(2);
    }

    public bool DetectCollider(Collider2DBase targetCollider)
    {
        bool isCollider = false;
        switch (targetCollider.Collider2DType)
        {
            case Collider2DEnum.Box:
                isCollider = Collider2DDetectTool.DetectCollider(this, targetCollider as Collider2DBox, false);
                break;
            case Collider2DEnum.Circle:
                isCollider = Collider2DDetectTool.DetectCollider(targetCollider as Collider2DCircle, this);
                break;
        }
        //return base.DetectCollider(targetCollider);
        return isCollider;
    }
}
