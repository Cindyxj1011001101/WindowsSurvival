using System.Collections.Generic;

public class InGameEventData
{
    public float trendValue = 0; // 趋势值
    public InvasionEventConfig invasionConfig = new(); // 入侵事件配置
    public Dictionary<string, float> eventsOnCooldown = new();
    public Dictionary<string, GameEvent> ongoingEvents = new();
}