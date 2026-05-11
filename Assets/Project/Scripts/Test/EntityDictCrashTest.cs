using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 验证：foreach 字典 / EntityManager 时，调用 Add/Create / Remove/Destroy 必崩
/// 用法：挂到场景任意 GameObject 上，运行后看 Console
/// 预期：4 个 Test 全部抛出 InvalidOperationException
/// </summary>
public class EntityDictCrashTest : MonoBehaviour
{
    void Start()
    {
        Test1_RawDictRemoveCrash();
        Test2_RawDictAddCrash();
        Test3_EntityManagerDestroyCrash();
        Test4_EntityManagerCreateCrash();
    }

    // ========== 测试 1：原生字典 foreach 中 Remove ==========
    private void Test1_RawDictRemoveCrash()
    {
        Debug.Log("===== Test1: 字典 foreach 中 Remove =====");
        var dic = new Dictionary<int, string>();
        for (int i = 1; i <= 5; i++) dic.Add(i, "v" + i);

        try
        {
            foreach (var kv in dic)
            {
                Debug.Log($"遍历到 key={kv.Key}");
                if (kv.Key == 2)
                {
                    dic.Remove(2); // 💥 触发版本号校验，必崩
                }
            }
            Debug.Log("✅ 没崩（理论上不应该出现这行）");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"💥 Test1 崩了：{e.GetType().Name} -> {e.Message}");
        }
    }

    // ========== 测试 2：原生字典 foreach 中 Add ==========
    private void Test2_RawDictAddCrash()
    {
        Debug.Log("===== Test2: 字典 foreach 中 Add =====");
        var dic = new Dictionary<int, string>();
        for (int i = 1; i <= 3; i++) dic.Add(i, "v" + i);

        try
        {
            foreach (var kv in dic)
            {
                Debug.Log($"遍历到 key={kv.Key}");
                if (kv.Key == 1)
                {
                    dic.Add(99, "v99"); // 💥 Add 也会改版本号，同样崩
                }
            }
            Debug.Log("✅ 没崩（理论上不应该出现这行）");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"💥 Test2 崩了：{e.GetType().Name} -> {e.Message}");
        }
    }

    // ========== 测试 3：EntityManager foreach 中 DestroyEntity ==========
    private void Test3_EntityManagerDestroyCrash()
    {
        Debug.Log("===== Test3: EntityManager foreach 中 DestroyEntity =====");
        var mgr = new EntityManager();
        for (int i = 0; i < 5; i++) mgr.CreateEntity();

        try
        {
            foreach (var entity in mgr.GetAllEntities())
            {
                Debug.Log($"遍历到 Entity Id={entity.Id}");
                if (entity.Id == 2)
                {
                    mgr.DestroyEntity(2); // 💥 内部就是字典 Remove，必崩
                }
            }
            Debug.Log("✅ 没崩（理论上不应该出现这行）");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"💥 Test3 崩了：{e.GetType().Name} -> {e.Message}");
        }
    }

    // ========== 测试 4：EntityManager foreach 中 CreateEntity ==========
    private void Test4_EntityManagerCreateCrash()
    {
        Debug.Log("===== Test4: EntityManager foreach 中 CreateEntity =====");
        var mgr = new EntityManager();
        for (int i = 0; i < 3; i++) mgr.CreateEntity();

        try
        {
            foreach (var entity in mgr.GetAllEntities())
            {
                Debug.Log($"遍历到 Entity Id={entity.Id}");
                if (entity.Id == 1)
                {
                    mgr.CreateEntity(); // 💥 内部就是字典 Add，必崩
                }
            }
            Debug.Log("✅ 没崩（理论上不应该出现这行）");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"💥 Test4 崩了：{e.GetType().Name} -> {e.Message}");
        }
    }
}
