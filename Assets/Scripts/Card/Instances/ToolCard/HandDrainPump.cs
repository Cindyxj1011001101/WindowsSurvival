/// <summary>
/// 手压排水泵
/// </summary>
public class HandDrainPump : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("手压排水", "动手将飞船内的水排除", Event_Drain, Judge_Drain,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Sobriety, -3 }
            },
            () => new()
            {
                { EnvironmentStateEnum.WaterLevel, -9 }
            });
    }

    private void Event_Drain(out string tip)
    {
        tip = string.Empty;
        PlaySound("凿_01", true);
        Use();
        ApplyEventEffects(0);

    }
    private bool Judge_Drain(out string hint)
    {
        hint = string.Empty;

        if (!GameManager.Instance.CurEnvironmentBag.PlaceData.isInSpacecraft)
        {
            hint = "仅可在飞船内使用";
            return false;
        }

        if (StateManager.Instance.WaterLevel.CurValue <= 0)
        {
            hint = "当前水位为0，无需排水";
            return false;
        }

        return true;
    }
}