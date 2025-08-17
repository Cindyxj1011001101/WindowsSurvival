
public class SelfHeatingCookingBag : Card
{
    private SelfHeatingCookingBag()
    {
        Events = new()
        {
            new Event("烹饪", "烹饪",Event_Cook, Judge_Cook)
        };
    }
    public void Event_Cook(out string tip)
    {
        tip = string.Empty;
        //消耗1点耐久度
        Use();
        //让食物变为煮熟版本
        TimeManager.Instance.AddTime(15);
    }

    private bool Judge_Cook(out string hint)
    {
        hint = string.Empty;
        //需要判断卡牌是否有烹饪组件
        return true;
    }
}