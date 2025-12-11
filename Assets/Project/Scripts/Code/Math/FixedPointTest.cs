//using UnityEngine;

//public class CurrentCodeStressTest : MonoBehaviour
//{
//    // 对于 double 比较，我们需要一个误差容忍度
//    private const double Epsilon = 1e-9;

//    void Start()
//    {
//        Debug.Log("--- 开始对当前 FixedPoint 代码进行专属压力测试 ---");

//        // --- 测试 1: 构造函数精度测试 ---
//        // 1a: 测试正数浮点数的四舍五入构造
//        var p1 = new FixedPoint(1.5f); // 应该构造为 2.0 的内部值
//        var p2 = new FixedPoint(1.499f); // 应该构造为 1.0 的内部值
//        CompareResults(p1.ToInt(), 2, "测试 1a: 构造函数 Round(1.5f)");
//        CompareResults(p2.ToInt(), 1, "测试 1b: 构造函数 Round(1.499f)");

//        // 1b: 测试负数浮点数的四舍五入构造
//        var n1 = new FixedPoint(-1.5f); // 应该构造为 -2.0 的内部值
//        var n2 = new FixedPoint(-1.499f); // 应该构造为 -1.0 的内部值
//        CompareResults(n1.ToInt(), -2, "测试 1c: 构造函数 Round(-1.5f)");
//        CompareResults(n2.ToInt(), -1, "测试 1d: 构造函数 Round(-1.499f)");

//        // 1c: 测试整数构造
//        var i1 = new FixedPoint(100);
//        CompareResults(i1.ToDouble(), 100.0, "测试 1e: 构造函数 (int 100)");

//        // --- 测试 2: 基础四则运算 ---
//        var fp_a = new FixedPoint(10.5f); // 构造为 11
//        var fp_b = new FixedPoint(2.0f);  // 构造为 2

//        // 2a: 加法
//        CompareResults((fp_a + fp_b).ToDouble(), 13.0, "测试 2a: 加法 (11 + 2)");

//        // 2b: 减法
//        CompareResults((fp_a - fp_b).ToDouble(), 9.0, "测试 2b: 减法 (11 - 2)");

//        // 2c: 乘法 (使用了 decimal，应该非常精确)
//        // 注意：因为构造函数会舍入，所以我们用整数构造来获得精确的初始值
//        var fp_c = new FixedPoint(3);
//        var fp_d = new FixedPoint(4.25f); // 构造为 4
//        CompareResults((fp_c * fp_d).ToDouble(), 12.0, "测试 2c: 乘法 (3 * 4)");

//        // 2d: 除法 (使用了 decimal，应该非常精确)
//        var fp_e = new FixedPoint(10);
//        var fp_f = new FixedPoint(4);
//        CompareResults((fp_e / fp_f).ToDouble(), 2.5, "测试 2d: 除法 (10 / 4)");

//        // --- 测试 3: 复杂运算与精度保持 ---
//        // (20.1 / 3.9) * 1.5 - 7.8
//        // 注意：每个值在构造时都会被四舍五入
//        // 20.1f -> 20, 3.9f -> 4, 1.5f -> 2, 7.8f -> 8
//        // 预期运算: (20 / 4) * 2 - 8 = 5 * 2 - 8 = 10 - 8 = 2
//        var v1 = new FixedPoint(20.1f);
//        var v2 = new FixedPoint(3.9f);
//        var v3 = new FixedPoint(1.5f);
//        var v4 = new FixedPoint(7.8f);
//        var complex_result = (v1 / v2) * v3 - v4;
//        CompareResults(complex_result.ToDouble(), 2.0, "测试 3: 复杂运算 (基于舍入后的构造值)");

//        // --- 测试 4: 边界与特殊值测试 ---
//        // 4a: 极大值乘法 (验证 decimal 保护)
//        // 使用 long 构造函数来绕开 float 的精度限制
//        var max_int_part = new FixedPoint(2000000000L); // 2 * 10^9
//        var result_mul = max_int_part * max_int_part;
//        // 预期结果是 4 * 10^18
//        double expected_double = 4e18;
//        CompareResults(result_mul.ToDouble(), expected_double, "测试 4a: 极大值乘法");

//        // 4b: 极大值除法
//        var result_div = result_mul / max_int_part;
//        CompareResults(result_div.ToDouble(), 2000000000.0, "测试 4b: 极大值除法");

//        // 4c: 除以零 (你的代码会打印错误日志，并返回0)
//        Debug.Log("<color=yellow>--- 开始测试除以零，预期会看到一条错误日志 ---</color>");
//        var zero = new FixedPoint(0.0f);
//        var div_by_zero_result = new FixedPoint(100) / zero;
//        CompareResults(div_by_zero_result.ToDouble(), 0.0, "测试 4c: 除以零返回0");
//        Debug.Log("<color=yellow>--- 除以零测试结束 ---</color>");

//        // --- 测试 5: 负数运算 ---
//        var neg_a = new FixedPoint(-10);
//        var neg_b = new FixedPoint(-2);
//        var pos_c = new FixedPoint(5);

//        // 5a: 负数乘负数
//        CompareResults((neg_a * neg_b).ToDouble(), 20.0, "测试 5a: (-10) * (-2)");

//        // 5b: 负数乘正数
//        CompareResults((neg_a * pos_c).ToDouble(), -50.0, "测试 5b: (-10) * 5");

//        // 5c: 负数除以负数
//        CompareResults((neg_a / neg_b).ToDouble(), 5.0, "测试 5c: (-10) / (-2)");

//        // 5d: 负数除以正数
//        CompareResults((neg_a / pos_c).ToDouble(), -2.0, "测试 5d: (-10) / 5");

//        Debug.Log("--- 专属压力测试结束 ---");
//    }

//    // 泛型比较函数，适用于 int, double 等
//    private void CompareResults<T>(T result, T expected, string testName) where T : System.IEquatable<T>
//    {
//        bool pass = false;
//        if (result is double || result is float)
//        {
//            double r = System.Convert.ToDouble(result);
//            double e = System.Convert.ToDouble(expected);
//            if (System.Math.Abs(r - e) < Epsilon)
//            {
//                pass = true;
//            }
//        }
//        else
//        {
//            if (result.Equals(expected))
//            {
//                pass = true;
//            }
//        }

//        if (pass)
//        {
//            Debug.Log($"<color=green>✅ {testName} 通过！</color>");
//        }
//        else
//        {
//            Debug.LogError($"<color=red>❌ {testName} 失败！ 结果: {result}, 预期: {expected}</color>");
//        }
//    }
//}