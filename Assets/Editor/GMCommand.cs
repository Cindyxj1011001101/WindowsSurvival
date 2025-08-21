using UnityEditor;
using UnityEngine;
public class GMCommand
{
    static Card testCard;

    private static Card AddCard(string cardName)
    {
        var card = CardFactory.CreateCard(cardName);
        var window = GetFocusedBagWindow();
        if (window != null && window.Bag != null && window.Bag.CanAddCard(card, out _))
            window.Bag.AddCard(card);
        card.RefreshSlot();
        card.StartUpdating();

        return card;
    }

    private static BagWindow GetFocusedBagWindow()
    {
        var window = WindowsManager.Instance.GetCurrentFocusedWindow();
        if (window == null) return null;
        return window.GetComponentInChildren<BagWindow>(false);
    }

    [MenuItem("Command/添加/格子")]
    public static void AddSlot()
    {
        var window = GetFocusedBagWindow();
        if (window != null) window.Bag.AddSlot();
    }

    [MenuItem("Command/添加/9个格子")]
    public static void AddNineSlots()
    {
        var window = GetFocusedBagWindow();
        if (window != null)
        {
            for (int i = 0; i < 9; i++)
            {
                window.Bag.AddSlot();
            }
        }
    }

    [MenuItem("Command/添加/压缩饼干")]
    public static void A()
    {
        AddCard("压缩饼干");
    }
    [MenuItem("Command/添加/珊瑚礁")]
    public static void Coral()
    {
        AddCard("珊瑚礁");
    }
    [MenuItem("Command/添加/电动排水机")]
    public static void AddMachine()
    {
        AddCard("电动排水机");
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

    [MenuItem("Command/添加/通往织光藻墓园")]
    public static void D()
    {
        AddCard("从珊瑚礁海域到织光藻墓园");
    }

    [MenuItem("Command/添加/通往飞船外壳")]
    public static void E()
    {
        AddCard("从珊瑚礁海域到飞船外壳");
    }

    [MenuItem("Command/添加/野炊营火")]
    public static void AddPicnicCampfire()
    {
        AddCard("野炊营火");
    }

    [MenuItem("Command/添加/韧性胶管")]
    public static void G()
    {
        AddCard("韧性胶管");
    }

    [MenuItem("Command/添加/20新鲜度的老鼠尸体")]
    public static void H()
    {
        var card = AddCard("老鼠尸体");
        card.TryGetComponent<FreshnessComponent>(out var c);
        c.freshness = 20;
        card.RefreshSlot();
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
    [MenuItem("Command/添加/氧烛")]
    public static void GetOxygenCandle()
    {
        AddCard("氧烛");
    }
    [MenuItem("Command/添加/有产物的虹吸海葵")]
    public static void V()
    {
        AddCard("有产物的虹吸海葵");
    }
    [MenuItem("Command/添加/储物箱")]
    public static void W()
    {
        AddCard("储物箱");
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

    #region 保存

    [MenuItem("Command/保存/玩家背包")]
    public static void SavePlayerBag()
    {
        GameDataManager.Instance.SavePlayerBag();
    }

    [MenuItem("Command/保存/环境背包")]
    public static void SaveEnvironmentBag()
    {
        GameDataManager.Instance.SaveEnvironmentBag();
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

    [MenuItem("Command/保存/所有数据存档")]
    public static void SaveAllData()
    {
        GameDataManager.Instance.SaveAllData();
    }


    [MenuItem("Command/保存/状态数据")]
    public static void SaveStateData()
    {
        GameDataManager.Instance.SaveStateData();
    }

    [MenuItem("Command/保存/窗口数据")]
    public static void SaveWindowsData()
    {
        GameDataManager.Instance.SaveWindowsData();
    }
    #endregion


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

    [MenuItem("Command/添加/废铁铲")]
    public static void P()
    {
        AddCard("废铁铲");
    }

    [MenuItem("Command/添加/电池")]
    public static void Q()
    {
        AddCard("电池");
    }

    [MenuItem("Command/添加/白爆矿")]
    public static void R()
    {
        AddCard("白爆矿");
    }

    [MenuItem("Command/添加/矿石释氧机")]
    public static void S()
    {
        AddCard("矿石释氧机");
    }

    [MenuItem("Command/添加/爱情贝")]
    public static void T()
    {
        AddCard("爱情贝");
    }
    [MenuItem("Command/添加/诱捕陷阱")]
    public static void AddTrapTraps()
    {
        AddCard("诱捕陷阱");
    }
    [MenuItem("Command/添加/燃料炉")]
    public static void AddFuelFurnace()
    {
        AddCard("燃料炉");
    }

    [MenuItem("Command/添加/人力发电机")]
    public static void U()
    {
        AddCard("人力发电机");
    }

    [MenuItem("Command/添加/通往动力舱")]
    public static void X()
    {
        AddCard("从驾驶室到动力舱");
    }

    [MenuItem("Command/GC")]
    public static void GC()
    {
        // 通常与GC.Collect()配合使用
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
    }
}