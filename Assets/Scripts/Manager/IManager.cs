public interface IManager
{
    /// <summary>
    /// 初始化。进入新存档时调用
    /// </summary>
    void Init();
    /// <summary>
    /// 重置。离开当前存档时调用
    /// </summary>
    void Reset();
}