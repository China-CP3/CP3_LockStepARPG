public interface ILogicEntity
{
    /// <summary>
    /// 逻辑帧更新 15帧每秒
    /// </summary>
    void LogicUpdate(int frameId);

    /// <summary>
    /// 渲染帧更新 60帧每秒
    /// </summary>
    void RenderUpdate(float interpolation);

}