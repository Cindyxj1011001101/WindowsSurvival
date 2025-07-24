using System;
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

    [MenuItem("Command/添加/格子")]
    public static void AddSlot()
    {
        var bag = GetFocusedBag();
        if (bag != null) bag.AddSlot();
    }

    [MenuItem("Command/添加/压缩饼干")]
    public static void A()
    {
        AddCard("压缩饼干");
    }


    [MenuItem("Command/添加/废金属")]
    public static void B()
    {
        AddCard("废金属");
    }

    [MenuItem("Command/添加/瓶装水")]
    public static void C()
    {
        AddCard("瓶装水");
    }

    [MenuItem("Command/添加/通往驾驶室的门")]
    public static void D()
    {
        AddCard("通往驾驶室的门");
    }

    [MenuItem("Command/添加/通往动力舱的门")]
    public static void E()
    {
        AddCard("通往动力舱的门");
    }

    [MenuItem("Command/添加/安全泡沫覆盖的废料堆")]
    public static void F()
    {
        AddCard("废料堆");
    }

    [MenuItem("Command/添加/硬质纤维")]
    public static void G()
    {
        AddCard("硬质纤维");
    }

    [MenuItem("Command/添加/老鼠尸体")]
    public static void H()
    {
        AddCard("老鼠尸体");
    }

    [MenuItem("Command/添加/小块生肉")]
    public static void I()
    {
        AddCard("小块生肉");
    }

    [MenuItem("Command/添加/废铁刀")]
    public static void J()
    {
        AddCard("废铁刀");
    }

    [MenuItem("Command/添加/腐烂物")]
    public static void K()
    {
        AddCard("腐烂物");
    }
    [MenuItem("Command/播放/心跳_01")]
    public static void PlayHeart()
    {
        SoundManager.Instance.PlayBGM("心跳_01", true, 1f);
    }
    [MenuItem("Command/播放/心跳_01高音量")]
    public static void PlayHeart2()
    {
        SoundManager.Instance.PlayBGM("心跳_01", true, 1f,2f);
    }
    [MenuItem("Command/播放/飞船内_01")]
    public static void PlatPlane()
    {
        SoundManager.Instance.PlayBGM("飞船内_01", true, 1f);
    }
    

    [MenuItem("Command/保存/玩家背包")]
    public static void SavePlayerBag()
    {
        GameDataManager.Instance.SavePlayerBagRuntimeData();
    }

    [MenuItem("Command/保存/环境背包")]
    public static void SaveEnvironmentBag()
    {
        GameDataManager.Instance.SaveEnvironmentBagRuntimeData();
    }

    [MenuItem("Command/保存/当前地点")]
    public static void SaveLastPlace()
    {
        GameDataManager.Instance.SaveLastPlace();
    }

    [MenuItem("Command/保存/音频数据")]
    public static void SaveAudioData()
    {
        GameDataManager.Instance.SaveAudioData();
    }

    [MenuItem("Command/保存/已解锁的配方")]
    public static void SaveUnlockedRecipes()
    {
        GameDataManager.Instance.SaveUnlockedRecipes();
    }

    [MenuItem("Command/保存/科技进度")]
    public static void SaveTechnologyData()
    {
        GameDataManager.Instance.SaveTechnologyData();
    }


    [MenuItem("Command/保存/装备数据")]
    public static void SaveEquipment()
    {
        GameDataManager.Instance.SaveEquipmentData();
    }

    [MenuItem("Command/时间+50min")]
    public static void L()
    {
        TimeManager.Instance.AddTime(50);
    }

    [MenuItem("Command/添加/氧气罐")]
    public static void M()
    {
        AddCard("氧气罐");
    }

    [MenuItem("Command/添加/氧气面罩")]
    public static void N()
    {
        AddCard("氧气面罩");
    }

    [MenuItem("Command/添加/渗水裂缝")]
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
}