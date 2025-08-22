/// <summary>
/// 氧气罐
/// </summary>
public abstract class EquipmentCard : Card
{
    protected EquipmentComponent equipment;
    protected EquipmentCard()
    {
        Events = new()
        {
            new Event("装备", "", Event_Equip, Judge_Equip),
            new Event("卸下", "", Event_UnEquip, Judge_UnEquip)
        };
    }

    protected override void LateInit()
    {
        base.LateInit();
        TryGetComponent(out equipment);
    }

    public abstract void OnEquipped();
    public abstract void OnUnEquipped();

    protected void Event_Equip(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.Equip(this);
    }

    protected bool Judge_Equip(out string hint)
    {
        return GameManager.Instance.CanEquip(this, out hint);
    }

    protected void Event_UnEquip(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.Unequip(this);
    }

    protected bool Judge_UnEquip(out string hint)
    {
        hint = string.Empty;
        return equipment.isEquipped;
    }
}