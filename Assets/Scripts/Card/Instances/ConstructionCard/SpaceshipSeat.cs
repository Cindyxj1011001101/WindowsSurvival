using System.Collections.Generic;
using UnityEngine;

public class SpaceshipSeat : ConstructionCard
{
    public int curReduceCount = 0;
    public int maxReduceCount = 2;
    public float reduceRate = 0.5f;
    private SpaceshipSeat()
    {
        Events = new()
        {
            new Event("靠着休息", "靠在驾驶座上休息。休息效率为每15分钟+2.7清醒度，每15分钟+2精神（休息行为1天内多次进行数值恢复减半，最多叠加2次）", Event_Rest, Judge_Rest),
        };
    }

    private void Event_Rest(out string tip)
    {
        tip = string.Empty;

        var sobrietyChangeRate = +2.7f * Mathf.Pow(reduceRate, curReduceCount);
        var sanChangeRate = +2f * Mathf.Pow(reduceRate, curReduceCount);

        // 唤起时间窗口，设置休息时长为0-60分钟
        var window = (WindowsManager.Instance.OpenWindow("TimeSelect", true) as TimeSelectWindow);
        window.SetTimeRange(0, 60);
        window.onConfirm += (time) =>
        {
            StateManager.Instance.Sleep(time, new() { { PlayerStateEnum.Sobriety, sobrietyChangeRate }, { PlayerStateEnum.San, sanChangeRate } });

            curReduceCount++;
            if (curReduceCount >= maxReduceCount) curReduceCount = maxReduceCount;
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
            return ($"休息{t}分钟", t, p, null);
        };
    }

    private bool Judge_Rest(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater)
        {
            hint = "只能在非水域地点休息";
            return false;
        }
        return true;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (TimeManager.Instance.AnotherDay())
        {
            curReduceCount = 0; // 隔天时刷新可使用次数
            RefreshSlot();
        }
    }
}