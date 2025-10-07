/// <summary>
/// 氧烛
/// </summary>
public class OxygenCandle : Card
{
    private OxygenCandle()
    {
        Events = new()
        {
            new CardEvent("点燃", "点燃氧烛", Event_Light, null),
        };
    }

    private void Event_Light(out string tip)
    {
        tip = string.Empty;
        TurnTo("点燃的氧烛", Bag);
        SoundManager.Instance.PlaySound("点火_01");
    }
}