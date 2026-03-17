
using UnityEngine.UIElements;

public static class Collider2DDetectTool
{
    //当碰撞器重叠时 总是第一个参数被拉回 谁移动谁被拉回
    //现在碰撞器种类少 可以这么写 如果多 最好是传参数 决定谁拉回 不然两两组合要写很多种 很麻烦

    #region AABB
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

        //哪边陷进去得浅，就往哪边推
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
        FixedPointVector2 pushDir;
        if (moveDistance <= FixedPoint.Zero)
        {
            // 特殊情况处理：中心点完全重合 如果不处理，下面除以 moveDistance 会报错
            pushDir = new FixedPointVector2(FixedPoint.One, FixedPoint.Zero);
        }
        else
        {
            pushDir = distanceV2 / moveDistance;//回拉的方向
        }

        FixedPoint overlap = circleA.radius - moveDistance;//陷入的距离 或者说重叠后需要回拉的距离

        circleA.AdjustPos = circleA.LogicPos + pushDir * overlap;

        return true;
    }

    //Box VS Circle
    public static bool DetectCollider(Collider2DBox boxB, Collider2DCircle circleA, bool needAdjustPos)
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
        FixedPointVector2 pushDir;
        // 特殊情况处理：圆心完全重合 (moveDistance == 0) 如果不处理，下面除以 moveDistance 会报错 (DivideByZero)
        if (moveDistance <= FixedPoint.Zero)
        {
            pushDir = new FixedPointVector2(FixedPoint.One, FixedPoint.Zero);
        }
        else
        {
            pushDir = distanceV2 / moveDistance;//回拉的方向
        }

        FixedPoint overlap = circleA.radius - moveDistance;//陷入的距离 或者说重叠后需要回拉的距离
        boxB.AdjustPos = boxB.LogicPos - pushDir * overlap;

        return true;
    }

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
    #endregion

    #region Swept AABB (扫描包围盒)
    //根据上一帧的位置和当前帧位置 以及自身体积  生成一个大的覆盖整个路径的BOX碰撞器
    //不管传入的是圆还是box 最终都生成box 不然生成胶囊体的话 性能没那么好 计算起来也复杂 长方型只在四个角比胶囊体多一丁点区域
    //这一点区域带来的误检测 是完全可以接受的 换来的是性能和开发进度
    private static Collider2DBox GetSweptCollier(Collider2DBase collider, FixedPointVector2 lastFramePos, FixedPointVector2 curFramePos)
    {
        return collider.GenerateSweptAABB(lastFramePos, curFramePos);
    }

    public static bool DetectCollider(Collider2DBase colliderA, Collider2DBase colliderB, FixedPointVector2 colliderALastFramePos, FixedPointVector2 colliderACurFramePos)
    {
        Collider2DBox sweptBox = GetSweptCollier(colliderA, colliderALastFramePos, colliderACurFramePos);

        if(colliderB is Collider2DBox)
        {
            return DetectCollider(sweptBox, colliderB as Collider2DBox, false);
        }
        else if (colliderB is Collider2DCircle)
        {
            return DetectCollider(sweptBox, colliderB as Collider2DCircle, false);
        }

        return false;
    }

    //todo 再写一个函数DetectCollider 一个只检测swept  另一个swpet+射线检测
    #endregion

    #region 射线检测AABB
    //todo 用射线作为第二道检测 弥补sweptbox的误判 
    public static bool RayCastBox(Collider2DBox targetBox, FixedPointVector2 startPos, FixedPointVector2 direction, FixedPoint distance)
    {
        if(!targetBox.Active)
        {
            return false;
        }

        FixedPoint minX = targetBox.x - targetBox.HalfWidth;//targetBox最左的X
        FixedPoint maxX = targetBox.x + targetBox.HalfWidth;//targetBox最右的X
        FixedPoint minY = targetBox.y - targetBox.HalfHeight;//targetBox最左的Y
        FixedPoint maxY = targetBox.y + targetBox.HalfHeight;//targetBox最右的Y

        //判断进入时 后进入的点为准 退出时 先退出的点为准  退出的点 大于进入的点 即可判断为碰撞
        //要求的是 左边那条边 和右边那条边 谁是进入点 谁是退出点 距离短的就是进入点 上下2边同理
        //时间 = 距离/速度 向量的x 就是在x轴上的速度  向量的y就是在y轴上的速度

        FixedPoint xEnterTime;
        FixedPoint xExitTime;
        if (direction.x == FixedPoint.Zero)
        {
            if (startPos.x < minX || startPos.x > maxX) return false;

            xEnterTime = FixedPoint.MinValue;
            xExitTime = FixedPoint.MaxValue;
        }
        else
        {
            FixedPoint xTimeA = (minX - startPos.x) / direction.x;
            FixedPoint xTimeB = (maxX - startPos.x) / direction.x;
            xEnterTime = FixedPointMath.Min(xTimeA, xTimeB);
            xExitTime = FixedPointMath.Max(xTimeA, xTimeB);
        }

        FixedPoint yEnterTime;
        FixedPoint yExitTime;
        if (direction.y == FixedPoint.Zero)
        {
            if (startPos.y < minY || startPos.y > maxY) return false;

            yEnterTime = FixedPoint.MinValue;
            yExitTime = FixedPoint.MaxValue;
        }
        else
        {
            FixedPoint yTimeA = (minY - startPos.y) / direction.y;
            FixedPoint yTimeB = (maxY - startPos.y) / direction.y;
            yEnterTime = FixedPointMath.Min(yTimeA, yTimeB);
            yExitTime = FixedPointMath.Max(yTimeA, yTimeB);
        }

        FixedPoint finalEnterTime = FixedPointMath.Max(xEnterTime, yEnterTime);
        FixedPoint finalExitTime = FixedPointMath.Min(xExitTime, yExitTime);

        bool hasIntersection = finalEnterTime <= finalExitTime;//是否真的有交集 (进入时间 <= 离开时间) 判断进入时 后进入的点为准 退出时 先退出的点为准  退出的点 大于进入的点 即可判断为碰撞
        bool isInFront = finalExitTime >= FixedPoint.Zero;//目标是否在前方 (离开时间 >= 0，防止打中背后的东西) 盒子不在射线屁股后面
        bool isWithinRange = finalEnterTime <= distance;//距离限制 是否在射程范围内 (进入时间 <= 射程)
        return hasIntersection && isInFront && isWithinRange;
    }

    #endregion
}
