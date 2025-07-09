public class ToolCardInstance : CardInstance
{
    public int currentEndurance;

    public override void InitFromCardData(CardData cardData)
    {
        currentEndurance = (cardData as ToolCardData).maxEndurance;
    }
}