public enum EquipmentType
{
    Head = 0,
    Body = 1,
    Back = 2,
    Leg = 3,
}

public class EquipmentBag : BagBase
{
    private void Awake()
    {
        EventManager.Instance.AddListener<EquipmentCardInstance>(EventType.Equip, OnCardEquipped);
        EventManager.Instance.AddListener<EquipmentCardInstance>(EventType.Unequip, OnCardUnequipped);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<EquipmentCardInstance>(EventType.Equip, OnCardEquipped);
        EventManager.Instance.RemoveListener<EquipmentCardInstance>(EventType.Unequip, OnCardUnequipped);
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
    public EquipmentCardInstance GetEquipmentByType(EquipmentType type)
    {
        int index = (int)type;
        if (slots[index].IsEmpty) return null;
        return slots[index].PeekCard() as EquipmentCardInstance;
    }

    private void OnCardEquipped(EquipmentCardInstance equipment)
    {
        StateManager.Instance.AddLoad(equipment.CardData.weight);
    }

    private void OnCardUnequipped(EquipmentCardInstance equipment)
    {
        StateManager.Instance.AddLoad(-equipment.CardData.weight);
    }

    public override void AddCard(CardInstance card)
    {
        // 在对应装备位置上添加装备卡
        var slotIndex = (int)(card.CardData as EquipmentCardData).equipmentType;
        slots[slotIndex].AddCard(card);
    }

    public override bool CanAddCard(CardInstance card)
    {
        // 不是装备卡无法添加
        if (card is not EquipmentCardInstance) return false;

        // 不是从玩家背包装备的，要看载重够不够
        if ((card.Slot == null || card.Slot.Bag is not PlayerBag) &&
            StateManager.Instance.curLoad + card.CardData.weight > StateManager.Instance.maxLoad)
            return false;
        
        // 最后看装备格子有没有位置
        var slotIndex = (int)(card.CardData as EquipmentCardData).equipmentType;
        return slots[slotIndex].IsEmpty;
    }
}