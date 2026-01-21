using System.Collections.Generic;

/// <summary>
/// 飞船驾驶座
/// </summary>
[CardId("损坏的飞船驾驶座")]
public class SpaceshipSeat : ConstructionCard
{
    private const float SOBRIETY_CHANGE_RATE_REST = +2.7f;
    private const float SANITY_CHANGE_RATE_REST = +2f;
    private const float SOBRIETY_CHANGE_RATE_SLEEP = +3f;

    protected override void RegisterCardEvents()
    {
        AddCardEvent(
            "靠着休息",
            $"靠在驾驶座上休息\n" +
            $"麦麦在休息时每{ColorManager.Colorize(15 + "分钟", ColorManager.Cyan)}" +
            $"回复{ColorManager.ColorizeNumber(SOBRIETY_CHANGE_RATE_REST, ColorManager.Green)}清醒度与" +
            $"{ColorManager.ColorizeNumber(SANITY_CHANGE_RATE_REST, ColorManager.Green)}精神值\n" +
            $"{ColorManager.Warning("休息行为1天内多次进行数值恢复减半，最多叠加2次")}",
            Event_Rest,
            Judge_Rest
        );
        AddCardEvent(
            "靠着睡觉",
            $"靠在驾驶座上睡觉\n" +
            $"麦麦在睡觉时每{ColorManager.Colorize(15 + "分钟", ColorManager.Cyan)}" +
            $"回复{ColorManager.ColorizeNumber(SOBRIETY_CHANGE_RATE_SLEEP, ColorManager.Green)}清醒度",
            Event_Sleep,
            Judge_Rest
        );
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnInit()
    {
        GlobalDataManager.Instance.GlobalData.AddReduceAction(CardId, new Reduce(2, .5f));
        EventManager.Instance.AddListener(EventType.AnotherDay, RefreshSlot); // 隔天刷新
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.AnotherDay, RefreshSlot);
    }

    private void Event_Rest(CardEvent e)
    {
        // 获取实际的状态变化率
        // 实际状态变化率 = 基础变化率 * 衰减率
        var sobrietyChangeRate = SOBRIETY_CHANGE_RATE_REST * GlobalDataManager.Instance.GlobalData.GetReduceRate(CardId);
        var sanChangeRate = SANITY_CHANGE_RATE_REST * GlobalDataManager.Instance.GlobalData.GetReduceRate(CardId);

        // 唤起时间窗口，设置休息时长为0-60分钟
        var window = (WindowsManager.Instance.OpenWindow("TimeSelect", true) as TimeSelectWindow);
        window.SetTimeRange(0, 60);
        window.onConfirm += (time) =>
        {
            StateManager.Instance.Rest(time, new() { { PlayerStateEnum.Sobriety, sobrietyChangeRate }, { PlayerStateEnum.Sanity, sanChangeRate } });
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
                        { PlayerStateEnum.Sanity, sanChange }
                    };
            }
            return ($"休息 {t} 分钟", t, p, null);
        };
    }

    private bool Judge_Rest(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater)
        {
            hint = "无法在水域地点休息或睡觉";
            return false;
        }
        return true;
    }

    private void Event_Sleep(CardEvent e)
    {
        // 唤起时间窗口，设置休息时长为1~8小时
        var window = (WindowsManager.Instance.OpenWindow("TimeSelect", true) as TimeSelectWindow);
        window.SetTimeRange(60, 60 * 8);
        window.onConfirm += (time) =>
        {
            StateManager.Instance.Rest(time, new() { { PlayerStateEnum.Sobriety, SOBRIETY_CHANGE_RATE_SLEEP }, { PlayerStateEnum.Sanity, SOBRIETY_CHANGE_RATE_SLEEP } });
        };
        window.getConfirmEffects += (t) =>
        {
            Dictionary<PlayerStateEnum, float> p = null;
            float sobrietyChange = t / TimeManager.SETTLEMENT_INTERVAL * SOBRIETY_CHANGE_RATE_SLEEP;
            if (sobrietyChange > 0)
            {
                p = new()
                    {
                        { PlayerStateEnum.Sobriety, sobrietyChange },
                    };
            }
            return ($"睡觉 {t} 分钟", t, p, null);
        };
    }
}