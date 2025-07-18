/// <summary>
/// 氧气罐
/// </summary>
public class OxygenCan : Card
{
    public OxygenCan()
    {
        cardName = "氧气罐";
        cardDesc = "氧气罐";
        cardType = CardType.Equipment;
        maxStackNum = 1;
        moveable = true;
        weight = 4f;
        events = new()
        {
            new Event("穿上", "穿上氧气罐", Event_Equip, Judge_Equip),
            new Event("脱下", "脱下氧气罐", Event_UnEquip,Judge_UnEquip)
        };
        components = new()
        {
            { typeof(EquipmentComponent), new EquipmentComponent(EquipmentType.Back)}
        };

        EventManager.Instance.AddListener<Card>(EventType.Equip, OnEquipped);
        EventManager.Instance.AddListener<Card>(EventType.Equip, OnUnequipped);
    }

    private void OnEquipped(Card equipment)
    {
        if (equipment == this)
        {
            TryGetComponent<EquipmentComponent>(out var component);
            component.isEquipped = true;
            // 当穿上装备时，额外氧气增加
            StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 120);
        }
    }

    private void OnUnequipped(Card equipment)
    {
        if (equipment == this)
        {
            TryGetComponent<EquipmentComponent>(out var component);
            component.isEquipped = false;

            // 当卸下装备时，额外氧气减少
            StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -120);
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