/// <summary>
/// 卡牌工厂，用于创建卡牌的实例
/// </summary>
public static class CardFactory
{
    public static Card CreateCard(string cardName)
    {
        return cardName switch
        {
            "气密舱门" => new AirtightDoor(),
            "水瓶鱼" => new AquariusFish(),
            "有产物的水瓶鱼" => new AquariusFishWithProduct(),
            "电池" => new Battery(),
            "瓶装水" => new BottledWater(),
            "被捉住的水瓶鱼" => new CaughtAquariusFish(),
            "有产物的被捉住的水瓶鱼" => new CaughtAquariusFishWithProduct(),
            "压缩饼干" => new CompactBiscuit(),
            "珊瑚" => new Coral(),
            "通往驾驶室的门" => new DoorToCockpit(),
            "通往动力舱的门" => new DoorToPowerCabin(),
            "捞网" => new FishingNet(),
            "玻璃" => new Glass(),
            "玻璃沙" => new GlassSand(),
            "硬质纤维" => new HardFiber(),
            "人力发电机" => new HumanPoweredGenerator(),
            "点燃的氧烛" => new LightenedOxygenCandle(),
            "小块生肉" => new LittleRawMeat(),
            "爱情贝" => new LoveBead(),
            "有产物的爱情贝" => new LoveBeadWithProduct(),
            "磁性触手" => new MagneticTentacle(),
            "矿石释氧机" => new OreReleaseOxygenMachine(),
            "氧气罐" => new OxygenCan(),
            "氧烛" => new OxygenCandle(),
            "氧气面罩" => new OxygenMask(),
            "裂缝填充物" => new Patch(),
            "老鼠尸体" => new RatBody(),
            "生贝肉" => new RawOysterMeat(),
            "腐烂物" => new RotMaterial(),
            "废铁刀" => new ScrapIronKnife(),
            "废金属" => new ScrapMetal(),
            "虹吸海葵" => new Siphonophyllum(),
            "有产物的虹吸海葵" => new SiphonophyllumWithProduct(),
            "废料堆" => new WasteHeap(),
            "渗水裂缝" => new WaterCrack(),
            "白爆矿" => new WhiteBlastMine(),
            _ => null,
        };
    }
}