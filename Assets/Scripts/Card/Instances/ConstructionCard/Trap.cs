using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Trap : Card
{
    public bool isWorking; // 是否已打开
    public string OutcomeCardID;
    private Trap()
    {
        isWorking = false;
        Events = new()
        {
            new Event("布置", "布置", Event_Arrange, Judge_Arrange),
            new Event("收获", "收获", Event_TakeOut, Judge_TakeOut),
        };

        // 仅在室内、非水域地点建造
        AddComponent(new ConstructionComponent()
        {
            onlyInDoor = true,
            onlyOutWater = true,
            needCable = true,
        });
    }
    

    public void Event_TakeOut(out string tip)
    {
        tip = string.Empty;
        if (OutcomeCardID != null)
        {
            AddCard(OutcomeCardID,true);
            OutcomeCardID = null;
            Use();
        }
    }
    public bool Judge_TakeOut(out string hint)
    {
        hint = string.Empty;
        return OutcomeCardID != null;
    }
    public void Event_Arrange(out string tip)
    {
        tip = string.Empty;
        TimeManager.Instance.AddTime(15);
        isWorking = true;

    }

    public bool Judge_Arrange(out string hint)
    {
        hint = string.Empty;
        return !isWorking;
    }

    protected override System.Action OnUpdate => () =>
    {
        int Probability = 48;
        TryGetComponent<InnerContentsComponent>(out InnerContentsComponent component);
        if(component.bag.EmptySlotCount==0)
        {
            Probability = 3;
        }
        if (Random.Range(0, Probability) == 0)
        {
            List<Card> dropCards= GameManager.Instance.CurEnvironmentBag.RepeatableDropList.RandomDropTrappable();
            if (dropCards != null)
            {
                isWorking = false;
                Use();
                foreach (var slot in component.bag.Slots)
                {
                    foreach (var card in slot.Cards)
                    {
                        card.DestroyThis();
                    }
                }
            }
            foreach (var card in dropCards)
            {
                if (card.CardId == "有产物的水瓶鱼")
                {
                    OutcomeCardID="有产物的被捉住的水瓶鱼";
                    //WAIT:可能需要处理生长度等的继承
                }
                else
                {
                    OutcomeCardID=card.CardId;
                }
            }

        }
    };
}