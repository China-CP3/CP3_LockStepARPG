using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Collider2DBase
{
    /// <summary>
    /// 激活状态(比如怪物死亡时，状态设置false，false：表示当前碰撞体无效，不需要进行碰撞检测)
    /// </summary>
    public bool Active { get; private set; }
    /// <summary>
    /// 当前碰撞体渲染对象
    /// </summary>
    public GameObject RenderObj { get; private set; }
    public FixedPointVector2 CenterPos { get; protected set; }//又有中心位置 又有逻辑位置 其实是为了做偏移
    public FixedPointVector2 LogicPos { get; protected set; }
    public FixedPoint X => LogicPos.x;
    public FixedPoint Y => LogicPos.y;
    public Collider2DEnum Collider2DType { get; protected set; }

    public Collider2DBase(FixedPointVector2 CenterPos, FixedPointVector2 LogicPos)
    {
        Active = true;  
        this.CenterPos = CenterPos;
        this.LogicPos = LogicPos;

    }

    public void SetRenderObj(GameObject renderObj)
    {
        this.RenderObj = renderObj;
    }

    public virtual void UpdateLogicPos(FixedPointVector2 newLogicPos)
    {
        LogicPos = newLogicPos;
    }

    public virtual void UpdateLogicSize(FixedPointVector2 logicSize) { }

    public virtual void UpdateLogicRadius(FixedPoint radius) { }

    public virtual void OnRelease()
    {
        this.Active = false;
        PhysicsMgr2D.Instance.RemoveCollider2D(this);
    }
}
