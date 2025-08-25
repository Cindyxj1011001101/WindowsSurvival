using UnityEditor.PackageManager;
using UnityEngine.Events;

public class KettleFlower : PlantCard
{
    private bool HasWound = false;
    private int WoundCount = 0;
    private int WoundMaxCount = 10;
    public KettleFlower()
    {
        Events = new()
        {
            new Event("划一个口", "划一个口",Event_Hurt,Judge_Hurt),
            new Event("铲起", "铲起",Event_DigUp,Judge_DigUp),
            new Event("饮用汁液", "饮用汁液",Event_Drink,Judge_Drink),
        };
    }
    public void Event_Hurt(out string tip)
    { 
        tip = string.Empty;
    }
    public bool Judge_Hurt(out string hint)
    {
        hint = string.Empty;
        return false;
    }
    public void Event_DigUp(out string tip)
    {
        tip = string.Empty;
    }
    public bool Judge_DigUp(out string hint)
    {
        hint = string.Empty;
        return false;
    }
    public void Event_Drink(out string tip)
    {
        tip = string.Empty;
    }
    public bool Judge_Drink(out string hint)
    {
        hint = string.Empty;
        return false;
    }
}