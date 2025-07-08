public class EnvironmentBag : BagBase
{
    public override void AddCard(CardInstance card)
    {
        // 如果放不下，就新增格子
        if (!CanAddCard(card))
        {
            // 暂定每次新增3个格子
            AddSlot(5);
            base.AddCard(card);
        }
    }
}