using UnityEngine;

/// <summary>
/// 装备卡牌格
/// </summary>
public class EquipmentCardSlot : CardSlot
{
    [SerializeField] private EquipmentType equipmentType;

    public override void AddCard(Card card)
    {
        base.AddCard(card);

        // 将装备状态设置为已装备
        card.TryGetComponent<EquipmentComponent>(out var equipmentComponent);
        equipmentComponent.isEquipped = true;

        // 触发穿上装备事件
        (card as EquipmentCard).OnEquipped();

        // 增加负重
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, card.Weight);
    }

    public override bool CanAddCard(Card card)
    {
        if (!IsEmpty) return false;

        if (!card.TryGetComponent<EquipmentComponent>(out var component)) return false;

        return component.equipmentType == equipmentType;
    }

    public override void RemoveCard(Card card)
    {
        base.RemoveCard(card);

        // 将装备状态设置为未装备
        card.TryGetComponent<EquipmentComponent>(out var equipmentComponent);
        equipmentComponent.isEquipped = false;

        // 触发脱下装备事件
        (card as EquipmentCard).OnUnEquipped();

        // 减少负重
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, -card.Weight);
    }
}