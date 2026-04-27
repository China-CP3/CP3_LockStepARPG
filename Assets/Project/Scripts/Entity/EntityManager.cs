using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager
{
    // 自增 Id 计数器（从 1 开始，0 留给"无效 Id"）
    private int nextId = 1;

    // 存储所有 Entity 的字典（key = Id，value = Entity）
    private Dictionary<int, Entity> entitiesDic = new Dictionary<int, Entity>();

    public Entity CreateEntity()
    {
        Entity entity = new Entity(nextId);
        entitiesDic.Add(nextId, entity);
        nextId++;
        return entity;
    }

    public Entity GetEntity(int id)
    {
        if (entitiesDic.TryGetValue(id, out var entity))
        {
            return entity;
        }
        return null;
    }

    public void DestroyEntity(int id)
    {
        if (entitiesDic.TryGetValue(id, out var entity))
        {
            entity.Destroy();           // 先让 Entity 自己清理组件
            entitiesDic.Remove(id);     // 再从字典移除
        }
    }
}
