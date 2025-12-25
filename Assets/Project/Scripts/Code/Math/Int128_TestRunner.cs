using System;
using System.Numerics; // 用于 BigInteger 对比
using UnityEngine;     // 用于 Debug.Log

/// <summary>
/// Unity 测试启动器。将此脚本附加到场景中的任何 GameObject 上即可运行测试。
/// </summary>
public class Int128_TestRunner : MonoBehaviour
{
    void Start()
    {
        Debug.Log("--- Int128 除法测试 ---");

        // 测试案例
        RunTest(100, 10);
        RunTest(123456789, 123);
        RunTest(Int128.MaxValue, 2);
        RunTest(Int128.MaxValue, Int128.MaxValue);
        RunTest(100, 101);

        // 负数测试
        RunTest(-100, 10);
        RunTest(100, -10);
        RunTest(-100, -10);

        // 跨越64位的关键测试
        Int128 a_large = new Int128(1, 0);
        Int128 b_small = new Int128(0, 2);
        RunTest(a_large, b_small);

        // 大数测试
        Int128 big_a = new Int128(500, 1234567890123456789);
        Int128 big_b = new Int128(0, 987654321);
        RunTest(big_a, big_b);

        // 除以零测试
        try
        {
            Int128 result = new Int128(100, 0) / Int128.Zero;
            Debug.Log("<color=red>FAIL ❌: 除以零没有抛出异常！</color>");
        }
        catch (DivideByZeroException)
        {
            Debug.Log("\n----- 测试: 除以零 -----\n  => <color=green>PASS ✔️ (正确抛出 DivideByZeroException)</color>");
        }

        Debug.Log("\n--- 测试结束 ---");
    }

    public void RunTest(Int128 a, Int128 b)
    {
        Debug.Log($"\n----- 测试: {a} / {b} -----");

        Int128 result = a / b;
        BigInteger bigA = a.ToBigInteger();
        BigInteger bigB = b.ToBigInteger();
        BigInteger expected = bigA / bigB;

        Debug.Log($"  我们的结果: {result}");
        Debug.Log($"  标准答案:   {expected}");

        if (result.ToBigInteger() == expected)
        {
            Debug.Log("  => <color=green>PASS ✔️</color>");
        }
        else
        {
            Debug.Log($"  => <color=red>FAIL ❌</color>");
        }
    }
}