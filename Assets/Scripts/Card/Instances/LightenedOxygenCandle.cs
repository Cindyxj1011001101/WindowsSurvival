using System;

/// <summary>
/// 点燃的氧烛
/// </summary>
public class LightenedOxygenCandle : Card
{
    public LightenedOxygenCandle()
    {
        cardName = "点燃的氧烛";
        cardDesc = "无需氧气助燃的化学氧烛，顶部有一个引信，按下后内部就会开始反应，在水下也能轻松点燃。";
        cardType = CardType.Tool;
        maxStackNum = 1;
        moveable = true;
        weight = 1.8f;
        components = new()
        {
            { typeof(DurabilityComponent), new DurabilityComponent(14, OnDurabilityChanged) }
        };
    }

    private void OnDurabilityChanged(int durability)
    {
        if (durability == 0)
        {
            DestroyThis();
        }
        slot.RefreshCurrentDisplay();
    }

    protected override Action OnUpdate => () =>
    {
        // 每回合消耗耐久
        TryGetComponent<DurabilityComponent>(out var component);
        component.Use();
        EnvironmentBag environmentBag = GameManager.Instance.CurEnvironmentBag;
        if (environmentBag.PlaceData.isIndoor)
        {
            StateManager.Instance.OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(environmentBag.PlaceData.placeType, EnvironmentStateEnum.Oxygen, 10));
        }
        else
        {
            StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Oxygen, 10));
        }
    };
}