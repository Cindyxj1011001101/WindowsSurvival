/// <summary>
/// 氧气面罩
/// </summary>
[CardId("氧气面罩")]
public class OxygenMask : EquipmentCard
{
    private const int OXYGEN_MAX_INCREASE = 60;

    public override string GetEquipDesc()
    {
        return $"麦麦的氧气上限增加{ColorManager.ColorizeNumber(OXYGEN_MAX_INCREASE, ColorManager.Green, "0")}";
    }

    public override void OnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, OXYGEN_MAX_INCREASE);
        MoveExploreManager.Instance.RemoveExploreInWaterExtraEffect("未装备氧气面罩");
    }

    public override void OnUnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -OXYGEN_MAX_INCREASE);
        MoveExploreManager.Instance.AddExploreInWaterExtraEffect("未装备氧气面罩", +.4f, new() { { PlayerStateEnum.Health, -4 } });
    }
}