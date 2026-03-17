using UnityEngine;

public class RayCastBoxTester : MonoBehaviour
{
    // 你的盒子
    private Collider2DBox testBox;

    // 射线显示状态
    private bool isRayActive = false;
    private FixedPointVector2 currentStart;
    private FixedPointVector2 currentDir;
    private FixedPoint currentDist;
    private bool currentHitResult;

    void Start()
    {
        // 初始化测试盒子：中心点(0,0)，逻辑点(0,0)，尺寸宽2高2 (X范围[-1,1], Y范围[-1,1])
        FixedPointVector2 centerPos = new FixedPointVector2(FixedPoint.CreateByInt(0), FixedPoint.CreateByInt(0));
        FixedPointVector2 logicPos = new FixedPointVector2(FixedPoint.CreateByInt(0), FixedPoint.CreateByInt(0));
        FixedPointVector2 size = new FixedPointVector2(FixedPoint.CreateByInt(2), FixedPoint.CreateByInt(2));

        testBox = new Collider2DBox(centerPos, logicPos, size);
        //testBox.Active = true;

        Debug.Log("🎯 测试环境已就绪！请在运行状态下按 Q(随机生成射线), E(清空)");
    }

    void Update()
    {
        // 按下 Q：50%概率命中，50%概率不命中
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GenerateRandomRay();
        }

        // 按下 E：清空射线
        if (Input.GetKeyDown(KeyCode.E))
        {
            isRayActive = false;
            Debug.Log("🧹 已清空射线显示");
        }
    }

    private void GenerateRandomRay()
    {
        // 1. 抛硬币决定这次是否命中 (50% 概率)
        bool forceHit = Random.value > 0.5f;

        int startX = 0, startY = 0;
        int dirX = 0, dirY = 0;

        // 2. 随机决定从哪个方向射击 (0:从左往右, 1:从右往左, 2:从下往上, 3:从上往下)
        int shootDirection = Random.Range(0, 4);

        if (shootDirection == 0) // 从左往右射击
        {
            startX = -5; dirX = 1; dirY = 0;
            // 如果必中，Y坐标在 [-1, 1] 之间；如果必不中，Y坐标在 [2, 5] 或 [-5, -2] 之间
            startY = forceHit ? Random.Range(-1, 2) : (Random.value > 0.5f ? Random.Range(2, 6) : Random.Range(-5, -2));
        }
        else if (shootDirection == 1) // 从右往左射击
        {
            startX = 5; dirX = -1; dirY = 0;
            startY = forceHit ? Random.Range(-1, 2) : (Random.value > 0.5f ? Random.Range(2, 6) : Random.Range(-5, -2));
        }
        else if (shootDirection == 2) // 从下往上射击
        {
            startY = -5; dirX = 0; dirY = 1;
            // 如果必中，X坐标在 [-1, 1] 之间；如果必不中，X坐标在 [2, 5] 或 [-5, -2] 之间
            startX = forceHit ? Random.Range(-1, 2) : (Random.value > 0.5f ? Random.Range(2, 6) : Random.Range(-5, -2));
        }
        else // 从上往下射击
        {
            startY = 5; dirX = 0; dirY = -1;
            startX = forceHit ? Random.Range(-1, 2) : (Random.value > 0.5f ? Random.Range(2, 6) : Random.Range(-5, -2));
        }

        // 3. 赋值给定点数
        currentStart = new FixedPointVector2(FixedPoint.CreateByInt(startX), FixedPoint.CreateByInt(startY));
        currentDir = new FixedPointVector2(FixedPoint.CreateByInt(dirX), FixedPoint.CreateByInt(dirY));
        currentDist = FixedPoint.CreateByInt(10); // 射程固定为10

        // 4. 执行检测并打印日志
        isRayActive = true;
        currentHitResult = RayCastBox(testBox, currentStart, currentDir, currentDist);

        string resultText = currentHitResult ? "<color=#00FF00><b>[发生碰撞]</b></color>" : "<color=#FF0000><b>[未碰撞]</b></color>";
        Debug.Log($"🎲 随机射线测试 -> {resultText} (起点: {startX},{startY} | 方向: {dirX},{dirY})");
    }

    // 使用 Gizmos 在 Scene 窗口画出盒子和射线
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || testBox == null) return;

        // 画出盒子 (白色)
        Gizmos.color = Color.white;

        // 【注意】如果你的定点数库不支持 (float) 强转，请改成你库里的转换方法，例如：testBox.x.AsFloat()
        float bx = (float)testBox.x;
        float by = (float)testBox.y;
        float hw = (float)testBox.HalfWidth;
        float hh = (float)testBox.HalfHeight;
        Gizmos.DrawWireCube(new Vector3(bx, by, 0), new Vector3(hw * 2, hh * 2, 0));

        // 画出射线 (命中为绿色，未命中为红色)
        if (isRayActive)
        {
            Gizmos.color = currentHitResult ? Color.green : Color.red;

            float sx = (float)currentStart.x;
            float sy = (float)currentStart.y;
            float dx = (float)currentDir.x;
            float dy = (float)currentDir.y;
            float dist = (float)currentDist;

            Vector3 startPos = new Vector3(sx, sy, 0);
            Vector3 endPos = startPos + new Vector3(dx, dy, 0).normalized * dist;

            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawSphere(startPos, 0.2f); // 画个小球代表起点
        }
    }

    // =================================================================================
    // 你的核心检测代码 (保持不变)
    // =================================================================================
    public static bool RayCastBox(Collider2DBox targetBox, FixedPointVector2 startPos, FixedPointVector2 direction, FixedPoint distance)
    {
        if (!targetBox.Active)
        {
            return false;
        }

        FixedPoint minX = targetBox.x - targetBox.HalfWidth;
        FixedPoint maxX = targetBox.x + targetBox.HalfWidth;
        FixedPoint minY = targetBox.y - targetBox.HalfHeight;
        FixedPoint maxY = targetBox.y + targetBox.HalfHeight;

        FixedPoint xEnterTime;
        FixedPoint xExitTime;
        if (direction.x == FixedPoint.Zero)
        {
            if (startPos.x < minX || startPos.x > maxX) return false;

            xEnterTime = FixedPoint.MinValue;
            xExitTime = FixedPoint.MaxValue;
        }
        else
        {
            FixedPoint xTimeA = (minX - startPos.x) / direction.x;
            FixedPoint xTimeB = (maxX - startPos.x) / direction.x;
            xEnterTime = FixedPointMath.Min(xTimeA, xTimeB);
            xExitTime = FixedPointMath.Max(xTimeA, xTimeB);
        }

        FixedPoint yEnterTime;
        FixedPoint yExitTime;
        if (direction.y == FixedPoint.Zero)
        {
            if (startPos.y < minY || startPos.y > maxY) return false;

            yEnterTime = FixedPoint.MinValue;
            yExitTime = FixedPoint.MaxValue;
        }
        else
        {
            FixedPoint yTimeA = (minY - startPos.y) / direction.y;
            FixedPoint yTimeB = (maxY - startPos.y) / direction.y;
            yEnterTime = FixedPointMath.Min(yTimeA, yTimeB);
            yExitTime = FixedPointMath.Max(yTimeA, yTimeB);
        }

        FixedPoint finalEnterTime = FixedPointMath.Max(xEnterTime, yEnterTime);
        FixedPoint finalExitTime = FixedPointMath.Min(xExitTime, yExitTime);

        bool hasIntersection = finalEnterTime <= finalExitTime;
        bool isInFront = finalExitTime >= FixedPoint.Zero;
        bool isWithinRange = finalEnterTime <= distance;

        return hasIntersection && isInFront && isWithinRange;
    }
}