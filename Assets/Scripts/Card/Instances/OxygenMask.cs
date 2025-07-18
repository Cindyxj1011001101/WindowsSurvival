/// <summary>
/// 氧气面罩
/// </summary>

public class OxygenMask : Card
{
    //public OxygenMask()
    //{
    //    cardName = "氧气面罩";
    //    cardDesc = "用于水下探索的简易氧气面罩，让你能看清水下世界。";
    //    cardType = CardType.Equipment;
    //    maxStackNum = 1;
    //    moveable = true;
    //    weight = 1.1f;
    //    events = new()
    //    {
    //        new Event("装备", "装备氧气面罩", Event_Equip, Judge_Equip),
    //        new Event("卸下", "卸下氧气面罩", Event_UnEquip, Judge_UnEquip)
    //    };
    //    components = new()
    //    {
    //        { typeof(EquipmentComponent), new EquipmentComponent(EquipmentType.Head) }
    //    };
    //}

    //public void Event_Equip()
    //{
    //    if (!isEquipped)
    //    {
    //        isEquipped = true;
    //        EquipmentManager.Instance.EquipCard(this);
    //        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 30);
    //    }
    //}

    //public bool Judge_Equip()
    //{
    //    return !isEquipped;
    //}

    //public void Event_UnEquip()
    //{
    //    if (isEquipped)
    //    {
    //        isEquipped = false;
    //        EquipmentManager.Instance.UnequipCard(this);
    //        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -30);
    //    }
    //}

    //public bool Judge_UnEquip()
    //{
    //    return isEquipped;
    //}
}