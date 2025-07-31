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
        //EventManager.Instance.AddListener<Card>(EventType.Equip, OnCardEquipped);
        //EventManager.Instance.AddListener<Card>(EventType.Unequip, OnCardUnequipped);
    }

    //private void OnDestroy()
    //{
    //    EventManager.Instance.RemoveListener<Card>(EventType.Equip, OnCardEquipped);
    //    EventManager.Instance.RemoveListener<Card>(EventType.Unequip, OnCardUnequipped);
    //}

    public override void Init()
    {
        InitBag(GameDataManager.Instance.EquipmentData);
    }

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        if (runtimeData.cardSlots.Count == 0) return;

        headSlot.Init(runtimeData.cardSlots[0]);
        bodySlot.Init(runtimeData.cardSlots[1]);
        backSlot.Init(runtimeData.cardSlots[2]);
        legSlot.Init(runtimeData.cardSlots[3]);
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

    //private void OnCardEquipped(Card equipment)
    //{
    //    StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, equipment.Weight);
    //}

    //private void OnCardUnequipped(Card equipment)
    //{
    //    StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, -equipment.Weight);
    //}

    ///// <summary>
    ///// 穿上装备
    ///// </summary>
    ///// <param name="equipment"></param>
    //public void Equip(Card equipment)
    //{
    //    var originalSlot = equipment.Slot;
    //    // 从原来的格子里移除
    //    originalSlot.RemoveCard(equipment);
    //    originalSlot.RefreshCurrentDisplay();
    //    // 添加到装备格子里
    //    AddCard(equipment);
    //    equipment.Slot.RefreshCurrentDisplay();
    //}

    ///// <summary>
    ///// 脱下装备
    ///// </summary>
    ///// <param name="type"></param>
    //public void Unequip(Card equipment)
    //{
    //    // 从装备格子中移除
    //    equipment.TryGetComponent<EquipmentComponent>(out var component);
    //    equipmentSlotDict[component.equipmentType].RemoveCard(equipment);
    //    equipmentSlotDict[component.equipmentType].RefreshCurrentDisplay();

    //    // 添加到背包(优先)或环境中
    //    GameManager.Instance.AddCardWithTween(equipment, equipmentSlotDict[component.equipmentType].transform.position, true);
    //}

    ///// <summary>
    ///// 判断能否装备
    ///// </summary>
    ///// <param name="equipment"></param>
    ///// <returns></returns>
    //public bool CanEquip(Card equipment)
    //{
    //    return CanAddCard(equipment);
    //}

    public override void AddCard(Card card/*, bool refreshImmediately = true*/)
    {
        // 在对应装备位置上添加装备卡
        card.TryGetComponent<EquipmentComponent>(out var component);
        equipmentSlotDict[component.equipmentType].AddCard(card/*, refreshImmediately*/);
    }

    public override bool CanAddCard(Card card)
    {
        // 不是装备卡无法添加
        if (!card.TryGetComponent<EquipmentComponent>(out var component)) return false;


        float curLoad = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].CurValue;
        float maxLoad = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].MaxValue;
        // 不是从玩家背包装备的，要看载重够不够
        if ((card.Slot == null || card.Slot.Bag is not PlayerBag) &&
            curLoad + card.Weight > maxLoad)
            return false;
        
        // 最后看装备格子有没有位置
        return equipmentSlotDict[component.equipmentType].IsEmpty;
    }

    //public override List<(CardSlot, int)> GetSlotsCanAddCard(Card card, int count)
    //{
    //    List<(CardSlot, int)> result = new();

    //    if (!card.TryGetComponent<EquipmentComponent>(out var component)) return result;

    //    if (!equipmentSlotDict[component.equipmentType].IsEmpty) return result;

    //    result.Add((equipmentSlotDict[component.equipmentType], 1));

    //    return result;
    //}
}