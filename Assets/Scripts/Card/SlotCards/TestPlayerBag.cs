//public class TestPlayerBag : Bag
//{
//    public override void OnAddCard(Card card)
//    {
//        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, card.Weight);
//    }

//    public override void OnRemoveCard(Card card)
//    {
//        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, -card.Weight);
//    }

//    //public override bool CanAddCard(Card card, out string tip)
//    //{
//    //    float curLoad = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].CurValue;
//    //    float maxLoad = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].MaxValue;



//    //    // 因为背包和装备共用载重
//    //    // 不是从装备中添加的，要看载重够不够
//    //    if ((card.Slot == null || card.Slot.Bag is not EquipmentBag) &&
//    //        curLoad + card.Weight > maxLoad)
//    //    {
//    //        tip = "再带上它就太重了";
//    //        return false;
//    //    }

//    //    // 载重足够则按照父类的判断标准进行判断
//    //    return base.CanAddCard(card, out tip);
//    //}
//}