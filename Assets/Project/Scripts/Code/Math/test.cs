using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 创建两个MyInt8
        MyInt8 a = new MyInt8(37);  // 37 = 2*16 + 5
        MyInt8 b = new MyInt8(20);  // 20 = 1*16 + 4
        MyInt8 c = new MyInt8(12);  // 12 = 0*16 + 12

        Debug.Log("a = " + a.ToString());
        Debug.Log("b = " + b.ToString());
        Debug.Log("c = " + c.ToString());

        // 测试不进位的加法
        MyInt8 result1 = a + b;
        Debug.Log($"a + b = {result1.ToString()}  (真实值: 37 + 20 = {37 + 20})");

        // 测试带进位的加法
        MyInt8 result2 = a + c;
        Debug.Log($"a + c = {result2.ToString()}  (真实值: 37 + 12 = {37 + 12})");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
