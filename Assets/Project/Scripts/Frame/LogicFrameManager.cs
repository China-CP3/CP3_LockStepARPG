using System.Collections.Generic;
using UnityEngine;

public class LogicFrameManager : MonoBehaviour
{
    public static LogicFrameManager Instance { get; private set; }
    public const int LOGIC_FRAME_RATE = 15;  // 逻辑帧 每秒15帧
    private const float FRAME_TIME = 1f / LOGIC_FRAME_RATE;  // 每帧时间 大约0.66
    private int currentFrame = 0;
    private float accumulator = 0f;// 累积时间

    // 逻辑实体列表
    private List<ILogicEntity> logicEntities = new List<ILogicEntity>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.targetFrameRate = 60;// 设置目标帧率为60fps
            QualitySettings.vSyncCount = 0;// 关闭垂直同步 让targetFrameRate生效
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        accumulator += Time.deltaTime;

        while (accumulator >= FRAME_TIME)
        {
            LogicUpdate(currentFrame);
            currentFrame++;
            accumulator -= FRAME_TIME;
        }

        float interpolation = accumulator / FRAME_TIME;//求插值比例
        RenderUpdate(interpolation);
    }

    private void LogicUpdate(int frameId)
    {
        for (int i = 0; i < logicEntities.Count; i++)
        {
            logicEntities[i].LogicUpdate(frameId);
        }
    }

    private void RenderUpdate(float interpolation)
    {
        for (int i = 0; i < logicEntities.Count; i++)
        {
            logicEntities[i].RenderUpdate(interpolation);
        }
    }

    public void RegisterEntity(ILogicEntity entity)
    {
        if (!logicEntities.Contains(entity))
        {
            logicEntities.Add(entity);
        }
    }

    public void UnregisterEntity(ILogicEntity entity)
    {
        logicEntities.Remove(entity);
    }

}
