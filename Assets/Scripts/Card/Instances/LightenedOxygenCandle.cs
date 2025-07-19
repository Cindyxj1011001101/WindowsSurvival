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