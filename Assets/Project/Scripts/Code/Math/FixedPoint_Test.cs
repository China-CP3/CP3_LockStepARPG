using UnityEngine;
using System;

public class FixedPoint_ComprehensiveTest : MonoBehaviour
{
    // 定义一个很小的误差容忍度
    private readonly FixedPoint Epsilon = new FixedPoint(0.00001);

    // ===================================================================
    // 增强的断言工具 (Enhanced Assertion Tools)
    // ===================================================================

    /// <summary>
    /// 断言两个值完全相等。
    /// </summary>
    /// <typeparam name="T">要比较的类型</typeparam>
    /// <param name="actual">实际值</param>
    /// <param name="expected">期望值</param>
    /// <param name="message">测试描述</param>
    private void AssertEqual<T>(T actual, T expected, string message)
    {
        if (!actual.Equals(expected))
        {
            Debug.LogError($"<color=red>ASSERT FAILED:</color> {message}\nExpected: <color=yellow>{expected}</color>, But got: <color=orange>{actual}</color>");
        }
        else
        {
            Debug.Log($"<color=green>PASSED:</color> {message}");
        }
    }

    /// <summary>
    /// 断言两个定点数近似相等。
    /// </summary>
    private void AssertApprox(FixedPoint actual, FixedPoint expected, string message)
    {
        long diffScaledValue = actual.ScaledValue > expected.ScaledValue
            ? actual.ScaledValue - expected.ScaledValue
            : expected.ScaledValue - actual.ScaledValue;

        if (diffScaledValue > Epsilon.ScaledValue)
        {
            Debug.LogError($"<color=red>ASSERT FAILED (Approx):</color> {message}\nExpected near: <color=yellow>{expected}</color>, But got: <color=orange>{actual}</color>");
        }
        else
        {
            Debug.Log($"<color=green>PASSED (Approx):</color> {message}");
        }
    }

    /// <summary>
    /// 断言一个条件为真。
    /// </summary>
    private void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError($"<color=red>ASSERT FAILED:</color> {message}. Condition was false, expected true.");
        }
        else
        {
            Debug.Log($"<color=green>PASSED:</color> {message}");
        }
    }

    /// <summary>
    /// 标记测试失败。
    /// </summary>
    private void Fail(string message)
    {
        Debug.LogError($"<color=red>ASSERT FAILED:</color> {message}");
    }


    // ===================================================================
    // 测试执行入口 (Test Runner)
    // ===================================================================

    void Start()
    {
        Debug.Log("--- Running FixedPoint Comprehensive Tests ---");

        TestConstructors();
        TestArithmetic();
        TestCasting();
        TestPrecision();
        TestEdgeCases();

        Debug.Log("--- All Tests Completed ---");
    }

    // ===================================================================
    // 具体的测试用例 (Specific Test Cases)
    // ===================================================================

    void TestConstructors()
    {
        Debug.Log("--- Testing Constructors ---");
        AssertEqual(new FixedPoint(0).ScaledValue, 0L, "Constructor from int 0");
        AssertEqual(new FixedPoint(10).ScaledValue, 10L << 32, "Constructor from int 10");
        AssertEqual(new FixedPoint(-10).ScaledValue, -10L << 32, "Constructor from int -10");
        AssertApprox(new FixedPoint(123.456), new FixedPoint(123.456), "Constructor from double 123.456");
        AssertApprox(new FixedPoint(-123.456), new FixedPoint(-123.456), "Constructor from double -123.456");
        AssertApprox(new FixedPoint(0.9999999999), new FixedPoint(1), "Rounding up from double");
    }

    void TestArithmetic()
    {
        Debug.Log("--- Testing Arithmetic ---");
        FixedPoint a = new FixedPoint(1.5);
        FixedPoint b = new FixedPoint(2.5);
        AssertApprox(a + b, new FixedPoint(4.0), "Addition: 1.5 + 2.5");

        FixedPoint c = new FixedPoint(10);
        FixedPoint d = new FixedPoint(-5);
        AssertApprox(c - d, new FixedPoint(15), "Subtraction: 10 - (-5)");

        AssertApprox(new FixedPoint(10.0) / new FixedPoint(4.0), new FixedPoint(2.5), "Division: 10.0 / 4.0");

        FixedPoint oneThird = new FixedPoint(1) / new FixedPoint(3);
        AssertApprox(oneThird, new FixedPoint(0.333333), "Division with precision: 1 / 3");

        AssertApprox(new FixedPoint(2.5) * new FixedPoint(3.0), new FixedPoint(7.5), "Multiplication: 2.5 * 3.0");
    }

    void TestCasting()
    {
        Debug.Log("--- Testing Casting ---");
        FixedPoint val = new FixedPoint(10.9);
        AssertEqual((int)val, 10, "Cast to int (truncation)");

        FixedPoint negVal = new FixedPoint(-10.9);
        AssertEqual((int)negVal, -10, "Cast negative to int (truncation towards zero)");

        // 对于浮点数比较，最好检查差值是否在容忍范围内
        bool isDoubleCloseEnough = Math.Abs((double)val - 10.9) < 0.00001;
        AssertTrue(isDoubleCloseEnough, "Cast to double");
    }

    void TestPrecision()
    {
        Debug.Log("--- Testing Precision ---");
        FixedPoint val = new FixedPoint(123);
        FixedPoint divisor = new FixedPoint(7);
        FixedPoint result = (val / divisor) * divisor;
        AssertApprox(result, val, "Multiplication and division should roughly cancel out");

        FixedPoint sum = new FixedPoint(0);
        FixedPoint increment = new FixedPoint(0.1);
        for (int i = 0; i < 10; i++)
        {
            sum += increment;
        }
        AssertApprox(sum, new FixedPoint(1.0), "Cumulative addition of 0.1 ten times");
    }

    void TestEdgeCases()
    {
        Debug.Log("--- Testing Edge Cases ---");
        try
        {
            var _ = new FixedPoint(1) / new FixedPoint(0);
            // 如果代码能执行到这里，说明没有抛出异常，测试失败！
            Fail("Division by zero should throw an exception, but it didn't.");
        }
        catch (DivideByZeroException)
        {
            // 成功捕获了正确的异常，测试通过！
            Debug.Log("<color=green>PASSED:</color> Division by zero threw correct exception.");
        }
        catch (Exception e)
        {
            Fail($"Division by zero threw wrong exception type: {e.GetType()}");
        }

        try
        {
            double largeDouble = ((double)long.MaxValue / (1L << 32)) * 2.0;
            var _ = new FixedPoint(largeDouble);
            Fail("Constructor with large double should throw an exception, but it didn't.");
        }
        catch (OverflowException)
        {
            Debug.Log("<color=green>PASSED:</color> Constructor overflow threw correct exception.");
        }
        catch (Exception e)
        {
            Fail($"Constructor overflow threw wrong exception type: {e.GetType()}");
        }
    }
}