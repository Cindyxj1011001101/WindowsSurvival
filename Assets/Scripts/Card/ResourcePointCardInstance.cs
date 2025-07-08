public class ResourcePointCardInstance : CardInstance
{
    private int currentEndurance;

    public int CurrentEndurance => currentEndurance;

    public override void InitFromCardData(CardData cardData)
    {
        currentEndurance = (cardData as ResourcePointCardData).maxEndurance;
    }
}