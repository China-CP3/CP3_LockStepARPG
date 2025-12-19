using UnityEngine;
using System;

// =======================================================================================
// 增强版 FixedPoint 测试脚本
// (假设 FixedPoint 结构体已在项目中定义)
// =======================================================================================

public class EnhancedFixedPointTester : MonoBehaviour
{
    private int testsPassed = 0;
    private int testsFailed = 0;

    void Start()
    {
        Debug.Log("========== 运行定点数 (FixedPoint) 测试 ==========");

        RunTestSuite("构造函数", TestConstructors);
        RunTestSuite("四则运算", TestArithmetic);
        RunTestSuite("比较运算", TestComparisons);
        RunTestSuite("类型转换", TestConversions);
        RunTestSuite("平方根 (Sqrt)", TestSqrt);
        RunTestSuite("边界情况", TestEdgeCases);

        string summaryColor = testsFailed > 0 ? "red" : "green";
        Debug.Log($"========== 测试完成: <color={summaryColor}>通过 {testsPassed}, 失败 {testsFailed}</color> ==========");
    }

    private void RunTestSuite(string suiteName, Action testAction)
    {
        Debug.Log($"--- 开始测试: {suiteName} ---");
        int initialFails = testsFailed;
        testAction.Invoke();
        if (testsFailed == initialFails)
        {
            Debug.Log($"--- <color=green>测试套件 [{suiteName}] 全部通过</color> ---");
        }
        else
        {
            Debug.Log($"--- <color=red>测试套件 [{suiteName}] 存在失败项</color> ---");
        }
    }

    // --- 测试套件 ---

    private void TestConstructors()
    {
        AssertEquals(10 * 1024L, FixedPoint.CreateByInt(10).ScaledValue, "CreateByInt(10)");
        AssertEquals(12641L, FixedPoint.CreateByFloat(12.345f).ScaledValue, "CreateByFloat(12.345f)");
        AssertEquals(-5814L, FixedPoint.CreateByDouble(-5.678).ScaledValue, "CreateByDouble(-5.678)");
    }

    private void TestArithmetic()
    {
        var a = FixedPoint.CreateByDouble(10.5); // Scaled: 10752
        var b = FixedPoint.CreateByDouble(2.25); // Scaled: 2304

        AssertAlmostEquals(12.75, (double)(a + b), "加法: 10.5 + 2.25");
        AssertAlmostEquals(8.25, (double)(a - b), "减法: 10.5 - 2.25");
        AssertAlmostEquals(23.625, (double)(a * b), "乘法: 10.5 * 2.25");
        AssertAlmostEquals(4.6666, (double)(a / b), "除法: 10.5 / 2.25", 0.001);
    }

    private void TestComparisons()
    {
        var fp1 = FixedPoint.CreateByInt(100);
        var fp2 = FixedPoint.CreateByInt(200);
        var fp3 = FixedPoint.CreateByInt(100);

        AssertTrue(fp1 < fp2, "比较: 100 < 200");
        AssertTrue(fp2 > fp1, "比较: 200 > 100");
        AssertTrue(fp1 <= fp3, "比较: 100 <= 100");
        AssertTrue(fp1 >= fp3, "比较: 100 >= 100");
        AssertTrue(fp1 == fp3, "比较: 100 == 100");
        AssertTrue(fp1 != fp2, "比较: 100 != 200");
    }

    private void TestConversions()
    {
        var fpPos = FixedPoint.CreateByDouble(123.789);
        var fpNeg = FixedPoint.CreateByDouble(-45.6);

        AssertEquals(123, (int)fpPos, "转换为 int (正数)");
        AssertEquals(-45, (int)fpNeg, "转换为 int (负数)");
        AssertAlmostEquals(123.788, (double)fpPos, "转换为 double (正数)", 0.001);
        AssertAlmostEquals(-45.599, (double)fpNeg, "转换为 double (负数)", 0.001);
    }

    private void TestSqrt()
    {
        TestSqrtValue(0);
        TestSqrtValue(1);
        TestSqrtValue(2);
        TestSqrtValue(4);
        TestSqrtValue(100);
        TestSqrtValue(1234.567);
        TestSqrtValue(9000000000000000.0); // 9E+15, 之前失败的测试

        long largeVal = 1L << 40;
        var fpLarge = FixedPoint.CreateByLong(largeVal);
        var fpLargeSqrt = FixedPoint.Sqrt(fpLarge);
        long expectedSqrt = 1L << 20;
        AssertAlmostEquals((double)expectedSqrt, (double)fpLargeSqrt, $"Sqrt(2^40)", 1.0);
    }

    private void TestEdgeCases()
    {
        AssertEquals(FixedPoint.MaxValue, FixedPoint.MaxValue + FixedPoint.One, "溢出: MaxValue + 1");
        AssertEquals(FixedPoint.MinValue, FixedPoint.MinValue - FixedPoint.One, "溢出: MinValue - 1");

        AssertThrows<DivideByZeroException>(() => { var r = FixedPoint.One / FixedPoint.Zero; }, "除以零");
        AssertThrows<ArgumentException>(() => { var r = FixedPoint.Sqrt(FixedPoint.CreateByInt(-1)); }, "对负数开方");
    }

    // --- 断言辅助方法 ---

    private void TestSqrtValue(double value)
    {
        var fpValue = FixedPoint.CreateByDouble(value);
        var fpResult = FixedPoint.Sqrt(fpValue);
        double expected = Math.Sqrt(value);
        AssertAlmostEquals(expected, (double)fpResult, $"Sqrt({value})", 0.002);
    }

    private void AssertEquals<T>(T expected, T actual, string message) where T : IEquatable<T>
    {
        if (expected.Equals(actual))
        {
            Debug.Log($"<color=green>通过:</color> {message} | 预期值: {expected}, 实际值: {actual}");
            testsPassed++;
        }
        else
        {
            Debug.LogError($"<color=red>失败:</color> {message} | 预期值: {expected}, 实际值: {actual}");
            testsFailed++;
        }
    }

    private void AssertAlmostEquals(double expected, double actual, string message, double tolerance = 0.001)
    {
        if (Math.Abs(expected - actual) < tolerance)
        {
            Debug.Log($"<color=green>通过:</color> {message} | 预期值: ≈{expected:F4}, 实际值: {actual:F4}");
            testsPassed++;
        }
        else
        {
            Debug.LogError($"<color=red>失败:</color> {message} | 预期值: ≈{expected:F4}, 实际值: {actual:F4}");
            testsFailed++;
        }
    }

    private void AssertTrue(bool condition, string message)
    {
        if (condition)
        {
            Debug.Log($"<color=green>通过:</color> {message}");
            testsPassed++;
        }
        else
        {
            Debug.LogError($"<color=red>失败:</color> {message} | 预期条件为 True, 实际为 False");
            testsFailed++;
        }
    }

    private void AssertThrows<TException>(Action action, string message) where TException : Exception
    {
        try
        {
            action.Invoke();
            Debug.LogError($"<color=red>失败:</color> {message} | 预期抛出异常 {typeof(TException).Name}, 但未抛出");
            testsFailed++;
        }
        catch (TException)
        {
            Debug.Log($"<color=green>通过:</color> {message} | 成功捕获到预期的异常 {typeof(TException).Name}");
            testsPassed++;
        }
        catch (Exception e)
        {
            Debug.LogError($"<color=red>失败:</color> {message} | 预期抛出异常 {typeof(TException).Name}, 但抛出了不同的异常 {e.GetType().Name}");
            testsFailed++;
        }
    }
}