using System.Collections.Generic;

public class StateData
{
    public bool init;
    public EnvironmentState electricity;
    public EnvironmentState waterLevel;
    public Dictionary<PlayerStateEnum, PlayerState> playerState = new();
    public float maxLoad;
    public float curLoad;
}