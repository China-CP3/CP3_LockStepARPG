using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
                instance.Init();
            }
            return instance;
        }
    }

    private Func<Collider2DBase, Collider2DBase, bool, bool>[,] detectFunc;

    //场景中所有碰撞器
    private List<Collider2DBase> collider2DList = new List<Collider2DBase>();

    private void Init()
    {
        int maxDetectCount = (int)Collider2DEnum.Max;
        detectFunc = new Func<Collider2DBase, Collider2DBase, bool, bool>[maxDetectCount, maxDetectCount];

        Register(Collider2DEnum.Box, Collider2DEnum.Box, (a, b, c) => { return Collider2DDetectTool.DetectCollider((Collider2DBox)a, (Collider2DBox)b, c); });
        Register(Collider2DEnum.Circle, Collider2DEnum.Box, (a, b, c) => { return Collider2DDetectTool.DetectCollider((Collider2DCircle)a, (Collider2DBox)b, c); });
        Register(Collider2DEnum.Circle, Collider2DEnum.Circle, (a, b, c) => { return Collider2DDetectTool.DetectCollider((Collider2DCircle)a, (Collider2DCircle)b, c); });

    }

    private void Register(Collider2DEnum a, Collider2DEnum b, Func<Collider2DBase, Collider2DBase, bool, bool> func)
    {
        detectFunc[(int)a, (int)b] = func;

        if(a != b)
        {
            detectFunc[(int)b, (int)a] = (c1, c2, result)=> { return func(c2, c1, result); };
        }
    }

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
