using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider2DBase
{
    /// <summary>
    /// 激活状态(比如怪物死亡时，状态设置false，false：表示当前碰撞体无效，不需要进行碰撞检测)
    /// </summary>
    public bool Active { get; private set; }

    public FixedPointVector2 CenterPos { get; private set; }//为啥又有中心位置 又有逻辑位置 其实是为了做偏移
    public FixedPointVector2 LogicPos { get; private set; }
    public FixedPoint X => LogicPos.x;
    public FixedPoint Y => LogicPos.y;

    public Collider2DBase(FixedPointVector2 CenterPos, FixedPointVector2 LogicPos)
    {
        Active = true;  
        this.CenterPos = CenterPos;
        this.LogicPos = LogicPos;

    }

    public void UpdateLogicPos(FixedPointVector2 newPos)
    {
        LogicPos = newPos;
    }
}
