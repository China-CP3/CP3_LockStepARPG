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
}