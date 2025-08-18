using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 电动排水机
/// </summary>
public class FuelFurnace : Card
{
    public bool isLightened; // 是否已打开
    public float Furl;//剩余燃料数
    public List<Card> CardsToProcesss;
    public int curRounds;
    public int MaxRound = 16;
    public bool IsProcessing;
    public float CurTempture;
    public List<TempertureData> TempertureDatas= new List<TempertureData>();
    public string OutComeCardID;
    
    //TODO:将拥有BurnableComponent卡牌拖拽到本卡牌上，增加燃料
    
    private FuelFurnace()
    {
        Events = new()
        {
            new Event("点燃", "打开", Event_Lighting, Judge_Lighting),
            new Event( "加工" , "加工", Event_Process, Judge_Process),
            new Event( "熄灭" , "熄灭", Event_Unlightened, Judge_Unlightened),
            new Event( "取出" , "取出", Event_TakeOut, Judge_TakeOut),
        };

        // 仅在室内、非水域地点建造
        AddComponent(new ConstructionComponent()
        {
            onlyInDoor = true,
            onlyOutWater = true,
            needCable = true,
        });
    }

    protected override void LateInit()
    {
        base.LateInit();
        //YONG-TODO：对过滤器做初始化，限制放入物体的可加工属性
    }

    public void Event_Lighting(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfName("燃料点火器").Use();
        isLightened = true;
    }
    public bool Judge_Lighting(out string hint)
    {
        hint = string.Empty;
        if(Furl!=0&&StateManager.Instance.WaterLevel.CurValue<=30&&
           GameManager.Instance.PlayerBag.FindCardOfName("燃料点火器")!=null&&
           isLightened==false)
        {
            return true;
        }
        return false;
    }
    public void Event_TakeOut(out string tip)
    {
        tip = string.Empty;
        AddCard(OutComeCardID, true);
        OutComeCardID = null;
    }
    public bool Judge_TakeOut(out string hint)
    {
        hint = string.Empty;
        return OutComeCardID != null;
    }

    
    public void Event_Process(out string tip)
    { 
        tip = string.Empty;
        //TODO:限制InnerBag可放入拖出卡牌
        if (TryGetComponent(out InnerContentsComponent component))
        {
            foreach (var slot in component.bag.Slots)
            {
                foreach (var card in slot.Cards)
                {
                    CardsToProcesss.Add(card);
                }
            }
        }
        IsProcessing = true;
        curRounds = MaxRound;
        TempertureDatas= new List<TempertureData>();
        TempertureDatas.Add(new TempertureData(TempertureType.Normal, 0));
        TempertureDatas.Add(new TempertureData(TempertureType.Low, 0));
        TempertureDatas.Add(new TempertureData(TempertureType.Medium, 0));
        TempertureDatas.Add(new TempertureData(TempertureType.High, 0));
    }

    public bool Judge_Process(out string hint)
    {
        hint = string.Empty;
        if (IsProcessing) return false;
        if (OutComeCardID != null) return false;
        if (TryGetComponent(out InnerContentsComponent component))
        {
            if (component.bag.IsBagFull) return true;
        }
        return false;
    }
    public void Event_Unlightened(out string tip)
    {
        tip = string.Empty;
        isLightened = false;
    }
    public bool Judge_Unlightened(out string hint)
    {
        hint = string.Empty;
        if (isLightened)return true;
        return false;
    }
    protected override System.Action OnUpdate => () =>
    {
        if (isLightened)
        {
            Furl -= 1;
            if (StateManager.Instance.WaterLevel.CurValue > 0)
            {
                Furl -= 4;
            }
            CurTempture += 17;
            if (StateManager.Instance.WaterLevel.CurValue >= 30)
            {
                isLightened = false;
            }
            Furl=Mathf.Clamp(Furl,0,96);
            if (Furl <= 0)
            {
                isLightened = false;
            }
        }
        else
        {
            CurTempture -= 4;
            if (StateManager.Instance.WaterLevel.CurValue > 0)
            {
                CurTempture -= 4;
            }
            if (StateManager.Instance.WaterLevel.CurValue >= 30)
            {
                CurTempture -= 8;
            }
            CurTempture = Mathf.Clamp(CurTempture,0, 300);
        }

        
        if (curRounds != 0)
        {
            if (CurTempture <= 50)
            {
                TempertureDatas[0].Round++;
            }
            else if (CurTempture <= 100)
            {
                TempertureDatas[1].Round++;
            }
            else if (CurTempture <= 200)
            {
                TempertureDatas[2].Round++;
            }
            else
            {
                TempertureDatas[3].Round++;
            }
            curRounds--;
        }
        else
        {
            string outcomeID = ProcessManager.GetProcessOutcomeID(CardsToProcesss, TempertureDatas);
            IsProcessing = false;
            //TODO:恢复InnerBag可放入拖出卡牌
            OutComeCardID= outcomeID;
        }
    };
    
}