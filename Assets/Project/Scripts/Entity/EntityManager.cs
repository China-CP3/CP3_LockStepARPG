using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager
{
    // 自增 Id 计数器（从 1 开始，0 留给"无效 Id"）
    private int nextId = 1;

    public int EntityCount { get { return entitiesDic.Count; }}

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

    public void DestroyEntity(Entity deleteEntity)
    {
        if (deleteEntity == null) return;
        DestroyEntity(deleteEntity.Id);
    }

    //为什么不用list而是这个接口呢  因为这样外部只能foreach访问 不能add remove改动字典
    public IEnumerable<Entity> GetAllEntities()
    {
        return entitiesDic.Values;
    }

    public List<Entity> GetEntityListWithComponent<T>() where T : EntityComponent
    {
        List<Entity> result = new List<Entity>();
        foreach (var entity in entitiesDic.Values)
        {
            if (entity.HasComponent<T>())
            {
                result.Add(entity);
            }
        }
        return result;
    }

    public void Clear()
    {
        foreach (var entity in entitiesDic.Values)
        {
            entity.Destroy();
        }
        entitiesDic.Clear();
        nextId = 1;
    }
}
