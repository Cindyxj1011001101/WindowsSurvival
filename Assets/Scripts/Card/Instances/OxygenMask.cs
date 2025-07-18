/// <summary>
/// 氧气面罩
/// </summary>
public class OxygenMask : Card
{
    public OxygenMask()
    {
        cardName = "氧气面罩";
        cardDesc = "用于水下探索的简易氧气面罩，让你能看清水下世界。";
        cardType = CardType.Equipment;
        maxStackNum = 1;
        moveable = true;
        weight = 1.1f;
        events = new()
        {
            new Event("装备", "装备氧气面罩", Event_Equip, Judge_Equip),
            new Event("卸下", "卸下氧气面罩", Event_UnEquip, Judge_UnEquip)
        };
        components = new()
        {
            { typeof(EquipmentComponent), new EquipmentComponent(EquipmentType.Head) }
        };
    }

    private void OnEquipped(Card equipment)
    {
        if (equipment == this)
        {
            TryGetComponent<EquipmentComponent>(out var component);
            component.isEquipped = true;
            // 当穿上装备时，额外氧气增加
            StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 30);
        }
    }

    private void OnUnequipped(Card equipment)
    {
        if (equipment == this)
        {
            TryGetComponent<EquipmentComponent>(out var component);
            component.isEquipped = false;

            // 当卸下装备时，额外氧气减少
            StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -30);
        }
    }

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener<Card>(EventType.Equip, OnEquipped);
        EventManager.Instance.RemoveListener<Card>(EventType.Equip, OnUnequipped);
    }

    public void Event_Equip()
    {
        GameManager.Instance.EquipmentBag.Equip(this);
    }

    public bool Judge_Equip()
    {
        TryGetComponent<EquipmentComponent>(out var component);
        // 已经穿上装备了
        if (component.isEquipped) return false;
        return GameManager.Instance.EquipmentBag.CanEquip(this);
    }

    public void Event_UnEquip()
    {
        GameManager.Instance.EquipmentBag.Unequip(this);
    }

    public bool Judge_UnEquip()
    {
        return TryGetComponent<EquipmentComponent>(out var component) && component.isEquipped;
    }
}