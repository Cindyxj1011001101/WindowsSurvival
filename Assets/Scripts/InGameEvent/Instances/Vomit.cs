using UnityEngine;

/// <summary>
/// Å»ÍÂ
/// </summary>
public class Vomit : InGameEvent
{
    private const float SAN_THRESHOLD = 0.15f; // ¾«Éñ×´Ì¬ãÐÖµ

    public override bool CanTriggerThisEvent()
    {
        var san = StateManager.Instance.PlayerStateDict[PlayerStateEnum.San];
        return san.CurValue / san.MaxValue <= SAN_THRESHOLD;
    }

    public override void TriggerThisEvent()
    {
        var thirstChange = -Random.Range(10, 51);
        var fullnessChange = -Random.Range(10, 41);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, thirstChange);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, fullnessChange);
        // µôÂä¸¯ÀÃÎï
        GameManager.Instance.AddCardsToTargetEnv(GameManager.Instance.CurEnvironmentBag, CardFactory.CreateCard("¸¯ÀÃÎï"));
    }
}
