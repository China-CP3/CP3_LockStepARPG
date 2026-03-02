using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    private PhysicsMgr2D() { }

    private int maxDetectCount;//碰撞器的数量 Collider2DEnum.Max
    private Func<Collider2DBase, Collider2DBase, bool, bool>[,] detectFunc;

    //场景中所有碰撞器
    private List<Collider2DBase> collider2DList = new List<Collider2DBase>();
    //待删除列表
    private List<Collider2DBase> toRemoveList = new List<Collider2DBase>();

    private void Init()
    {
        maxDetectCount = (int)Collider2DEnum.Max;
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
        #region 只处理碰撞事件系统 比如A进入了B的范围 或者退出了 不处理位移拉回 重叠后拉回 是各个对象自己移动时处理
        for (int i = 0; i < collider2DList.Count - 1; i++)
        {
            Collider2DBase colliderA = collider2DList[i];
            if (!CheckCooliderCondition(colliderA)) continue;

            for (int j = i + 1; j < collider2DList.Count; j++)
            {
                Collider2DBase colliderB = collider2DList[j];
                if (!CheckCooliderCondition(colliderB) || !CheckCooliderCondition(colliderA, colliderB)) continue;

                Func<Collider2DBase, Collider2DBase, bool, bool> func = detectFunc[(int)colliderA.Collider2DType, (int)colliderB.Collider2DType];
                if (func != null)
                {
                    bool isColliding = func.Invoke(colliderA, colliderB, false);//涉及重叠拉回 只判断是否碰撞了 然后处理进入和退出等碰撞事件
                    if (isColliding)
                    {
                        //让A和B各自记录 已经撞上了对方
                        colliderA.AddCollisionToCurrentFrameList(colliderB);
                        colliderB.AddCollisionToCurrentFrameList(colliderA);
                    }
                }
            }
        }

        //通知所有碰撞器 更新自己的状态 判断这一帧和上一帧已碰撞的列表 进行对比 
        for (int i = 0; i < collider2DList.Count; i++)
        {
            Collider2DBase colliderA = collider2DList[i];
            if (!CheckCooliderCondition(colliderA)) continue;
            colliderA.UpdateCollisionState();
        }

        ClearRemoveList();
        #endregion


    }

    public void AddCollider2D(Collider2DBase collider2D)
    {
        collider2DList.Add(collider2D);
    }

    public void AddToRemoveList(Collider2DBase collider2D)
    {
        if(collider2DList.Contains(collider2D) && !toRemoveList.Contains(collider2D))
        {
            toRemoveList.Add(collider2D);   
        }
    }

    private void ClearRemoveList()
    {
        if (toRemoveList.Count == 0) return;

        for (int i = 0; i < toRemoveList.Count; i++)
        {
            collider2DList.Remove(toRemoveList[i]);
        }

        toRemoveList.Clear();
    }

    private bool CheckCooliderCondition(Collider2DBase aCollider, Collider2DBase bCollider)
    {
        return aCollider.Active && bCollider.Active;

        //todo 未来考虑加入layer或者其他条件 暂时先这样
    }

    private bool CheckCooliderCondition(Collider2DBase collider)
    {
        return collider.Active;

        //todo 未来考虑加入layer或者其他条件 暂时先这样
    }
}
