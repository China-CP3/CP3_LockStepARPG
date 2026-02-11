using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsMgr2D
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

    public void AddCollider2D(Collider2DBase collider2D)
    {
        collider2DList.Add(collider2D);
    }

    public void RemoveCollider2D(Collider2DBase collider2D)
    {
        collider2DList.Remove(collider2D);
    }
}
