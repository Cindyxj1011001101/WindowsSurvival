/// <summary>
/// 氧烛
/// </summary>
public class OxygenCandle : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("点燃", "", Event_Light, null);
    }

    private void Event_Light(out string tip, CardEvent e)
    {
        PlaySound("点火_01");
        tip = string.Empty;
        TurnTo("点燃的氧烛", Bag);
    }
}