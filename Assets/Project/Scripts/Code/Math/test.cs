using UnityEngine;

public class TestInt128 : MonoBehaviour
{
    void Start()
    {
        TestSubtraction();
    }
    void TestSubtraction()
    {
        Debug.Log("--- Int128 减法测试开始 (十六进制验证) ---");

        // --- 测试 1: 简单减法，无借位 ---
        Debug.Log("【测试1: 简单减法，无借位】");
        Int128 a1 = new Int128(0, 300); // low: 0x12C
        Int128 b1 = new Int128(0, 100); // low: 0x64
        Int128 result1 = a1 - b1;
        Debug.Log($"{a1} - {b1} = {result1}");
        // 期望结果: [High: 0x0000000000000000, Low: 0x00000000000000C8] (200的16进制是C8)
        Debug.Log("---------------------------------");


        // --- 测试 2: 关键测试！低位不够减，需要向高位借位 ---
        Debug.Log("【测试2: 低位不够减，需要借位】");
        Int128 a2 = new Int128(1, 0);
        Int128 b2 = new Int128(0, 1);
        Int128 result2 = a2 - b2;
        Debug.Log($"{a2} - {b2} = {result2}");
        // 期望结果: [High: 0x0000000000000000, Low: 0xFFFFFFFFFFFFFFFF]
        // 解释: high位的1被借走变0。low位的0-1发生环绕，变成ulong.MaxValue。
        Debug.Log("---------------------------------");


        // --- 测试 3: 带有非零高位的减法，并需要借位 ---
        Debug.Log("【测试3: 带有高位的减法，并借位】");
        Int128 a3 = new Int128(0x1F, 0x04); // high:31, low:4
        Int128 b3 = new Int128(0x14, 0x0A); // high:20, low:10
        Int128 result3 = a3 - b3;
        Debug.Log($"{a3} - {b3} = {result3}");
        // 期望结果: [High: 0x000000000000000A, Low: 0xFFFFFFFFFFFFFFFA]
        // 解释:
        // low: 4 - 10, 不够减，发生借位。结果是 ulong.MaxValue - 5 (0x...FA)。
        // high: (31 - 1(借位)) - 20 = 10 (0x...A)。
        Debug.Log("---------------------------------");


        // --- 测试 4: 两个大数相减 (加法测试4的逆运算) ---
        Debug.Log("【测试4: 两个大数相减】");
        Int128 a4 = new Int128(0xBBBB_BBBB_BBBB_BBBC, 0x4444_4444_4444_4443);
        Int128 b4 = new Int128(0x1111_1111_1111_1111, 0x8888_8888_8888_8888);
        Int128 result4 = a4 - b4;
        Debug.Log($"{a4} - {b4} = {result4}");
        // 期望结果: [High: 0xAAAAAAAAAAAAAAAA, Low: 0xBBBBBBBBBBBBBBBB]
        // 解释: 这应该精确地得到我们加法测试4中的第一个加数。
        Debug.Log("---------------------------------");
    }
}