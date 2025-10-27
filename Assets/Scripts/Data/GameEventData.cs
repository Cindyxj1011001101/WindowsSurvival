using System.Collections.Generic;

public class GameEventData
{
    public float trendValue; // 趋势值
    public InvasionEventConfig invasionConfig = new(); // 入侵事件配置
    public Dictionary<string, float> eventsOnCooldown = new();
    public Dictionary<string, GameEvent> ongoingEvents = new();
}