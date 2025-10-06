using System.Collections.Generic;

/// <summary>
/// 板床
/// </summary>
public class PlankBed : ConstructionCard
{
    private PlankBed()
    {
        Events = new()
        {
            new Event(
                "睡觉",
                @"在板床上睡觉。
                +3.5清醒度/15min
                +0.4精神/15min",
                Event_Rest,
                Judge_Rest
            ),
        };
    }

    private void Event_Rest(out string tip)
    {
        tip = string.Empty;

        // 获取实际的状态变化率
        // 实际状态变化率 = 基础变化率 * 衰减率
        var sobrietyChangeRate = +3.5f;
        var sanChangeRate = +0.4f;

        // 唤起时间窗口，设置休息时长为0-60分钟
        var window = (WindowsManager.Instance.OpenWindow("TimeSelect", true) as TimeSelectWindow);
        window.SetTimeRange(60, 60 * 8); // 可休息1-8小时
        window.onConfirm += (time) =>
        {
            StateManager.Instance.Rest(time, new() { { PlayerStateEnum.Sobriety, sobrietyChangeRate }, { PlayerStateEnum.San, sanChangeRate } });
        };
        window.getConfirmEffects += (t) =>
        {
            Dictionary<PlayerStateEnum, float> p = null;
            float sobrietyChange = t / TimeManager.Instance.SettleInterval * sobrietyChangeRate;
            float sanChange = t / TimeManager.Instance.SettleInterval * sanChangeRate;
            if (sobrietyChange > 0)
            {
                p = new()
                    {
                        { PlayerStateEnum.Sobriety, sobrietyChange },
                        { PlayerStateEnum.San, sanChange }
                    };
            }
            return ($"在板床上睡觉 {t} 分钟", t, p, null);
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