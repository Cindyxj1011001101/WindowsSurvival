/// <summary>
/// 渗水裂缝
/// </summary>
public class WaterCrack : Card
{
    private WaterCrack()
    {
        Events = new()
        {
            new Event("堵住", "消耗裂缝填充物修补裂缝", Event_Fix, Jugde_Fix, () => 15),
        };
    }

    public void Event_Fix(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        GameManager.Instance.PlayerBag.FindCardOfName("裂缝填充物").DestroyThis();
        TimeManager.Instance.AddTime(15);
    }

    public bool Jugde_Fix(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("裂缝填充物") == null)
        {
            hint = "需要裂缝填充物";
            return false;
        }
        return true;
    }

    protected override System.Action OnUpdate => () =>
    {
        var bag = Slot.Bag as EnvironmentBag;
        // 渗水裂缝所在的地点每回合-8氧气
        bag.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, -3);
        // 每个渗水裂缝每回合会使飞船水平面高度+0.3
        StateManager.Instance.ChangeWaterLevel(+0.3f);
    };
}