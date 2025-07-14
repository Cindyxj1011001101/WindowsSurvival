using UnityEditor;
using UnityEngine;

public class GMCommand
{
    [MenuItem("Command/初始化游戏数据")]
    public static void Init()
    {
        GameManager.Instance.Init();
    }

    private static void AddCard(string cardName)
    {
        string dataPath = "ScriptableObject/Card/" + cardName;
        CardData defaultData = Resources.Load<CardData>(dataPath);
        var card = CardFactory.CreateCardIntance(defaultData);

        var bag = GetFocusedBag();
        if (bag != null) bag.AddCard(card);
    }

    [MenuItem("Command/添加一个格子")]
    public static void AddSlot()
    {
        var bag = GetFocusedBag();
        if (bag != null) bag.AddSlot();
    }

    private static BagBase GetFocusedBag()
    {
        var window = WindowsManager.Instance.GetCurrentFocusedWindow();
        if (window == null) return null;
        BagBase bag = window.GetComponentInChildren<BagBase>(false);
        if (bag == null) return null;
        return bag;
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

    [MenuItem("Command/添加被安全泡沫覆盖的废料堆")]
    public static void F()
    {
        AddCard("被安全泡沫覆盖的废料堆");
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

    [MenuItem("Command/添加小块肉")]
    public static void I()
    {
        AddCard("小块肉");
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
}
