using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Time = OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime.Time;

public class SleepInstrument : Card
{
    public bool isWorking; // 是否已打开
    private SleepInstrument()
    {
        isWorking = false;
        Events = new()
        {
            new Event("接电", "接电", Event_ConnectElectricity, Judge_ConnectElectricity),
            new Event("断电", "断电", Event_DisconnectElectricity, Judge_DisconnectElectricity),
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
    
    public void Event_ConnectElectricity(out string tip)
    {
        tip = string.Empty;
        isWorking = true;
        
    }
    public bool Judge_ConnectElectricity(out string hint)
    {
        hint = string.Empty;
        if (StateManager.Instance.Electricity.CurValue != 0 && isWorking == false)
        {
            return true;
        }

        return false;
    }
    public void Event_DisconnectElectricity(out string tip)
    {
        tip = string.Empty;
        isWorking = false;
    }
    public bool Judge_DisconnectElectricity(out string hint)
    {
        hint = string.Empty;
        return isWorking;
    }
    public void Event_CompleteTearDown(out string tip)
    {
        tip = string.Empty;
        isWorking=false;
        GameManager.Instance.PlayerBag.FindCardOfName("精密扳手").Use();
        DestroyThis();
        AddCard("建筑工程包(睡眠脉冲仪)",true);
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
        isWorking=false;
        GameManager.Instance.PlayerBag.FindCardOfName("钢锤").Use();
        DestroyThis();
        AddCards("韧性胶管", 2,true);
        AddCards("玻璃沙",3, true);
        AddCards("废金属", 2,true);
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

    //YONG-TODO:实现睡眠开始与结束的数据处理
}