using System.Collections.Generic;
using UnityEngine;

public class ScrapIronKnife:Card
{
    public int maxEndurance;//最大耐久度
    public int curEndurance;//当前耐久度

    public ScrapIronKnife()
    {
        cardName = "废铁刀";
        cardDesc = "一把废铁刀，可以用来切割食物。";
        cardImage = Resources.Load<Sprite>("CardImage/废铁刀");
        cardType = CardType.Tool;
        maxStackNum = 1;
        maxEndurance = 60;
        curEndurance = 60;
        moveable = true;
        weight = 0.3f;
        tags = new List<CardTag>();
        tags.Add(CardTag.Cut);
        events = new List<Event>();
    }

    public override void Use()
    {
        curEndurance--;
        if(curEndurance<=0)
        {
            //TODO:删除本卡牌
        }
        return;
    }
}