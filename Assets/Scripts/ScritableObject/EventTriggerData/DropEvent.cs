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
    public CardData cardData;

    public int DropNum;

    //掉落描述
    public string DropDesc;
}

[CreateAssetMenu(fileName = "DropEvent", menuName = "ScritableObject/DropEvent")]
public class DropEvent : EventTrigger
{
    public List<Drop> dropList;
    public List<Drop> curDropList;

    public override void EventResolve()
    {
        if (curDropList.Count != 0)
        {
            int sumProb = 0;
            foreach (var drop in curDropList)
            {
                sumProb += drop.DropProb;
            }

            int rand = Random.Range(0, sumProb);
            foreach (var drop in curDropList)
            {
                if (rand < drop.DropProb)
                {
                    EffectResolve.Instance.AddDropCard(drop, true);
                    curDropList.Remove(drop);
                    return;
                }

                rand -= drop.DropProb;
            }
        }
    }

    public override void Init()
    {
        curDropList = new List<Drop>(dropList);
        return;
    }
}