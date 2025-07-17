using System.Collections.Generic;

/// <summary>
/// 废铁刀
/// </summary>
public class ScrapIronKnife : Card
{
    public ScrapIronKnife()
    {
        cardName = "废铁刀";
        cardDesc = "一把废铁刀，可以用来切割食物。";
        //cardImage = Resources.Load<Sprite>("CardImage/废铁刀");
        cardType = CardType.Tool;
        maxStackNum = 1;
        moveable = true;
        weight = 0.3f;
        curEndurance = maxEndurance = 60;
        tags = new List<CardTag>
        {
            CardTag.Cut,
        };
        events = new List<Event>();
        components = new();
    }
}