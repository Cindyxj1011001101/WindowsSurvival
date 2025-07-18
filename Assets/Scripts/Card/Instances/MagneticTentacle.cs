using System;

/// <summary>
/// 磁性触手
/// </summary>
public class MagneticTentacle : Card
{
    public MagneticTentacle()
    {
        //初始化参数
        cardName = "磁性触手";
        cardDesc = "用于吸引含铁浮游生物的磁性触手。处理得当可作为电池的原材料。是在饿了的时候可作为食物，可食用部分主要是触手的外套膜，有着咸腥的生锈金属味。";
        cardType = CardType.Food;
        maxStackNum = 5;
        moveable = true;
        weight = 0.6f;
        events = new()
        {
            new Event("食用", "食用磁性触手", Event_Eat,null)
        };
        components = new()
        {
            { typeof(FreshnessComponent), new FreshnessComponent(11520) }
        };
    }

    private void OnRotton()
    {
        DestroyThis();
        GameManager.Instance.AddCard(new ScrapMetal(), true);
    }

    public void Event_Eat()
    {
        DestroyThis();
        //+14饱食
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 14));
        //-6精神
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -6));
        //-5健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -5));
        //消耗30分钟
        TimeManager.Instance.AddTime(30);
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<FreshnessComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnRotton);
    };
}