using UnityEngine;
public class SafeInsurance : Card
{
    private SafeInsurance()
    {
        Events = new()
        {
            new Event("用手砸", "用手砸", Event_UseHand, Judge_UseHand),
            new Event("用铲子凿", "用铲子凿", Event_UseShovel, Judge_UseShovel),
            new Event("用锤子砸", "用锤子砸", Event_UseHammer, Judge_UseHammer)
        };

        AddComponent(new ConstructionComponent()
        {
        });
    }
    public void Event_UseHand(out string tip)
    {
        tip = string.Empty;
        Use(3);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -5);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, 15);
    }

    public bool Judge_UseHand(out string hint)
    {
        hint = string.Empty;
        return true;
    }
    public void Event_UseShovel(out string tip)
    {
        tip = string.Empty;
        Use(8);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -4);
        GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig).Use();
        TimeManager.Instance.AddTime(15);
    }

    public bool Judge_UseShovel(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) != null)
        {
            return true;
        }
        return false;
    }
    public void Event_UseHammer(out string tip)
    {
        tip = string.Empty;
        Use(20);
        GameManager.Instance.PlayerBag.FindCardOfName("钢锤").Use();
        TimeManager.Instance.AddTime(15);
    }

    public bool Judge_UseHammer(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("钢锤")!=null)
        {
            return true;
        }
        return false;
    }
}