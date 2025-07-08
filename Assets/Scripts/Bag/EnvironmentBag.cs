using System.Collections.Generic;

public class EnvironmentBag : BagBase
{
    public CardEvent exploreEvent; // 探索事件
    public List<Drop> disposableDropList; // 一次性掉落的掉落列表
    public float currentExploratoryDegree; // 当前探索度

    public override void AddCard(CardInstance card)
    {
        // 如果放不下，就新增格子
        if (!CanAddCard(card))
            // 暂定每次新增3个格子
            AddSlot(3);

        // 添加卡牌
        base.AddCard(card);
    }
}