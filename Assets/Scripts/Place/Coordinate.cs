using Newtonsoft.Json;
using UnityEngine;

public class Coordinate
{
    [JsonIgnore] public EnvironmentBag Location { get; private set; } // 地点

    [JsonProperty] public float Position { get; private set; } // 位置

    public void SetLocation(EnvironmentBag location)
    {
        Location = location;
    }

    public void SetPosition(float position)
    {
        Position = position;
    }

    public float DistanceTo(Coordinate other)
    {
        if (Location != other.Location) return float.MaxValue;

        return Mathf.Abs(Position - other.Position);
    }
}