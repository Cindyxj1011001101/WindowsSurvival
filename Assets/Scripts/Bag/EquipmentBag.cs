using System.Collections.Generic;

public class EquipmentBag : BagBase
{
    private Dictionary<EquipmentType, EquipmentCardSlot> equipmentSlotDict;

    public override void Init()
    {
        InitBag(GameDataManager.Instance.EquipmentData);
    }

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        for (int i = 0; i < runtimeData.cardSlots.Count; i++)
        {
            slots[i].Init(runtimeData.cardSlots[i]);
        }
        equipmentSlotDict = new()
        {
            { EquipmentType.Head, slots[0] as EquipmentCardSlot},
            { EquipmentType.Body, slots[1] as EquipmentCardSlot},
            { EquipmentType.Back, slots[2] as EquipmentCardSlot},
            { EquipmentType.Leg, slots[3] as EquipmentCardSlot},
        };
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

    public override void AddCard(Card card)
    {
        // 在对应装备位置上添加装备卡
        card.TryGetComponent<EquipmentComponent>(out var component);
        equipmentSlotDict[component.equipmentType].AddCard(card);
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
}