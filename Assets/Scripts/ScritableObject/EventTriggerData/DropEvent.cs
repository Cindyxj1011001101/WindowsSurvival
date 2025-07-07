using System;
using System.Collections.Generic;
using UnityEngine;

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
public class DropEvent:EventTrigger
{
        public List<Drop> dropList;
        public override void EventResolve()
        {
        }

        public override void Init()
        {
            return;
        }
}