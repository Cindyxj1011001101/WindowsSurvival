using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 鼠患
/// </summary>
public class RatInfestation : GameEvent
{
    private List<Card> foodCards = new();

    public override bool CanTriggerThisEvent()
    {
        foodCards = GameManager.Instance.CurEnvironmentBag.FindCardsOfTag(CardTag.Food);
        return !foodCards.IsNullOrEmpty();
    }

    public override void OnTrigger()
    {
        var destroyCount = Random.Range(2, 8); // 随机破坏2~7张食物卡牌
        destroyCount = Mathf.Min(destroyCount, foodCards.Count); // 不超过现有食物卡牌数量

        for (int i = 0; i < destroyCount; i++)
        {
            var index = Random.Range(0, foodCards.Count);
            var cardToDestroy = foodCards[index];
            foodCards.RemoveAt(index);
            cardToDestroy.DestroyThis();
        }

        // 50%概率生成一张老鼠卡牌
        if (Random.value < 0.5f)
        {
            GameManager.Instance.AddCardsToTargetEnv(GameManager.Instance.CurEnvironmentBag, CardFactory.CreateCard("老鼠"));
        }

        foodCards.Clear();
    }
}
