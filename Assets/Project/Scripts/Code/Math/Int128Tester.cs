// 文件名: Int128Tester.cs
using UnityEngine;

public class Int128Tester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("========== 开始 Int128 乘法测试 ==========");
        RunAllMultiplicationTests();
        Debug.Log("========== 所有测试已完成 ==========");
    }

    void RunAllMultiplicationTests()
    {
        // --- 基础测试 ---
        RunTest("2 * 3", new Int128(2), new Int128(3), new Int128(6));
        RunTest("(-2) * 3", new Int128(-2), new Int128(3), new Int128(-6));
        RunTest("2 * (-3)", new Int128(2), new Int128(-3), new Int128(-6));
        RunTest("(-2) * (-3)", new Int128(-2), new Int128(-3), new Int128(6));

        // --- 零、一、负一 测试 ---
        RunTest("MaxValue * 0", Int128.MaxValue, Int128.Zero, Int128.Zero);
        RunTest("MinValue * 1", Int128.MinValue, Int128.One, Int128.MinValue);
        RunTest("MaxValue * (-1)", Int128.MaxValue, Int128.MinusOne, -Int128.MaxValue);

        // --- 边界和溢出测试 (long 范围) ---
        Int128 longMax = new Int128(long.MaxValue);
        Int128 longMin = new Int128(long.MinValue);

        // long.MaxValue * 2 = (2^63 - 1) * 2 = 2^64 - 2.
        // High: 1, Low: ulong.MaxValue - 1
        RunTest("long.MaxValue * 2", longMax, new Int128(2), new Int128(0, ulong.MaxValue - 1));

        // long.MinValue * 2 = (-2^63) * 2 = -2^64.
        // High: -1, Low: 0
        RunTest("long.MinValue * 2", longMin, new Int128(2), new Int128(-1, 0));

        // long.MaxValue * long.MaxValue = (2^63-1)^2 = 2^126 - 2*2^63 + 1 = 2^126 - 2^64 + 1
        // 这是一个正数，但 high 部分会是 0x3FFFFFFFFFFFFFFF
        RunTest("long.MaxValue * long.MaxValue", longMax, longMax, new Int128(0x3FFFFFFFFFFFFFFF, 1));

        // --- 边界和溢出测试 (Int128 范围) ---

        // MaxValue * 2. 结果应该是 -2
        // (2^127-1)*2 = 2^128 - 2. 在128位有符号数中，2^128被截断，结果是 -2
        RunTest("Int128.MaxValue * 2", Int128.MaxValue, new Int128(2), new Int128(-2));

        // MinValue * MinValue = (-2^127) * (-2^127) = 2^254.
        // 结果远远超出128位，所有128位都将被清零。
        RunTest("Int128.MinValue * Int128.MinValue", Int128.MinValue, Int128.MinValue, Int128.Zero);

        // --- 交叉项测试 ---
        // (2^64 + 1) * (2^64 + 1)
        // = (2^64)^2 + 2*2^64 + 1.
        // 2^128 (溢出) + 2^65 + 1.
        // 2^65 = High: 2, Low: 0.  所以结果是 High: 2, Low: 1.
        Int128 twoPow64Plus1 = new Int128(1, 1);
        RunTest("(2^64+1) * (2^64+1)", twoPow64Plus1, twoPow64Plus1, new Int128(2, 1));

        // (2^64 - 1) * (2^64 + 1) = (2^64)^2 - 1 = 2^128 - 1.
        // 在128位有符号数中，这是 -1.
        Int128 twoPow64Minus1 = new Int128(0, ulong.MaxValue);
        RunTest("(2^64-1) * (2^64+1)", twoPow64Minus1, twoPow64Plus1, Int128.MinusOne);
    }

    /// <summary>
    /// 运行单个测试用例并打印结果
    /// </summary>
    private void RunTest(string testName, Int128 a, Int128 b, Int128 expected)
    {
        Int128 actual = a * b;
        if (actual == expected)
        {
            Debug.Log($"<color=green>[通过]</color> {testName}");
        }
        else
        {
            Debug.LogError($"<color=red>[失败]</color> {testName}\n" +
                           $"       操作: {a} * {b}\n" +
                           $"       预期值: {expected}\n" +
                           $"       实际值: {actual}");
        }
    }
}