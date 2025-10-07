using System;
using System.Collections.Generic;
using System.Data;

public enum TemperatureType
{
    Normal, // 常温
    Low,    // 低温
    Medium, // 中温
    High,   // 高温
}

public enum CalculateType
{
    Greater, // 大于
    Less,    // 小于
    Equal,   // 等于
}

public class ValueRequirement
{
    public int targetValue; // 需求值
    public CalculateType calculateType; // 计算类型

    public ValueRequirement(int targetValue, CalculateType calculateType)
    {
        this.targetValue = targetValue;
        this.calculateType = calculateType;
    }

    public bool IsMet(int value)
    {
        return calculateType switch
        {
            CalculateType.Greater => value >= targetValue,
            CalculateType.Less => value <= targetValue,
            CalculateType.Equal => value == targetValue,
            _ => throw new Exception($"无效的计算类型: {calculateType}")
        };
    }
}

public class CardRequirement : ValueRequirement
{
    public List<string> requiredCardIdList; // 需求卡牌列表

    public CardRequirement(List<string> requiredCardIdList, int targetValue, CalculateType calculateType)
        : base(targetValue, calculateType)
    {
        this.requiredCardIdList = requiredCardIdList;
    }
}

public class FoodPropertyRequirement : ValueRequirement
{
    public FoodProperty foodProperty; // 食物属性

    public FoodPropertyRequirement(FoodProperty foodProperty, int targetValue, CalculateType calculateType)
        : base(targetValue, calculateType)
    {
        this.foodProperty = foodProperty;
    }
}

public class ProcessConfig
{
    public string OutcomeID; // 产出卡牌ID
    public int Priority; // 优先级
    public List<TemperatureType> TempertureRequirementList = new(); // 温度需求
    public ValueRequirement RoundRequirement; // 回合数需求
    public List<FoodPropertyRequirement> FoodPropertyRequirementList = new(); // 食物属性需求
    public List<CardRequirement> CardRequirementList = new(); // 卡牌需求

    public static ProcessConfig Parse(DataRow row)
    {
        return new ProcessConfig()
        {
            OutcomeID = row[0].ToString(),
            Priority = ExcelReader.ParseInt(row[1].ToString()),
            TempertureRequirementList = ParseTemperatureRequirementList(row[2].ToString()),
            RoundRequirement = ParseRoundRequirement(row[3].ToString()),
            FoodPropertyRequirementList = ParseFoodPropertyRequirementList(row),
            CardRequirementList = ParseCardRequirementList(row)
        };
    }

    private static List<TemperatureType> ParseTemperatureRequirementList(string s)
    {
        var result = new List<TemperatureType>();
        if (string.IsNullOrEmpty(s) || s == "/")
        {
            return result;
        }

        foreach (string tempStr in s.Split('+'))
        {
            TemperatureType tempertureType = tempStr switch
            {
                "常温" => TemperatureType.Normal,
                "低温" => TemperatureType.Low,
                "中温" => TemperatureType.Medium,
                "高温" => TemperatureType.High,
                _ => throw new Exception($"无效的温度类型: {tempStr}")
            };
            result.Add(tempertureType);
        }

        return result;
    }

    private static (int value, CalculateType type) ParseValueRequirement(string s)
    {
        if (string.IsNullOrEmpty(s) || s == "/")
        {
            return (0, CalculateType.Greater);
        }

        CalculateType calculateType = s[..2] switch
        {
            ">=" => CalculateType.Greater,
            "<=" => CalculateType.Less,
            "==" => CalculateType.Equal,
            _ => throw new Exception($"无效的计算类型: {s[..2]}")
        };
        return (ExcelReader.ParseInt(s[2..]), calculateType);
    }

    private static ValueRequirement ParseRoundRequirement(string s)
    {
        (int value, CalculateType type) = ParseValueRequirement(s);
        return new ValueRequirement(value, type);
    }

    private static List<FoodPropertyRequirement> ParseFoodPropertyRequirementList(DataRow row)
    {
        var result = new List<FoodPropertyRequirement>();
        for (int i = 4; i <= 12; i++)
        {
            var s = row[i].ToString();
            if (string.IsNullOrEmpty(s) || s == "/") continue;

            (int value, CalculateType calculateType) = ParseValueRequirement(s);
            result.Add(new((FoodProperty)(i - 4), value, calculateType));
        }
        return result;
    }

    private static CardRequirement ParseCardRequirement(string cardIds, string cardNum)
    {
        if (string.IsNullOrEmpty(cardIds) || cardIds == "/") return null;

        var cardIdList = new List<string>(cardIds.Split('+'));

        (int value, CalculateType type) = ParseValueRequirement(cardNum);
        return new CardRequirement(cardIdList, value, type);
    }

    private static List<CardRequirement> ParseCardRequirementList(DataRow row)
    {
        var result = new List<CardRequirement>();

        var startColIndex = 13;
        for (int i = 0; i < 6; i += 2)
        {
            var cardIds = row[startColIndex + i].ToString();
            var cardNum = row[startColIndex + i + 1].ToString();

            var cardRequirement = ParseCardRequirement(cardIds, cardNum);
            if (cardRequirement != null)
            {
                result.Add(cardRequirement);
            }
        }

        return result;
    }
}