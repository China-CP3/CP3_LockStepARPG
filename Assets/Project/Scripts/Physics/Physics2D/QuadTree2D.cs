using System.Collections.Generic;
using UnityEngine;

public class QuadTree2D <T> where T : Collider2DBase
{
    private int MAX_Objs = 4;//单个节点 最多容纳4个物体
    private int MAx_Level = 5;//最多分裂5层
    private List<T> nodeList = new List<T>();//所有的节点


}