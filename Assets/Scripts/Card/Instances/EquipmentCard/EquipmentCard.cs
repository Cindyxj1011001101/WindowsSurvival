/// <summary>
/// 氧气罐
/// </summary>
public abstract class EquipmentCard : Card
{
    public abstract void OnEquipped();
    public abstract void OnUnEquipped();

    protected void Event_Equip()
    {
        GameManager.Instance.Equip(this);
    }

    protected bool Judge_Equip()
    {
        TryGetComponent<EquipmentComponent>(out var component);
        // 已经穿上装备了
        if (component.isEquipped) return false;
        return GameManager.Instance.CanEquip(this);
    }

    protected void Event_UnEquip()
    {
        GameManager.Instance.Unequip(this);
    }

    protected bool Judge_UnEquip()
    {
        return TryGetComponent<EquipmentComponent>(out var component) && component.isEquipped;
    }
}