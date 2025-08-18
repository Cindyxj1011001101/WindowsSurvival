using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class GarbageDestroyer : Card
{
    public bool isWorking; // 是否已打开
    private GarbageDestroyer()
    {
        isWorking = false;
        Events = new()
        {
            new Event("销毁垃圾", "销毁垃圾", Event_Destroy, Judge_Destroy),
            new Event("完整拆卸", "完整拆卸", Event_CompleteTearDown, Judge_CompleteTearDown),
            new Event("暴力拆毁", "暴力拆毁", Event_ViolentTearDown, Judge_ViolentTearDown),
        };

        // 仅在室内、非水域地点建造
        AddComponent(new ConstructionComponent()
        {
            onlyInDoor = true,
            onlyOutWater = true,
            needCable = true,
        });
    }
    
    public void Event_Destroy(out string tip)
    {
        tip = string.Empty;
        TryGetComponent<InnerContentsComponent>(out InnerContentsComponent component);
        foreach (var slot in component.bag.Slots)
        {
            foreach (var card in slot.Cards)
            {
                card.DestroyThis();
            }
        }
    }

    public bool Judge_Destroy(out string hint)
    {
        hint = string.Empty;
        return true;
    }
    
    public void Event_CompleteTearDown(out string tip)
    {
        tip = string.Empty;
        isWorking=false;
        GameManager.Instance.PlayerBag.FindCardOfName("精密扳手").Use();
        DestroyThis();
        AddCard("建筑工程包(垃圾分解器)",true);
        TimeManager.Instance.AddTime(15);
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
        isWorking=false;
        GameManager.Instance.PlayerBag.FindCardOfName("钢锤").Use();
        DestroyThis();
        AddCard("废金属", true);
        AddCard("腐烂物", true);
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
    protected override System.Action OnUpdate => () =>
    {
    };
}