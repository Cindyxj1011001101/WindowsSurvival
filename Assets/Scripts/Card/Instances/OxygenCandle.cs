/// <summary>
/// 氧烛
/// </summary>
[CardId("氧烛")]
public class OxygenCandle : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("点燃", $"点燃{CardName}\n点燃后每{ColorManager.ColorizeNumber(15, ColorManager.Cyan, "0")}分钟可产生{ColorManager.ColorizeNumber(10, ColorManager.Green, "0")}氧气，" +
            $"总计可以产生{ColorManager.ColorizeNumber(140, ColorManager.Green, "0")}氧气", Event_Light, null, sound: "点火_01");
    }

    private void Event_Light(CardEvent e)
    {
        TurnTo("点燃的氧烛", Bag);
    }
}