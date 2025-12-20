using UnityEngine;

public class TestInt128 : MonoBehaviour
{
    void Start()
    {
        Debug.Log("--- Int128 加法测试开始 ---");

        // --- 测试 1: 简单加法，无进位 ---
        Debug.Log("【测试1: 简单加法，无进位】");
        Int128 a1 = new Int128(0, 100);
        Int128 b1 = new Int128(0, 200);
        Int128 result1 = a1 + b1;
        Debug.Log($"{a1} + {b1} = {result1}");
        // 期望结果: [High: 0x0000000000000000, Low: 0x000000000000012C] (300的16进制是12C)
        Debug.Log("---------------------------------");


        // --- 测试 2: 关键测试！低位溢出，产生进位 ---
        Debug.Log("【测试2: 低位溢出，产生进位】");
        Int128 a2 = new Int128(0, ulong.MaxValue); // low部分是全F
        Int128 b2 = new Int128(0, 1);
        Int128 result2 = a2 + b2;
        Debug.Log($"{a2} + {b2} = {result2}");
        // 期望结果: [High: 0x0000000000000001, Low: 0x0000000000000000]
        // 解释: low位的 ulong.MaxValue + 1 溢出归零，并向high位进1。
        Debug.Log("---------------------------------");


        // --- 测试 3: 带有非零高位的加法，并产生进位 ---
        Debug.Log("【测试3: 带有高位的加法，并进位】");
        Int128 a3 = new Int128(10, ulong.MaxValue - 5);
        Int128 b3 = new Int128(20, 10);
        Int128 result3 = a3 + b3;
        Debug.Log($"{a3} + {b3} = {result3}");
        // 期望结果: [High: 0x000000000000001F, Low: 0x0000000000000004] (31的16进制是1F)
        // 解释:
        // low: (ulong.MaxValue - 5) + 10 = ulong.MaxValue + 5, 溢出后结果是 4, 产生进位 1。
        // high: 10 + 20 + (进位1) = 31。
        Debug.Log("---------------------------------");


        // --- 测试 4: 两个高位和低位都有大数值的加法 ---
        Debug.Log("【测试4: 两个大数相加】");
        Int128 a4 = new Int128(0xAAAA_AAAA_AAAA_AAAA, 0xBBBB_BBBB_BBBB_BBBB);
        Int128 b4 = new Int128(0x1111_1111_1111_1111, 0x8888_8888_8888_8888);
        Int128 result4 = a4 + b4;
        Debug.Log($"{a4} + {b4} = {result4}");
        // 期望结果: [High: 0xBBBBBBBBBBBBBBBC, Low: 0x4444444444444443]
        // 解释:
        // low: 0xBBBB... + 0x8888... = 0x14444... 溢出后结果是 0x4444..., 产生进位 1。
        // high: 0xAAAA... + 0x1111... + (进位1) = 0xBBBB... + 1 = 0xBBBB...C。
        Debug.Log("---------------------------------");
    }
}