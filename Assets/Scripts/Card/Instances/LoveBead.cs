using System;

/// <summary>
/// 爱情贝
/// </summary>
public class LoveBead : Card
{
    public LoveBead()
    {
        cardName = "爱情贝";
        cardDesc = "形似扇贝的古怪异星生物。其强大的肌肉使其能像鱼一样扇动贝壳游动。繁殖期的雄贝会搜集各类珍宝藏在贝壳中以吸引雌贝。";
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = true;
        weight = 1.5f;
        events = new()
        {
            new Event("取贝肉", "取贝肉", Event_GetMeat, null)
        };
        components = new()
        {
            { typeof(ProgressComponent), new ProgressComponent(4320) },
        };
    }

    public void Event_GetMeat()
    {
        DestroyThis();
        TimeManager.Instance.AddTime(30);
        GameManager.Instance.AddCard(new RawOysterMeat(), true);
        GameManager.Instance.AddCard(new RawOysterMeat(), true);
    }

    private void OnProgressChanged()
    {
        DestroyThis();
        GameManager.Instance.AddCard(new LoveBeadWithProduct(), true);
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnProgressChanged);
    };
}