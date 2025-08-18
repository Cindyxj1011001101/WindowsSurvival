
public class SelfHeatingCookingBag : Card
{
    private SelfHeatingCookingBag()
    {
        Events = new()
        {
            new Event("烹饪", "烹饪",Event_Cook, Judge_Cook)
        };
    }
    //TODO:拖拽交互后将食物变为煮熟版本
    public void Event_Cook(out string tip)
    {
        tip = string.Empty;
        Use();
        //让食物变为煮熟版本
        TimeManager.Instance.AddTime(15);
    }

    private bool Judge_Cook(out string hint)
    {
        hint = string.Empty;
        TryGetComponent<CookComponent>(out CookComponent component);
        return component!=null;
    }
}