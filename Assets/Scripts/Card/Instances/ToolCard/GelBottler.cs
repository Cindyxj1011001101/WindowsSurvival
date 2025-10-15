/// <summary>
/// 凝胶装瓶器
/// </summary>
public class GelBottler : Card
{
    private GelBottler()
    {
        Events = new()
        {
            new CardEvent("液体装瓶", "消耗当前地点中的水，获得一瓶盐水", Event_Bottling, Judge_Bottling, () => 15, null, () =>
            {
                if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInSpacecraft)
                {
                    return new () { { EnvironmentStateEnum.WaterLevel, -2 } };
                }
                return null;
            })
        };
    }

    public override void Init()
    {
        base.Init();
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

    private void Event_Bottling(out string tip)
    {
        tip = string.Empty;
        Use();
        AddCard("盐水", true);
        if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInSpacecraft)
        {
            StateManager.Instance.ChangeWaterLevel(-2);
        }
        TimeManager.Instance.AddTime(15);
    }

    private bool Judge_Bottling(out string hint)
    {
        hint = string.Empty;

        var env = GameManager.Instance.CurEnvironmentBag;

        if (env.PlaceData.isInWater) return true;

        if (env.PlaceData.isInSpacecraft && StateManager.Instance.WaterLevel.CurValue <= 0)
        {
            hint = "水位不足，无法装瓶";
            return false;
        }

        return true;
    }
}