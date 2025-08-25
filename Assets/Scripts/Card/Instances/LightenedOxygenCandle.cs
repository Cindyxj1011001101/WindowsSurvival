/// <summary>
/// 点燃的氧烛
/// </summary>
public class LightenedOxygenCandle : Card
{
    private LightenedOxygenCandle()
    {

    }

    public override void OnAdded(Bag bag)
    {
        // 在玩家背包时，玩家每回合氧气的变化率+10
        if (bag is PlayerBag)
            StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Oxygen, +10);

        if (bag is EnvironmentBag env)
            env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, +10);

        if (bag is InnerBag innerBag)
            OnAdded(innerBag.BelongedCard.Bag);
    }

    public override void OnRemoved(Bag bag)
    {
        if (bag is PlayerBag)
            StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Oxygen, -10);

        if (bag is EnvironmentBag env)
            env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, -10);

        if (bag is InnerBag innerBag)
            OnRemoved(innerBag.BelongedCard.Bag);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // 每回合消耗耐久
        Use(1, () =>
        {
            ShowTip("氧烛燃烧殆尽了");
        });
    }
}