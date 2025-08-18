using UnityEngine;

public class Campfire : Card
{
    private InnerContentsComponent innerContents;
    public bool isLightened;
    public float Fuel;
    private Campfire()
    {
        isLightened = false;
        Fuel = 0;
        Events = new()
        {
            new Event("点燃", "点燃", Event_Light, Judge_Light),
            new Event("熄灭", "熄灭", Event_UnLight, Judge_UnLight)
        };
    }

    private bool ContentFilter(Card c, out string s)
    {
        // TODO
        throw new System.NotImplementedException();
    }

    //TODO:将拥有BurnableComponent卡牌拖拽到本卡牌上，增加燃料（和燃料炉逻辑一致）
    public void Event_Light(out string tip)
    {
        tip = string.Empty;

        var env = Bag as EnvironmentBag;
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, -4);
        env.ChangeEnvironmentState(EnvironmentStateEnum.CarbonMonoxideLevel, +2);


        isLightened = true;
    }

    public bool Judge_Light(out string hint)
    {
        hint = string.Empty;
        return !isLightened;
    }
    public void Event_UnLight(out string tip)
    {
        tip = string.Empty;

        var env = Bag as EnvironmentBag;
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, +4);
        env.ChangeEnvironmentState(EnvironmentStateEnum.CarbonMonoxideLevel, -2);

        isLightened = false;
    }

    public bool Judge_UnLight(out string hint)
    {
        hint = string.Empty;
        return isLightened;
    }
    protected override System.Action OnUpdate => () =>
    {
        if (isLightened)
        {
            Fuel -= 2;
            Fuel = Mathf.Clamp(Fuel, 0, 100);
            foreach (var slot in innerContents.bag.Slots)
            {
                foreach (var card in slot.Cards)
                {
                    card.TryGetComponent(out CookComponent cookComponent);
                    string outcomeID = cookComponent.AddProgress();
                    if (!string.IsNullOrEmpty(outcomeID))
                    {
                        // 获得煮熟产物
                        AddCard(outcomeID, innerContents.bag);
                    }
                }
            }
        }
    };
}