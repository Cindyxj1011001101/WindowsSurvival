public abstract class EquipmentCard : Card
{
    protected override void RegisterCardEvents()
    {
        var desc = GetEquipDesc();
        if (!string.IsNullOrEmpty(desc))
            desc = "\n装备后" + desc;
        desc = "装备" + CardName + desc;
        AddCardEvent("装备", desc, Event_Equip, Judge_Equip);
        AddCardEvent("卸下", "", Event_UnEquip, Judge_UnEquip);
    }

    protected override void OnInit()
    {
        // 装备损坏后尝试从背包中重新找到一件相同的装备并且穿上
        if (durability != null)
            durability.onBroken = TryEquipSameOneOnBroken;
    }

    private void TryEquipSameOneOnBroken()
    {
        var sameEquipment = GameManager.Instance.PlayerBag.FindCardOfName(CardName);
        if (sameEquipment != null && GameManager.Instance.CanEquip(sameEquipment, out _))
        {
            GameManager.Instance.Equip(sameEquipment, sameEquipment.Slot.transform.position);
        }
    }

    public abstract void OnEquipped();
    public abstract void OnUnEquipped();
    public virtual string GetEquipDesc() => "";

    protected void Event_Equip(CardEvent e)
    {
        GameManager.Instance.Equip(this, Slot.transform.position);
    }

    protected bool Judge_Equip(out string hint)
    {
        return GameManager.Instance.CanEquip(this, out hint);
    }

    protected void Event_UnEquip(CardEvent e)
    {
        GameManager.Instance.Unequip(this);
    }

    protected bool Judge_UnEquip(out string hint)
    {
        hint = string.Empty;
        return equipment.isEquipped;
    }
}