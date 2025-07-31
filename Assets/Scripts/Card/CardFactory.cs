using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卡牌工厂，用于创建卡牌的实例
/// </summary>
public static class CardFactory
{
    // 键是卡牌ID，值是卡牌配置
    private static Dictionary<string, CardConfig> configCache = null;
    // 键是卡牌ID，值是对应的卡牌类类型
    private static Dictionary<string, Type> classTypes = null;

    private static void InitCardConfig()
    {
        configCache ??= ExcelReader.ReadCardConfig("CardConfig");
        classTypes ??= new()
        {
            { "气密舱门", typeof(AirtightDoor) },
            { "水瓶鱼", typeof(AquariusFish) },
            { "有产物的水瓶鱼", typeof(AquariusFishWithProduct) },
            { "电池", typeof(Battery) },
            { "瓶装水", typeof(BottledWater) },
            { "被捉住的水瓶鱼", typeof(CaughtAquariusFish) },
            { "有产物的被捉住的水瓶鱼", typeof(CaughtAquariusFishWithProduct) },
            { "压缩饼干", typeof(CompactBiscuit) },
            { "珊瑚", typeof(Coral) },
            { "通往驾驶室的门", typeof(DoorToCockpit) },
            { "通往维生舱的门", typeof(DoorToLifeSupportCabin) },
            { "通往动力舱的门", typeof(DoorToPowerCabin) },
            { "捞网", typeof(FishingNet) },
            { "玻璃", typeof(Glass) },
            { "玻璃沙", typeof(GlassSand) },
            { "韧性胶管", typeof(ResilientRubberHose) },
            { "人力发电机", typeof(HumanPoweredGenerator) },
            { "点燃的氧烛", typeof(LightenedOxygenCandle) },
            { "小块生肉", typeof(LittleRawMeat) },
            { "爱情贝", typeof(LoveBead) },
            { "有产物的爱情贝", typeof(LoveBeadWithProduct) },
            { "磁性触手", typeof(MagneticTentacle) },
            { "矿石释氧机", typeof(OreReleaseOxygenMachine) },
            { "氧气罐", typeof(OxygenCan) },
            { "氧烛", typeof(OxygenCandle) },
            { "氧气面罩", typeof(OxygenMask) },
            { "裂缝填充物", typeof(Patch) },
            { "老鼠尸体", typeof(RatBody) },
            { "生贝肉", typeof(RawOysterMeat) },
            { "腐烂物", typeof(RotMaterial) },
            { "废铁刀", typeof(ScrapIronKnife) },
            { "废金属", typeof(ScrapMetal) },
            { "虹吸海葵", typeof(Siphonophyllum) },
            { "有产物的虹吸海葵", typeof(SiphonophyllumWithProduct) },
            { "废料堆", typeof(WasteHeap) },
            { "渗水裂缝", typeof(WaterCrack) },
            { "白爆矿", typeof(WhiteBlastMine) },
            { "海麻线", typeof(SeaGrass) },
            { "海爬虫", typeof(SeaLizard) },
            { "熟海爬虫", typeof(CookedSeaLizard) },
            { "石砖", typeof(StoneBrick) },
            { "电动排水机", typeof(ElectricDrainageMachine) },
            { "废铁铲", typeof(WasteShovel) },
            { "储物箱", typeof(StorageBox) },
            { "食物残渣", typeof(FoodScrap) },
            { "海麻线丛", typeof(SeaGrassBed) },
            { "纤维", typeof(Fiber) },
            { "珊瑚礁", typeof(CoralReef) },
            { "燃素", typeof(Phlogiston) },
            { "铁齿铜牙餐盘", typeof(IronMeal) },
            { "黑金炭烤肉", typeof(CoalGrilledMeat) },
            { "蛤蜊浓汤", typeof(ClamSoup) },
            { "肉排", typeof(Steak) },
            { "炸虫串", typeof(FriedInsectStick) },
            { "白灼触手", typeof(ScaldedClaw) },
            { "厨房恶物", typeof(KitchenFoes) },
        };
    }

    public static Sprite GetCardImage(string cardId)
    {
        InitCardConfig();
        if (configCache.TryGetValue(cardId, out var config))
        {
            // 获取图集的所有图片
            var sprites = Resources.LoadAll<Sprite>("Sprites/" + config.CardType.ToString());
            // 找到图片的索引
            var index = int.Parse(config.CardImagePath);
            return sprites[index];
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static bool GetIsBigIcon(string cardId)
    {
        InitCardConfig();
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.IsBigIcon;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static string GetCardName(string cardId)
    {
        InitCardConfig();
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.CardName;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static string GetCardDesc(string cardId)
    {
        InitCardConfig();
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.CardDesc;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static CardType GetCardType(string cardId)
    {
        InitCardConfig();
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.CardType;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static int GetMaxStackNum(string cardId)
    {
        InitCardConfig();
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.MaxStackNum;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static bool GetMoveable(string cardId)
    {
        InitCardConfig();
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.Moveable;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static float GetWeight(string cardId)
    {
        InitCardConfig();
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.Weight;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static List<CardTag> GetTags(string cardId)
    {
        InitCardConfig();
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.Tags;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static string GetExtraInfo(string cardId)
    {
        InitCardConfig();
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.CardExtraInfo;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static Card CreateCard(string cardId)
    {
        // 读取卡牌配置
        InitCardConfig();

        // 从缓存中获取配置
        var config = configCache[cardId];
        var classType = classTypes[cardId];

        // 创建卡牌实例
        Card card = Activator.CreateInstance(classType, true) as Card;

        // 配置基础属性
        card.SetCardId(cardId);
        //card.CardId = config.CardId;
        //card.CardName = config.CardName;
        //card.CardDesc = config.CardDesc;
        //card.CardType = config.CardType;
        //card.MaxStackNum = config.MaxStackCount;
        //card.Moveable = config.Moveable;
        //card.Weight = config.Weight;
        //card.Tags = config.Tags;

        // 配置可变属性
        //card.components = new();
        if (config.HasFreshness)
        {
            card.AddComponent(typeof(FreshnessComponent), new FreshnessComponent(config.MaxFreshness));
        }
        if (config.HasDurability)
        {
            card.AddComponent(typeof(DurabilityComponent), new DurabilityComponent(config.MaxDurability));
        }
        if (config.HasGrowth)
        {
            card.AddComponent(typeof(GrowthComponent), new GrowthComponent(config.MaxGrowth));
        }
        if (config.HasProgress)
        {
            card.AddComponent(typeof(ProgressComponent), new ProgressComponent(config.MaxProgress));
        }
        if (config.IsEquipment)
        {
            card.AddComponent(typeof(EquipmentComponent), new EquipmentComponent(config.EquipmentType));
        }
        if (config.IsTool)
        {
            card.AddComponent(typeof(ToolComponent), new ToolComponent(config.ToolTypes));
        }
        if (config.HasInnerContents)
        {
            card.AddComponent(typeof(InnerContentsComponent), new InnerContentsComponent(config.InnerContentSlotCount, cardId));
        }

        return card;
    }

    // 环境一次性掉落列表
    private static Dictionary<PlaceEnum, DisposableDropList> disposableDropListDict = null;

    private static void InitDisposableDropList()
    {
        disposableDropListDict ??= ExcelReader.GenerateDisposableDropList();
    }

    public static DisposableDropList GetDisposableDropList(PlaceEnum place)
    {
        InitDisposableDropList();
        if (disposableDropListDict.ContainsKey(place))
            return disposableDropListDict[place];
        return new DisposableDropList();
    }

    // 环境重复掉落列表
    private static Dictionary<PlaceEnum, RepeatableDropList> repeatableDropListDict = null;

    private static void InitRepeatableDropList()
    {
        repeatableDropListDict ??= ExcelReader.GenerateRepeatableDropList();
    }

    public static RepeatableDropList GetRepeatableDropList(PlaceEnum place)
    {
        InitRepeatableDropList();
        if (repeatableDropListDict.ContainsKey(place))
            return repeatableDropListDict[place];
        return new RepeatableDropList();
    }
}
