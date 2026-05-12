using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class EntityManager
{
    // 自增 Id 计数器（从 1 开始，0 留给"无效 Id"）
    private int nextId = 1;

    public int EntityCount { get { return entitiesDic.Count; } }

    // 存储所有 Entity 的字典（key = Id，value = Entity）
    private Dictionary<int, Entity> entitiesDic = new Dictionary<int, Entity>();

    private List<Entity> pendingAddList = new List<Entity>();//延迟增加列表
    private List<Entity> pendingRemoveList = new List<Entity>();//延迟删除列表

    public Entity CreateEntity()
    {
        Entity entity = new Entity(nextId);
        pendingAddList.Add(entity);
        nextId++;
        return entity;

        /*  Entity 是 new 出来的对象，存在 ≠ 在字典里

            创建后立刻做的事（加组件、读 Id）操作的是对象自己，跟字典无关

            别人通过 Id 反查的需求通常发生在下一帧，那时候待办列表已经清空、Entity 已经在字典里*/
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
            pendingRemoveList.Add(entity);
        }
        else
        {
            return;
        }
        
        //if (entitiesDic.TryGetValue(id, out var entity))
        //{
        //    entity.Destroy();           // 先让 Entity 自己清理组件
        //    entitiesDic.Remove(id);     // 再从字典移除
        //}
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
