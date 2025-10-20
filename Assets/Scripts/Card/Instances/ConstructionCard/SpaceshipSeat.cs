using System.Collections.Generic;

/// <summary>
/// 飞船驾驶座
/// </summary>
public class SpaceshipSeat : ConstructionCard
{
    private SpaceshipSeat()
    {
        Events = new()
        {
            new CardEvent(
                "靠着休息",
                "靠在驾驶座上休息。\n" +
                "+2.7清醒度/15min\n" +
                "+2精神/15min\n" +
                "（休息行为1天内多次进行数值恢复减半，最多叠加2次）",
                Event_Rest,
                Judge_Rest
            ),
        };
    }

    public override void Init()
    {
        base.Init();
        GlobalDataManager.Instance.GlobalData.AddReduceAction(CardId, new Reduce(2, .5f));

        EventManager.Instance.AddListener(EventType.AnotherDay, RefreshSlot); // 隔天刷新
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.AnotherDay, RefreshSlot);
    }

    private void Event_Rest(out string tip)
    {
        tip = string.Empty;

        // 获取实际的状态变化率
        // 实际状态变化率 = 基础变化率 * 衰减率
        var sobrietyChangeRate = +2.7f * GlobalDataManager.Instance.GlobalData.GetReduceRate(CardId);
        var sanChangeRate = +2f * GlobalDataManager.Instance.GlobalData.GetReduceRate(CardId);

        // 唤起时间窗口，设置休息时长为0-60分钟
        var window = (WindowsManager.Instance.OpenWindow("TimeSelect", true) as TimeSelectWindow);
        window.SetTimeRange(0, 60);
        window.onConfirm += (time) =>
        {
            StateManager.Instance.Rest(time, new() { { PlayerStateEnum.Sobriety, sobrietyChangeRate }, { PlayerStateEnum.San, sanChangeRate } });
            // 衰减次数+1
            GlobalDataManager.Instance.GlobalData.AddReduceCount(CardId);
        };
        window.getConfirmEffects += (t) =>
        {
            Dictionary<PlayerStateEnum, float> p = null;
            float sobrietyChange = t / TimeManager.SETTLEMENT_INTERVAL * sobrietyChangeRate;
            float sanChange = t / TimeManager.SETTLEMENT_INTERVAL * sanChangeRate;
            if (sobrietyChange > 0)
            {
                p = new()
                    {
                        { PlayerStateEnum.Sobriety, sobrietyChange },
                        { PlayerStateEnum.San, sanChange }
                    };
            }
            return ($"靠在驾驶座上休息 {t} 分钟", t, p, null);
        };
    }

    private bool Judge_Rest(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater)
        {
            hint = "无法在水域地点休息";
            return false;
        }
        return true;
    }
}