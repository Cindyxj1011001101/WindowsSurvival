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
        EnvironmentBag environmentBag = GameManager.Instance.CurEnvironmentBag;
        // 因为在室内环境加玩家氧气时会优先加到环境里，所以这里可以写直接加给玩家
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Oxygen, 10));
    };
}