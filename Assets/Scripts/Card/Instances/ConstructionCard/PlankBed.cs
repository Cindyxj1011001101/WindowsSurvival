using System.Collections.Generic;

/// <summary>
/// 板床
/// </summary>
public class PlankBed : ConstructionCard
{
    private const float SOBRIETY_CHANGE_RATE = +3.5f;
    private const float SANITY_CHANGE_RATE = +0.4f;

    protected override void RegisterCardEvents()
    {
        AddCardEvent(
            "睡觉",
            $"在板床上睡觉。\n" +
            $"{ColorManager.Colorize(SOBRIETY_CHANGE_RATE, ColorManager.Green)}清醒度/15min\n" +
            $"{ColorManager.Colorize(SANITY_CHANGE_RATE, ColorManager.Green)}精神/15min\n",
            Event_Rest,
            Judge_Rest
            );
        base.RegisterCardEvents(); // 拆毁
    }

    private void Event_Rest(CardEvent e)
    {
        // 唤起时间窗口，设置休息时长为0-60分钟
        var window = (WindowsManager.Instance.OpenWindow("TimeSelect", true) as TimeSelectWindow);
        window.SetTimeRange(60, 60 * 8); // 可休息1-8小时
        window.onConfirm += (time) =>
        {
            StateManager.Instance.Rest(time, new() { { PlayerStateEnum.Sobriety, SOBRIETY_CHANGE_RATE }, { PlayerStateEnum.Sanity, SANITY_CHANGE_RATE } });
        };
        window.getConfirmEffects += (t) =>
        {
            Dictionary<PlayerStateEnum, float> p = null;
            float sobrietyChange = t / TimeManager.SETTLEMENT_INTERVAL * SOBRIETY_CHANGE_RATE;
            float sanChange = t / TimeManager.SETTLEMENT_INTERVAL * SANITY_CHANGE_RATE;
            if (sobrietyChange > 0)
            {
                p = new()
                    {
                        { PlayerStateEnum.Sobriety, sobrietyChange },
                        { PlayerStateEnum.Sanity, sanChange }
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