using System.Collections.Generic;

public class PlayerData
{
    public float basicMoveDistPerMin = 0.5f;
    public List<float> moveSpeedMultiplier = new();
    public Coordinate coordinate = new();
}