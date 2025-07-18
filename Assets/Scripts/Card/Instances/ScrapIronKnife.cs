/// <summary>
/// 废铁刀
/// </summary>
public class ScrapIronKnife : Card
{
    public ScrapIronKnife()
    {
        cardName = "废铁刀";
        cardDesc = "一把废铁刀，可以用来切割食物。";
        cardType = CardType.Tool;
        maxStackNum = 1;
        moveable = true;
        weight = 4f;
        components = new()
        {
            { typeof(ToolComponent), new ToolComponent(ToolType.Cut) },
            { typeof(DurabilityComponent), new DurabilityComponent(60) }
        };
    }
}