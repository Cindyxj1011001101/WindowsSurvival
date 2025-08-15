using System;

/// <summary>
/// 点燃的氧烛
/// </summary>
public class LightenedOxygenCandle : Card
{
    private LightenedOxygenCandle()
    {

    }

    protected override Action OnUpdate => () =>
    {
        Use();

        // 每回合消耗耐久
        Bag targetBag;
        if (ParentCard != null)
            // 自身作为内容物
            targetBag = ParentCard.Bag;
        else
            targetBag = Bag;

        if (targetBag is PlayerBag playerBag)
        {
            // 氧烛在玩家背包里
            // 给玩家加氧气
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Oxygen, +10);
        }
        else if (targetBag is EnvironmentBag environmentBag)
        {
            // 氧烛在环境里
            // 给环境加氧气
            environmentBag.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, +10);
        }
    };
}