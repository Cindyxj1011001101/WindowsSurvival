/// <summary>
/// 生贝肉
/// </summary>
public class RawOysterMeat : Card
{
    public RawOysterMeat()
    {
        //初始化参数
        cardName = "生贝肉";
        cardDesc = "肌纤维极其发达的贝肉，咬完感觉有点塞牙。希望生吃不会感染寄生虫。";
        cardType = CardType.Food;
        maxStackNum = 5;
        moveable = true;
        weight = 0.3f;
        events = new()
        {
            new Event("食用", "食用生贝肉", Event_Eat, null),
        };
        components = new()
        {
            { typeof(FreshnessComponent), new FreshnessComponent(1440, OnFreshnessChanged) }
        };
    }

    private void OnFreshnessChanged(int freshness)
    {
        if (freshness == 0)
        {
            DestroyThis();
        }
    }

    #region 食用
    public void Event_Eat()
    {
        DestroyThis();
        //+6饱食
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 6));
        //-1.2健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -1.2f));
        //消耗5分钟
        TimeManager.Instance.AddTime(5);
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        TryGetComponent<FreshnessComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval);
    };
}