/// <summary>
/// 重型氧气罐
/// </summary>
[CardId("重型氧气罐")]
public class HeavyOxygenCan : EquipmentCard
{
    private const int OXYGEN_MAX_INCREASE = 800;
    private const float TIME_INCREAST_RATE = 0.2f;

    public override string GetEquipDesc()
    {
        return $"麦麦的氧气上限增加{ColorManager.ColorizeNumber(OXYGEN_MAX_INCREASE, ColorManager.Green, "0")}，但移动和探索消耗的时间增加{ColorManager.ColorizePercent(TIME_INCREAST_RATE, ColorManager.Red, "0")}";
    }

    public override void OnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, OXYGEN_MAX_INCREASE);
        // 装备以后在地点移动额外消耗20%时间，探索额外消耗20%时间
        MoveExploreManager.Instance.AddMoveExtraEffect("装备了重型氧气罐", TIME_INCREAST_RATE, null);
        MoveExploreManager.Instance.AddExploreExtraEffect("装备了重型氧气罐", TIME_INCREAST_RATE, null);
    }

    public override void OnUnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -OXYGEN_MAX_INCREASE);
        MoveExploreManager.Instance.RemoveMoveExtraEffect("装备了重型氧气罐");
        MoveExploreManager.Instance.RemoveExploreExtraEffect("装备了重型氧气罐");
    }
}