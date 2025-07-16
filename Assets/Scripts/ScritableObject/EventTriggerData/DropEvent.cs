using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Drop
{
    //掉落概率
    public int DropProb;

    //掉落物体及其数量
    public Card card;

    public int DropNum;

    //掉落描述
    public string DropDesc;


    public string cardName;

    [JsonIgnore]
    public CardData CardData => Resources.Load<CardData>("ScriptableObject/Card/" + cardName);

    [JsonIgnore]
    public bool IsEmpty => DropNum == 0;

    public Drop(Card card, int dropNum, string dropDesc)
    {
        this.card = card;
        this.DropNum = dropNum;
        this.DropDesc = dropDesc;
    }

}

[CreateAssetMenu(fileName = "DropEvent", menuName = "ScritableObject/DropEvent")]
public class DropEvent : EventTrigger
{
    public List<Drop> dropList;
    private int sumProb;

    public override void Invoke()
    {
        if (dropList != null)
        {
            int rand = Random.Range(0, sumProb);
            foreach (var drop in dropList)
            {
                if (rand < drop.DropProb)
                {
                    GameManager.Instance.AddDropCard(drop, true);
                    return;
                }

                rand -= drop.DropProb;
            }
        }
    }

    public override void Init()
    {
        sumProb = 0;
        foreach (var drop in dropList)
        {
            sumProb += drop.DropProb;
        }
        return;
    }
}