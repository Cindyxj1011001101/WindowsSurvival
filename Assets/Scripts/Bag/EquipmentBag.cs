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
        tip = string.Empty;
        // 不是装备卡无法添加
        if (!card.TryGetComponent<EquipmentComponent>(out var component))
        {
            tip = "这个不可以装备";
            return false;
        }

        if (component.isEquipped)
        {
            tip = "你无需重复穿上该装备";
            return false;
        }

        float curLoad = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].CurValue;
        float maxLoad = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].MaxValue;

        // 不是从玩家背包装备的，要看载重够不够
        if ((card.Bag == null || card.Bag is not PlayerBag) &&
            curLoad + card.Weight > maxLoad)
        {
            tip = "再穿上它就太重了";
            return false;
        }

        if (!Slots[(int)component.equipmentType].IsEmpty)
        {
            tip = "同样的部位上已经有一件装备了";
            return false;
        }
        
        // 最后看装备格子有没有位置
        return true;
    }

    public override void OnAddCard(Card card)
    {
        // 将装备状态设置为已装备
        card.TryGetComponent<EquipmentComponent>(out var equipmentComponent);
        equipmentComponent.isEquipped = true;

        // 触发穿上装备事件
        (card as EquipmentCard).OnEquipped();
    }

    public override void OnRemoveCard(Card card)
    {
        // 将装备状态设置为未装备
        card.TryGetComponent<EquipmentComponent>(out var equipmentComponent);
        equipmentComponent.isEquipped = false;

        // 触发脱下装备事件
        (card as EquipmentCard).OnUnEquipped();
    }
}