using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    public int Id { get; }
    public bool IsDestroyed { get; private set; }
    private Dictionary<Type, EntityComponent> componentsDic = new Dictionary<Type, EntityComponent>();
    public Entity(int id)
    {
        this.Id = id;
    }

    public T AddComponent<T>() where T : EntityComponent,new()
    {
        Type type = typeof(T);
        if (componentsDic.TryGetValue(type, out var existingComponent))
        {
            Debug.LogError($"Entity {Id} 已存在组件 {type.Name}，请勿重复添加！");
            return existingComponent as T;
        }

        T component = new T();
        componentsDic.Add(type, component);
        component.SetOwner(this);
        component.OnAddComponent();
        return component;
    }

    public T GetComponent<T>() where T : EntityComponent
    {
        Type type = typeof(T);
        if (componentsDic.TryGetValue(type, out var component))
        {
            return component as T;
        }

        return null;
    }

    public bool HasComponent<T>() where T:EntityComponent
    {
        return componentsDic.ContainsKey(typeof(T));
    }

    public void RemoveComponent<T>() where T :EntityComponent
    {
        Type type = typeof(T);
        if (componentsDic.TryGetValue(type, out var component))
        {
            component.OnRemoveComponent();
            componentsDic.Remove(type);
        }
        
    }

    public void Destroy()
    {
        IsDestroyed = true;

        foreach (var component in componentsDic.Values)
        {
            component.OnRemoveComponent();
        }

        componentsDic.Clear();
    }
}
