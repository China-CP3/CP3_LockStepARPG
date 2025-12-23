using UnityEngine;

// 将这个脚本挂载到场景中的任何一个GameObject上即可运行测试。
public class Int128Tester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("========== Int128 乘法测试开始 ==========");

        Test("1. 简单正数乘法",
             new Int128(2),
             new Int128(3),
             new Int128(6));

        Test("2. ulong.MaxValue * 2 (测试进位)",
             new Int128(ulong.MaxValue),
             new Int128(2),
             new Int128(1, 18446744073709551614UL)); // 预期 high=1, low=ulong.MaxValue-1

        Test("3. 两个大数相乘 (产生128位结果)",
             new Int128(ulong.MaxValue),
             new Int128(ulong.MaxValue),
             new Int128(-2, 1)); // high64 的值是 -2，其十六进制表示为 0xFFFFFFFFFFFFFFFE

        Debug.Log("--- 有符号数测试 ---");

        Test("4. 正数 * 负数",
             new Int128(100),
             new Int128(-1),
             new Int128(-100));

        Test("5. 负数 * 正数",
             new Int128(-5),
             new Int128(20),
             new Int128(-100));

        Test("6. 负数 * 负数 (-1 * -1)",
             Int128.MinusOne,
             Int128.MinusOne,
             Int128.One);

        Test("7. 负数 * 负数 (-2 * -3)",
             new Int128(-2),
             new Int128(-3),
             new Int128(6));

        Test("8. 涉及高位的乘法 (2^64 * 2)",
             new Int128(1, 0), // 代表 2^64
             new Int128(2),
             new Int128(2, 0)); // 预期 2 * 2^64

        Test("9. 涉及高位的负数乘法 (-2^64 * 2)",
             new Int128(-1, 0), // 代表 -2^64 (近似值)
             new Int128(2),
             new Int128(-2, 0)); // 预期 -2 * 2^64

        Test("10. 零乘任何数",
             new Int128(12345, 67890),
             Int128.Zero,
             Int128.Zero);

        Debug.Log("========== Int128 乘法测试结束 ==========");
    }

    /// <summary>
    /// 辅助测试函数，用于执行乘法并比较结果。
    /// </summary>
    /// <param name="testName">测试的名称</param>
    /// <param name="a">乘数 a</param>
    /// <param name="b">乘数 b</param>
    /// <param name="expected">预期的结果</param>
    private void Test(string testName, Int128 a, Int128 b, Int128 expected)
    {
        // 执行乘法
        Int128 actual = a * b;

        // 检查结果是否与预期相符
        bool success = (actual == expected);

        // 构造输出信息
        string status = success ? "<color=green>成功</color>" : "<color=red>失败</color>";

        // 使用 Debug.Log 打印，支持 Unity 的富文本格式
        Debug.Log($"<b>{testName}</b>: {status}");

        if (!success)
        {
            Debug.Log($"   算式: ({a}) * ({b})");
            Debug.Log($"   预期: {expected}");
            Debug.Log($"   实际: {actual}");
        }
    }
}