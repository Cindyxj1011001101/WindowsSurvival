using System.Collections.Generic;

public class CardConfig
{
    public string CardId; // 卡牌ID
    public string CardName; // 卡牌名称
    public string CardExtraInfo; // 额外信息
    public string CardDesc; // 卡牌描述
    public string CardImagePath; // 卡牌图片路径
    public bool IsBigIcon; // 是否是大图标
    public CardType CardType; // 卡牌类型
    public int MaxStackNum; // 最大堆叠数
    public bool Moveable; // 是否可移动
    public float Weight; // 重量
    public List<CardTag> Tags = new(); // 标签
    public CardTextureType TextureType; // 质感（用于音效）
    public bool HasMultipleStates; // 是否有多种状态
    public List<CardState> States = new(); // 卡牌状态列表
    public bool HasFreshness; // 是否有新鲜度
    public int MaxFreshness; // 新鲜度最大值
    public bool HasDurability; // 是否有耐久度
    public int MaxDurability; // 耐久度最大值
    public bool HasGrowth; // 是否有生长进度
    public int MaxGrowth; // 生长最大进度
    public bool HasProgress; // 是否有产物进度
    public int MaxProgress; // 产物最大进度
    public bool IsEquipment; // 是否是装备
    public EquipmentType EquipmentType; // 装备类型
    public bool IsTool; // 是否是工具
    public List<ToolType> ToolTypes; // 工具类型
    public bool HasInnerContents; // 是否有内部内容（如生物、建筑等）
    public int InnerContentSlotCount; // 内部内容槽位数量
    public bool IsFuel; // 是否有可燃烧组件
    public int FuelValue; // 可燃烧时间
    public bool HasFuelStorage; // 是否有燃料存储组件
    public int FuelStorageCapacity; // 燃料存储容量
    public bool HasFoodProperty; // 是否有食物属性
    public Dictionary<FoodProperty, int> FoodPropertyDict = new(); // 食物属性
    public bool IsPassage; // 是否是通道
    public int MoveTime; // 移动时间
    public PlaceEnum TargetPlace; // 目标地点
    public string InteractAudio; // 交互音效
    public bool CanCook; // 能否烹饪
    public int CookTime; // 烹饪时长
    public string OutcomeCardId; // 烹饪产物
    public bool IsConstruction; // 是否是建筑
    public bool OnlyInWater; // 是否仅建造在水域地点
    public bool OnlyOutWater; // 是否仅建造在陆地地点
    public bool OnlyInDoor; // 是否仅建造在室内
    public bool OnlyOutDoor; // 是否仅建造在室外
    public bool NeedCable; // 是否需要电缆
    public bool CanBeDemolished; // 能否被拆毁
    public string DemolitionDebris; // 拆毁后产物ID
    public bool IsPlant; // 是否是植物
    public float GrowthRate; // 生长速度
    public float MinConfortTempreture; // 舒适温度下限
    public float MaxConfortTempreture; // 舒适温度上限
    public float MinGrowTempture; // 生长温度下限
    public float MaxGrowTempture; // 生长温度上限
    public float MinLiveTempture; // 存活温度下限
    public float MaxLiveTempture; // 存活温度上限
    public string DeadcardName; // 死亡后掉落的卡帕名称
    public List<PressureLevel> Pressures; // 存活压强(_隔开)
    public bool HasCoordinate; // 是否有坐标
    public float Position; // 坐标位置
    public bool IsWeapon; // 是否是武器
    public float WeaponAtk; // 武器攻击力
    public float MinAtkDist; // 最小攻击距离
    public float MaxAtkDist; // 最大攻击距离
    public AttackForm AtkForm; // 攻击方式
    public int AtkTime; // 攻击时间
    public bool IsEntity; // 是否是实体
    public float MaxHealth; // 最大生命值
    public float EntityAtk; // 实体攻击力
    public float MoveDistPerMin; // 每分钟移动距离
    public BehavioralTendency BehavioralTendency; // 行为倾向
    public int AIRefreshInterval; // AI刷新间隔
    public string DeadDrops; // 死亡掉落
}
