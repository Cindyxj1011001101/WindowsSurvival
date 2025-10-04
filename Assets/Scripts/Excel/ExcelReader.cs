using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Excel;
using UnityEngine;

public static class ExcelReader
{
    public static Dictionary<string, CardConfig> ReadCardConfig(string fileName)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/{fileName}.xlsx", FileMode.Open, FileAccess.Read);
        var excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        var result = excelReader.AsDataSet();
        DataTable table = result.Tables[0]; // 配置在第一张表中

        // 存储卡牌配置的字典
        Dictionary<string, CardConfig> cardConfigs = new();

        //Debug.Log("卡牌表列数：" + table.Columns.Count);

        DataRow row;
        int count = 0;
        for (int i = 1; i < table.Rows.Count; i++) // 从1开始跳过表头
        {
            row = table.Rows[i];
            if (string.IsNullOrEmpty(row[0].ToString())) continue; // 如果卡牌ID为空，跳过读取
            count++;

            // 必要字段
            CardConfig cardConfig = new()
            {
                CardId = row[0].ToString(),
                CardName = row[1].ToString(),
                CardExtraInfo = row[2].ToString(),
                CardDesc = row[3].ToString(),
                CardImagePath = row[4].ToString(),
                CardType = Enum.Parse<CardType>(row[5].ToString()),
                MaxStackNum = ParseInt(row[6].ToString()),
                Moveable = ParseBool(row[7].ToString()),
                Weight = ParseFloat(row[8].ToString()),
                Tags = ParseTags(row[9].ToString()),
                HasFreshness = ParseBool(row[10].ToString()),
                HasDurability = ParseBool(row[12].ToString()),
                HasGrowth = ParseBool(row[14].ToString()),
                HasProgress = ParseBool(row[16].ToString()),
                IsEquipment = ParseBool(row[18].ToString()),
                IsTool = ParseBool(row[20].ToString()),
                IsBigIcon = ParseBool(row[22].ToString()),
                HasInnerContents = ParseBool(row[23].ToString()),
                IsFlammable = ParseBool(row[25].ToString()),
                HasFoodProperty = ParseBool(row[27].ToString()),
                IsPassage = ParseBool(row[36].ToString()),
                CanCook = ParseBool(row[40].ToString()),
                IsConstruction = ParseBool(row[43].ToString()),
                IsPlant = ParseBool(row[51].ToString()),
                HasCoordinate = ParseBool(row[56].ToString()),
                IsWeapon = ParseBool(row[58].ToString()),
                IsEntity = ParseBool(row[63].ToString()),
            };
            // 可选字段
            if (cardConfig.HasFreshness)
            {
                cardConfig.MaxFreshness = ParseInt(row[11].ToString());
            }
            if (cardConfig.HasDurability)
            {
                cardConfig.MaxDurability = ParseInt(row[13].ToString());
            }
            if (cardConfig.HasGrowth)
            {
                cardConfig.MaxGrowth = ParseInt(row[15].ToString());
            }
            if (cardConfig.HasProgress)
            {
                cardConfig.MaxProgress = ParseInt(row[17].ToString());
            }
            if (cardConfig.IsEquipment)
            {
                cardConfig.EquipmentType = Enum.Parse<EquipmentType>(row[19].ToString());
            }
            if (cardConfig.IsTool)
            {
                cardConfig.ToolTypes = ParseToolTypes(row[21].ToString());
            }
            if (cardConfig.HasInnerContents)
            {
                cardConfig.InnerContentSlotCount = ParseInt(row[24].ToString());
            }
            if (cardConfig.IsFlammable)
            {
                cardConfig.FuelValue = ParseInt(row[26].ToString());
            }
            if (cardConfig.HasFoodProperty)
            {
                cardConfig.FoodPropertyDict = new Dictionary<FoodProperty, int>
                {
                    { FoodProperty.EatableDegree, ParseInt(row[28].ToString()) },   // 可食用度
                    { FoodProperty.UneatableDegree, ParseInt(row[29].ToString()) }, // 不可食用度   
                    { FoodProperty.Meatiness, ParseInt(row[30].ToString()) },       // 肉度
                    { FoodProperty.Fishiness, ParseInt(row[31].ToString()) },       // 鱼度
                    { FoodProperty.Shellfishiness, ParseInt(row[32].ToString()) },  // 贝度
                    { FoodProperty.Wateriness, ParseInt(row[33].ToString()) },      // 水度
                    { FoodProperty.Vegetableness, ParseInt(row[34].ToString()) },   // 菜度
                    { FoodProperty.Fruitiness, ParseInt(row[35].ToString()) }       // 果度
                };
            }
            if (cardConfig.IsPassage)
            {
                cardConfig.MoveTime = ParseInt(row[37].ToString());
                cardConfig.TargetPlace = Enum.Parse<PlaceEnum>(row[38].ToString());
                cardConfig.InteractAudio = row[39].ToString();
            }
            if (cardConfig.CanCook)
            {
                cardConfig.CookTime = ParseInt(row[41].ToString());
                cardConfig.OutcomeCardId = row[42].ToString();
            }
            if (cardConfig.IsConstruction)
            {
                cardConfig.OnlyInWater = ParseBool(row[44].ToString());
                cardConfig.OnlyOutWater = ParseBool(row[45].ToString());
                cardConfig.OnlyInDoor = ParseBool(row[46].ToString());
                cardConfig.OnlyOutDoor = ParseBool(row[47].ToString());
                cardConfig.NeedCable = ParseBool(row[48].ToString());
                cardConfig.CanBeDemolished = ParseBool(row[49].ToString());
                cardConfig.DemolitionDebris = row[50].ToString();
            }
            if (cardConfig.IsPlant)
            {
                cardConfig.GrowthRate = ParseFloat(row[52].ToString());
                string[] tempretures = row[53].ToString().Split('_');
                cardConfig.MinConfortTempreture = ParseFloat(tempretures[0]);
                cardConfig.MaxConfortTempreture = ParseFloat(tempretures[1]);
                cardConfig.MinGrowTempture = ParseFloat(tempretures[2]);
                cardConfig.MaxGrowTempture = ParseFloat(tempretures[3]);
                cardConfig.MinLiveTempture = ParseFloat(tempretures[4]);
                cardConfig.MaxLiveTempture = ParseFloat(tempretures[5]);
                cardConfig.DeadcardName = row[54].ToString();
                cardConfig.Pressures = ParsePressureLevels(row[55].ToString());
            }
            if (cardConfig.HasCoordinate)
            {
                cardConfig.Position = ParseFloat(row[57].ToString());
            }
            if (cardConfig.IsWeapon)
            {
                cardConfig.WeaponAtk = ParseFloat(row[59].ToString());
                cardConfig.MinAtkDist = ParseFloat(row[60].ToString());
                cardConfig.MaxAtkDist = ParseFloat(row[61].ToString());
                cardConfig.AtkForm = Enum.Parse<AttackForm>(row[62].ToString());
            }
            if (cardConfig.IsEntity)
            {
                cardConfig.MaxHealth = ParseFloat(row[64].ToString());
                cardConfig.EntityAtk = ParseFloat(row[65].ToString());
                cardConfig.MoveDistPerMin = ParseFloat(row[66].ToString());
                cardConfig.BehavioralTendency = Enum.Parse<BehavioralTendency>(row[67].ToString());
                cardConfig.AIRefreshInterval = ParseInt(row[68].ToString());
            }
            cardConfigs.Add(cardConfig.CardId, cardConfig);
        }

        //Debug.Log($"卡牌配置读取完成。读取数量：{count}");

        fs.Close();

        return cardConfigs;
    }

    public static bool ParseBool(string str) => bool.TryParse(str, out var value) && value;

    public static int ParseInt(string str) => int.TryParse(str, out int value) ? value : default;

    public static float ParseFloat(string str) => float.TryParse(str, out float value) ? value : default;


    private static List<CardTag> ParseTags(string tagsStr)
    {
        var tags = new List<CardTag>();
        if (string.IsNullOrEmpty(tagsStr)) return tags;

        var tagArray = tagsStr.Split(',');
        foreach (var tag in tagArray)
        {
            tags.Add(Enum.Parse<CardTag>(tag.Trim()));
        }

        return tags;
    }

    private static List<ToolType> ParseToolTypes(string toolTypesStr)
    {
        var toolTypes = new List<ToolType>();
        if (string.IsNullOrEmpty(toolTypesStr)) return toolTypes;
        var toolTypeArray = toolTypesStr.Split(',');
        foreach (var toolType in toolTypeArray)
        {
            toolTypes.Add(Enum.Parse<ToolType>(toolType.Trim()));
        }
        return toolTypes;
    }

    private static List<PressureLevel> ParsePressureLevels(string pressureLevelsStr)
    {
        var result = new List<PressureLevel>();
        foreach (var pressure in pressureLevelsStr.Split("_"))
        {
            result.Add(pressure switch
            {
                "高压强" => PressureLevel.High,
                "极高压强" => PressureLevel.VeryHigh,
                "极低压强" => PressureLevel.VeryLow,
                "低压强" => PressureLevel.Low,
                "标准压强" => PressureLevel.Standard,
                _ => throw new Exception($"无效的温度类型: {pressure}")
            });
        }

        return result;
    }

    public static Dictionary<PlaceEnum, DisposableDropList> GenerateDisposableDropList()
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/DisposableDropListConfig.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet result = excelReader.AsDataSet();

        Dictionary<PlaceEnum, DisposableDropList> dict = new();

        foreach (DataTable table in result.Tables)
        {
            // 假设每个表都是一次性掉落列表
            List<Drop> dropList = new();
            DataRow row;
            for (int i = 1; i < table.Rows.Count; i++) // 从1开始跳过表头
            {
                row = table.Rows[i];

                if (string.IsNullOrEmpty(row[0].ToString())) break; // 遇到空行说明读取完毕了，后续是内容物的配置

                // 读取掉落配置
                DropConfig config = new()
                {
                    CardId = row[0].ToString(),
                    DropNum = ParseInt(row[1].ToString()),
                    DropProb = ParseInt(row[2].ToString()),
                    OverwriteFreshness = ParseBool(row[3].ToString()),
                    OverwriteDurability = ParseBool(row[5].ToString()),
                    OverwriteGrowth = ParseBool(row[7].ToString()),
                    OverwriteProgress = ParseBool(row[9].ToString()),
                    OverwriteInnerContents = ParseBool(row[11].ToString())
                };
                // 创建卡牌实例
                var card = CardFactory.CreateCard(config.CardId);
                // 覆写卡牌属性
                if (config.OverwriteFreshness)
                {
                    if (card.TryGetComponent<FreshnessComponent>(out var freshnessComponent))
                        freshnessComponent.freshness = ParseInt(row[4].ToString()); // 设置新鲜度
                }
                if (config.OverwriteDurability)
                {
                    if (card.TryGetComponent<DurabilityComponent>(out var durabilityComponent))
                        durabilityComponent.durability = ParseInt(row[6].ToString()); // 设置耐久度
                }
                if (config.OverwriteGrowth)
                {
                    if (card.TryGetComponent<GrowthComponent>(out var growthComponent))
                        growthComponent.growth = ParseInt(row[8].ToString()); // 设置生长进度
                }
                if (config.OverwriteProgress)
                {
                    if (card.TryGetComponent<ProgressComponent>(out var progressComponent))
                        progressComponent.progress = ParseInt(row[10].ToString()); // 设置产物进度
                }
                if (config.OverwriteInnerContents)
                {
                    var startRowIndex = ParseInt(row[12].ToString());
                    var endRowIndex = ParseInt(row[13].ToString());
                    if (card.TryGetComponent<InnerContentsComponent>(out var innerContentsComponent))
                        foreach (var c in ReadInnerContents(table, startRowIndex, endRowIndex))
                        {
                            innerContentsComponent.bag.AddCard(c);
                        }
                }
                // 添加到掉落列表
                dropList.Add(new Drop
                {
                    card = card,
                    dropNum = config.DropNum,
                    dropProb = config.DropProb
                });
            }
            // 保存为Json
            DisposableDropList disposableDropList = new() { maxCount = dropList.Count, dropList = dropList };
            dict.Add(Enum.Parse<PlaceEnum>(table.TableName), disposableDropList);
        }
        return dict;
    }

    private static List<Card> ReadInnerContents(DataTable table, int startRowIndex, int endRowIndex)
    {
        List<Card> result = new();

        DataRow row;
        for (int i = startRowIndex - 1; i < endRowIndex; i++) // 从1开始跳过表头
        {
            row = table.Rows[i];
            // 读取掉落配置
            DropConfig config = new()
            {
                CardId = row[0].ToString(),
                DropNum = ParseInt(row[1].ToString()),
                //DropProb = ParseInt(row[2].ToString()),
                OverwriteFreshness = ParseBool(row[3].ToString()),
                OverwriteDurability = ParseBool(row[5].ToString()),
                OverwriteGrowth = ParseBool(row[7].ToString()),
                OverwriteProgress = ParseBool(row[9].ToString()),
                //OverwriteInnerContents = ParseBool(row[11].ToString())
            };
            // 创建卡牌实例
            var card = CardFactory.CreateCard(config.CardId);
            // 覆写卡牌属性
            if (config.OverwriteFreshness)
            {
                if (card.TryGetComponent<FreshnessComponent>(out var freshnessComponent))
                    freshnessComponent.freshness = ParseInt(row[4].ToString()); // 设置新鲜度
            }
            if (config.OverwriteDurability)
            {
                if (card.TryGetComponent<DurabilityComponent>(out var durabilityComponent))
                    durabilityComponent.durability = ParseInt(row[6].ToString()); // 设置耐久度
            }
            if (config.OverwriteGrowth)
            {
                if (card.TryGetComponent<GrowthComponent>(out var growthComponent))
                    growthComponent.growth = ParseInt(row[8].ToString()); // 设置生长进度
            }
            if (config.OverwriteProgress)
            {
                if (card.TryGetComponent<ProgressComponent>(out var progressComponent))
                    progressComponent.progress = ParseInt(row[10].ToString()); // 设置产物进度
            }
            // 添加到掉落列表
            for (int j = 0; j < config.DropNum; j++)
            {
                result.Add(JsonManager.DeepCopy(card));
            }
        }

        return result;
    }

    public static Dictionary<PlaceEnum, DeepExploreDropList> GenerateDeepExploreDropList()
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/DeepExploreDropListConfig.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet result = excelReader.AsDataSet();

        Dictionary<PlaceEnum, DeepExploreDropList> dict = new();

        foreach (DataTable table in result.Tables)
        {
            DataRow emptyPopulationConfig = table.Rows[1]; // 假设第一行是空种群配置

            Population emptyPopulation = new()
            {
                curSize = ParseInt(emptyPopulationConfig[2].ToString()),
                maxSize = ParseInt(emptyPopulationConfig[3].ToString()),
                sizeChangePerRound = ParseInt(emptyPopulationConfig[4].ToString()),
            };
            int sizeChangeOnNotCaught = ParseInt(emptyPopulationConfig[6].ToString());

            // 假设每个表都是重复掉落列表
            List<Population> populationList = new();
            DataRow row;
            for (int i = 2; i < table.Rows.Count; i++) // 从2开始跳过表头个空种群配置
            {
                row = table.Rows[i];
                // 读取掉落配置
                PopulationConfig config = new()
                {
                    CardId = row[0].ToString(),
                    DropNum = ParseInt(row[1].ToString()),
                    Size = ParseInt(row[2].ToString()),
                    MaxSize = ParseInt(row[3].ToString()),
                    SizeChangePerRound = ParseInt(row[4].ToString()),
                    SizeChangeOnCaught = ParseInt(row[5].ToString()),
                    OverwriteFreshness = ParseBool(row[7].ToString()),
                    OverwriteDurability = ParseBool(row[9].ToString()),
                    OverwriteGrowth = ParseBool(row[11].ToString()),
                    OverwriteProgress = ParseBool(row[13].ToString()),
                    Trappable = ParseBool(row[15].ToString()),
                };
                // 创建卡牌实例
                var card = CardFactory.CreateCard(config.CardId);
                // 覆写卡牌属性
                if (config.OverwriteFreshness)
                {
                    if (card.TryGetComponent<FreshnessComponent>(out var freshnessComponent))
                        freshnessComponent.freshness = ParseInt(row[8].ToString()); // 设置新鲜度
                }
                if (config.OverwriteDurability)
                {
                    if (card.TryGetComponent<DurabilityComponent>(out var durabilityComponent))
                        durabilityComponent.durability = ParseInt(row[10].ToString()); // 设置耐久度
                }
                if (config.OverwriteGrowth)
                {
                    if (card.TryGetComponent<GrowthComponent>(out var growthComponent))
                        growthComponent.growth = ParseInt(row[12].ToString()); // 设置生长进度
                }
                if (config.OverwriteProgress)
                {
                    if (card.TryGetComponent<ProgressComponent>(out var progressComponent))
                        progressComponent.progress = ParseInt(row[14].ToString()); // 设置产物进度
                }
                // 添加到掉落列表
                populationList.Add(new Population()
                {
                    card = card,
                    dropNum = config.DropNum,
                    curSize = config.Size,
                    maxSize = config.MaxSize,
                    trappable = config.Trappable,
                    sizeChangePerRound = config.SizeChangePerRound,
                    sizeChangeOnCaught = config.SizeChangeOnCaught
                });
            }
            // 保存为Json
            DeepExploreDropList repeatableDropList = new()
            {
                emptyPopulation = emptyPopulation,
                emptyPopulationSizeChangeOnNotCaught = sizeChangeOnNotCaught,
                populationList = populationList
            };
            dict.Add(Enum.Parse<PlaceEnum>(table.TableName), repeatableDropList);
        }
        return dict;
    }

    #region 读取加工表配置
    public static List<ProcessData> ReadProcess(string filename)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/{filename}.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet result = excelReader.AsDataSet();
        List<ProcessData> processDataList = new();
        foreach (DataTable table in result.Tables)
        {
            DataRow row;
            for (int i = 1; i < table.Rows.Count; i++)
            {
                row = table.Rows[i];
                processDataList.Add(new ProcessData(row));
            }
        }
        return processDataList;
    }
    #endregion
}

public class CardConfig
{
    public string CardId; // 卡牌ID
    public string CardName; // 卡牌名称
    public string CardExtraInfo; // 额外信息
    public string CardDesc; // 卡牌描述
    public string CardImagePath; // 卡牌图片路径
    public CardType CardType; // 卡牌类型
    public int MaxStackNum; // 最大堆叠数
    public bool Moveable; // 是否可移动
    public float Weight; // 重量
    public List<CardTag> Tags = new(); // 标签
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
    public bool IsBigIcon; // 是否是大图标
    public bool HasInnerContents; // 是否有内部内容（如生物、建筑等）
    public int InnerContentSlotCount; // 内部内容槽位数量
    public bool IsFlammable; // 是否有可燃烧组件
    public int FuelValue; // 可燃烧时间
    public bool HasFoodProperty; // 是否有食物属性
    public Dictionary<FoodProperty, int> FoodPropertyDict; // 食物属性
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
    public bool IsEntity; // 是否是实体
    public float MaxHealth; // 最大生命值
    public float EntityAtk; // 实体攻击力
    public float MoveDistPerMin; // 每分钟移动距离
    public BehavioralTendency BehavioralTendency; // 行为倾向
    public int AIRefreshInterval; // AI刷新间隔
}

public class DropConfig
{
    public string CardId; // 卡牌
    public int DropNum; // 掉落数量
    public int DropProb;
    public bool OverwriteFreshness; // 是否覆盖新鲜度
    public bool OverwriteDurability; // 是否覆盖耐久度
    public bool OverwriteGrowth;
    public bool OverwriteProgress; // 是否覆盖产物进度
    public bool OverwriteInnerContents; // 是否覆盖内容物
}

public class PopulationConfig
{
    public string CardId; // 卡牌ID
    public int DropNum; // 掉落数量
    public int Size; // 人口数量
    public int MaxSize; // 最大人口数量
    public int SizeChangePerRound; // 每回合数量变化
    public int SizeChangeOnCaught; // 捕捞后的数量变化
    public bool OverwriteFreshness; // 是否覆盖新鲜度
    public bool OverwriteDurability; // 是否覆盖耐久度
    public bool OverwriteGrowth;
    public bool OverwriteProgress; // 是否覆盖产物进度
    public bool Trappable;
}