/// <summary>
/// 氧气面罩
/// </summary>
public class OxygenMask : EquipmentCard
{
    public override void OnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 60);
        GameManager.Instance.RemoveExploreInWaterExtraEffect("未装备氧气面罩");
    }

    public override void OnUnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -60);
        GameManager.Instance.AddExploreInWaterExtraEffect("未装备氧气面罩", +.4f, new() { { PlayerStateEnum.Health, -4 } });
    }
}