/// <summary>
/// 氧烛
/// </summary>
public class OxygenCandle : Card
{
    private OxygenCandle()
    {
        Events = new()
        {
            new Event("点燃", "点燃氧烛", Event_Light, null),
        };
    }

    public void Event_Light(out string tip)
    {
        StopUpdating();

        tip = string.Empty;
        AddCard("点燃的氧烛", true);
        SoundManager.Instance.PlaySound("点火");

        DestroyThis();
    }
}