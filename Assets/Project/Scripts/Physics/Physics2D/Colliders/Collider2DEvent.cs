using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Collider2DBase
{
    protected bool mIsCollider = false;
    protected HashSet<Collider2DBase> CurrentFrameSet = new HashSet<Collider2DBase>();//当前帧已碰撞列表
    protected HashSet<Collider2DBase> PreviousFrameSet = new HashSet<Collider2DBase>();//上一帧已碰撞列表

    public event Action<Collider2DBase> OnEnterAction2D;
    public event Action<Collider2DBase> OnStayAction2D;
    public event Action<Collider2DBase> OnExitAction2D;

    public void AddCollisionToCurrentFrameList(Collider2DBase target)
    {
        CurrentFrameSet.Add(target);
    }

    public void UpdateCollisionState()
    {
        foreach (var curCollider in CurrentFrameSet)
        {
            if(!PreviousFrameSet.Contains(curCollider))
            {
                //属于是enter
                OnEnterAction2D?.Invoke(curCollider);
                OnEnterCollider(curCollider);
            }
        }

        foreach (var preCollider in PreviousFrameSet)
        {
            if (CurrentFrameSet.Contains(preCollider))
            {
                //属于是stay
                OnStayAction2D?.Invoke(preCollider);
                OnStayCollider(preCollider);
            }
            else
            {
                //属于是exit
                OnExitAction2D?.Invoke(preCollider);
                OnExitCollider(preCollider);
            }
        }

        HashSet<Collider2DBase> temp = PreviousFrameSet;
        PreviousFrameSet = CurrentFrameSet;
        CurrentFrameSet = temp;
        CurrentFrameSet.Clear();
    }

    public virtual void OnEnterCollider(Collider2DBase target)
    {

    }

    public virtual void OnStayCollider(Collider2DBase target)
    {

    }

    public virtual void OnExitCollider(Collider2DBase target)
    {

    }


}
