/// <summary>
/// 手压排水泵
/// </summary>
[CardId("手压排水泵")]
public class HandDrainPump : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("手压排水", "动手将飞船内的水排除", EasyEvent_Use, Judge_Drain,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Sobriety, -3 }
            },
            () => new()
            {
                { EnvironmentStateEnum.WaterLevel, -9 }
            },
            sound: "凿_01");
    }

    protected override void OnInit()
    {
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnWaterLevelChanged);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnWaterLevelChanged);
    }

    private void OnWaterLevelChanged(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.WaterLevel) return;

        RefreshSlot();
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