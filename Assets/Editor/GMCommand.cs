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

    [MenuItem("Command/添加一个渗水裂缝")]
    public static void O()
    {
        AddCard("渗水裂缝");
    }

    [MenuItem("Command/生成掉落列表的Json文件")]
    public static void P()
    {
        ExcelReader.GenerateDisposableDropListJson("DisposableDropListConfig");
        ExcelReader.GenerateRepeatableDropListJson("RepeatableDropListConfig");
    }

    [MenuItem("Command/掉落渗水裂缝")]
    public static void Q()
    {
        AddCard("渗水裂缝");
    }

    [MenuItem("Command/测试读取卡牌配置")]
    public static void R()
    {
        foreach (var config in ExcelReader.ReadCardConfig("CardConfig"))
        {
            Debug.Log(config);
        }
    }
}