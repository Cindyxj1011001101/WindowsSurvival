/// <summary>
/// 卡牌工厂，用于创建卡牌的实例
/// </summary>
public static class CardFactory
{
    public static Card CreateCard(string cardName)
    {
        return cardName switch
        {
            "水瓶鱼" => new AquariusFish(),
            "瓶装水" => new BottledWater(),
            "被捉住的水瓶鱼" => new CaughtAquariusFish(),
            "压缩饼干" => new CompactBiscuit(),
            "通往驾驶室的门" => new DoorToCockpit(),
            "通往动力舱的门" => new DoorToPowerCabin(),
            "硬质纤维" => new HardFiber(),
            "小块生肉" => new LittleRawMeat(),
            "老鼠尸体" => new RatBody(),
            "腐烂物" => new RotMaterial(),
            "废铁刀" => new ScrapIronKnife(),
            "废金属" => new ScrapMetal(),
            "废料堆" => new WasteHeap(),
            _ => null,
        };
    }
}