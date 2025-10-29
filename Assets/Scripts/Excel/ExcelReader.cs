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
                IsPassage = ParseBool(row[37].ToString()),
                CanCook = ParseBool(row[41].ToString()),
                IsConstruction = ParseBool(row[44].ToString()),
                IsPlant = ParseBool(row[52].ToString()),
                HasCoordinate = ParseBool(row[57].ToString()),
                IsWeapon = ParseBool(row[59].ToString()),
                IsEntity = ParseBool(row[65].ToString()),
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
                    { FoodProperty.EatableDegree, ParseInt(row[28].ToString()) },     // 可食用度
                    { FoodProperty.UneatableDegree, ParseInt(row[29].ToString()) },   // 不可食用度   
                    { FoodProperty.Meatiness, ParseInt(row[30].ToString()) },         // 肉度
                    { FoodProperty.Fishiness, ParseInt(row[31].ToString()) },         // 鱼度
                    { FoodProperty.Shellfishiness, ParseInt(row[32].ToString()) },    // 贝度
                    { FoodProperty.Wateriness, ParseInt(row[33].ToString()) },        // 水度
                    { FoodProperty.Vegetableness, ParseInt(row[34].ToString()) },     // 菜度
                    { FoodProperty.Fruitiness, ParseInt(row[35].ToString()) },        // 果度
                    { FoodProperty.FoulSmellingDegree, ParseInt(row[36].ToString()) } // 恶臭度
                };
            }
            if (cardConfig.IsPassage)
            {
                cardConfig.MoveTime = ParseInt(row[38].ToString());
                cardConfig.TargetPlace = Enum.Parse<PlaceEnum>(row[39].ToString());
                cardConfig.InteractAudio = row[40].ToString();
            }
            if (cardConfig.CanCook)
            {
                cardConfig.CookTime = ParseInt(row[42].ToString());
                cardConfig.OutcomeCardId = row[43].ToString();
            }
            if (cardConfig.IsConstruction)
            {
                cardConfig.OnlyInWater = ParseBool(row[45].ToString());
                cardConfig.OnlyOutWater = ParseBool(row[46].ToString());
                cardConfig.OnlyInDoor = ParseBool(row[47].ToString());
                cardConfig.OnlyOutDoor = ParseBool(row[48].ToString());
                cardConfig.NeedCable = ParseBool(row[49].ToString());
                cardConfig.CanBeDemolished = ParseBool(row[50].ToString());
                cardConfig.DemolitionDebris = row[51].ToString();
            }
            if (cardConfig.IsPlant)
            {
                cardConfig.GrowthRate = ParseFloat(row[53].ToString());
                string[] tempretures = row[54].ToString().Split('_');
                cardConfig.MinConfortTempreture = ParseFloat(tempretures[0]);
                cardConfig.MaxConfortTempreture = ParseFloat(tempretures[1]);
                cardConfig.MinGrowTempture = ParseFloat(tempretures[2]);
                cardConfig.MaxGrowTempture = ParseFloat(tempretures[3]);
                cardConfig.MinLiveTempture = ParseFloat(tempretures[4]);
                cardConfig.MaxLiveTempture = ParseFloat(tempretures[5]);
                cardConfig.DeadcardName = row[55].ToString();
                cardConfig.Pressures = ParsePressureLevels(row[56].ToString());
            }
            if (cardConfig.HasCoordinate)
            {
                cardConfig.Position = ParseFloat(row[58].ToString());
            }
            if (cardConfig.IsWeapon)
            {
                cardConfig.WeaponAtk = ParseFloat(row[60].ToString());
                cardConfig.MinAtkDist = ParseFloat(row[61].ToString());
                cardConfig.MaxAtkDist = ParseFloat(row[62].ToString());
                cardConfig.AtkForm = Enum.Parse<AttackForm>(row[63].ToString());
                cardConfig.AtkTime = ParseInt(row[64].ToString());
            }
            if (cardConfig.IsEntity)
            {
                cardConfig.MaxHealth = ParseFloat(row[66].ToString());
                cardConfig.EntityAtk = ParseFloat(row[67].ToString());
                cardConfig.MoveDistPerMin = ParseFloat(row[68].ToString());
                cardConfig.BehavioralTendency = Enum.Parse<BehavioralTendency>(row[69].ToString());
                cardConfig.AIRefreshInterval = ParseInt(row[70].ToString());
                cardConfig.DeadDrops = row[71].ToString();
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

    public static Dictionary<PlaceEnum, DropList> GenerateDisposableDropList()
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/DisposableDropListConfig.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet result = excelReader.AsDataSet();

        Dictionary<PlaceEnum, DropList> dict = new();

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
                DisposableDropConfig config = new()
                {
                    CardId = row[0].ToString(),
                    DropNum = ParseInt(row[1].ToString()),
                    DropWeight = ParseInt(row[2].ToString()),
                    OverwriteFreshness = ParseBool(row[3].ToString()),
                    OverwriteDurability = ParseBool(row[5].ToString()),
                    OverwriteGrowth = ParseBool(row[7].ToString()),
                    OverwriteProgress = ParseBool(row[9].ToString()),
                    OverwriteInnerContents = ParseBool(row[11].ToString())
                };

                List<Card> droppedCards = new();

                for (int j = 0; j < config.DropNum; j++)
                {
                    var card = CardFactory.CreateCard(config.CardId);
                    // 覆写卡牌属性
                    if (config.OverwriteFreshness && card.TryGetComponent<FreshnessComponent>(out var f))
                    {
                        f.SetValue(ParseInt(row[4].ToString())); // 覆写新鲜度
                    }
                    if (config.OverwriteDurability && card.TryGetComponent<DurabilityComponent>(out var d))
                    {
                        d.SetValue(ParseInt(row[6].ToString())); // 覆写耐久度
                    }
                    if (config.OverwriteGrowth && card.TryGetComponent<GrowthComponent>(out var g))
                    {
                        g.SetValue(ParseInt(row[8].ToString())); // 覆写生长进度
                    }
                    if (config.OverwriteProgress && card.TryGetComponent<ProgressComponent>(out var p))
                    {
                        p.SetValue(ParseInt(row[10].ToString())); // 覆写产物进度
                    }
                    if (config.OverwriteInnerContents && card.TryGetComponent<InnerContentsComponent>(out var inn))
                    {
                        var startRowIndex = ParseInt(row[12].ToString()); // 覆写内容物
                        var endRowIndex = ParseInt(row[13].ToString());
                        foreach (var c in ReadInnerContents(table, startRowIndex, endRowIndex))
                        {
                            inn.AddCard(c);
                        }
                    }
                    droppedCards.Add(card);
                }

                // 添加到掉落列表
                dropList.Add(new Drop(config.DropWeight, droppedCards));
            }
            // 保存为Json
            DropList disposableDropList = new(dropList, true);
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
            DisposableDropConfig config = new()
            {
                CardId = row[0].ToString(),
                DropNum = ParseInt(row[1].ToString()),
                OverwriteFreshness = ParseBool(row[3].ToString()),
                OverwriteDurability = ParseBool(row[5].ToString()),
                OverwriteGrowth = ParseBool(row[7].ToString()),
                OverwriteProgress = ParseBool(row[9].ToString()),
            };
            // 创建卡牌实例
            var card = CardFactory.CreateCard(config.CardId);
            // 覆写卡牌属性
            if (config.OverwriteFreshness)
            {
                if (card.TryGetComponent<FreshnessComponent>(out var freshnessComponent))
                    freshnessComponent.SetValue(ParseInt(row[4].ToString())); // 设置新鲜度
            }
            if (config.OverwriteDurability)
            {
                if (card.TryGetComponent<DurabilityComponent>(out var durabilityComponent))
                    durabilityComponent.SetValue(ParseInt(row[6].ToString())); // 设置耐久度
            }
            if (config.OverwriteGrowth)
            {
                if (card.TryGetComponent<GrowthComponent>(out var growthComponent))
                    growthComponent.SetValue(ParseInt(row[8].ToString())); // 设置生长进度
            }
            if (config.OverwriteProgress)
            {
                if (card.TryGetComponent<ProgressComponent>(out var progressComponent))
                    progressComponent.SetValue(ParseInt(row[10].ToString())); // 设置产物进度
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
                        freshnessComponent.SetValue(ParseInt(row[8].ToString())); // 设置新鲜度
                }
                if (config.OverwriteDurability)
                {
                    if (card.TryGetComponent<DurabilityComponent>(out var durabilityComponent))
                        durabilityComponent.SetValue(ParseInt(row[10].ToString())); // 设置耐久度
                }
                if (config.OverwriteGrowth)
                {
                    if (card.TryGetComponent<GrowthComponent>(out var growthComponent))
                        growthComponent.SetValue(ParseInt(row[12].ToString())); // 设置生长进度
                }
                if (config.OverwriteProgress)
                {
                    if (card.TryGetComponent<ProgressComponent>(out var progressComponent))
                        progressComponent.SetValue(ParseInt(row[14].ToString())); // 设置产物进度
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
    public static List<ProcessConfig> ReadProcessConfig(string filename)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/{filename}.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet result = excelReader.AsDataSet();
        DataTable table = result.Tables[0]; // 配置在第一张表中
        
        List<ProcessConfig> processConfigList = new();

        for (int i = 1; i < table.Rows.Count; i++) // 从1开始跳过表头
        {
            processConfigList.Add(ProcessConfig.Parse(table.Rows[i]));
        }

        return processConfigList;
    }
    #endregion

    #region 读取事件配置
    public static List<GameEvent> ReadGameEventConfig(string filename)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/{filename}.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet result = excelReader.AsDataSet();
        DataTable table = result.Tables[0]; // 配置在第一张表中
        List<GameEvent> eventList = new();
        for (int i = 1; i < table.Rows.Count; i++) // 从1开始跳过表头
        {
            DataRow row = table.Rows[i];
            if (string.IsNullOrEmpty(row[0].ToString())) continue; // 如果事件名称为空，跳过读取
            GameEvent e = GameEvent.ParseDataRow(row);
            if (e == null) continue;
            eventList.Add(e);
        }
        return eventList;
    }
    #endregion

    #region 读取入侵组合配置
    public static List<InvasionComposition> ReadInvasionCompositionConfig(string filename)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/{filename}.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet result = excelReader.AsDataSet();
        DataTable table = result.Tables[0]; // 配置在第一张表中
        List<InvasionComposition> compositionList = new();
        for (int i = 1; i < table.Rows.Count; i++) // 从1开始跳过表头
        {
            DataRow row = table.Rows[i];
            if (string.IsNullOrEmpty(row[0].ToString())) continue; // 如果组合称为空，跳过读取
            InvasionComposition composition = InvasionComposition.ParseDataRow(row);
            if (composition == null) continue;
            compositionList.Add(composition);
        }
        return compositionList;
    }
    #endregion
}
