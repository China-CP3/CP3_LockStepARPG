using UnityEngine;

public class PlayerBoxController : MonoBehaviour
{
    // 玩家方块（红色，可动）
    private Collider2DBox playerBox;
    // 障碍方块（绿色，静止）
    private Collider2DBox obstacleBox;

    [Header("设置")]
    public float moveSpeed = 5.0f;
    public Vector2 playerSize = new Vector2(1, 1);
    public Vector2 obstacleSize = new Vector2(2, 2);
    public Vector2 obstaclePos = new Vector2(0, 0);

    void Start()
    {
        // 1. 初始化障碍物 (静止)
        FixedPointVector2 sizeB = new FixedPointVector2(FixedPoint.CreateByFloat(obstacleSize.x), FixedPoint.CreateByFloat(obstacleSize.y));
        FixedPointVector2 posB = new FixedPointVector2(FixedPoint.CreateByFloat(obstaclePos.x), FixedPoint.CreateByFloat(obstaclePos.y));
        obstacleBox = new Collider2DBox(posB, posB, sizeB);

        // 2. 初始化玩家 (起始位置在左边)
        FixedPointVector2 sizeA = new FixedPointVector2(FixedPoint.CreateByFloat(playerSize.x), FixedPoint.CreateByFloat(playerSize.y));
        FixedPointVector2 posA = new FixedPointVector2(FixedPoint.CreateByFloat(-3), FixedPoint.CreateByFloat(0));
        playerBox = new Collider2DBox(posA, posA, sizeA);

        Debug.Log("🎮 游戏开始！使用 WASD 或 方向键 控制红色方块撞击绿色方块。");
    }

    void Update()
    {
        if (playerBox == null || obstacleBox == null) return;

        // --- 1. 处理输入移动 ---
        float h = Input.GetAxis("Horizontal"); // A/D 或 左/右
        float v = Input.GetAxis("Vertical");   // W/S 或 上/下

        if (h != 0 || v != 0)
        {
            // 计算这一帧的位移
            float dt = Time.deltaTime;
            float moveX = h * moveSpeed * dt;
            float moveY = v * moveSpeed * dt;

            // 更新玩家逻辑位置 (先移动，再检测)
            FixedPoint currentX = playerBox.x;
            FixedPoint currentY = playerBox.y;

            FixedPoint nextX = currentX + FixedPoint.CreateByFloat(moveX);
            FixedPoint nextY = currentY + FixedPoint.CreateByFloat(moveY);

            playerBox.UpdateLogicPos(new FixedPointVector2(nextX, nextY));
        }

        // --- 2. 碰撞检测与修正 ---
        // 传入 true，表示如果撞了，请帮我把 playerBox 拉出来
        bool isHit = Collider2DDetectTool.DetectCollider(playerBox, obstacleBox, true);

        if (isHit)
        {
            Debug.Log($"<color=red>💥 发生碰撞！</color> 修正后位置: ({playerBox.x.ScaledValue}, {playerBox.y.ScaledValue})");
        }
    }

    // --- 3. 可视化 (画出来) ---
    void OnDrawGizmos()
    {
        if (playerBox != null)
        {
            Gizmos.color = Color.red;
            DrawBox(playerBox);
        }

        if (obstacleBox != null)
        {
            Gizmos.color = Color.green;
            DrawBox(obstacleBox);
        }
    }

    // 辅助函数：把 FixedPoint 的 Box 画成 Unity 的 Gizmos
    void DrawBox(Collider2DBox box)
    {
        // 把 FixedPoint 转回 float 用于显示
        float x = (float)box.x; // 假设你有显式转换，或者用 (float)box.LogicPos.x
        float y = (float)box.y;
        float w = (float)box.Size.x; // 或者 box.HalfWidth * 2
        float h = (float)box.Size.y;

        Gizmos.DrawWireCube(new Vector3(x, y, 0), new Vector3(w, h, 0));

        // 画个半透明的实心，方便看
        Color c = Gizmos.color;
        c.a = 0.3f;
        Gizmos.color = c;
        Gizmos.DrawCube(new Vector3(x, y, 0), new Vector3(w, h, 0));
    }
}