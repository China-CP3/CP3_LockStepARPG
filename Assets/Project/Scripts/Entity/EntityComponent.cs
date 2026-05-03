using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityComponent
{
    public Entity Owner { get; private set; }

    /// <summary>
    /// 被从 Entity 移除 / Entity 销毁时调用（生命周期结束）
    /// 子类应在此释放外部资源，如 Collider、GameObject 等
    /// </summary>
    public virtual void OnRemoveComponent()
    {

        SetOwner(null);
    }

    /// <summary>
    /// 被挂载到 Entity 时调用（生命周期开始）
    /// </summary>
    public virtual void OnAddComponent()
    {
        


    }

    public void SetOwner(Entity owner)
    {
        Owner = owner;
    }
}
