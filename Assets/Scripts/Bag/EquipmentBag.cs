public class EquipmentBag : Bag
{
    /// <summary>
    /// 得到指定部位的装备
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Card GetEquipmentByType(EquipmentType type)
    {
        return Slots[(int)type].PeekCard();
    }

    public override void AddCard(Card card)
    {
        // 在对应装备位置上添加装备卡
        card.TryGetComponent<EquipmentComponent>(out var component);
        Slots[(int)component.equipmentType].AddCard(card);
    }

    public override bool CanAddCard(Card card, out string tip)
    {
        // 不是装备卡无法添加
        if (!card.TryGetComponent<EquipmentComponent>(out var component))
        {
            tip = "不可装备";
            return false;
        }

        if (component.isEquipped)
        {
            tip = "该装备已经穿上了";
            return false;
        }

        if (!Slots[(int)component.equipmentType].IsEmpty)
        {
            tip = "相同部位上已有一件装备";
            return false;
        }

        // 考虑重量
        if (!CanAddCardConsideringWeight(card, out tip))
        {
            return false;
        }

        return true;
    }

    public override void OnAddCard(Card card)
    {
        // 将装备状态设置为已装备
        card.TryGetComponent<EquipmentComponent>(out var equipmentComponent);
        equipmentComponent.isEquipped = true;

        // 触发穿上装备事件
        (card as EquipmentCard).OnEquipped();

        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, card.Weight);
    }

    public override void OnRemoveCard(Card card)
    {
        // 将装备状态设置为未装备
        card.TryGetComponent<EquipmentComponent>(out var equipmentComponent);
        equipmentComponent.isEquipped = false;

        // 触发脱下装备事件
        (card as EquipmentCard).OnUnEquipped();

        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, -card.Weight);
    }
}