using System.Collections.Generic;

public class OxygenCandle : Card
{   
    public OxygenCandle()
    {
        cardName = "氧烛";
        cardDesc = "无需氧气助燃的化学氧烛，顶部有一个引信，按下后内部就会开始反应，在水下也能轻松点燃。";
        cardType = CardType.Tool;
        maxStackNum = 5;
        moveable = true;
        weight = 1.8f;
        events = new List<Event>();
        events.Add(new Event("点燃", "点燃氧烛", Event_Light, null));
        tags = new List<CardTag>();
        components = new();
    }

    public void Event_Light()
    {
        Use();
        GameManager.Instance.AddCard(new LightenedOxygenCandle(), true);
    }
}