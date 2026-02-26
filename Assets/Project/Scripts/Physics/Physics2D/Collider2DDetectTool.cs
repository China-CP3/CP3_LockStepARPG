
public static class Collider2DDetectTool
{

    //Box VS Box
    public static bool DetectCollider(Collider2DBox boxA, Collider2DBox boxB, bool canEnterBlock)
    {
        if(!boxA.Active || !boxB.Active)
        return false;

        //todo 考虑检测layer

        // 只要有一个轴没重叠，就是没撞
        if (boxA.x + boxA.HalfWidth < boxB.x - boxB.HalfWidth) return false; // A在B左边
        if (boxA.x - boxA.HalfWidth > boxB.x + boxB.HalfWidth) return false; // A在B右边
        if (boxA.y + boxA.HalfHeight < boxB.y - boxB.HalfHeight) return false; // A在B下边
        if (boxA.y - boxA.HalfHeight > boxB.y + boxB.HalfHeight) return false; // A在B上边

        return true;
    }

    //Circle VS Box
    public static bool DetectCollider(Collider2DCircle circleA, Collider2DBox boxB, bool canEnterBlock)
    {
        if (!circleA.Active || !boxB.Active)
            return false;

        FixedPoint clampedX = FixedPointMath.Clamp(circleA.x, boxB.x - boxB.HalfWidth, boxB.x + boxB.HalfWidth);
        FixedPoint clampedY = FixedPointMath.Clamp(circleA.y, boxB.y - boxB.HalfHeight, boxB.y + boxB.HalfHeight);
        FixedPointVector2 closestPoint = new FixedPointVector2(clampedX, clampedY);//box上距离圆心最近的点
        FixedPointVector2 dir = circleA.LogicPos - closestPoint;

        return circleA.radius * circleA.radius >= dir.SqrMagnitude();
    }

    //Box Vs Circle
    //public static bool DetectCollider(Collider2DBox boxB, Collider2DCircle circleA)
    //{
    //    return DetectCollider(circleA, boxB);
    //}

    //Circle VS Circle
    public static bool DetectCollider(Collider2DCircle circleA, Collider2DCircle circleB, bool canEnterBlock)
    {
        if (!circleA.Active || !circleB.Active)
            return false;

        FixedPointVector2 distance = circleB .LogicPos - circleA .LogicPos;
        FixedPoint radiusSum = circleB.radius + circleA.radius;
        return distance.SqrMagnitude() <= radiusSum * radiusSum;
    }
}
