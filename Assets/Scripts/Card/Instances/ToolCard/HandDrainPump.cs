/// <summary>
/// 手压排水泵
/// </summary>
public class HandDrainPump : ToolCard
{
    private HandDrainPump()
    {
        Events = new()
        {
            new Event("手压排水", "手压排水", Event_Drain, Judge_Drain, () => 30, () => new() { { PlayerStateEnum.Sobriety, -3 } }, () => new(){ { EnvironmentStateEnum.WaterLevel, -7 } }),
        };

    }

    private void Event_Drain(out string tip)
    {
        tip = string.Empty;
        Use();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -3);
        StateManager.Instance.ChangeWaterLevel(-7);
        TimeManager.Instance.AddTime(30);

    }
    private bool Judge_Drain(out string hint)
    {
        hint = string.Empty;
        if (StateManager.Instance.WaterLevel.CurValue <= 0)
        {
            hint = "当前水位为0，无需排水";
            return false;
        }
        return true;
    }
}