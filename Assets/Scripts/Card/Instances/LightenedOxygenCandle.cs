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
        // 每回合消耗耐久
        TryUse();
        BagBase bag;
        if (ParentCard != null)
            // 自身作为内容物
            bag = ParentCard.Slot.Bag;
        else
            bag = Slot.Bag;

        if (bag is PlayerBag playerBag)
        {
            // 氧烛在玩家背包里
            // 给玩家加氧气
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Oxygen, +10);
        }
        else if (bag is EnvironmentBag environmentBag)
        {
            // 氧烛在环境里
            // 给环境加氧气
            environmentBag.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, +10);
        }
    };
}