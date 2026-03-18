using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree2D
{
    private int MAX_OBJECTS = 4; // 一个格子里最多装几个物体？超过就分裂
    private int MAX_LEVELS = 5;  // 树最深分几层？防止无限细分

    private int level;              // 当前是第几层（0是根节点）
    private List<Collider2DBase> objects; // 当前格子里装的物体列表
    private Collider2DBox bounds;   // 当前格子的物理范围
    private QuadTree2D[] nodes;     // 4个子节点（0:右上, 1:左上, 2:左下, 3:右下）

    public QuadTree2D(int pLevel, Collider2DBox pBounds)
    {
        this.level = pLevel;
        this.bounds = pBounds;
        this.objects = new List<Collider2DBase>();
        this.nodes = new QuadTree2D[4]; // 初始化数组，但先不创建子节点对象
    }


}
