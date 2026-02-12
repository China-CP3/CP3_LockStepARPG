using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class Collider2DDetectTool
{
    //public virtual bool DetectCollider(Collider2DBase targetCollider)
    //{

    //}

    public static bool DetectCollider(Collider2DBox boxA, Collider2DBox boxB, bool canEnterBlock)
    {
        if(!boxA.Active || !boxB.Active)
        return false;

        //todo ¿¼ÂÇ¼ì²âlayer

        bool xIsOver = boxA.X + boxA.HalfWidth >= boxB.X - boxB.HalfWidth && boxA.X - boxA.HalfWidth <= boxB.X + boxB.HalfWidth;
        bool yIsOver = boxA.Y + boxA.HalfHeight >= boxB.Y - boxB.HalfHeight && boxA.Y - boxA.HalfHeight <= boxB.Y + boxB.HalfHeight;

        return xIsOver && yIsOver;
    }

    public static bool DetectCollider(Collider2DCircle circleA, Collider2DBox boxB)
    {
        return false;
    }

    public static bool DetectCollider(Collider2DCircle circleA, Collider2DCircle circleB)
    {
        if (!circleA.Active || !circleB.Active)
            return false;

        FixedPointVector2 distance = circleB .LogicPos - circleA .LogicPos;
        FixedPoint radiusSum = circleB.radius + circleA.radius;
        if (distance.SqrMagnitude() <= radiusSum * radiusSum )
        {
            return true;
        }
        return false;
    }
}
