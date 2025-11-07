/// <summary>
/// 氧烛
/// </summary>
public class OxygenCandle : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("点燃", "", Event_Light, null);
    }

    private void Event_Light(out string tip)
    {
        tip = string.Empty;
        PlaySound("点火_01");
        TurnTo("点燃的氧烛", Bag);
    }
}