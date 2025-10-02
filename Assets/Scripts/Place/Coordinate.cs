using Newtonsoft.Json;
using UnityEngine;

public class Coordinate
{
    [JsonIgnore] public PlaceData Location { get; private set; }

    [JsonProperty] public float Current { get; private set; }

    public void SetLocation(PlaceData location)
    {
        Location = location;
    }

    public void SetCurrentCoordinate(float current)
    {
        Current = current;
    }

    public void Move(float distance)
    {
        Current += distance;
        Current = Mathf.Clamp(Current, Location.minCoord, Location.maxCoord);
    }

    public float DistanceTo(Coordinate other)
    {
        if (Location != other.Location) return float.MaxValue;

        return Mathf.Abs(Current - other.Current);
    }
}