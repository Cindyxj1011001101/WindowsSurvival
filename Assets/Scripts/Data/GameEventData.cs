using System.Collections.Generic;

public class GameEventData
{
    public bool init;
    public float trendValue; // 趋势值
    public InvasionEventConfig invasionConfig = new(); // 入侵事件配置
    public Dictionary<string, GameEvent> allEvents = new(); // 所有事件
}