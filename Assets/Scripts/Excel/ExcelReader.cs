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
        var dataSet = excelReader.AsDataSet();
        DataTable table = dataSet.Tables[0]; // 配置在第一张表中

        // 存储卡牌配置的字典
        Dictionary<string, CardConfig> configs = new();

        //Debug.Log("卡牌表列数：" + table.Columns.Count);

        DataRow row;
        CardConfig currentConfig = null;
        int count = 0;
        for (int i = 1; i < table.Rows.Count; i++) // 从1开始跳过表头
        {
            row = table.Rows[i];

            if (string.IsNullOrEmpty(row[0].ToString()))
            {
                if (currentConfig != null && currentConfig.CardName == row[1].ToString())
                {
                    // 卡牌名相同说明这是同一张卡牌的不同状态
                    currentConfig.States.Add(ParseCardState(row));
                }
                continue;
            }
            
            count++;

            // 必要字段
            currentConfig = new()
            {
                CardId = row[0].ToString(),
                CardName = row[1].ToString(),
                CardExtraInfo = row[2].ToString(),
                CardDesc = row[3].ToString(),
                HasMultipleStates = !string.IsNullOrEmpty(row[4].ToString()),
                CardImagePath = row[5].ToString(),
                IsBigIcon = ParseBool(row[6].ToString()),
                CardType = Enum.Parse<CardType>(row[7].ToString()),
                MaxStackNum = ParseInt(row[8].ToString()),
                Moveable = ParseBool(row[9].ToString()),
                Weight = ParseFloat(row[10].ToString()),
                Tags = ParseTags(row[11].ToString()),
                TextureType = ParseTextureType(row[12].ToString()),
                HasFreshness = ParseBool(row[13].ToString()),
                HasDurability = ParseBool(row[15].ToString()),
                HasGrowth = ParseBool(row[17].ToString()),
                HasProgress = ParseBool(row[19].ToString()),
                IsEquipment = ParseBool(row[21].ToString()),
                IsTool = ParseBool(row[23].ToString()),
                HasInnerContents = ParseBool(row[25].ToString()),
                IsFuel = ParseBool(row[27].ToString()),
                HasFuelStorage = ParseBool(row[29].ToString()),
                HasFoodProperty = ParseBool(row[31].ToString()),
                IsPassage = ParseBool(row[41].ToString()),
                CanCook = ParseBool(row[45].ToString()),
                IsConstruction = ParseBool(row[48].ToString()),
                IsPlant = ParseBool(row[56].ToString()),
                HasCoordinate = ParseBool(row[61].ToString()),
                IsWeapon = ParseBool(row[63].ToString()),
                IsEntity = ParseBool(row[69].ToString()),
            };
            // 可选字段
            if (currentConfig.HasMultipleStates)
            {
                currentConfig.States.Add(ParseCardState(row));
            }
            if (currentConfig.HasFreshness)
            {
                currentConfig.MaxFreshness = ParseInt(row[14].ToString());
            }
            if (currentConfig.HasDurability)
            {
                currentConfig.MaxDurability = ParseInt(row[16].ToString());
            }
            if (currentConfig.HasGrowth)
            {
                currentConfig.MaxGrowth = ParseInt(row[18].ToString());
            }
            if (currentConfig.HasProgress)
            {
                currentConfig.MaxProgress = ParseInt(row[20].ToString());
            }
            if (currentConfig.IsEquipment)
            {
                currentConfig.EquipmentType = Enum.Parse<EquipmentType>(row[22].ToString());
            }
            if (currentConfig.IsTool)
            {
                currentConfig.ToolTypes = ParseToolTypes(row[24].ToString());
            }
            if (currentConfig.HasInnerContents)
            {
                currentConfig.InnerContentSlotCount = ParseInt(row[26].ToString());
            }
            if (currentConfig.IsFuel)
            {
                currentConfig.FuelValue = ParseInt(row[28].ToString());
            }
            if (currentConfig.HasFuelStorage)
            {
                currentConfig.FuelStorageCapacity = ParseInt(row[30].ToString());
            }
            if (currentConfig.HasFoodProperty)
            {
                currentConfig.FoodPropertyDict = new Dictionary<FoodProperty, int>
                {
                    { FoodProperty.EatableDegree, ParseInt(row[32].ToString()) },     // 可食用度
                    { FoodProperty.UneatableDegree, ParseInt(row[33].ToString()) },   // 不可食用度   
                    { FoodProperty.Meatiness, ParseInt(row[34].ToString()) },         // 肉度
                    { FoodProperty.Fishiness, ParseInt(row[35].ToString()) },         // 鱼度
                    { FoodProperty.Shellfishiness, ParseInt(row[36].ToString()) },    // 贝度
                    { FoodProperty.Wateriness, ParseInt(row[37].ToString()) },        // 水度
                    { FoodProperty.Vegetableness, ParseInt(row[38].ToString()) },     // 菜度
                    { FoodProperty.Fruitiness, ParseInt(row[39].ToString()) },        // 果度
                    { FoodProperty.FoulSmellingDegree, ParseInt(row[40].ToString()) } // 恶臭度
                };
            }
            if (currentConfig.IsPassage)
            {
                currentConfig.MoveTime = ParseInt(row[42].ToString());
                currentConfig.TargetPlace = Enum.Parse<PlaceEnum>(row[43].ToString());
                currentConfig.InteractAudio = row[44].ToString();
            }
            if (currentConfig.CanCook)
            {
                currentConfig.CookTime = ParseInt(row[46].ToString());
                currentConfig.OutcomeCardId = row[47].ToString();
            }
            if (currentConfig.IsConstruction)
            {
                currentConfig.OnlyInWater = ParseBool(row[49].ToString());
                currentConfig.OnlyOutWater = ParseBool(row[50].ToString());
                currentConfig.OnlyInDoor = ParseBool(row[51].ToString());
                currentConfig.OnlyOutDoor = ParseBool(row[52].ToString());
                currentConfig.NeedCable = ParseBool(row[53].ToString());
                currentConfig.CanBeDemolished = ParseBool(row[54].ToString());
                currentConfig.DemolitionDebris = row[55].ToString();
            }
            if (currentConfig.IsPlant)
            {
                currentConfig.GrowthRate = ParseFloat(row[57].ToString());
                string[] tempretures = row[58].ToString().Split('_');
                currentConfig.MinConfortTempreture = ParseFloat(tempretures[0]);
                currentConfig.MaxConfortTempreture = ParseFloat(tempretures[1]);
                currentConfig.MinGrowTempture = ParseFloat(tempretures[2]);
                currentConfig.MaxGrowTempture = ParseFloat(tempretures[3]);
                currentConfig.MinLiveTempture = ParseFloat(tempretures[4]);
                currentConfig.MaxLiveTempture = ParseFloat(tempretures[5]);
                currentConfig.DeadcardName = row[59].ToString();
                currentConfig.Pressures = ParsePressureLevels(row[60].ToString());
            }
            if (currentConfig.HasCoordinate)
            {
                currentConfig.Position = ParseFloat(row[62].ToString());
            }
            if (currentConfig.IsWeapon)
            {
                currentConfig.WeaponAtk = ParseFloat(row[64].ToString());
                currentConfig.MinAtkDist = ParseFloat(row[65].ToString());
                currentConfig.MaxAtkDist = ParseFloat(row[66].ToString());
                currentConfig.AtkForm = Enum.Parse<AttackForm>(row[67].ToString());
                currentConfig.AtkTime = ParseInt(row[68].ToString());
            }
            if (currentConfig.IsEntity)
            {
                currentConfig.MaxHealth = ParseFloat(row[70].ToString());
                currentConfig.EntityAtk = ParseFloat(row[71].ToString());
                currentConfig.MoveDistPerMin = ParseFloat(row[72].ToString());
                currentConfig.BehavioralTendency = Enum.Parse<BehavioralTendency>(row[73].ToString());
                currentConfig.AIRefreshInterval = ParseInt(row[74].ToString());
                currentConfig.DeadDrops = row[75].ToString();
            }
            configs.Add(currentConfig.CardId, currentConfig);
        }

        fs.Close();

        return configs;
    }

    public static CardState ParseCardState(DataRow row)
    {
        return new CardState()
        {
            stateName = row[4].ToString(),
            extraInfo = row[2].ToString(),
            imagePath = row[5].ToString(),
            isBigIcon = ParseBool(row[6].ToString()),
        };
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

    private static CardTextureType ParseTextureType(string textureStr)
    {
        if (string.IsNullOrWhiteSpace(textureStr)) return CardTextureType.Default;

        textureStr = textureStr.Trim();
        switch (textureStr.ToLower())
        {
            case "默认质感":
            case "default":
                return CardTextureType.Default;
            case "肉质感":
            case "flesh":
                return CardTextureType.Flesh;
            case "金属质感":
            case "metal":
                return CardTextureType.Metal;
            case "液体质感":
            case "liquid":
                return CardTextureType.Liquid;
            default:
                Debug.LogWarning($"未知的卡牌质感: {textureStr}，使用默认质感");
                return CardTextureType.Default;
        }
    }

    #region 掉落列表
    public static DropList ReadDisposableDropListConfig(PlaceEnum placeType)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/DisposableDropListConfig.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet dataSet = excelReader.AsDataSet();

        if (!dataSet.Tables.Contains(placeType.ToString())) return new();

        var table = dataSet.Tables[placeType.ToString()];

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

        return new(dropList, true);
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

    public static DeepExploreDropList ReadDeepExploreDropListConfig(PlaceEnum placeType)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/DeepExploreDropListConfig.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet dataSet = excelReader.AsDataSet();

        if (!dataSet.Tables.Contains(placeType.ToString())) return new();

        var table = dataSet.Tables[placeType.ToString()];

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

        return new()
        {
            emptyPopulation = emptyPopulation,
            emptyPopulationSizeChangeOnNotCaught = sizeChangeOnNotCaught,
            populationList = populationList
        };
    }
    #endregion

    #region 读取加工表配置
    public static List<ProcessConfig> ReadProcessConfig(string filename)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.streamingAssetsPath + $"/Excel/{filename}.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet dataSet = excelReader.AsDataSet();
        DataTable table = dataSet.Tables[0]; // 配置在第一张表中
        
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
        DataSet dataSet = excelReader.AsDataSet();
        DataTable table = dataSet.Tables[0]; // 配置在第一张表中
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
        DataSet dataSet = excelReader.AsDataSet();
        DataTable table = dataSet.Tables[0]; // 配置在第一张表中
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
