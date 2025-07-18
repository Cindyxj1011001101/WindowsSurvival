using System;

/// <summary>
/// 小块肉
/// </summary>
public class LittleRawMeat : Card
{
    public LittleRawMeat()
    {
        //初始化参数
        cardName = "小块生肉";
        cardDesc = "一小块生肉，最好烹饪一下再食用。";
        cardType = CardType.Food;
        maxStackNum = 5;
        moveable = true;
        weight = 0.5f;
        events = new()
        {
            new Event("食用", "食用小块生肉", Event_Eat, null)
        };
        components = new()
        {
            { typeof(FreshnessComponent), new FreshnessComponent(2880, OnFreshnessChanged) }
        };
    }

    private void OnFreshnessChanged(int freshness)
    {
        if (freshness == 0)
        {
            DestroyThis();
            GameManager.Instance.AddCard(new RotMaterial(), true);
        }
    }

    public void Event_Eat()
    {
        DestroyThis();
        //+12饱食
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 12));
        //-2精神值
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -2));
        //-3健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -3));
        //消耗15分钟
        TimeManager.Instance.AddTime(15);
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<FreshnessComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval);
    };
}