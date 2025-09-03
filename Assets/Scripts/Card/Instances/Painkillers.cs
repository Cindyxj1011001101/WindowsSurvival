using UnityEngine;

/// <summary>
/// 止痛药
/// </summary>
public class Painkillers : Card
{
    public int maxReduceCount = 2;
    public int curReduceCount = 0;
    public float reduceRate = 0.5f;
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
        DestroyThis();
        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, -50 * GlobalDataManager.Instance.saveData.GetReduce(CardId));
        // 播放吃的音效
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01",true);
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