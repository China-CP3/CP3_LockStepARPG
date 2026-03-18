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
        //把半径看成正方形的宽高

        // 求上帧位置和当前帧位置里 最小的中心点 和最大的中心点
        FixedPoint minCenterX = FixedPointMath.Min(lastFramePos.x, curFramePos.x);
        FixedPoint minCenterY = FixedPointMath.Min(lastFramePos.y, curFramePos.y);
        FixedPoint maxCenterX = FixedPointMath.Max(lastFramePos.x, curFramePos.x);
        FixedPoint maxCenterY = FixedPointMath.Max(lastFramePos.y, curFramePos.y);

        // 计算新的Box边界
        FixedPoint newBoxMinX = minCenterX - radius;// Box的最左边 = 最小中心X - 半径
        FixedPoint newBoxMaxX = maxCenterX + radius;// Box的最右边 = 最大中心X + 半径
        FixedPoint newBoxMinY = minCenterY - radius;// Box的最低点 = 最小中心Y - 半径
        FixedPoint newBoxMaxY = maxCenterY + radius;// Box的最高点 = 最大中心Y + 半径

        // 计算这个大Box的宽高
        FixedPoint newWidth = newBoxMaxX - newBoxMinX;
        FixedPoint newHeight = newBoxMaxY - newBoxMinY;

        //中心点
        FixedPoint two = FixedPoint.CreateByInt(2);
        FixedPoint newCenterX = (newBoxMaxX + newBoxMinX) / two;
        FixedPoint newCenterY = (newBoxMinY + newBoxMaxY) / two;

        FixedPointVector2 newCenter = new FixedPointVector2(newCenterX, newCenterY);

        // 返回一个新的 Collider2DBox
        // 注意：这里生成的 Box 是为了做宽相检测用的，所以 LogicPos 和 CenterPos 设为一样即可
        return new Collider2DBox(newCenter, newCenter, new FixedPointVector2(newWidth, newHeight));
    }
}
