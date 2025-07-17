using System.Collections.Generic;

/// <summary>
/// 捞网
/// </summary>
public class FishingNet : Card
{
    public FishingNet()
    {
        cardName = "捞网";
        cardDesc = "捞网";
        cardType = CardType.Tool;
        maxStackNum = 1;
        moveable = true;
        weight = 1.2f;
        curEndurance = maxEndurance = 60;
        tags = new List<CardTag>();
        events = new List<Event>();
        components = new();
    }
}