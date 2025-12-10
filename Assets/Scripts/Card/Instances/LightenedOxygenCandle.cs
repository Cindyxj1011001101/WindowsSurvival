/// <summary>
/// 点燃的氧烛
/// </summary>
[CardId("点燃的氧烛")]
public class LightenedOxygenCandle : Card
{
    public override void OnAdd(Bag bag)
    {
        // 在玩家背包时，玩家每回合氧气的变化率+10
        if (bag is PlayerBag || bag is EquipmentBag)
            StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Oxygen, +10);

        if (bag is EnvironmentBag env)
            env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, +10);

        if (bag is InnerBag innerBag)
            OnAdd(innerBag.BelongedCard.Bag);
    }

    public override void OnRemove(Bag bag)
    {
        if (bag is PlayerBag || bag is EquipmentBag)
            StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Oxygen, -10);

        if (bag is EnvironmentBag env)
            env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, -10);

        if (bag is InnerBag innerBag)
            OnRemove(innerBag.BelongedCard.Bag);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // 每回合消耗耐久
        Use();

        if (durability.value == 0) ShowTip("氧烛燃烧殆尽了");
    }
}