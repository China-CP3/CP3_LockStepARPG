using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsMgr2D
{
    public bool DetectCollider(Collider2DBox boxA, Collider2DBox boxB, bool canEnterBlock)
    {
        if(!boxA.Active || !boxB.Active)
        return false;

        //todo ¿¼ÂÇ¼ì²âlayer

        bool xIsOver = boxA.X + boxA.HalfWidth >= boxB.X - boxB.HalfWidth && boxA.X - boxA.HalfWidth <= boxB.X + boxB.HalfWidth;
        bool yIsOver = boxA.Y + boxA.HalfHeight >= boxB.Y - boxB.HalfHeight && boxA.Y - boxA.HalfHeight <= boxB.Y + boxB.HalfHeight;

        return xIsOver && yIsOver;
    }

    public bool DetectCollider(Collider2DCircle circleA, Collider2DBox boxB)
    {
        return false;
    }

    public bool DetectCollider(Collider2DCircle circleA, Collider2DCircle circleB)
    {
        return false;
    }
}
