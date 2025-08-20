/// <summary>
/// 手压排水泵
/// </summary>
public class HandDrainPump : Card
{
    private HandDrainPump()
    {
        Events = new()
        {
            new Event("手压排水", "手压排水", Event_Drain, Judge_Drain),
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
        if (!GameManager.Instance.CurEnvironmentBag.PlaceData.isInSpacecraft || StateManager.Instance.WaterLevel.CurValue <= 0)
        {
            hint = "你无需在此地排水";
            return false;
        }
        return true;
    }
}