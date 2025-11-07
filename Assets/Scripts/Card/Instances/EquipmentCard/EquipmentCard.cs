public abstract class EquipmentCard : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("装备", "", Event_Equip, Judge_Equip);
        AddCardEvent("卸下", "", Event_UnEquip, Judge_UnEquip);
    }

    protected override void OnInit()
    {
        // 装备损坏后尝试从背包中重新找到一件相同的装备并且穿上
        durability.onBroken = () =>
        {
            var sameEquipment = GameManager.Instance.PlayerBag.FindCardOfName(CardName);
            if (sameEquipment != null && GameManager.Instance.CanEquip(sameEquipment, out _))
            {
                GameManager.Instance.Equip(sameEquipment, sameEquipment.Slot.transform.position);
            }
        };
    }

    public abstract void OnEquipped();
    public abstract void OnUnEquipped();

    protected void Event_Equip(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.Equip(this, Slot.transform.position);
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