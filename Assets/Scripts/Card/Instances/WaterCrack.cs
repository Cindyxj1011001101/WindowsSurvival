/// <summary>
/// 渗水裂缝
/// </summary>
public class WaterCrack : Card
{
    private WaterCrack()
    {
        events = new()
        {
            new Event("堵住", "堵住渗水裂缝", Event_Fix, Jugde_Fix),
        };
    }

    public void Event_Fix()
    {
        DestroyThis();
        GameManager.Instance.PlayerBag.FindCardOfName("裂缝填充物").DestroyThis();
        TimeManager.Instance.AddTime(15);
    }

    public bool Jugde_Fix()
    {
        return GameManager.Instance.PlayerBag.FindCardOfName("裂缝填充物") != null;
    }

    protected override System.Action OnUpdate => () =>
    {
        //每个渗水裂缝每回合会使飞船水平面高度+0.3，渗水裂缝所在的地点每回合-8氧气。
        EnvironmentBag environmentBag = GameManager.Instance.CurEnvironmentBag;
        if (environmentBag != null)
        {
            StateManager.Instance.OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(environmentBag.PlaceData.placeType, EnvironmentStateEnum.Height, 0.3f));
            (slot.Bag as EnvironmentBag).EnvironmentStateDict[EnvironmentStateEnum.Oxygen].curValue -= 8;
            if ((slot.Bag as EnvironmentBag).EnvironmentStateDict[EnvironmentStateEnum.Oxygen].curValue <= 0)
            {
                (slot.Bag as EnvironmentBag).EnvironmentStateDict[EnvironmentStateEnum.Oxygen].curValue = 0;
            }
        }
    };
}