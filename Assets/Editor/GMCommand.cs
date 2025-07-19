using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class GMCommand
{
    private static void AddCard(string cardName)
    {
        var card = CardFactory.CreateCard(cardName);
        card.StartUpdating();
        var bag = GetFocusedBag();
        if (bag != null && bag.CanAddCard(card)) bag.AddCard(card);
    }

    private static BagBase GetFocusedBag()
    {
        var window = WindowsManager.Instance.GetCurrentFocusedWindow();
        if (window == null) return null;
        return window.GetComponentInChildren<BagBase>(false);
    }

    [MenuItem("Command/添加一个格子")]
    public static void AddSlot()
    {
        var bag = GetFocusedBag();
        if (bag != null) bag.AddSlot();
    }

    [MenuItem("Command/添加压缩饼干")]
    public static void A()
    {
        AddCard("压缩饼干");
    }

    [MenuItem("Command/添加废金属")]
    public static void B()
    {
        AddCard("废金属");
    }

    [MenuItem("Command/添加瓶装水")]
    public static void C()
    {
        AddCard("瓶装水");
    }

    [MenuItem("Command/添加通往驾驶室的门")]
    public static void D()
    {
        AddCard("通往驾驶室的门");
    }

    [MenuItem("Command/添加通往动力舱的门")]
    public static void E()
    {
        AddCard("通往动力舱的门");
    }

    [MenuItem("Command/添加安全泡沫覆盖的废料堆")]
    public static void F()
    {
        AddCard("废料堆");
    }

    [MenuItem("Command/添加硬质纤维")]
    public static void G()
    {
        AddCard("硬质纤维");
    }

    [MenuItem("Command/添加老鼠尸体")]
    public static void H()
    {
        AddCard("老鼠尸体");
    }

    [MenuItem("Command/添加小块生肉")]
    public static void I()
    {
        AddCard("小块生肉");
    }

    [MenuItem("Command/添加废铁刀")]
    public static void J()
    {
        AddCard("废铁刀");
    }

    [MenuItem("Command/添加腐烂物")]
    public static void K()
    {
        AddCard("腐烂物");
    }

    [MenuItem("Command/保存玩家背包")]
    public static void SavePlayerBag()
    {
        GameDataManager.Instance.SavePlayerBagRuntimeData();
    }

    [MenuItem("Command/保存环境背包")]
    public static void SaveEnvironmentBag()
    {
        GameDataManager.Instance.SaveEnvironmentBagRuntimeData();
    }

    [MenuItem("Command/保存当前地点")]
    public static void SaveLastPlace()
    {
        GameDataManager.Instance.SaveLastPlace();
    }

    [MenuItem("Command/保存当前音频数据")]
    public static void SaveAudioData()
    {
        GameDataManager.Instance.SaveAudioData();
    }

    [MenuItem("Command/保存已解锁的配方")]
    public static void SaveUnlockedRecipes()
    {
        GameDataManager.Instance.SaveUnlockedRecipes();
    }

    [MenuItem("Command/保存科技进度")]
    public static void SaveTechnologyData()
    {
        GameDataManager.Instance.SaveTechnologyData();
    }


    [MenuItem("Command/保存装备数据")]
    public static void SaveEquipment()
    {
        GameDataManager.Instance.SaveEquipmentData();
    }

    [MenuItem("Command/时间+50min")]
    public static void L()
    {
        TimeManager.Instance.AddTime(50);
    }
    [MenuItem("Command/添加装备")]
    public static void M()
    {
        AddCard("氧气罐");
        AddCard("氧气面罩");
    }

    [MenuItem("Command/读取Excel")]
    public static void O()
    {
        var configs = ExcelReader.ReadCardConfig("CardConfig");
        var config = configs["水瓶鱼"];
        Card a = new AquariusFish()
        {
            cardName = config.CardName,
            cardDesc = config.CardDesc,
            cardType = config.CardType,
            maxStackNum = config.MaxStackCount,
            moveable = config.Moveable,
            weight = config.Weight,
            tags = config.Tags,
        };
        if (config.HasFreshness)
        {
            a.components.Add(typeof(FreshnessComponent), new FreshnessComponent(config.MaxFreshness));
        }
        if (config.HasDurability)
        {
            a.components.Add(typeof(DurabilityComponent), new DurabilityComponent(config.MaxDurability));
        }
        if (config.HasGrowth)
        {
            a.components.Add(typeof(GrowthComponent), new GrowthComponent(config.MaxGrowth));
        }
        if (config.HasProgress)
        {
            a.components.Add(typeof(ProgressComponent), new ProgressComponent(config.MaxProgress));
        }
        if (config.IsEquipment)
        {
            a.components.Add(typeof(EquipmentComponent), new EquipmentComponent(config.EquipmentType));
        }
        if (config.IsTool)
        {
            a.components.Add(typeof(ToolComponent), new ToolComponent(config.ToolTypes));
        }

        var jsonStr = JsonConvert.SerializeObject(a, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All
        });
        Debug.Log(jsonStr);
    }
}