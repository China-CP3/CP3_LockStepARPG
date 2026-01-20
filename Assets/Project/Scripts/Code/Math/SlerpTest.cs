using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class SlerpPreciseTest : MonoBehaviour
{
    private GameObject pivot;
    private Text debugText;
    private InputField tInputField;

    private long currentT_Raw = 0;
    private long ScaleFactor => (1L << FixedPoint.ShiftBits);

    void Awake()
    {
        EnsureEventSystem();
        CreateScene();
        CreateUI();
    }

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    void CreateScene()
    {
        pivot = new GameObject("RotationPivot");
        GameObject xAxis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        xAxis.transform.localScale = new Vector3(0.01f, 5f, 0.01f);
        xAxis.transform.rotation = Quaternion.Euler(0, 0, 90);
        xAxis.GetComponent<Renderer>().material.color = Color.gray;

        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.SetParent(pivot.transform);
        ball.transform.localPosition = new Vector3(3, 0, 0);
        ball.GetComponent<Renderer>().material.color = Color.red;
    }

    void CreateUI()
    {
        GameObject canvasObj = new GameObject("SyncCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 1. 还原左上角调试文字
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(canvasObj.transform);
        debugText = textObj.AddComponent<Text>();
        debugText.font = font;
        debugText.fontSize = 30;
        debugText.color = Color.yellow;
        RectTransform rt = debugText.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(50, -50);
        rt.sizeDelta = new Vector2(800, 600);

        // 2. 还原底部输入框 (0-100)
        GameObject inputObj = new GameObject("T_InputField");
        inputObj.transform.SetParent(canvasObj.transform);
        tInputField = inputObj.AddComponent<InputField>();
        tInputField.contentType = InputField.ContentType.IntegerNumber;

        inputObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        RectTransform inputRt = inputObj.GetComponent<RectTransform>();
        inputRt.anchorMin = new Vector2(0.5f, 0); inputRt.anchorMax = new Vector2(0.5f, 0);
        inputRt.pivot = new Vector2(0.5f, 0);
        inputRt.sizeDelta = new Vector2(300, 80);
        inputRt.anchoredPosition = new Vector2(-120, 100);

        GameObject childText = new GameObject("Text");
        childText.transform.SetParent(inputObj.transform);
        Text t = childText.AddComponent<Text>();
        t.font = font; t.fontSize = 40; t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        tInputField.textComponent = t;
        ((RectTransform)childText.transform).sizeDelta = inputRt.sizeDelta;

        GameObject holderObj = new GameObject("Placeholder");
        holderObj.transform.SetParent(inputObj.transform);
        Text holder = holderObj.AddComponent<Text>();
        holder.font = font; holder.text = "进度0-100...";
        holder.color = Color.gray; holder.alignment = TextAnchor.MiddleCenter;
        tInputField.placeholder = holder;
        ((RectTransform)holderObj.transform).sizeDelta = inputRt.sizeDelta;

        tInputField.onValueChanged.AddListener(OnInputValueChanged);

        // 3. 还原右下角重置按钮
        GameObject btnObj = new GameObject("ResetBtn");
        btnObj.transform.SetParent(canvasObj.transform);
        Button btn = btnObj.AddComponent<Button>();
        btnObj.AddComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform);
        Text bt = btnTextObj.AddComponent<Text>();
        bt.font = font; bt.text = "RESET"; bt.color = Color.white;
        bt.alignment = TextAnchor.MiddleCenter; bt.fontSize = 30;

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0); btnRt.anchorMax = new Vector2(0.5f, 0);
        btnRt.pivot = new Vector2(0.5f, 0);
        btnRt.sizeDelta = new Vector2(180, 80);
        btnRt.anchoredPosition = new Vector2(150, 100);
        ((RectTransform)btnTextObj.transform).sizeDelta = btnRt.sizeDelta;

        btn.onClick.AddListener(ResetValue);
    }

    void OnInputValueChanged(string val)
    {
        if (float.TryParse(val, out float percent))
        {
            // 这里修改逻辑：输入 50，对应 0.5 * ScaleFactor
            float normalized = Mathf.Clamp(percent, 0f, 100f) / 100f;
            currentT_Raw = (long)(normalized * ScaleFactor);
        }
    }

    void ResetValue()
    {
        currentT_Raw = 0;
        tInputField.text = "";
    }

    void Update()
    {
        FixedPointQuaternion qStart = FixedPointQuaternion.Identity;
        FixedPointQuaternion qEnd = FixedPointQuaternion.AngleAxis(1800, FixedPointVector3.Forward);

        FixedPoint tFP = FixedPoint.CreateByScaledValue(currentT_Raw);
        FixedPointQuaternion result = FixedPointQuaternion.Slerp(qStart, qEnd, tFP);

        if (pivot != null) pivot.transform.rotation = ToUnityQuaternion(result);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"输入进度: {tInputField.text}%");
        sb.AppendLine($"转换后的 Raw_T: {currentT_Raw}");
        sb.AppendLine("--------------------------------");
        sb.AppendLine($"Q_W_Raw: {result.w.ScaledValue}");
        sb.AppendLine($"Q_Z_Raw: {result.z.ScaledValue}");
        sb.AppendLine("--------------------------------");
        sb.AppendLine($"Unity角度: {pivot.transform.rotation.eulerAngles.z:F3}°");

        debugText.text = sb.ToString();
    }

    private Quaternion ToUnityQuaternion(FixedPointQuaternion q)
    {
        float f = (float)ScaleFactor;
        return new Quaternion((float)q.x.ScaledValue / f, (float)q.y.ScaledValue / f, (float)q.z.ScaledValue / f, (float)q.w.ScaledValue / f);
    }
}