using System.Collections.Generic;
using UnityEngine;

public class QuadTree2D<T> where T : Collider2DBase
{
    private const int MAX_Objs = 4;//单个节点 最多容纳4个物体
    private const int MAx_Level = 5;//最多分裂5层
    private int level;// 当前层级
    private List<T> nodeList = new List<T>();//该节点存储的所有物体
    private QuadTree2D<T>[] children = new QuadTree2D<T>[MAX_Objs];//4个子节点 从0-3 按顺序 左上 右上 左下 右下

    private FixedPointVector2 center;
    private FixedPoint width;
    private FixedPoint height;

    public QuadTree2D(FixedPointVector2 center, FixedPoint width, FixedPoint height, int level)
    {
        this.center = center;
        this.width = width;
        this.height = height;
        this.level = level;

    }

    private void Split()
    {
        if (level == MAx_Level)//层数到达极限了 不能再分裂了
        {
# if UNITY_EDITOR
            Debug.LogError("四叉树层数满了 不能再分裂");
# endif
            return;
        }

        FixedPoint halfWidth = width / FixedPoint.Two;
        FixedPoint halfHigh = height / FixedPoint.Two;
        FixedPoint quarterWidth = halfWidth / FixedPoint.Two;
        FixedPoint quarterHigh = halfHigh / FixedPoint.Two;

        FixedPointVector2 leftUpCenter = new FixedPointVector2(center.x - quarterWidth, center.y + quarterHigh);
        FixedPointVector2 rightUpCenter = new FixedPointVector2(center.x + quarterWidth, center.y + quarterHigh);
        FixedPointVector2 leftBottomCenter = new FixedPointVector2(center.x - quarterWidth, center.y - quarterHigh);
        FixedPointVector2 rightBottomCenter = new FixedPointVector2(center.x + quarterWidth, center.y - quarterHigh);

        children[0] = new QuadTree2D<T>(leftUpCenter, halfWidth, halfHigh, level + 1);
        children[1] = new QuadTree2D<T>(rightUpCenter, halfWidth, halfHigh, level + 1);
        children[2] = new QuadTree2D<T>(leftBottomCenter, halfWidth, halfHigh, level + 1);
        children[3] = new QuadTree2D<T>(rightBottomCenter, halfWidth, halfHigh, level + 1);

    }

    private void Insert(T childObj)
    {
        if (children[0] != null)//已经分裂过了 有了4个子节点 那么就找到对应的子节点 插入
        {
            int index = GetChildIndex(childObj);
            children[index].Insert(childObj);
        }
        else
        {
            if(MAX_Objs > nodeList.Count)//该节点还可以容纳更多物体
            {
                nodeList.Add(childObj);
            }
            else if(level < MAx_Level)//该节点已满 分裂到下一层
            {
                Split();
                for (int i = 0; i < nodeList.Count; i++)
                {
                    int index = GetChildIndex(nodeList[i]);
                    children[index].Insert(nodeList[i]);
                }
                nodeList.Clear();//原nodeList里的物体已经重新分配到子节点里了 不清空就重复了

                int newIndex = GetChildIndex(childObj);
                children[newIndex].Insert(childObj);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("四叉树层数满了 不能再分裂");
#endif
                return;
            }
        }
    }

    /// <summary>
    /// 物体属于4个子节点中哪一个 左上 右上 左下 右下
    /// </summary>
    private int GetChildIndex(T newChild)
    {
        bool left = newChild.x < center.x;
        bool up = newChild.y > center.y;

        if (left && up) return 0;
        if (!left && up) return 1;
        if (left && !up) return 2;
        return 3;
    }
}