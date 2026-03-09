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
        base.UpdateLogicRadius(radius);
        this.radius = radius;
    }

    public override Collider2DBox GenerateSweptAABB(FixedPointVector2 lastFramePos, FixedPointVector2 curFramePos)
    {
        //依然返回一个box出去 不是圆 不然用椭圆和box做扫描检测 复杂度增加 性能不好 完全可以接受用这一点误差换开发进度和性能


        return null;
    }
}
