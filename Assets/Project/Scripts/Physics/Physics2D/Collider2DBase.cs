using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider2DBase
{
    /// <summary>
    /// 激活状态(比如怪物死亡时，状态设置false，false：表示当前碰撞体无效，不需要进行碰撞检测)
    /// </summary>
    public bool Active { get; private set; }


}
