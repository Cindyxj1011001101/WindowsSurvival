/// <summary>
/// 氧气罐
/// </summary>
public class OxygenCan : Card
{
    //public OxygenCan()
    //{
    //    cardName = "氧气罐";
    //    cardDesc = "氧气罐";
    //    cardType = CardType.Equipment;
    //    maxStackNum = 1;
    //    moveable = true;
    //    weight = 4f;
    //    events = new()
    //    {
    //        new Event("穿上", "穿上氧气罐", Event_Equip, Judge_Equip),
    //        new Event("脱下", "脱下氧气罐", Event_UnEquip,Judge_UnEquip)
    //    };
    //    components = new()
    //    {
    //        { typeof(EquipmentComponent), new EquipmentComponent(EquipmentType.Back)}
    //    };
    //}

    // public void Event_Equip()
    // {
    //     TryGetComponent<EquipmentComponent>(out var component);
    //     component.isEquipped = true;
    //     GameManager.Instance.EquipmentBag.AddCard(this);
    //     GameManager.Instance.PlayerBag.RemoveCards(this.cardName,1);
    //     StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 120);
    // }
    //public void Event_Equip()
    //{
    //    TryGetComponent<EquipmentComponent>(out var component);
    //    component.isEquipped = true;
    //    EquipmentManager.Instance.EquipCard(this);
    //    StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 120);
    //}

    //public bool Judge_Equip()
    //{
    //    return TryGetComponent<EquipmentComponent>(out var component) && !component.isEquipped;
    //}

    // public void Event_UnEquip()
    // {
    //     TryGetComponent<EquipmentComponent>(out var component);
    //     component.isEquipped = false;   
    //     GameManager.Instance.EquipmentBag.RemoveCards(this.cardName,1);
    //     GameManager.Instance.PlayerBag.AddCard(this);
    //     StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -120);
    // }
    //public void Event_UnEquip()
    //{
    //    TryGetComponent<EquipmentComponent>(out var component);
    //    component.isEquipped = false;
    //    EquipmentManager.Instance.UnequipCard(this);
    //    StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -120);
    //}

    //public bool Judge_UnEquip()
    //{
    //    return TryGetComponent<EquipmentComponent>(out var component) && component.isEquipped;
    //}
}