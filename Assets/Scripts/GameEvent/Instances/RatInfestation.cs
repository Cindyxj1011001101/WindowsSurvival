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
               $"损失了这些东西: " + ColorManager.Warning(lostCardsStr);
    }

    protected override bool CanTriggerThisEvent()
    {
        foodCards = GameManager.Instance.CurEnvironmentBag.FindCardsOfTag(CardTag.Food);
        return !foodCards.IsNullOrEmpty();
    }

    protected override void OnTrigger()
    {
        var destroyCount = Random.Range(2, 8); // 随机破坏2~7张食物卡牌
        destroyCount = Mathf.Min(destroyCount, foodCards.Count); // 不超过现有食物卡牌数量

        var destroyedCards = new Dictionary<string, int>();

        for (int i = 0; i < destroyCount; i++)
        {
            // 随机选择一张食物卡牌进行破坏
            var cardToDestroy = foodCards.GetRandomly(false);
            cardToDestroy.DestroyThis();

            // 记录被破坏的卡牌数量
            if (!destroyedCards.ContainsKey(cardToDestroy.CardName))
                destroyedCards.Add(cardToDestroy.CardName, 1);
            else
                destroyedCards[cardToDestroy.CardName]++;
        }

        lostCardsStr = "";
        foreach (var kvp in destroyedCards)
        {
            lostCardsStr += $"{kvp.Key} x {kvp.Value}、";
        }

        lostCardsStr = lostCardsStr.TrimEnd('、');
        foodCards.Clear();

        // 50%概率生成一张老鼠卡牌
        if (Random.value < 0.5f)
        {
            GameManager.Instance.AddCardToTargetEnv(CardFactory.CreateCard("老鼠"), GameManager.Instance.CurEnvironmentBag);
        }
    }
}
