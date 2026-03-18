using System.Collections.Generic;

public class QuadTree2D
{
    // --- 配置参数 ---
    private const int MAX_OBJECTS = 5; // 每个节点最多容纳的物体数，超过则分裂
    private const int MAX_LEVELS = 5;  // 树的最大深度

    private int level;                 // 当前层级
    private List<Collider2DBase> objects; // 当前节点存储的物体
    private Collider2DBox bounds;      // 当前节点的边界范围
    private QuadTree2D[] nodes;        // 四个子节点

    public QuadTree2D(int pLevel, Collider2DBox pBounds)
    {
        level = pLevel;
        bounds = pBounds;
        objects = new List<Collider2DBase>();
        nodes = new QuadTree2D[4];
    }

    // 1. 清空四叉树
    public void Clear()
    {
        objects.Clear();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].Clear();
                nodes[i] = null;
            }
        }
    }

    // 2. 分裂节点 (十字切分)
    private void Split()
    {
        FixedPoint subWidth = bounds.HalfWidth;
        FixedPoint subHeight = bounds.HalfHeight;
        FixedPointVector2 subSize = new FixedPointVector2(subWidth, subHeight);

        FixedPoint two = FixedPoint.CreateByInt(2);
        FixedPoint offsetX = subWidth / two;
        FixedPoint offsetY = subHeight / two;

        FixedPoint x = bounds.x;
        FixedPoint y = bounds.y;

        // 0:右上, 1:左上, 2:左下, 3:右下
        nodes[0] = new QuadTree2D(level + 1, new Collider2DBox(new FixedPointVector2(x + offsetX, y + offsetY), new FixedPointVector2(x + offsetX, y + offsetY), subSize));
        nodes[1] = new QuadTree2D(level + 1, new Collider2DBox(new FixedPointVector2(x - offsetX, y + offsetY), new FixedPointVector2(x - offsetX, y + offsetY), subSize));
        nodes[2] = new QuadTree2D(level + 1, new Collider2DBox(new FixedPointVector2(x - offsetX, y - offsetY), new FixedPointVector2(x - offsetX, y - offsetY), subSize));
        nodes[3] = new QuadTree2D(level + 1, new Collider2DBox(new FixedPointVector2(x + offsetX, y - offsetY), new FixedPointVector2(x + offsetX, y - offsetY), subSize));
    }

    // 3. 确定物体属于哪个子节点 (-1表示跨越了多个象限，留在父节点)
    private int GetIndex(Collider2DBase collider)
    {
        int index = -1;

        FixedPoint verticalMidpoint = bounds.x;
        FixedPoint horizontalMidpoint = bounds.y;

        // --- 修复点：动态获取物体的半宽和半高 ---
        FixedPoint halfWidth = FixedPoint.Zero;
        FixedPoint halfHeight = FixedPoint.Zero;

        if (collider is Collider2DBox box)
        {
            halfWidth = box.HalfWidth;
            halfHeight = box.HalfHeight;
        }
        else if (collider is Collider2DCircle circle)
        {
            // 圆形的包围盒，半宽和半高就是半径
            halfWidth = circle.radius;
            halfHeight = circle.radius;
        }

        // 这里的逻辑是：物体必须完全位于象限内
        bool topQuadrant = (collider.y - halfHeight > horizontalMidpoint);
        bool bottomQuadrant = (collider.y + halfHeight < horizontalMidpoint);

        if (collider.x - halfWidth > verticalMidpoint) // 在右侧
        {
            if (topQuadrant) index = 0;
            else if (bottomQuadrant) index = 3;
        }
        else if (collider.x + halfWidth < verticalMidpoint) // 在左侧
        {
            if (topQuadrant) index = 1;
            else if (bottomQuadrant) index = 2;
        }

        return index;
    }

    // 4. 插入物体
    public void Insert(Collider2DBase collider)
    {
        // 如果有子节点，尝试插入子节点
        if (nodes[0] != null)
        {
            int index = GetIndex(collider);
            if (index != -1)
            {
                nodes[index].Insert(collider);
                return;
            }
        }

        // 否则留在当前节点
        objects.Add(collider);

        // 如果超过容量且未达最大深度，则分裂
        if (objects.Count > MAX_OBJECTS && level < MAX_LEVELS)
        {
            if (nodes[0] == null)
            {
                Split();
            }

            int i = 0;
            while (i < objects.Count)
            {
                int index = GetIndex(objects[i]);
                if (index != -1)
                {
                    nodes[index].Insert(objects[i]);
                    objects.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    // 5. 查询：返回所有可能与该物体碰撞的候选者
    public List<Collider2DBase> Retrieve(List<Collider2DBase> returnObjects, Collider2DBase collider)
    {
        int index = GetIndex(collider);
        if (index != -1 && nodes[0] != null)
        {
            nodes[index].Retrieve(returnObjects, collider);
        }

        // 把当前节点的所有物体也加进去（因为它们可能跟子节点里的物体相撞）
        returnObjects.AddRange(objects);

        return returnObjects;
    }
}