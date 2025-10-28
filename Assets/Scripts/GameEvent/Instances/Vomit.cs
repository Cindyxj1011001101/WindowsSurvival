using UnityEngine;

/// <summary>
/// 呕吐
/// </summary>
public class Vomit : GameEvent
{
    private const float SAN_THRESHOLD = 0.15f; // 精神状态阈值

    public override string GetDetails()
    {
        return $"麦麦突然吐了，吐得到处都是。\n\n" +
               $"也许最近她的精神压力太大了。\n\n" +
               $"麦麦的饱食和水分减少了。";
    }

    public override bool CanTriggerThisEvent()
    {
        var san = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Sanity];
        return san.CurValue / san.MaxValue <= SAN_THRESHOLD;
    }

    public override void OnTrigger()
    {
        var thirstChange = -Random.Range(10, 51);
        var fullnessChange = -Random.Range(10, 41);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Hydration, thirstChange);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Hunger, fullnessChange);

        // 掉落腐烂物
        GameManager.Instance.AddCardsToTargetEnv(GameManager.Instance.CurEnvironmentBag, CardFactory.CreateCard("腐烂物"));

        // TODO: 中止睡眠行为
    }
}
