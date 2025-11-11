/// <summary>
/// 氧烛
/// </summary>
public class OxygenCandle : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("点燃", "", Event_Light, null, sound: "点火_01");
    }

    private void Event_Light(CardEvent e)
    {
        TurnTo("点燃的氧烛", Bag);
    }
}