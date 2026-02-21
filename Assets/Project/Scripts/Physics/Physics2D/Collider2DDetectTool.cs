using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
public static class Collider2DDetectTool
{

    //Box”ÎBox≈ˆ◊≤ºÏ≤‚
    public static bool DetectCollider(Collider2DBox boxA, Collider2DBox boxB, bool canEnterBlock)
    {
        if(!boxA.Active || !boxB.Active)
        return false;

        //todo øº¬«ºÏ≤‚layer

        bool xIsOver = boxA.x + boxA.HalfWidth >= boxB.x - boxB.HalfWidth && boxA.x - boxA.HalfWidth <= boxB.x + boxB.HalfWidth;
        bool yIsOver = boxA.y + boxA.HalfHeight >= boxB.y - boxB.HalfHeight && boxA.y - boxA.HalfHeight <= boxB.y + boxB.HalfHeight;

        return xIsOver && yIsOver;
    }

    public static bool DetectCollider(Collider2DCircle circleA, Collider2DBox boxB)
    {
        if (!circleA.Active || !boxB.Active)
            return false;

        FixedPoint clampedX = FixedPointMath.Clamp(circleA.x, boxB.x - boxB.HalfWidth, boxB.x + boxB.HalfWidth);
        FixedPoint clampedY = FixedPointMath.Clamp(circleA.y, boxB.y - boxB.HalfHeight, boxB.y + boxB.HalfHeight);
        FixedPointVector2 closestPoint = new FixedPointVector2(clampedX, clampedY);//box…œæ‡¿Î‘≤–ƒ◊ÓΩ¸µƒµ„
        FixedPointVector2 dir = circleA.LogicPos - closestPoint;

        return circleA.radius * circleA.radius >= dir.SqrMagnitude();
    }

    //Circle”ÎBox ≈ˆ◊≤ºÏ≤‚
    public static bool DetectCollider(Collider2DCircle circleA, Collider2DCircle circleB)
    {
        if (!circleA.Active || !circleB.Active)
            return false;

        FixedPointVector2 distance = circleB .LogicPos - circleA .LogicPos;
        FixedPoint radiusSum = circleB.radius + circleA.radius;
        return distance.SqrMagnitude() <= radiusSum * radiusSum;
    }
}
