/// <summary>
/// 手压排水泵
/// </summary>
public class HandDrainPump : Card
{
    private HandDrainPump()
    {
        Events = new()
        {
            new CardEvent("手压排水", "手压排水", Event_Drain, Judge_Drain, () => 30, () => new() { { PlayerStateEnum.Sobriety, -3 } }, () => new(){ { EnvironmentStateEnum.WaterLevel, -9 } }),
        };

    }

    private void Event_Drain(out string tip)
    {
        tip = string.Empty;
        // 播放吃的音效
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("凿_01",true);
        Use();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -3);
        StateManager.Instance.ChangeWaterLevel(-9);
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