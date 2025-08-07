/// <summary>
/// 氧气罐
/// </summary>
public class OxygenCan : EquipmentCard
{
    private OxygenCan()
    {
        Events = new()
        {
            new Event("穿上", "穿上氧气罐", Event_Equip, Judge_Equip),
            new Event("脱下", "脱下氧气罐", Event_UnEquip, Judge_UnEquip)
        };
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