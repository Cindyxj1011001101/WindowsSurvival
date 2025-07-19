using Excel;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

public static class ExcelReader
{
    public static Dictionary<string, CardConfig> ReadCardConfig(string fileName)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.dataPath + $"/Excel/{fileName}.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet result = excelReader.AsDataSet();
        DataTable table = result.Tables[0]; // 假设配置在第一张表中

        // 存储卡牌配置的字典
        Dictionary<string, CardConfig> cardConfigs = new();

        DataRow row;
        for (int i = 1; i < table.Rows.Count; i++) // 从1开始跳过表头
        {
            row = table.Rows[i];
            if (string.IsNullOrEmpty(row[0].ToString())) break; // 如果卡牌ID为空，跳出循环
            CardConfig cardConfig = new()
            {
                CardId = row[0].ToString(),
                CardName = row[1].ToString(),
                CardDesc = row[2].ToString(),
                CardImagePath = row[3].ToString(),
                CardType = ParseCardType(row[4].ToString()),
                MaxStackCount = int.Parse(row[5].ToString()),
                Moveable = bool.Parse(row[6].ToString()),
                Weight = float.Parse(row[7].ToString()),
                Tags = ParseTags(row[8].ToString()),
                HasFreshness = bool.Parse(row[9].ToString()),
                HasDurability = bool.Parse(row[11].ToString()),
                HasGrowth = bool.Parse(row[13].ToString()),
                HasProgress = bool.Parse(row[15].ToString()),
                IsEquipment = bool.Parse(row[17].ToString()),
                IsTool = bool.Parse(row[19].ToString()),
            };
            if (cardConfig.HasFreshness)
            {
                cardConfig.MaxFreshness = int.Parse(row[10].ToString());
            }
            if (cardConfig.HasDurability)
            {
                cardConfig.MaxDurability = int.Parse(row[12].ToString());
            }
            if (cardConfig.HasGrowth)
            {
                cardConfig.MaxGrowth = int.Parse(row[14].ToString());
            }
            if (cardConfig.HasProgress)
            {
                cardConfig.MaxProgress = int.Parse(row[16].ToString());
            }
            if (cardConfig.IsEquipment)
            {
                cardConfig.EquipmentType = ParseEquipmentType(row[18].ToString());
            }
            if (cardConfig.IsTool)
            {
                cardConfig.ToolTypes = ParseToolTypes(row[20].ToString());
            }
            Debug.Log($"读取卡牌配置: {cardConfig.CardName}");
            cardConfigs.Add(cardConfig.CardId, cardConfig);
        }

        fs.Close();

        return cardConfigs;
    }

    private static CardType ParseCardType(string typeStr)
    {
        return (CardType)System.Enum.Parse(typeof(CardType), typeStr);
    }

    private static List<CardTag> ParseTags(string tagsStr)
    {
        var tags = new List<CardTag>();
        if (string.IsNullOrEmpty(tagsStr)) return tags;

        var tagArray = tagsStr.Split(',');
        foreach (var tag in tagArray)
        {
            tags.Add((CardTag)System.Enum.Parse(typeof(CardTag), tag.Trim()));
        }

        return tags;
    }

    private static EquipmentType ParseEquipmentType(string typeStr)
    {
        return (EquipmentType)System.Enum.Parse(typeof(EquipmentType), typeStr);
    }

    private static List<ToolType> ParseToolTypes(string toolTypesStr)
    {
        var toolTypes = new List<ToolType>();
        if (string.IsNullOrEmpty(toolTypesStr)) return toolTypes;
        var toolTypeArray = toolTypesStr.Split(',');
        foreach (var toolType in toolTypeArray)
        {
            toolTypes.Add((ToolType)System.Enum.Parse(typeof(ToolType), toolType.Trim()));
        }
        return toolTypes;
    }

    public static void GenerateDisposableDropListJson(string fileName)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.dataPath + $"/Excel/{fileName}.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet result = excelReader.AsDataSet();

        foreach (DataTable table in result.Tables)
        {
            // 假设每个表都是一次性掉落列表
            List<Drop> dropList = new();
            DataRow row;
            for (int i = 1; i < table.Rows.Count; i++) // 从1开始跳过表头
            {
                row = table.Rows[i];
                // 读取掉落配置
                DropConfig config = new()
                {
                    CardId = row[0].ToString(),
                    DropNum = int.Parse(row[1].ToString()),
                    DropProb = int.Parse(row[2].ToString()),
                    OverwriteFreshness = bool.Parse(row[3].ToString()),
                    OverwriteDurability = bool.Parse(row[5].ToString()),
                    OverwriteGrowth = bool.Parse(row[7].ToString()),
                    OverwriteProgress = bool.Parse(row[9].ToString())
                };
                // 创建卡牌实例
                var card = CardFactory.CreateCard(config.CardId);
                // 覆写卡牌属性
                if (config.OverwriteFreshness)
                {
                    config.freshness = int.Parse(row[4].ToString());
                    if (card.TryGetComponent<FreshnessComponent>(out var freshnessComponent))
                        freshnessComponent.freshness = config.freshness; // 设置新鲜度
                }
                if (config.OverwriteDurability)
                {
                    config.Durability = int.Parse(row[6].ToString());
                    if (card.TryGetComponent<DurabilityComponent>(out var durabilityComponent))
                        durabilityComponent.durability = config.Durability; // 设置耐久度
                }
                if (config.OverwriteGrowth)
                {
                    config.Growth = int.Parse(row[8].ToString());
                    if (card.TryGetComponent<GrowthComponent>(out var growthComponent))
                        growthComponent.growth = config.Growth; // 设置生长进度
                }
                if (config.OverwriteProgress)
                {
                    config.Progress = int.Parse(row[10].ToString());
                    if (card.TryGetComponent<ProgressComponent>(out var progressComponent))
                        progressComponent.progress = config.Progress; // 设置产物进度
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
            JsonManager.SaveData(disposableDropList, table.TableName + "一次性掉落列表");
        }
        Debug.Log("Disposable drop list generated successfully!");
    }

    public static void GenerateRepeatableDropListJson(string fileName)
    {
        // 打开Excel文件
        using FileStream fs = File.Open(Application.dataPath + $"/Excel/{fileName}.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet result = excelReader.AsDataSet();


        foreach (DataTable table in result.Tables)
        {
            DataRow emptyPopulationConfig = table.Rows[1]; // 假设第一行是空种群配置

            Population emptyPopulation = new()
            {
                curSize = int.Parse(emptyPopulationConfig[2].ToString()),
                maxSize = int.Parse(emptyPopulationConfig[3].ToString()),
                sizeChangePerRound = int.Parse(emptyPopulationConfig[4].ToString()),
            };
            int sizeChangeOnNotCaught = int.Parse(emptyPopulationConfig[6].ToString());

            // 假设每个表都是重复掉落列表
            List <Population> populationList = new();
            DataRow row;
            for (int i = 2; i < table.Rows.Count; i++) // 从2开始跳过表头个空种群配置
            {
                row = table.Rows[i];
                // 读取掉落配置
                PopulationConfig config = new()
                {
                    CardId = row[0].ToString(),
                    DropNum = int.Parse(row[1].ToString()),
                    Size = int.Parse(row[2].ToString()),
                    MaxSize = int.Parse(row[3].ToString()),
                    SizeChangePerRound = int.Parse(row[4].ToString()),
                    SizeChangeOnCaught = int.Parse(row[5].ToString()),
                    OverwriteFreshness = bool.Parse(row[7].ToString()),
                    OverwriteDurability = bool.Parse(row[9].ToString()),
                    OverwriteGrowth = bool.Parse(row[11].ToString()),
                    OverwriteProgress = bool.Parse(row[13].ToString())
                };
                // 创建卡牌实例
                var card = CardFactory.CreateCard(config.CardId);
                // 覆写卡牌属性
                if (config.OverwriteFreshness)
                {
                    config.freshness = int.Parse(row[8].ToString());
                    if (card.TryGetComponent<FreshnessComponent>(out var freshnessComponent))
                        freshnessComponent.freshness = config.freshness; // 设置新鲜度
                }
                if (config.OverwriteDurability)
                {
                    config.Durability = int.Parse(row[10].ToString());
                    if (card.TryGetComponent<DurabilityComponent>(out var durabilityComponent))
                        durabilityComponent.durability = config.Durability; // 设置耐久度
                }
                if (config.OverwriteGrowth)
                {
                    config.Growth = int.Parse(row[12].ToString());
                    if (card.TryGetComponent<GrowthComponent>(out var growthComponent))
                        growthComponent.growth = config.Growth; // 设置生长进度
                }
                if (config.OverwriteProgress)
                {
                    config.Progress = int.Parse(row[14].ToString());
                    if (card.TryGetComponent<ProgressComponent>(out var progressComponent))
                        progressComponent.progress = config.Progress; // 设置产物进度
                }
                // 添加到掉落列表
                populationList.Add(new Population()
                {
                    card = card,
                    dropNum = config.DropNum,
                    curSize = config.Size,
                    maxSize = config.MaxSize,
                    sizeChangePerRound = config.SizeChangePerRound,
                    sizeChangeOnCaught = config.SizeChangeOnCaught
                });
            }
            // 保存为Json
            RepeatableDropList repeatableDropList = new()
            {
                emptyPopulation = emptyPopulation,
                emptyPopulationSizeChangeOnNotCaught = sizeChangeOnNotCaught,
                populationList = populationList
            };
            JsonManager.SaveData(repeatableDropList, table.TableName + "重复掉落列表");
        }
        Debug.Log("Repeatable drop list generated successfully!");
    }
}
public class CardConfig
{
    public string CardId; // 卡牌ID
    public string CardName; // 卡牌名称
    public string CardDesc; // 卡牌描述
    public string CardImagePath; // 卡牌图片路径
    public CardType CardType; // 卡牌类型
    public int MaxStackCount; // 最大堆叠数
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
}

public class DropConfig
{
    public string CardId; // 卡牌
    public int DropNum; // 掉落数量
    public int DropProb;
    public bool OverwriteFreshness; // 是否覆盖新鲜度
    public int freshness; // 新鲜度
    public bool OverwriteDurability; // 是否覆盖耐久度
    public int Durability; // 耐久度
    public bool OverwriteGrowth;
    public int Growth; // 生长进度
    public bool OverwriteProgress; // 是否覆盖产物进度
    public int Progress;
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
    public int freshness; // 新鲜度
    public bool OverwriteDurability; // 是否覆盖耐久度
    public int Durability; // 耐久度
    public bool OverwriteGrowth;
    public int Growth; // 生长进度
    public bool OverwriteProgress; // 是否覆盖产物进度
    public int Progress;
}