using Newtonsoft.Json;

/// <summary>
/// 针刺攻击
/// </summary>
public class AcupunctureAttackIntention : RangedAttackIntention
{
    [JsonProperty] private float playerItchinessIncrease; // 玩家瘙痒值增加量

    public AcupunctureAttackIntention(int preparationMinutes, string targetUuid, float dmg, AttackForm atkForm, (float, float) atkRange, float playerItchinessIncrease) : base(preparationMinutes, targetUuid, dmg, atkForm, atkRange)
    {
        this.playerItchinessIncrease = playerItchinessIncrease;
    }

    public override void OnExecute()
    {
        base.OnExecute();
        // 玩家瘙痒值增加
        if (EntityTarget is Player)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Itchiness, playerItchinessIncrease);
        }
    }
}