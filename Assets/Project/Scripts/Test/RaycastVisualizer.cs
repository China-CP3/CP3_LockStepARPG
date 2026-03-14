using UnityEngine;

public class RaycastVisualizer : MonoBehaviour
{
    // 盒子的大小
    public Vector2 boxSize = new Vector2(4, 4);

    private GameObject _rayObj;
    private GameObject _hitPointObj1;
    private GameObject _hitPointObj2;

    void Start()
    {
        // 1. 生成环境（盒子 + 无限走廊）
        CreateVisuals();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 生成一个【没射中】的射线 (从左下角往上射，错开)
            FireRay(new Vector2(-8, -6), new Vector2(1, 2).normalized);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            // 生成一个【射中】的射线 (从左边直接射向中心)
            FireRay(new Vector2(-8, 0), new Vector2(1, 0.2f).normalized);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ClearRay();
        }
    }

    // --- 核心逻辑：这就是你要学的算法 ---
    // 返回值：是否撞击。 out tEnter 和 out tExit 是进入和离开的距离
    bool CheckRayIntersection(Vector2 origin, Vector2 dir, out float tEnter, out float tExit)
    {
        // 盒子边界
        float minX = -boxSize.x / 2;
        float maxX = boxSize.x / 2;
        float minY = -boxSize.y / 2;
        float maxY = boxSize.y / 2;

        // 1. 计算 X轴 (蓝色走廊) 的进入和离开
        // 距离 = (边界 - 起点) / 方向
        float tMinX = (minX - origin.x) / dir.x;
        float tMaxX = (maxX - origin.x) / dir.x;

        // 确保 tMin 是进，tMax 是出 (交换大小)
        if (tMinX > tMaxX) { float temp = tMinX; tMinX = tMaxX; tMaxX = temp; }

        // 2. 计算 Y轴 (红色走廊) 的进入和离开
        float tMinY = (minY - origin.y) / dir.y;
        float tMaxY = (maxY - origin.y) / dir.y;

        if (tMinY > tMaxY) { float temp = tMinY; tMinY = tMaxY; tMaxY = temp; }

        // 3. 求交集 (Slab Method 核心)
        // 最终进入点 = 两个进入点里【最晚】的那个 (Max)
        tEnter = Mathf.Max(tMinX, tMinY);
        // 最终离开点 = 两个离开点里【最早】的那个 (Min)
        tExit = Mathf.Min(tMaxX, tMaxY);

        // 4. 判断是否撞击
        // 如果 进入点 <= 离开点，说明有重叠，撞上了！
        // 另外还要保证离开点 > 0 (不能在射线背后)
        if (tEnter <= tExit && tExit > 0)
        {
            return true;
        }
        return false;
    }

    // ---------------------------------------------------------
    // 下面都是画图用的代码，不用深究，只负责显示
    // ---------------------------------------------------------

    void FireRay(Vector2 origin, Vector2 dir)
    {
        ClearRay();

        // 1. 运行算法
        float tEnter, tExit;
        bool hit = CheckRayIntersection(origin, dir, out tEnter, out tExit);

        // 2. 画射线
        _rayObj = new GameObject("Ray");
        LineRenderer lr = _rayObj.AddComponent<LineRenderer>();
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + dir * 20); // 画长一点

        // 射中是绿色，没射中是黄色
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = hit ? Color.green : Color.yellow;
        lr.material = mat;

        // 3. 画出计算出来的“进入点”和“离开点” (关键！)
        // 只有当计算结果合理时才画点
        if (tEnter < tExit)
        {
            _hitPointObj1 = CreatePoint(origin + dir * tEnter, Color.cyan, "EnterPoint"); // 青色是进入
            _hitPointObj2 = CreatePoint(origin + dir * tExit, Color.magenta, "ExitPoint"); // 紫色是离开
        }

        Debug.Log(hit ? "<color=green>撞上了！</color>" : "<color=red>没撞上！</color>");
    }

    void ClearRay()
    {
        if (_rayObj) Destroy(_rayObj);
        if (_hitPointObj1) Destroy(_hitPointObj1);
        if (_hitPointObj2) Destroy(_hitPointObj2);
    }

    GameObject CreatePoint(Vector2 pos, Color col, string name)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        p.name = name;
        p.transform.position = pos;
        p.transform.localScale = Vector3.one * 0.5f;
        p.GetComponent<Renderer>().material.color = col;
        return p;
    }

    void CreateVisuals()
    {
        // 1. 创建中间的盒子 (白色线框)
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "TargetBox";
        box.transform.localScale = new Vector3(boxSize.x, boxSize.y, 1);
        Destroy(box.GetComponent<BoxCollider>());
        Material boxMat = new Material(Shader.Find("Sprites/Default"));
        boxMat.color = new Color(1, 1, 1, 0.3f); // 半透明白
        box.GetComponent<Renderer>().material = boxMat;

        // 2. 创建 X轴走廊 (蓝色半透明，无限高)
        GameObject slabX = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slabX.name = "Slab_X_Blue";
        slabX.transform.localScale = new Vector3(boxSize.x, 100, 0.1f); // 宽度和盒子一样，高度无限
        slabX.transform.position = new Vector3(0, 0, 1); // 往后放一点
        Material matX = new Material(Shader.Find("Sprites/Default"));
        matX.color = new Color(0, 0, 1, 0.2f); // 蓝色半透明
        slabX.GetComponent<Renderer>().material = matX;

        // 3. 创建 Y轴走廊 (红色半透明，无限宽)
        GameObject slabY = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slabY.name = "Slab_Y_Red";
        slabY.transform.localScale = new Vector3(100, boxSize.y, 0.1f); // 高度和盒子一样，宽度无限
        slabY.transform.position = new Vector3(0, 0, 2); // 再往后放一点
        Material matY = new Material(Shader.Find("Sprites/Default"));
        matY.color = new Color(1, 0, 0, 0.2f); // 红色半透明
        slabY.GetComponent<Renderer>().material = matY;
    }
}