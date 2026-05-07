using UnityEngine;

/// <summary>
/// 最土的测试方式：挂到场景里任意一个 GameObject 上，按 Play 即可看 Console
/// 全绿 [PASS] 11 条 = 通过
/// </summary>
public class EntityTagTest : MonoBehaviour
{
    private int passed;
    private int failed;

    private void Start()
    {
        passed = 0;
        failed = 0;

        Test_添加单标签();
        Test_添加多标签();
        Test_重复添加幂等();
        Test_添加组合标签();
        Test_移除已存在标签();
        Test_移除不存在标签();
        Test_HasAllTag_全部命中();
        Test_HasAllTag_部分命中();
        Test_HasAnyTag_任一命中();
        Test_HasAnyTag_全不命中();
        Test_ClearTags();

        if (failed == 0)
            Debug.Log($"<color=#00FF00>✅ 全部通过  {passed}/{passed}</color>");
        else
            Debug.LogError($"❌ 失败 {failed} 个  通过 {passed} 个");
    }

    // ---------- 断言工具 ----------
    private void Assert(bool cond, string name)
    {
        if (cond) { passed++; Debug.Log($"<color=#00FF00>[PASS]</color> {name}"); }
        else      { failed++; Debug.LogError($"[FAIL] {name}"); }
    }

    // ---------- 11 个用例 ----------
    private void Test_添加单标签()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player);
        Assert(e.Tags == EntityTag.Player, nameof(Test_添加单标签));
    }

    private void Test_添加多标签()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player);
        e.AddTags(EntityTag.Enemy);
        Assert(e.Tags == (EntityTag.Player | EntityTag.Enemy), nameof(Test_添加多标签));
    }

    private void Test_重复添加幂等()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player);
        e.AddTags(EntityTag.Player);
        Assert(e.Tags == EntityTag.Player, nameof(Test_重复添加幂等));
    }

    private void Test_添加组合标签()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player | EntityTag.Item);
        Assert(e.HasAllTag(EntityTag.Player) && e.HasAllTag(EntityTag.Item) && !e.HasAllTag(EntityTag.Enemy),
               nameof(Test_添加组合标签));
    }

    private void Test_移除已存在标签()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player | EntityTag.Enemy | EntityTag.Item);
        e.RemoveTags(EntityTag.Enemy);
        Assert(e.Tags == (EntityTag.Player | EntityTag.Item), nameof(Test_移除已存在标签));
    }

    private void Test_移除不存在标签()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player);
        e.RemoveTags(EntityTag.Enemy);
        Assert(e.Tags == EntityTag.Player, nameof(Test_移除不存在标签));
    }

    private void Test_HasAllTag_全部命中()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player | EntityTag.Enemy);
        Assert(e.HasAllTag(EntityTag.Player | EntityTag.Enemy), nameof(Test_HasAllTag_全部命中));
    }

    private void Test_HasAllTag_部分命中()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player);
        Assert(!e.HasAllTag(EntityTag.Player | EntityTag.Enemy), nameof(Test_HasAllTag_部分命中));
    }

    private void Test_HasAnyTag_任一命中()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player);
        Assert(e.HasAnyTag(EntityTag.Player | EntityTag.Enemy), nameof(Test_HasAnyTag_任一命中));
    }

    private void Test_HasAnyTag_全不命中()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player);
        Assert(!e.HasAnyTag(EntityTag.Enemy | EntityTag.Item), nameof(Test_HasAnyTag_全不命中));
    }

    private void Test_ClearTags()
    {
        var e = new Entity(1);
        e.AddTags(EntityTag.Player | EntityTag.Enemy | EntityTag.Item);
        e.ClearTags();
        Assert(e.Tags == EntityTag.None, nameof(Test_ClearTags));
    }
}
