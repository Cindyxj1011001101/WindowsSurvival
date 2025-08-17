using UnityEngine;
public class OpenedInsurance : Card
{
    private OpenedInsurance()
    {
        Events = new()
        {
            new Event("完整拆卸", "完整拆卸", Event_CompleteTearDown, Judge_CompleteTearDown),
            new Event("暴力拆毁", "暴力拆毁", Event_ViolentTearDown, Judge_ViolentTearDown),
        };

        AddComponent(new ConstructionComponent()
        {
        });
    }
    public void Event_CompleteTearDown(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfName("精密扳手").Use();
        DestroyThis();
        AddCard("建筑工程包(被撬开的保险柜)",true);
        TimeManager.Instance.AddTime(45);
    }

    public bool Judge_CompleteTearDown(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("精密扳手")!=null)
        {
            return true;
        }
        return false;
    }
    public void Event_ViolentTearDown(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfName("钢锤").Use();
        DestroyThis();
        AddCard("钢材", true);
        TimeManager.Instance.AddTime(15);
        
    }

    public bool Judge_ViolentTearDown(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("钢锤")!=null)
        {
            return true;
        }
        return false;
    }
}