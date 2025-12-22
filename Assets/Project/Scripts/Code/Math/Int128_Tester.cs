using UnityEngine;

public class Int128_Tester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=============== Int128 Test Suite Starting ===============");

        // --- 1. 构造函数和常量测试 ---
        Debug.Log("<b>--- 1. Constructor & Constant Tests ---</b>");
        RunTest("Constructor from long (Positive)", new Int128(5), new Int128(0, 5));
        RunTest("Constructor from long (Negative)", new Int128(-1), new Int128(-1, ulong.MaxValue));
        RunTest("Constant: MinusOne", Int128.MinusOne, new Int128(-1));
        RunTest("Constant: Zero", Int128.Zero, new Int128(0, 0));

        // --- 2. 一元负号 (-) 测试 ---
        Debug.Log("<b>--- 2. Unary Minus Operator (-) Tests ---</b>");
        RunTest("Opposite of 5", -new Int128(5), new Int128(-5));
        RunTest("Opposite of -5", -new Int128(-5), new Int128(5));
        RunTest("Opposite of 0", -Int128.Zero, Int128.Zero);
        RunTest("Opposite of 1", -Int128.One, Int128.MinusOne);
        RunTest("Opposite of -1", -Int128.MinusOne, Int128.One);
        RunTest("Edge Case: Opposite of MinValue", -Int128.MinValue, Int128.MinValue);

        // --- 3. 加法 (+) 测试 ---
        Debug.Log("<b>--- 3. Addition Operator (+) Tests ---</b>");
        RunTest("Simple Addition: 5 + 3", new Int128(5) + new Int128(3), new Int128(8));
        RunTest("Positive + Negative: 10 + (-3)", new Int128(10) + new Int128(-3), new Int128(7));
        RunTest("Negative + Negative: (-5) + (-3)", new Int128(-5) + new Int128(-3), new Int128(-8));
        RunTest("Critical Carry Test: ulong.MaxValue + 1", new Int128(0, ulong.MaxValue) + Int128.One, new Int128(1, 0));
        RunTest("Carry with Negative High: -1 + (ulong.MaxValue + 1)", new Int128(-1, ulong.MaxValue) + Int128.One, Int128.Zero);

        // --- 4. 减法 (-) 测试 ---
        Debug.Log("<b>--- 4. Subtraction Operator (-) Tests ---</b>");
        RunTest("Simple Subtraction: 10 - 3", new Int128(10) - new Int128(3), new Int128(7));
        RunTest("Resulting in Negative: 3 - 10", new Int128(3) - new Int128(10), new Int128(-7));
        RunTest("Subtracting a Negative: 10 - (-3)", new Int128(10) - new Int128(-3), new Int128(13));
        RunTest("Critical Borrow Test: [1,0] - 1", new Int128(1, 0) - Int128.One, new Int128(0, ulong.MaxValue));
        RunTest("Zero - 1", Int128.Zero - Int128.One, Int128.MinusOne);

        // --- 5. 比较运算符测试 ---
        Debug.Log("<b>--- 5. Comparison Operator Tests ---</b>");
        RunTest("Comparison: 5 > 3", new Int128(5) > new Int128(3), true);
        RunTest("Comparison: 3 > 5", new Int128(3) > new Int128(5), false);
        RunTest("Comparison: 5 > 5", new Int128(5) > new Int128(5), false);
        RunTest("Comparison: 5 >= 5", new Int128(5) >= new Int128(5), true);
        RunTest("Comparison: -5 > -3", new Int128(-5) > new Int128(-3), false);
        RunTest("Comparison: -3 > -5", new Int128(-3) > new Int128(-5), true);
        RunTest("Comparison: 5 > -5", new Int128(5) > new Int128(-5), true);
        RunTest("Comparison: MaxValue > MinValue", Int128.MaxValue > Int128.MinValue, true);
        RunTest("Comparison: 5 == 5", new Int128(5) == new Int128(5), true);
        RunTest("Comparison: 5 == 6", new Int128(5) == new Int128(6), false);
        RunTest("Comparison: 5 != 6", new Int128(5) != new Int128(6), true);

        Debug.Log("=============== Int128 Test Suite Finished ===============");
    }

    /// <summary>
    /// 运行一个返回 Int128 的测试用例并打印结果
    /// </summary>
    private void RunTest(string testName, Int128 actual, Int128 expected)
    {
        bool success = (actual == expected);
        string result = success
            ? "<color=green>PASS</color>"
            : "<color=red>FAIL</color>";

        string message = $"[{testName}] - {result}";
        if (!success)
        {
            message += $"\n  Expected: {expected}\n  Actual:   {actual}";
        }

        Debug.Log(message);
    }

    /// <summary>
    /// 运行一个返回布尔值的测试用例并打印结果
    /// </summary>
    private void RunTest(string testName, bool actual, bool expected)
    {
        bool success = (actual == expected);
        string result = success
            ? "<color=green>PASS</color>"
            : "<color=red>FAIL</color>";

        string message = $"[{testName}] - {result}";
        if (!success)
        {
            message += $"\n  Expected: {expected}\n  Actual:   {actual}";
        }

        Debug.Log(message);
    }
}