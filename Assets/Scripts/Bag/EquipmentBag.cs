public class EquipmentBag : BagBase
{
    private void Awake()
    {
        EventManager.Instance.AddListener<Card>(EventType.Equip, OnCardEquipped);
        EventManager.Instance.AddListener<Card>(EventType.Unequip, OnCardUnequipped);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<Card>(EventType.Equip, OnCardEquipped);
        EventManager.Instance.RemoveListener<Card>(EventType.Unequip, OnCardUnequipped);
    }

    protected override void Init()
    {
        InitBag(GameDataManager.Instance.EquipmentData);
    }

    /// <summary>
    /// 得到指定部位的装备
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Card GetEquipmentByType(EquipmentType type)
    {
        int index = (int)type;
        if (slots[index].IsEmpty) return null;
        return slots[index].PeekCard();
    }

    private void OnCardEquipped(Card equipment)
    {
        StateManager.Instance.AddLoad(equipment.weight);
    }

    private void OnCardUnequipped(Card equipment)
    {
        StateManager.Instance.AddLoad(-equipment.weight);
    }

    public override void AddCard(Card card)
    {
        // 在对应装备位置上添加装备卡
        card.TryGetComponent<EquipmentComponent>(out var component);
        var slotIndex = (int)component.equipmentType;
        slots[slotIndex].AddCard(card);
    }

    public override bool CanAddCard(Card card)
    {
        // 不是装备卡无法添加
        //if (card is not EquipmentCardInstance) return false;
        if (!card.TryGetComponent<EquipmentComponent>(out var component)) return false;

        // 不是从玩家背包装备的，要看载重够不够
        if ((card.slot == null || card.slot.Bag is not PlayerBag) &&
            StateManager.Instance.curLoad + card.weight > StateManager.Instance.maxLoad)
            return false;
        
        // 最后看装备格子有没有位置
        var slotIndex = (int)component.equipmentType;
        return slots[slotIndex].IsEmpty;
    }
}