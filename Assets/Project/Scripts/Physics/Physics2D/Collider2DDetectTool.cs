
public static class Collider2DDetectTool
{

    //Box VS Box
    public static bool DetectCollider(Collider2DBox boxA, Collider2DBox boxB, bool needAdjustPos)
    {
        if (!boxA.Active || !boxB.Active)
            return false;

        //todo 考虑检测layer

        // 只要有一个轴没重叠，就是没撞
        if (boxA.x + boxA.HalfWidth < boxB.x - boxB.HalfWidth) return false; // A在B左边
        if (boxA.x - boxA.HalfWidth > boxB.x + boxB.HalfWidth) return false; // A在B右边
        if (boxA.y + boxA.HalfHeight < boxB.y - boxB.HalfHeight) return false; // A在B下边
        if (boxA.y - boxA.HalfHeight > boxB.y + boxB.HalfHeight) return false; // A在B上边

        if (!needAdjustPos) return true;

        FixedPointVector2 distance = boxB.LogicPos - boxA.LogicPos;
        FixedPoint absdisX = distance.x > FixedPoint.Zero ? distance.x : -distance.x;
        FixedPoint absdisY = distance.y > FixedPoint.Zero ? distance.y : -distance.y;

        FixedPoint overlapX = (boxA.HalfWidth + boxB.HalfWidth) - absdisX;
        FixedPoint overlapY = (boxA.HalfHeight + boxB.HalfHeight) - absdisY;

        //谁陷进去得浅，就往谁那边推
        if (overlapX < overlapY)
        {
            // 如果 distance.x > 0 (B在A右边)，A往左推(-overlapX)
            // 如果 distance.x < 0 (B在A左边)，A往右推(+overlapX)
            FixedPoint moveX = distance.x > FixedPoint.Zero ? -overlapX : overlapX;
            boxA.AdjustPos = new FixedPointVector2(boxA.x + moveX, boxA.y);
        }
        else
        {
            // 如果 distance.y > 0 (B在A上边)，A往下推(-overlapY)
            // 如果 distance.y < 0 (B在A下边)，A往上推(+overlapY)
            FixedPoint moveY = distance.y > FixedPoint.Zero ? -overlapY : overlapY;
            boxA.AdjustPos = new FixedPointVector2(boxA.x, boxA.y + moveY);
        }

        return true;
    }

    //Circle VS Box
    public static bool DetectCollider(Collider2DCircle circleA, Collider2DBox boxB, bool needAdjustPos)
    {
        if (!circleA.Active || !boxB.Active)
            return false;

        FixedPoint clampedX = FixedPointMath.Clamp(circleA.x, boxB.x - boxB.HalfWidth, boxB.x + boxB.HalfWidth);
        FixedPoint clampedY = FixedPointMath.Clamp(circleA.y, boxB.y - boxB.HalfHeight, boxB.y + boxB.HalfHeight);
        FixedPointVector2 closestPointV2 = new FixedPointVector2(clampedX, clampedY);//box上距离圆心最近的点
        FixedPointVector2 distanceV2 = circleA.LogicPos - closestPointV2;

        bool result = circleA.radius * circleA.radius >= distanceV2.SqrMagnitude();
        if (!needAdjustPos)
            return result;

        if (!result)
            return false;

        FixedPoint moveDistance = distanceV2.Magnitude();
        FixedPointVector2 pushDir = distanceV2 / moveDistance;//回拉的方向

        boxB.AdjustPos = boxB.LogicPos + pushDir * moveDistance;

        return true;
    }

    //Box Vs Circle
    //public static bool DetectCollider(Collider2DBox boxB, Collider2DCircle circleA)
    //{
    //    return DetectCollider(circleA, boxB);
    //}

    //Circle VS Circle
    public static bool DetectCollider(Collider2DCircle circleA, Collider2DCircle circleB, bool needAdjustPos)
    {
        if (!circleA.Active || !circleB.Active)
            return false;

        FixedPointVector2 distanceLogicPos = circleB.LogicPos - circleA.LogicPos;
        FixedPoint radiusSum = circleB.radius + circleA.radius;
        FixedPoint radiusSqr = radiusSum * radiusSum;
        FixedPoint distanceSqr = distanceLogicPos.SqrMagnitude();

        bool result = distanceSqr <= radiusSqr;
        if (!needAdjustPos)
            return result;

        if (!result)
            return false;

        FixedPoint distance;
        FixedPointVector2 pushDir;
        // 特殊情况处理：圆心完全重合 (distanceSqr == 0) 如果不处理，下面除以 distance 会报错 (DivideByZero)
        if (distanceSqr <= FixedPoint.Zero)
        {
            // 两个圆心重合了，随便给个方向推开 (比如向右)
            distance = FixedPoint.Zero;
            pushDir = new FixedPointVector2(FixedPoint.One, FixedPoint.Zero);
        }
        else
        {
            distance = FixedPointMath.Sqrt(distanceSqr);//不得不开方了
            pushDir = distanceLogicPos / distance;//已经开方了 就别再用Normalize 避免内部再次开方 直接除以距离 一样的
        }

        FixedPoint overlap = radiusSum - distance;//陷入深度 半径之和 减去 圆心之间的距离
        circleA.AdjustPos = circleA.LogicPos - (pushDir * overlap);

        return result;
    }
}
