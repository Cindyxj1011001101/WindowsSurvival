/// <summary>
/// 重型氧气罐
/// </summary>
[CardId("重型氧气罐")]
public class HeavyOxygenCan : EquipmentCard
{
    public override void OnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 800);
        // 装备以后在地点移动额外消耗20%时间，探索额外消耗20%时间
        MoveExploreManager.Instance.AddMoveExtraEffect("装备了重型氧气罐", +.2f, null);
        MoveExploreManager.Instance.AddExploreExtraEffect("装备了重型氧气罐", +.2f, null);
    }

    public override void OnUnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -800);
        MoveExploreManager.Instance.RemoveMoveExtraEffect("装备了重型氧气罐");
        MoveExploreManager.Instance.RemoveExploreExtraEffect("装备了重型氧气罐");
    }
}