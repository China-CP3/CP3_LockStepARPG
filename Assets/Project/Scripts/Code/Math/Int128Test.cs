//using UnityEngine;

//public class Int128Test : MonoBehaviour
//{
//    void Start()
//    {
//        // 基础赋值
//        Int128 a = 1234567890123456789L;
//        Int128 b = Int128.Parse("98765432109876543210987654321");

//        // 算术运算
//        Int128 sum = a + b;
//        Int128 product = a * b;
//        Int128 quotient = b / a;

//        // 比较
//        if (sum > Int128.MaxValue / 2)
//        {
//            Debug.Log($"Sum: {sum} 超过Int128最大值的一半");
//        }

//        // 转换
//        if (a.IsInInt64Range())
//        {
//            long aLong = (long)a;
//            Debug.Log($"a转换为long: {aLong}");
//        }

//        // 输出
//        Debug.Log($"a = {a}");
//        Debug.Log($"b = {b}");
//        Debug.Log($"a + b = {sum}");
//        Debug.Log($"a * b = {product}");
//    }
//}