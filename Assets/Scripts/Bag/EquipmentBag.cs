using System.Collections.Generic;

public class EquipmentBag : BagBase
{
    public EquipmentCardSlot headSlot;
    public EquipmentCardSlot bodySlot;
    public EquipmentCardSlot backSlot;
    public EquipmentCardSlot legSlot;

    private Dictionary<EquipmentType, EquipmentCardSlot> equipmentSlotDict;
    private void Awake()
    {
        equipmentSlotDict = new()
        {
            { EquipmentType.Head, headSlot},
            { EquipmentType.Body, bodySlot},
            { EquipmentType.Back, backSlot},
            { EquipmentType.Leg, legSlot},
        };
        foreach (var slot in equipmentSlotDict.Values)
        {
            slot.ClearSlot();
            slot.SetBag(this);
        }
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

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        if(runtimeData.cardSlotsRuntimeData.Count == 0)return;
        headSlot.InitFromRuntimeData(runtimeData.cardSlotsRuntimeData[0]);
        bodySlot.InitFromRuntimeData(runtimeData.cardSlotsRuntimeData[1]);
        backSlot.InitFromRuntimeData(runtimeData.cardSlotsRuntimeData[2]);
        legSlot.InitFromRuntimeData(runtimeData.cardSlotsRuntimeData[3]);
    }

    /// <summary>
    /// 得到指定部位的装备
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Card GetEquipmentByType(EquipmentType type)
    {
        return equipmentSlotDict[type].PeekCard();
    }

    private void OnCardEquipped(Card equipment)
    {
        StateManager.Instance.AddLoad(equipment.Weight);
    }

    private void OnCardUnequipped(Card equipment)
    {
        StateManager.Instance.AddLoad(-equipment.Weight);
    }

    /// <summary>
    /// 穿上装备
    /// </summary>
    /// <param name="equipment"></param>
    public void Equip(Card equipment)
    {
        // 从原来的格子里移除
        equipment.Slot.RemoveCard(equipment);
        // 添加到装备格子里
        AddCard(equipment);
    }

    /// <summary>
    /// 脱下装备
    /// </summary>
    /// <param name="type"></param>
    public void Unequip(Card equipment)
    {
        // 从装备格子中移除
        equipment.TryGetComponent<EquipmentComponent>(out var component);
        equipmentSlotDict[component.equipmentType].RemoveCard(equipment);

        // 添加到背包(优先)或环境中
        GameManager.Instance.AddCard(equipment, true);
    }

    /// <summary>
    /// 判断能否装备
    /// </summary>
    /// <param name="equipment"></param>
    /// <returns></returns>
    public bool CanEquip(Card equipment)
    {
        return CanAddCard(equipment);
    }

    public override void AddCard(Card card, bool refreshImmediately = true)
    {
        // 在对应装备位置上添加装备卡
        card.TryGetComponent<EquipmentComponent>(out var component);
        equipmentSlotDict[component.equipmentType].AddCard(card, refreshImmediately);
    }

    public override bool CanAddCard(Card card)
    {
        // 不是装备卡无法添加
        if (!card.TryGetComponent<EquipmentComponent>(out var component)) return false;

        // 不是从玩家背包装备的，要看载重够不够
        if ((card.Slot == null || card.Slot.Bag is not PlayerBag) &&
            StateManager.Instance.CurLoad + card.Weight > StateManager.Instance.MaxLoad)
            return false;
        
        // 最后看装备格子有没有位置
        return equipmentSlotDict[component.equipmentType].IsEmpty;
    }

    public override List<(CardSlot, int)> GetSlotsCanAddCard(Card card, int count)
    {
        List<(CardSlot, int)> result = new();

        if (!card.TryGetComponent<EquipmentComponent>(out var component)) return result;

        if (!equipmentSlotDict[component.equipmentType].IsEmpty) return result;

        result.Add((equipmentSlotDict[component.equipmentType], 1));

        return result;
    }
}