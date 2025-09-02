using System.Collections.Generic;

public class StateData:VersionMigrator
{
    public bool init;
    public EnvironmentState electricity;
    public EnvironmentState waterLevel;
    public Dictionary<PlayerStateEnum, PlayerState> playerState = new();
    public override IVersionMigrator ReadJSON(string FilePath, string FileName)
    {
        return JsonManager.LoadData<StateData>(FilePath, FileName);
    }
}