/// <summary>
/// 氧气罐
/// </summary>
public abstract class EquipmentCard : Card
{
    public abstract void OnEquipped();
    public abstract void OnUnEquipped();

    protected void Event_Equip(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.Equip(this);
    }

    protected bool Judge_Equip()
    {
        return GameManager.Instance.CanEquip(this, out _);
    }

    protected void Event_UnEquip(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.Unequip(this);
    }

    protected bool Judge_UnEquip()
    {
        return TryGetComponent<EquipmentComponent>(out var component) && component.isEquipped;
    }
}