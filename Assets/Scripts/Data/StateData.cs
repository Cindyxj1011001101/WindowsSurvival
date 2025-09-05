using System.Collections.Generic;

public class StateData
{
    public bool init;
    public State electricity;
    public State waterLevel;
    public Dictionary<PlayerStateEnum, State> playerState = new();
}