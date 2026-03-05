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

    public override Collider2DBox GenerateSweptAABB(FixedPointVector2 lastFramePos, FixedPointVector2 curFramePos)
    {
        //根据2个box 求一个大的能包裹2个box的大box 

        //求上帧位置和当前帧位置里 最小的中心点 和最大的中心点
        FixedPoint minCenterX = FixedPointMath.Min(lastFramePos.x, curFramePos.x);
        FixedPoint minCenterY = FixedPointMath.Min(lastFramePos.y, curFramePos.y);
        FixedPoint maxCenterX = FixedPointMath.Max(lastFramePos.x, curFramePos.x);
        FixedPoint maxCenterY = FixedPointMath.Max(lastFramePos.y, curFramePos.y);

        //计算新的box上 最左边的点 最右边的点 最高的点 最低的点 或者说box的四条边
        FixedPoint newBoxMinX = minCenterX - HalfWidth;
        FixedPoint newBoxMaxX = maxCenterX + HalfWidth;
        FixedPoint newBoxMinY = minCenterY - HalfHeight;
        FixedPoint newBoxMaxY = maxCenterY + HalfHeight;

        FixedPoint newWidth = newBoxMaxX - newBoxMinX;
        FixedPoint newHight = newBoxMaxY - newBoxMinY;
        FixedPoint newCenterX = (newBoxMaxX + newBoxMinX) / FixedPoint.CreateByInt(2);//不是宽度直接除以2 是左右坐标相加除以2 比如 -2 + 4 的一半 = 1  按宽带除以2就是3了
        FixedPoint newCenterY = (newBoxMinY + newBoxMaxY) / FixedPoint.CreateByInt(2);//不是宽度直接除以2 是左右坐标相加除以2 比如 -2 + 4 的一半 = 1  按宽带除以2就是3了

        FixedPointVector2 newCenter = new FixedPointVector2(newCenterX, newCenterY);

        return new Collider2DBox(newCenter, newCenter,new FixedPointVector2(newWidth, newHight));
    }
}
