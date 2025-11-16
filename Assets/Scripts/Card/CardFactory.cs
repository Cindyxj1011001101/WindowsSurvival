using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 卡牌工厂，用于创建卡牌的实例
/// </summary>
public static class CardFactory
{
    // 键是卡牌ID，值是卡牌配置
    private static Dictionary<string, CardConfig> configCache = null;
    // 键是卡牌ID，值是对应的卡牌类类型
    private static Dictionary<string, Type> classTypes = new()
    {
        { "从动力舱到驾驶室", typeof(FromPowerCabinToCockpit) },
        { "从驾驶室到动力舱", typeof(FromCockpitToPowerCabin) },
        { "从驾驶室到维生舱", typeof(FromCockpitToLifeSupportCabin) },
        { "从维生舱到驾驶室", typeof(FromLifeSupportCabinToCockpit) },
        { "从驾驶室到珊瑚礁海域", typeof(FromCockpitToCoralCoast) },
        { "从珊瑚礁海域到驾驶室", typeof(FromCoralCoastToCockpit) },
        { "从珊瑚礁海域到织光藻墓园", typeof(FromCoralCoastToPhosphorTomb) },
        { "从织光藻墓园到珊瑚礁海域", typeof(FromPhosphorTombToCoralCoast) },
        { "从珊瑚礁海域到飞船外壳", typeof(FromCoralCoastToSpaceshipOuterHull) },
        { "从飞船外壳到珊瑚礁海域", typeof(FromSpaceshipOuterHullToCoralCoast) },
        { "水瓶鱼", typeof(AquariusFish) },
        { "有产物的水瓶鱼", typeof(AquariusFishWithProduct) },
        { "电池", typeof(Battery) },
        { "瓶装水", typeof(BottledWater) },
        { "被捉住的水瓶鱼", typeof(CaughtAquariusFish) },
        { "被捉住的有产物的水瓶鱼", typeof(CaughtAquariusFishWithProduct) },
        { "压缩饼干", typeof(CompactBiscuit) },
        { "珊瑚", typeof(Coral) },
        { "捞网", typeof(FishingNet) },
        { "玻璃", typeof(Glass) },
        { "玻璃沙", typeof(GlassSand) },
        { "韧性胶管", typeof(ResilientRubberHose) },
        { "人力发电机", typeof(HumanPoweredGenerator) },
        { "点燃的氧烛", typeof(LightenedOxygenCandle) },
        { "小块生肉", typeof(RawLittleMeat) },
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
        { "腐烂物", typeof(SpoiledFood) },
        { "废铁刀", typeof(ScrapIronKnife) },
        { "废金属", typeof(ScrapMetal) },
        { "虹吸海葵", typeof(Siphonophyllum) },
        { "有产物的虹吸海葵", typeof(SiphonophyllumWithProduct) },
        { "废料堆", typeof(WasteHeap) },
        { "渗水裂缝", typeof(WaterCrack) },
        { "白爆矿", typeof(WhiteBlastOre) },
        { "海麻线", typeof(SeaGrass) },
        { "海爬虫", typeof(SeaLizard) },
        { "熟海爬虫", typeof(CookedSeaLizard) },
        { "石砖", typeof(StoneBrick) },
        { "电动排水机", typeof(ElectricDrainageMachine) },
        { "废铁铲", typeof(ScrapShovel) },
        { "储物箱", typeof(StorageBox) },
        { "食物残渣", typeof(FoodScrap) },
        { "海麻线丛", typeof(SeaGrassBed) },
        { "纤维", typeof(Fiber) },
        { "珊瑚礁", typeof(CoralReef) },
        { "燃素", typeof(Phlogiston) },
        { "铁齿铜牙餐", typeof(IronMeal) },
        { "黑金炭烤肉", typeof(CoalGrilledMeat) },
        { "蛤蜊浓汤", typeof(ClamSoup) },
        { "肉排", typeof(Steak) },
        { "炸虫串", typeof(FriedInsectStick) },
        { "白灼触手", typeof(ScaldedClaw) },
        { "厨房恶物", typeof(KitchenFoes) },
        { "鱼汤", typeof(FishSoup) },
        { "贝类刺身", typeof(ShellSashimi) },
        { "白爆矿堆", typeof(WhiteBlastOreStack) },
        { "燃料炉", typeof(FuelFurnace) },
        { "诱捕陷阱", typeof(Trap) },
        { "手压排水泵", typeof(HandDrainPump) },
        { "变形的保险柜", typeof(SafeInsurance) },
        { "被撬开的保险柜", typeof(OpenedInsurance) },
        { "钢锤", typeof(SteelHammer) },
        { "钢铲", typeof(SteelShovel) },
        { "钢刀", typeof(SteelKnife) },
        { "睡眠脉冲仪", typeof(SleepInstrument) },
        { "精密元件", typeof(PrecisionComponent) },
        { "垃圾销毁器", typeof(GarbageDestroyer) },
        { "脚蹼", typeof(WebbedFeet) },
        { "塑料袋", typeof(PlasticBag) },
        { "渔获袋", typeof(FishingNetBag) },
        { "重型氧气罐", typeof(HeavyOxygenCan) },
        { "野炊营火", typeof(Campfire) },
        { "自热烹饪袋", typeof(SelfHeatingCookingBag) },
        { "止痛药", typeof(Painkillers) },
        { "数据传输台", typeof(DataTransmissionStation) },
        { "冰箱", typeof(Refrigerator) },
        { "凝胶装瓶器", typeof(GelBottler) },
        { "钢材", typeof(Steel) },
        { "烧焦的食物", typeof(BurntFood) },
        { "燃料蒸馏器", typeof(FuelDistiller) },
        { "小块熟肉", typeof(CookedLittleMeat) },
        { "熟贝肉", typeof(CookedOysterMeat) },
        { "熟触手", typeof(CookedTentacle) },
        { "盐水", typeof(SalineWater) },
        { "育卵液", typeof(EggRearingFluid) },
        { "水壶兰种子", typeof(KettleFlowerSeed) },
        { "熟水壶兰种", typeof(CookedKettleFlowerSeed) },
        { "水壶兰", typeof(KettleFlower) },
        { "损坏的飞船驾驶座", typeof(SpaceshipSeat) },
        { "从最后庇护所到遇难者大厅", typeof(FromLastSancutuaryToVictimsHall) },
        { "从遇难者大厅到最后庇护所", typeof(FromVictimsHallToLastSancutuary) },
        { "从织光藻墓园到浅层岩穴", typeof(FromPhosphorTombToShallowGrotto) },
        { "从浅层岩穴到织光藻墓园", typeof(FromShallowGrottoToPhosphorTomb) },
        { "从遇难者大厅到浅层岩穴", typeof(FromVictimsHallToShallowGrotto) },
        { "从浅层岩穴到遇难者大厅", typeof(FromShallowGrottoToVictimsHall) },
        { "四角菱", typeof(WaterChestnut) },
        { "菱果", typeof(WaterChestnutFruit) },
        { "菱果肉", typeof(WaterChestnutPulp) },
        { "烤菱果肉", typeof(CookedWaterChestnutPulp) },
        { "板床", typeof(PlankBed) },
        { "狮子水母尸体", typeof(LionJellyfishCorpse) },
        { "未处理的海蜇皮", typeof(JellyfishSkin) },
        { "腌渍中的海蜇皮", typeof(PickledJellyfishSkin) },
        { "已处理的海蜇皮", typeof(ProcessedJellyfishSkin) },
        { "恶臭肉", typeof(FoulSmellingMeat) },
        { "熟恶臭肉", typeof(CookedFoulSmellingMeat) },
        { "废铁矛", typeof(ScrapIronSpear) },
        { "废铁棍", typeof(ScrapIronRod) },
        { "活菌丝", typeof(LiveMycelium) },
        { "谜样菇", typeof(MysteryMushroom) },
        { "松动巨石", typeof(LooseBoulders) },
        { "小型气穴", typeof(SmallAirFilledCave) },
        { "凉拌海蜇", typeof(ColdJellyfishSalad) },
        { "水果布丁", typeof(FruitPudding) },
        { "坚果酥", typeof(NutCrisp) },
        { "蠕动盛宴", typeof(CreepFeast) },
        { "食果鲀", typeof(Fruitfish) },
        { "吸盘蠕虫", typeof(SuckerWorm) },
        { "裙水母", typeof(SkirtJellyfish) },
        { "狮子水母", typeof(LionJellyfish) },
        { "燃料发电机", typeof(FuelGenerator) },
        { "大块生鱼肉", typeof(RawFish) },
        { "大块熟鱼肉", typeof(CookedFish) },
        { "鱼皮", typeof(FishSkin) },
        { "立鳞烧", typeof(Tatsuage) },
        { "老鼠", typeof(Rat) },
        { "垃圾包裹", typeof(JunkPackage) },
    };

    // 每种卡牌的实例
    private static Dictionary<string, Card> cardInstances = new();

    public static void Init()
    {
        if (!configCache.IsNullOrEmpty()) return;

        configCache = ExcelReader.ReadCardConfig("CardConfig");
        foreach (var cardId in classTypes.Keys)
        {
            if (!configCache.ContainsKey(cardId)) continue;

            cardInstances.Add(cardId, CreateCard(cardId));
        }
    }

    /// <summary>
    /// 得到一个静态的卡牌实例
    /// </summary>
    /// <param name="cardId"></param>
    /// <returns></returns>
    public static Card GetStaticCardInstance(string cardId)
    {
        if (cardInstances.ContainsKey(cardId))
        {
            return cardInstances[cardId];
        }
        else
        {
            var card = CreateCard(cardId);
            cardInstances.Add(cardId, card);
            return card;
        }
    }

    /// <summary>
    /// 根据组件类型得到特定的一部分卡牌实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<Card> GetStaticCardInstancesByComponent<T>() where T : CardComponent
    {
        return cardInstances.Values.Where(c => c.TryGetComponent<T>(out _)).ToList();
    }

    public static bool ContainsCard(string cardId)
    {
        return configCache.ContainsKey(cardId);
    }

    public static Sprite GetCardImage(string cardId)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            // 获取图集的所有图片
            var sprites = Resources.LoadAll<Sprite>("Sprites/" + config.CardType.ToString());
            // 找到图片的索引
            if (int.TryParse(config.CardImagePath, out var index) && index < sprites.Length)
            {
                return sprites[index];
            }
            return null;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static Sprite GetCardImage(string cardId, string imagePath)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            // 获取图集的所有图片
            var sprites = Resources.LoadAll<Sprite>("Sprites/" + config.CardType.ToString());
            // 找到图片的索引
            if (int.TryParse(imagePath, out var index) && index < sprites.Length)
            {
                return sprites[index];
            }
            return null;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static bool GetIsBigIcon(string cardId)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.IsBigIcon;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static string GetCardName(string cardId)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.CardName;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static string GetCardDesc(string cardId)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.CardDesc;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static CardType GetCardType(string cardId)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.CardType;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static int GetMaxStackNum(string cardId)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.MaxStackNum;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static bool GetMoveable(string cardId)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.Moveable;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static float GetWeight(string cardId)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.Weight;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static List<CardTag> GetTags(string cardId)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.Tags;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static string GetExtraInfo(string cardId)
    {
        if (configCache.TryGetValue(cardId, out var config))
        {
            return config.CardExtraInfo;
        }
        throw new ArgumentException($"不存在ID为{cardId}的卡牌");
    }

    public static Card CreateCard(string cardId)
    {
        // 从缓存中获取配置
        var config = configCache[cardId];
        var classType = classTypes[cardId];

        // 创建卡牌实例
        Card card = Activator.CreateInstance(classType, true) as Card;

        // 配置基础属性
        card.SetCardId(cardId);

        // 配置可变属性
        if (config.HasFreshness)
        {
            card.AddComponent(new FreshnessComponent(config.MaxFreshness));
        }
        if (config.HasDurability)
        {
            card.AddComponent(new DurabilityComponent(config.MaxDurability));
        }
        if (config.HasGrowth)
        {
            card.AddComponent(new GrowthComponent(config.MaxGrowth));
        }
        if (config.HasProgress)
        {
            card.AddComponent(new ProgressComponent(config.MaxProgress));
        }
        if (config.IsEquipment)
        {
            card.AddComponent(new EquipmentComponent(config.EquipmentType));
        }
        if (config.IsTool)
        {
            card.AddComponent(new ToolComponent(config.ToolTypes));
        }
        if (config.HasInnerContents)
        {
            card.AddComponent(new InnerContentsComponent(config.InnerContentSlotCount));
        }
        if (config.IsFlammable)
        {
            card.AddComponent(new FuelComponent(config.FuelValue));
        }
        if (config.IsPassage)
        {
            card.AddComponent(new PassageComponent(config.TargetPlace, config.MoveTime, config.InteractAudio));
        }
        if (config.CanCook)
        {
            card.AddComponent(new CookComponent(config.CookTime, config.OutcomeCardId));
        }
        if (config.IsConstruction)
        {
            card.AddComponent(new ConstructionComponent(config.OnlyInWater, config.OnlyOutWater, config.OnlyInDoor, config.OnlyOutDoor, config.NeedCable, config.CanBeDemolished, config.DemolitionDebris));
        }
        if (config.HasFoodProperty)
        {
            card.AddComponent(new FoodPropertyComponent(config.FoodPropertyDict));
        }
        if (config.IsPlant)
        {
            card.AddComponent(new PlantGrowthComponent(config.GrowthRate, config.MinConfortTempreture, config.MaxConfortTempreture, config.MinGrowTempture, config.MaxGrowTempture, config.MinLiveTempture, config.MaxLiveTempture, config.DeadcardName, config.Pressures));
        }
        if (config.HasCoordinate)
        {
            card.AddComponent(new CoordinateComponent(config.Position));
        }
        if (config.IsWeapon)
        {
            card.AddComponent(new WeaponComponent(config.WeaponAtk, config.MinAtkDist, config.MaxAtkDist, config.AtkForm, config.AtkTime));
        }
        if (config.IsEntity)
        {
            card.AddComponent(new EntityComponent(config.MaxHealth, config.EntityAtk, config.MoveDistPerMin, config.AIRefreshInterval, config.BehavioralTendency, config.DeadDrops));
        }

        card.LateConstrcutor();

        return card;
    }
}
