public class KettleFlower : Card
{
    public bool hasWound = false;
    public int woundCount = 0;
    public int woundMaxCount = 10;
    private KettleFlower()
    {
        Events = new()
        {
            new Event("划一个口", "", Event_Hurt, Judge_Hurt),
            new Event("铲起", "", Event_DigUp, Judge_DigUp),
            new Event("饮用汁液", "", Event_Drink, Judge_Drink),
        };
    }
    private void Event_Hurt(out string tip)
    {
        tip = string.Empty;
    }
    private bool Judge_Hurt(out string hint)
    {
        hint = string.Empty;
        return false;
    }
    private void Event_DigUp(out string tip)
    {
        tip = string.Empty;
    }
    private bool Judge_DigUp(out string hint)
    {
        hint = string.Empty;
        return false;
    }
    private void Event_Drink(out string tip)
    {
        tip = string.Empty;
    }
    private bool Judge_Drink(out string hint)
    {
        hint = string.Empty;
        return false;
    }
}