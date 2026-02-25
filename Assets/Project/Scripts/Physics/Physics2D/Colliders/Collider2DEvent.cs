using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider2DEvent
{
    protected bool mIsCollider = false;
    protected List<Collider2DBase> colliderList = new List<Collider2DBase>();
    private Action<Collider2DBase> OnEnterAction2D;
    private Action<Collider2DBase> OnStayAction2D;
    private Action<Collider2DBase> OnExitAction2D;

    public void OnEnterCollider()
    {

    }

    public void OnStayCollider()
    {

    }

    public void OnExitCollider()
    {

    }
}
