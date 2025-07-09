public class ResourcePointCardInstance : CardInstance
{
    public int currentEndurance;

    public override void InitFromCardData(CardData cardData)
    {
        currentEndurance = (cardData as ResourcePointCardData).maxEndurance;
    }
}