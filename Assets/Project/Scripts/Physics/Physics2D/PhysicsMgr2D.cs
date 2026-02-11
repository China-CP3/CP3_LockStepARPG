using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsMgr2D
{
    private static PhysicsMgr2D instance;
    public static PhysicsMgr2D Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PhysicsMgr2D();
            }
            return instance;
        }
    }

    private List<Collider2DBase> collider2DList = new List<Collider2DBase>();
    
    public void LogicUpdate()
    {

    }

    public bool DetectCollider(Collider2DBox boxA, Collider2DBox boxB,bool canEnterBlock)
    {
        return false;
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
