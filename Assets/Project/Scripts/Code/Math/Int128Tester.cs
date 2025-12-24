using UnityEngine;

public class Int128Tester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("--- Int128 Left Shift (<<) Final Corrected Test Suite v2 ---");

        // ... 其他测试保持不变 ...
        TestShift(new Int128(0x1234, 0x5678), 0, new Int128(0x1234, 0x5678), "Test 1: shift == 0");
        TestShift(new Int128(0, 0x8000_0000_0000_0000), 1, new Int128(1, 0), "Test 2: shift < 64 (low to high carry)");
        TestShift(new Int128(0x1122_3344_5566_7788, 0x99AA_BBCC_DDEE_FF00), 8, new Int128(0x2233_4455_6677_8899, 0xAABB_CCDD_EEFF_0000), "Test 3: shift < 64 (general)");
        TestShift(new Int128(0x1234, 0xABCD_EF01_2345_6789), 64, new Int128(unchecked((long)0xABCD_EF01_2345_6789), 0), "Test 4: shift == 64");
        TestShift(new Int128(0, 0x00FF_EEDD_CCBB_AA99), 72, new Int128(unchecked((long)0xFFEEDDCCBBAA9900), 0), "Test 5: shift > 64");

        // Test 6: 修正了预期值！
        // 输入的最高位(bit 63)左移127位后，会超出128位范围，所以结果是0。
        TestShift(
            new Int128(0, 0x8000_0000_0000_0000), 127,
            Int128.Zero, // <-- 修正后的正确预期值
            "Test 6: shift > 64 (bit shifted out of bounds)"
        );

        // 额外增加一个测试，来验证我之前意图的场景
        TestShift(
            new Int128(0, 1), 127,
            new Int128(long.MinValue, 0), // 1 左移 127 位，会到达符号位
            "Test 6b: shift > 64 (1 shifted to sign bit)"
        );

        // ... 其他测试保持不变 ...
        TestShift(new Int128(long.MaxValue, ulong.MaxValue), 128, Int128.Zero, "Test 7: shift >= 128");
        TestShift(new Int128(long.MaxValue, ulong.MaxValue), 200, Int128.Zero, "Test 8: shift > 128");

        Debug.Log("--- Test Suite Finished ---");
    }

    // 辅助函数无需改动
    void TestShift(Int128 value, int shift, Int128 expected, string testName)
    {
        Debug.Log($"--- {testName} ---");
        Debug.Log($"Original: {value.ToString()} << {shift}");

        Int128 actual = value << shift;

        if (actual == expected)
        {
            Debug.Log($"<color=green>PASSED</color>: Result is {actual.ToString()}");
        }
        else
        {
            Debug.Log($"<color=red>FAILED</color>");
            Debug.Log($"  Expected: {expected.ToString()}");
            Debug.Log($"  Actual:   {actual.ToString()}");
        }
        Debug.Log("--------------------------");
    }
}