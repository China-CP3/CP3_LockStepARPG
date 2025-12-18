using System;
using UnityEngine;

/// <summary>
/// 专门用于深度测试 FixedPoint.Sqrt 函数的脚本。
/// 它会测试各种边界情况、大数、小数，并清晰地打印预期值和实际值。
/// </summary>
public class FixedPointSqrtTester : MonoBehaviour
{
    private int testsPassed = 0;
    private int testsFailed = 0;

    // 我们使用一个更小的容差来要求更高的精度
    private const double Epsilon = 0.0001;

    void Start()
    {
        Log("--- [FixedPoint.Sqrt] 开始深度测试 ---", "cyan");

        // 1. 基础和完美平方数
        Log("\n--- 1. 基础和完美平方数 ---", "yellow");
        TestSingleSqrt("Sqrt(0)", 0);
        TestSingleSqrt("Sqrt(1)", 1);
        TestSingleSqrt("Sqrt(4)", 4);
        TestSingleSqrt("Sqrt(100)", 100);
        TestSingleSqrt("Sqrt(65536)", 65536); // 256 * 256

        // 2. 常用无理数
        Log("\n--- 2. 常用无理数 ---", "yellow");
        TestSingleSqrt("Sqrt(2)", 2);
        TestSingleSqrt("Sqrt(3)", 3);
        TestSingleSqrt("Sqrt(10)", 10);

        // 3. 小于 1 的数
        Log("\n--- 3. 小于 1 的数 ---", "yellow");
        TestSingleSqrt("Sqrt(0.5)", 0.5);
        TestSingleSqrt("Sqrt(0.25)", 0.25); // 完美平方小数
        TestSingleSqrt("Sqrt(0.01)", 0.01); // 完美平方小数
        TestSingleSqrt("Sqrt(0.0009765625)", 0.0009765625); // 1/1024, 结果应为 1/32

        // 4. 大数和复杂数
        Log("\n--- 4. 大数和复杂数 ---", "yellow");
        TestSingleSqrt("Sqrt(98765.4321)", 98765.4321); // 之前失败的案例
        TestSingleSqrt("Sqrt(1,000,000)", 1000000);
        TestSingleSqrt("Sqrt(888,888,888)", 888888888);

        // --- FIX: Replaced private FixedPoint.ShiftBits with its known value (10) ---
        // 这个测试的目的是创建一个大数，其平方在定点数乘法中不会立即溢出 long.MaxValue
        long largeNum = (long.MaxValue >> (10 * 2)) - 1;
        TestSingleSqrt($"Sqrt(大数: {largeNum})", (double)largeNum);

        // 5. 边界情况
        Log("\n--- 5. 边界情况 ---", "yellow");
        TestException("Sqrt(-1)", () => FixedPoint.Sqrt(FixedPoint.CreateByInt(-1)), typeof(ArgumentException));
        TestException("Sqrt(MinValue)", () => FixedPoint.Sqrt(FixedPoint.MinValue), typeof(ArgumentException));

        // 总结
        Log("\n--- 测试总结 ---", "cyan");
        if (testsFailed == 0)
        {
            Log($"所有 {testsPassed} 项 Sqrt 测试全部通过！", "green");
        }
        else
        {
            Log($"{testsPassed} 项通过, {testsFailed} 项失败。", "red");
        }
    }

    /// <summary>
    /// 测试单个值的平方根
    /// </summary>
    private void TestSingleSqrt(string testName, double value)
    {
        var fpValue = FixedPoint.CreateByDouble(value);
        var expected = Math.Sqrt(value);

        FixedPoint fpResult;
        try
        {
            fpResult = FixedPoint.Sqrt(fpValue);
        }
        catch (Exception e)
        {
            Fail(testName, expected, $"抛出异常: {e.Message}");
            return;
        }

        var actual = (double)fpResult;

        if (Math.Abs(expected - actual) < Epsilon)
        {
            Pass(testName, expected, actual);
        }
        else
        {
            Fail(testName, expected, actual);
        }
    }

    /// <summary>
    /// 测试是否按预期抛出异常
    /// </summary>
    private void TestException(string testName, Action testAction, Type expectedExceptionType)
    {
        try
        {
            testAction();
            Fail(testName, $"应抛出 {expectedExceptionType.Name}", "未抛出异常");
        }
        catch (Exception e)
        {
            if (e.GetType() == expectedExceptionType)
            {
                Pass(testName, $"应抛出 {expectedExceptionType.Name}", $"成功抛出 {e.GetType().Name}");
            }
            else
            {
                Fail(testName, $"应抛出 {expectedExceptionType.Name}", $"抛出了错误的异常: {e.GetType().Name}");
            }
        }
    }

    #region Helper Methods for Logging

    private void Pass(string testName, object expected, object actual)
    {
        // 使用 "F6" 格式化，以更精确地显示浮点数
        string expectedStr = (expected is double d1) ? d1.ToString("F6") : expected.ToString();
        string actualStr = (actual is double d2) ? d2.ToString("F6") : actual.ToString();
        Log($"<color=green>[PASS]</color> {testName}. 预期: {expectedStr}, 实际: {actualStr}");
        testsPassed++;
    }

    private void Fail(string testName, object expected, object actual)
    {
        string expectedStr = (expected is double d1) ? d1.ToString("F6") : expected.ToString();
        string actualStr = (actual is double d2) ? d2.ToString("F6") : actual.ToString();
        Log($"<color=red>[FAIL]</color> {testName}. 预期: {expectedStr}, 实际: {actualStr}");
        testsFailed++;
    }

    private void Log(string message, string color = null)
    {
        if (!string.IsNullOrEmpty(color))
        {
            Debug.Log($"<color={color}>{message}</color>");
        }
        else
        {
            Debug.Log(message);
        }
    }

    #endregion
}