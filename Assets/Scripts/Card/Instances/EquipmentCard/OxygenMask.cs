/// <summary>
/// 氧气面罩
/// </summary>
public class OxygenMask : EquipmentCard
{
    private OxygenMask()
    {
        Events = new()
        {
            new Event("装备", "装备氧气面罩", Event_Equip, Judge_Equip),
            new Event("卸下", "卸下氧气面罩", Event_UnEquip, Judge_UnEquip)
        };
    }
    public override void OnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 60);
    }

    public override void OnUnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -60);
    }
}