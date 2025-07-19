/// <summary>
/// 氧烛
/// </summary>
public class OxygenCandle : Card
{   
    public OxygenCandle()
    {
        events = new()
        {
            new Event("点燃", "点燃氧烛", Event_Light, null),
        };
    }

    public void Event_Light()
    {
        DestroyThis();
        GameManager.Instance.AddCard("点燃的氧烛", true);
    }
}