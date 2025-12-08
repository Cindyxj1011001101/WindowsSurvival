public enum CardType
{
    Place,         // 地点
    ResourcePoint, // 资源点
    Entity,        // 实体
    Creature,      // 生物
    Crop,          // 作物
    Seed,          // 种子
    Construction,  // 建筑
    Food,          // 食物
    Liquid,        // 液体
    Medicine​,      // 药品
    Resource,      // 资源
    Tool,          // 工具
    Weapon,        // 武器
    Equipment,     // 装备
    Other,         // 其他
}

// 卡牌质感（用于拾取/放置音效选择）
public enum CardTextureType
{
    Default,
    Flesh,  // 肉质感
    Metal,  // 金属质感
    Liquid, // 液体质感
}