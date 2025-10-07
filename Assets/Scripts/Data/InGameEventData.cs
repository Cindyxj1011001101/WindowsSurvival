using System.Collections.Generic;

public class InGameEventData
{
    public Dictionary<string, float> eventsOnCooldown = new();
    public float trendValue = 0; // 趋势值
}