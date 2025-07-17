using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Drop
{
    public string cardName; // 卡牌名称
    public int dropNum; // 掉落数量
    public int dropProb; // 掉落概率，是正整数，dropProb除以一个掉落列表中所有地dropProb之和为真实掉落概率

    public UnityAction<Card> onCardCreated;

    public Drop() { }

    public Drop(string cardName, int dropNum, int dropProb)
    {
        this.cardName = cardName;
        this.dropNum = dropNum;
        this.dropProb = dropProb;
    }

    public Drop(string cardName, int dropNum, int dropProb, UnityAction<Card> onCardCreated) : this(cardName, dropNum, dropProb)
    {
        this.onCardCreated = onCardCreated;
    }
}

public abstract class DropList
{
    public List<Drop> dropList = new();

    public abstract List<Card> RandomDrop();

    public abstract List<Card> CertainDrop(string cardName);

    public abstract void AddDrop(Drop drop);
    protected List<Card> DropCards(Drop drop)
    {
        List<Card> droppedCards = new List<Card>();
        // 创建卡牌
        for (int j = 0; j < drop.dropNum; j++)
        {
            // 创建卡牌
            var card = CardFactory.CreateCard(drop.cardName);
            // 初始化卡牌
            drop.onCardCreated?.Invoke(card);
            // 添加到返回列表
            droppedCards.Add(card);
        }
        return droppedCards;
    }
}

[System.Serializable]
public class DisposableDropList : DropList
{
    public List<Drop> remainingDrops = new List<Drop>();

    public bool IsEmpty => remainingDrops.Count == 0;

    public void Init()
    {
        remainingDrops = new List<Drop>(dropList);
    }

    public void Init(List<Drop> remainingDrops)
    {
        this.remainingDrops = remainingDrops;
    }

    public override void AddDrop(Drop drop)
    {
        dropList.Add(drop);
        remainingDrops.Add(drop);
    }

    /// <summary>
    /// 随机掉落
    /// </summary>
    /// <returns></returns>
    public override List<Card> RandomDrop()
    {
        if (remainingDrops.Count == 0)
        {
            Debug.LogWarning("No drops remaining in disposable drop list!");
            return null;
        }

        // 计算总概率
        int totalProb = 0;
        foreach (var drop in remainingDrops)
        {
            totalProb += drop.dropProb;
        }

        // 随机选择
        int randomValue = Random.Range(0, totalProb);
        int currentProb = 0;

        for (int i = 0; i < remainingDrops.Count; i++)
        {
            currentProb += remainingDrops[i].dropProb;
            if (randomValue < currentProb)
            {
                // 获取掉落项
                Drop drop = remainingDrops[i];

                // 从剩余列表中移除（一次性掉落）
                remainingDrops.RemoveAt(i);

                return DropCards(drop);
            }
        }

        // 理论上不会执行到这里
        Debug.LogError("Drop selection failed!");
        return null;
    }

    /// <summary>
    /// 掉落指定卡牌
    /// </summary>
    /// <param name="cardName"></param>
    /// <returns></returns>
    public override List<Card> CertainDrop(string cardName)
    {
        for (int i = 0; i < remainingDrops.Count; i++)
        {
            Drop drop = remainingDrops[i];
            if (drop.cardName == cardName)
            {
                remainingDrops.RemoveAt(i);
                return DropCards(drop);
            }
        }

        return null;
    }
}

[System.Serializable]
public class RepeatableDropList : DropList
{
    public override void AddDrop(Drop drop)
    {
        dropList.Add(drop);
    }

    public override List<Card> CertainDrop(string cardName)
    {
        for (int i = 0; i < dropList.Count; i++)
        {
            Drop drop = dropList[i];
            if (drop.cardName == cardName)
            {
                return DropCards(drop);
            }
        }

        return null;
    }

    public override List<Card> RandomDrop()
    {
        if (dropList.Count == 0)
        {
            Debug.LogWarning("Drop list is empty!");
            return null;
        }

        // 计算总概率
        int totalProb = 0;
        foreach (var drop in dropList)
        {
            totalProb += drop.dropProb;
        }

        // 随机选择
        int randomValue = Random.Range(0, totalProb);
        int currentProb = 0;

        foreach (var drop in dropList)
        {
            currentProb += drop.dropProb;
            if (randomValue < currentProb)
            {
                return DropCards(drop);
            }
        }

        // 理论上不会执行到这里
        Debug.LogError("Drop selection failed!");
        return null;
    }
}