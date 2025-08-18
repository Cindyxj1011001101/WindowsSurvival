
using UnityEngine;

public class Campfire : Card
{
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
    //TODO:将拥有BurnableComponent卡牌拖拽到本卡牌上，增加燃料（和燃料炉逻辑一致）
    public void Event_Light(out string tip)
    {
        tip = string.Empty;
        isLightened=true;
    }

    public bool Judge_Light(out string hint)
    {
        hint = string.Empty;
        return !isLightened;
    }
    public void Event_UnLight(out string tip)
    {
        tip = string.Empty;
        isLightened=false;
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
            GameManager.Instance.CurEnvironmentBag.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, -4);
            GameManager.Instance.CurEnvironmentBag.ChangeEnvironmentState(EnvironmentStateEnum.CarbonMonoxideLevel, 2);
            TryGetComponent<InnerContentsComponent>(out InnerContentsComponent component);
            foreach (var slot in component.bag.Slots)
            {
                foreach (var card in slot.Cards)
                {
                    card.TryGetComponent<CookComponent>(out CookComponent cookComponent);
                    string outcomeID= cookComponent.AddProgress();
                    if (outcomeID != string.Empty)
                    {
                        component.bag.AddCard(component.bag.FindCardOfName(outcomeID));
                    }

                }
            }
        }

    };
}