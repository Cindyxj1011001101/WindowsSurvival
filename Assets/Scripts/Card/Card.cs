using System;

[Serializable]
public class Card
{
    public CardData cardData;
    public int cardNum;
    public Card(CardData data, int i)
    {
        cardData = data;
        cardNum = i;
    }
}
public class ToolCard:Card
{
    public int curEndurance;

    public ToolCard(CardData data, int i, int endurance) : base(data, i)
    {
        this.curEndurance = endurance;
    }
}
public class ResourcePointCard:Card
{
    public int curEndurance;

    public ResourcePointCard(CardData data, int i, int endurance) : base(data, i)
    {
        this.curEndurance = endurance;
    }
}