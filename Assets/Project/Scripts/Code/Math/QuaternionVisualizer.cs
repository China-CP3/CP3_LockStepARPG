using UnityEngine;

public class QuaternionVisualizer : MonoBehaviour
{
    private GameObject pointV, vectorT, vectorC;
    private int step = 0;

    // --- 90度旋转的精确数学数值 ---
    private Vector3 v_initial = new Vector3(1f, 1f, 0f);
    private Vector3 qV = new Vector3(0f, 0f, 0.7071f);   // Z轴 * sin(45)
    private float qw = 0.7071f;                           // cos(45)
    private Vector3 t_vec;
    private Vector3 currentV;

    void Start() { SetupScene(); ResetDemo(); }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) NextStep();
        if (Input.GetKeyDown(KeyCode.R)) ResetDemo();
    }

    void NextStep()
    {
        step++;
        Debug.Log($"<color=white><b>>>> 旋转90度 - 第 {step} 步 <<<</b></color>");

        switch (step)
        {
            case 1:
                t_vec = Vector3.Cross(qV, v_initial);
                DrawVector(Vector3.zero, t_vec, Color.yellow, out vectorT);
                Debug.Log($"<b>[1] 基础切线:</b> {t_vec}");
                break;
            case 2:
                t_vec = t_vec * 2f;
                UpdateVectorScale(vectorT, t_vec, Vector3.zero);
                Debug.Log($"<b>[2] 翻倍后 t:</b> {t_vec} (大约是 -1.41, 1.41)");
                break;
            case 3:
                currentV = v_initial + (t_vec * qw);
                pointV.transform.position = currentV;
                Debug.Log($"<b>[3] 切线推位移:</b> 点移动到了 {currentV} (刚好在Y轴上！)");
                break;
            case 4:
                Vector3 centripetal = Vector3.Cross(qV, t_vec);
                DrawVector(currentV, centripetal, Color.magenta, out vectorC);
                currentV = currentV + centripetal;
                pointV.transform.position = currentV;
                Debug.Log($"<b>[4] 向心力修正:</b> {centripetal} (斜着指回原点)");
                Debug.Log($"<b><color=green>最终坐标:</color></b> {currentV} (成功转到 90 度位置！)");
                break;
        }
    }

    // ... (SetupScene 和 DrawVector 等辅助函数保持不变)
    void ResetDemo()
    {
        step = 0; currentV = v_initial;
        if (vectorT) Destroy(vectorT); if (vectorC) Destroy(vectorC);
        pointV.transform.position = v_initial;
        Debug.ClearDeveloperConsole();
    }

    void SetupScene()
    {
        CreateAxis(Vector3.right, Color.red); CreateAxis(Vector3.up, Color.green); CreateAxis(Vector3.forward, Color.blue);
        DrawPoint(v_initial, Color.white, out pointV);
        Camera.main.transform.position = new Vector3(0, 0, -6); Camera.main.transform.LookAt(Vector3.zero);
    }

    void CreateAxis(Vector3 dir, Color col)
    {
        GameObject axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        axis.transform.localScale = new Vector3(0.01f, 5f, 0.01f);
        axis.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        axis.GetComponent<Renderer>().material.color = col;
    }

    void DrawPoint(Vector3 pos, Color col, out GameObject obj)
    {
        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        obj.transform.position = pos;
        obj.GetComponent<Renderer>().material.color = col;
    }

    void DrawVector(Vector3 start, Vector3 vec, Color col, out GameObject obj)
    {
        obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        UpdateVectorScale(obj, vec, start);
        obj.GetComponent<Renderer>().material.color = col;
    }

    void UpdateVectorScale(GameObject obj, Vector3 vec, Vector3 start)
    {
        obj.transform.localScale = new Vector3(0.05f, vec.magnitude / 2, 0.05f);
        obj.transform.position = start + vec / 2;
        obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, vec);
    }
}