using Newtonsoft.Json;
using UnityEngine;

public class Coordinate
{
    [JsonIgnore] public EnvironmentBag Location { get; private set; } // 地点

    [JsonProperty] public float Position { get; private set; } // 位置

    [JsonIgnore] public bool IsAtBoundary => Position == Location.PlaceData.minCoord || Position == Location.PlaceData.maxCoord; // 是否处于边界

    public void SetLocation(EnvironmentBag location)
    {
        Location = location;
    }

    public void SetPosition(float position)
    {
        Position = position;
        Position = Mathf.Clamp(Position, Location.PlaceData.minCoord, Location.PlaceData.maxCoord);
    }

    public float DistanceTo(Coordinate other)
    {
        if (Location != other.Location) return float.MaxValue;

        return Mathf.Abs(Position - other.Position);
    }

    public int DirectionTo(Coordinate other)
    {
        if (Location != other.Location) return 0;

        if (other.Position > Position) return 1;

        if (other.Position < Position) return -1;

        // 在同一位置

        // 中间位置
        var middle = (Location.PlaceData.maxCoord - Location.PlaceData.minCoord) / 2;
        
        // 在中间位置靠左，则向左
        if (Position < middle) return -1;
        // 在中间位置靠右，则向右
        if (Position > middle) return 1;

        // 在中间位置
        // 随机返回一个方向
        return Random.value > .5f ? 1 : -1;
    }

    public void Move(float dist)
    {
        SetPosition(Position + dist);
    }

    public void MoveTowards(Coordinate other, float dist, bool stopAfterReach = true)
    {
        if (stopAfterReach)
        {
            dist = Mathf.Min(dist, DistanceTo(other));
        }
        Move(DirectionTo(other) * dist);
    }

    public void MoveAwayFrom(Coordinate other, float dist)
    {
        Move(-DirectionTo(other) * dist);
    }
}