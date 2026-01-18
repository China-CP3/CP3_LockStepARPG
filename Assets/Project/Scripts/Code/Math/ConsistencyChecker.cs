using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Security.Cryptography;

public class ConsistencyChecker : MonoBehaviour
{
    public Text logText; // 请在 Inspector 中拖入你的 UI Text

    void Start()
    {
        if (logText == null) return;

        StringBuilder sb = new StringBuilder();

        // --- 1. 基础定点数运算测试 ---
        FixedPoint a = FixedPoint.CreateByDouble(10.5);
        FixedPoint b = FixedPoint.CreateByDouble(3.2);
        FixedPoint resDiv = a / b; // 测试 Int128 位移与除法

        // --- 2. 2D 向量与三角函数查表测试 ---
        FixedPointVector2 v2 = new FixedPointVector2(FixedPoint.One, FixedPoint.Zero);
        FixedPointVector2 v2Rot = FixedPointVector2.Rotate(v2, 450); // 旋转 45.0 度，测试 SinTable 和旋转矩阵

        // --- 3. 四元数与 3D 空间旋转测试 ---
        FixedPointVector3 axis = new FixedPointVector3(FixedPoint.Zero, FixedPoint.One, FixedPoint.Zero);
        // 绕 Y 轴旋转 90.0 度 (传入 900)
        FixedPointQuaternion q = FixedPointQuaternion.AngleAxis(900, axis);
        FixedPointVector3 pos = new FixedPointVector3(FixedPoint.One, FixedPoint.Zero, FixedPoint.Zero);
        FixedPointVector3 rotatedPos = q * pos; // 测试四元数乘法与叉乘

        // --- 4. 复杂数学算法测试 ---
        FixedPoint sqrtVal = FixedPointMath.Sqrt(FixedPoint.CreateByInt(2)); // 测试牛顿迭代法与位发现算法

        // --- 汇总所有原始 ScaledValue (特征值) ---
        string rawData = $"{resDiv.ScaledValue}_{v2Rot.x.ScaledValue}_{v2Rot.y.ScaledValue}_" +
                         $"{rotatedPos.x.ScaledValue}_{rotatedPos.z.ScaledValue}_{sqrtVal.ScaledValue}";

        // 生成最终指纹 (MD5)
        string md5Hash = GetMd5Hash(rawData);

        // --- 格式化打印输出 ---
        sb.AppendLine("<size=45><b>【同步校验结果】</b></size>");
        sb.AppendLine($"<color=yellow>最终哈希(MD5): {md5Hash}</color>");
        sb.AppendLine("------------------------------------------");
        sb.AppendLine("<b>原始内存值对比 (ScaledValue):</b>");
        sb.AppendLine($"1. 除法结果 (10.5/3.2): {resDiv.ScaledValue}");
        sb.AppendLine($"2. 2D旋转后X (45度): {v2Rot.x.ScaledValue}");
        sb.AppendLine($"3. 2D旋转后Y (45度): {v2Rot.y.ScaledValue}");
        sb.AppendLine($"4. 3D旋转后X (绕Y轴90度): {rotatedPos.x.ScaledValue}");
        sb.AppendLine($"5. 3D旋转后Z (绕Y轴90度): {rotatedPos.z.ScaledValue}");
        sb.AppendLine($"6. 开方结果 (Sqrt 2): {sqrtVal.ScaledValue}");
        sb.AppendLine("------------------------------------------");
        sb.AppendLine($"设备型号: {SystemInfo.deviceModel}");
        sb.AppendLine($"系统版本: {SystemInfo.operatingSystem}");

        logText.text = sb.ToString();

        // 同时输出到控制台方便查看
        Debug.Log($"定点数特征字符串: {rawData}");
        Debug.Log($"定点数最终哈希: {md5Hash}");
    }

    private string GetMd5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("X2"));
            return sb.ToString();
        }
    }
}