/// <summary>
/// 氧气罐
/// </summary>
public class OxygenCan : EquipmentCard
{
    public override void OnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 120);
    }

    public override void OnUnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -120);
    }
}