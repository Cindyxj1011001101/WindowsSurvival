using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 鼠患
/// </summary>
public class RatInfestation : GameEvent
{
    private List<Card> foodCards = new();

    private string lostCardsStr;

    public override string GetDetails()
    {
        return $"一道黑影闪过，地上的食物就不见了。\n\n" +
               $"也许别把食物放在地上会比较好。\n\n" +
               $"损失了这些东西: " + lostCardsStr;
    }

    public override bool CanTriggerThisEvent()
    {
        foodCards = GameManager.Instance.CurEnvironmentBag.FindCardsOfTag(CardTag.Food);
        return !foodCards.IsNullOrEmpty();
    }

    public override void OnTrigger()
    {
        var destroyCount = Random.Range(2, 8); // 随机破坏2~7张食物卡牌
        destroyCount = Mathf.Min(destroyCount, foodCards.Count); // 不超过现有食物卡牌数量

        lostCardsStr = "";
        for (int i = 0; i < destroyCount; i++)
        {
            var index = Random.Range(0, foodCards.Count);
            var cardToDestroy = foodCards[index];
            foodCards.RemoveAt(index);
            cardToDestroy.DestroyThis();
            lostCardsStr += $"{cardToDestroy.CardName}、";
        }
        lostCardsStr = lostCardsStr.TrimEnd('、');
        foodCards.Clear();

        // 50%概率生成一张老鼠卡牌
        if (Random.value < 0.5f)
        {
            GameManager.Instance.AddCardsToTargetEnv(GameManager.Instance.CurEnvironmentBag, CardFactory.CreateCard("老鼠"));
        }
    }
}
