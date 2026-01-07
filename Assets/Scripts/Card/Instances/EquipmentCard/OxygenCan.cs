/// <summary>
/// 氧气罐
/// </summary>
[CardId("氧气罐")]
public class OxygenCan : EquipmentCard
{
    private const int OXYGEN_MAX_INCREASE = 120;

    public override string GetEquipDesc()
    {
        return $"麦麦的氧气上限增加{ColorManager.ColorizeNumber(OXYGEN_MAX_INCREASE, ColorManager.Green, "0")}";
    }
    
    public override void OnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 120);
    }

    public override void OnUnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -120);
    }
}