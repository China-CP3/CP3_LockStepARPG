using System.Collections.Generic;
using UnityEngine;

public class QuadTree2D<T> where T : Collider2DBase
{
    private const int MAX_Objs = 4;//单个节点 最多容纳4个物体
    private const int MAx_Level = 5;//最多分裂5层
    private int level;// 当前层级
    private List<T> nodeList = new List<T>();//所有的节点
    private QuadTree2D<T>[] children = new QuadTree2D<T>[MAX_Objs];

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

    }
}