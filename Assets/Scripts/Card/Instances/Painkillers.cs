using UnityEngine;

/// <summary>
/// 止痛药
/// </summary>
public class Painkillers : Card
{
    private Painkillers()
    {
        Events = new()
        {
            new Event("使用", "使用", Event_Use, null, () => 5,  () => new () { { PlayerStateEnum.PainLevel, -50 * GlobalDataManager.Instance.saveData.GetReduce(CardId) } })
        };
    }

    public override void LateInit()
    {
        base.LateInit();
        if (!GlobalDataManager.Instance.saveData.ReduceActionDict.ContainsKey(CardId))
        {
            GlobalDataManager.Instance.saveData.ReduceActionDict.Add(CardId,
                new Reduce()
                {
                    maxReduceCount = 2,
                    curReduceCount = 0,
                    reduceRate = 0.5f
                });
        }
    }

    private void Event_Use(out string tip)
    {
        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, -50 * GlobalDataManager.Instance.saveData.GetReduce(CardId));
        TimeManager.Instance.AddTime(5);
        GlobalDataManager.Instance.saveData.AddCardReduce(CardId);
    }
    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (TimeManager.Instance.AnotherDay())
        {
            RefreshSlot();
        }
    }
}