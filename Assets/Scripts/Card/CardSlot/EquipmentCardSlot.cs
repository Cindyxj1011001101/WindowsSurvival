/// <summary>
/// 装备卡牌格
/// </summary>
public class EquipmentCardSlot : CardSlot
{
    public EquipmentType equipmentType;

    public override void AddCard(Card card, bool refreshImmediately = true)
    {
        base.AddCard(card, refreshImmediately);
        EventManager.Instance.TriggerEvent(EventType.Equip, card);
    }

    public override bool CanAddCard(Card card)
    {
        if (!IsEmpty) return false;

        if (!card.TryGetComponent<EquipmentComponent>(out var component)) return false;

        return component.equipmentType == equipmentType;
    }

    public override void RemoveCard(Card card, bool refreshImmediately = true)
    {
        base.RemoveCard(card, refreshImmediately);
        EventManager.Instance.TriggerEvent(EventType.Unequip, card);
    }
}