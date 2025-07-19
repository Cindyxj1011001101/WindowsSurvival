using System;
using System.Collections.Generic;

/// <summary>
/// 卡牌工厂，用于创建卡牌的实例
/// </summary>
public static class CardFactory
{
    private static Dictionary<string, CardConfig> configCache = null;

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
            { "硬质纤维", typeof(HardFiber) },
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
            { "白爆矿", typeof(WhiteBlastMine) }
        };
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
        card.cardId = config.CardId;
        card.cardName = config.CardName;
        card.cardDesc = config.CardDesc;
        card.cardType = config.CardType;
        card.maxStackNum = config.MaxStackCount;
        card.moveable = config.Moveable;
        card.weight = config.Weight;
        card.tags = config.Tags;

        // 配置可变属性
        card.components = new();
        if (config.HasFreshness)
        {
            card.components.Add(typeof(FreshnessComponent), new FreshnessComponent(config.MaxFreshness));
        }
        if (config.HasDurability)
        {
            card.components.Add(typeof(DurabilityComponent), new DurabilityComponent(config.MaxDurability));
        }
        if (config.HasGrowth)
        {
            card.components.Add(typeof(GrowthComponent), new GrowthComponent(config.MaxGrowth));
        }
        if (config.HasProgress)
        {
            card.components.Add(typeof(ProgressComponent), new ProgressComponent(config.MaxProgress));
        }
        if (config.IsEquipment)
        {
            card.components.Add(typeof(EquipmentComponent), new EquipmentComponent(config.EquipmentType));
        }
        if (config.IsTool)
        {
            card.components.Add(typeof(ToolComponent), new ToolComponent(config.ToolTypes));
        }

        return card;
    }
}
