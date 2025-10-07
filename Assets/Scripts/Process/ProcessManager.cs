using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public enum TempertureType
{
    Normal,//常温
    Low,//低温
    Medium,//中温
    High,//高温
}
public enum FoodProperty
{
    EatableDegree,     // 可食用度
    UneatableDegree,   // 不可食用度
    Meatiness,         // 肉度
    Fishiness,         // 鱼度
    Shellfishiness,    // 贝度
    Wateriness,        // 水度
    Vegetableness,     // 菜度
    Fruitiness,        // 果度
    FoulSmellingDegree // 恶臭度
}
public enum CalculateType
{
    Greater,//大于
    Less,//小于
    Equal,//等于
}
public class TempertureData
{
    public TempertureType TempertureType;//温度类型
    public int round;//回合数
    public TempertureData(TempertureType tempertureType, int round)
    {
        TempertureType = tempertureType;
        this.round = round;
    }
}
public class ProcessCardData
{
    public List<string> CardIdList;//卡牌列表（或卡牌ID）
    public CalculateType calculateType;
    public int CardCount;//卡牌数量
    public ProcessCardData(List<string> cardIdList, CalculateType calculateType, int cardCount)
    {
        CardIdList = cardIdList;
        this.calculateType = calculateType;
        CardCount = cardCount;
    }
}

public class RoundCalculate
{
    public int value;
    public CalculateType CalculateType;
    public RoundCalculate(int value, CalculateType calculateType)
    {
        this.value = value;
        CalculateType = calculateType;
    }
}
public class FoodPropertyCalculate
{
    public FoodProperty FoodProperty;//食物属性
    public CalculateType CalculateType;//计算类型
    public int Value;//值
    public FoodPropertyCalculate(FoodProperty foodProperty, CalculateType calculateType, int value)
    {
        FoodProperty = foodProperty;
        CalculateType = calculateType;
        Value = value;
    }
}
public class ProcessData
{
    public string OutcomeID;//产出卡牌ID
    public int Priority;//优先级
    public List<TempertureType> TempertureTypeList=new List<TempertureType>();//温度类型列表
    public RoundCalculate Round;//回合数
    public List<FoodPropertyCalculate> FoodPropertyCalculateList=new List<FoodPropertyCalculate>();//食物属性计算列表
    public List<ProcessCardData> CardDataList=new List<ProcessCardData>();//卡牌数据列表
    public ProcessData(DataRow row)
    {
        //产出卡牌ID
        OutcomeID = row[0].ToString();
        //优先级
        Priority = int.Parse(row[1].ToString());
        //温度类型列表
        if (row[2].ToString() == "/")
        {
            TempertureTypeList = new List<TempertureType>();
        }
        else
        {
            foreach (string tempertureType in row[2].ToString().Split('+'))
            {
                TempertureType TempertureType = tempertureType switch
                {
                    "常温" => TempertureType.Normal,
                    "低温" => TempertureType.Low,
                    "中温" => TempertureType.Medium,
                    "高温" => TempertureType.High,
                    _ => throw new Exception($"无效的温度类型: {row[2].ToString()}")
                };
                TempertureTypeList.Add(TempertureType);
            }
        }

        //回合数
        if (row[3].ToString() == "/")
        {
            Round = new RoundCalculate(0, CalculateType.Greater);
        }
        else
        {
            CalculateType calculateType = row[3].ToString().Substring(0, 2) switch
            {
                ">=" => CalculateType.Greater,
                "<=" => CalculateType.Less,
                "==" => CalculateType.Equal,
                _ => throw new Exception($"无效的计算类型: {row[3].ToString().Substring(0, 2)}")
            };
            Round = new RoundCalculate(int.Parse(row[3].ToString().Substring(2)), calculateType);
        }
        //食物属性计算列表
        for (int i = 4; i < 12; i++)
        {
            if (row[i].ToString() == "/") continue;
            CalculateType calculateType = row[i].ToString().Substring(0, 2) switch
            {
                ">=" => CalculateType.Greater,
                "<=" => CalculateType.Less,
                "==" => CalculateType.Equal,
                _ => throw new Exception($"无效的计算类型: {row[i].ToString().Substring(0, 2)}")
            };
            FoodPropertyCalculateList.Add(new FoodPropertyCalculate((FoodProperty)i - 4, calculateType, int.Parse(row[i].ToString().Substring(2))));
        }
        //卡牌数据列表
        CardDataList.Clear();
        if (row[12].ToString()!= "/")
        {
            CalculateType calculateType = row[13].ToString().Substring(0, 2) switch
            {
                ">=" => CalculateType.Greater,
                "<=" => CalculateType.Less,
                "==" => CalculateType.Equal,
                _ => throw new Exception($"无效的计算类型: {row[13].ToString().Substring(0, 2)}")
            };
            CardDataList.Add(new ProcessCardData(row[12].ToString().Split('+').ToList(),calculateType, int.Parse(row[13].ToString().Substring(2))));
        }
        if (row[14].ToString()!= "/")
        {
            CalculateType calculateType = row[15].ToString().Substring(0, 2) switch
            {
                ">=" => CalculateType.Greater,
                "<=" => CalculateType.Less,
                "==" => CalculateType.Equal,
                _ => throw new Exception($"无效的计算类型: {row[15].ToString().Substring(0, 2)}")
            };
            CardDataList.Add(new ProcessCardData(row[14].ToString().Split('+').ToList(),calculateType, int.Parse(row[15].ToString().Substring(2))));
        }
        if (row[16].ToString()!= "/")
        {
            CalculateType calculateType = row[17].ToString().Substring(0, 2) switch
            {
                ">=" => CalculateType.Greater,
                "<=" => CalculateType.Less,
                "==" => CalculateType.Equal,
                _ => throw new Exception($"无效的计算类型: {row[17].ToString().Substring(0, 2)}")
            };
            CardDataList.Add(new ProcessCardData(row[16].ToString().Split('+').ToList(),calculateType, int.Parse(row[17].ToString().Substring(2))));
        }
    }
}
public static class ProcessManager
{
    public static List<ProcessData> ProcessDataList;

    static ProcessManager()
    {
        ProcessDataList = ExcelReader.ReadProcess("ProcessData");
    }

    public static string GetProcessOutcomeID(List<Card> cards,List<TempertureData> TemptureDatas)
    {
        return FindProcessByPriority(FindProcessByCardsAndTemperture(cards, TemptureDatas)).OutcomeID;
    }
    //根据传入卡牌判断可加工配方
    public static List<ProcessData> FindProcessByCards(List<Card> cardList)
    {
        List<ProcessData> result = new List<ProcessData>();
        foreach (ProcessData processData in ProcessDataList)
        {
            bool isMatch = true;
            foreach (FoodPropertyCalculate foodPropertyCalculate in processData.FoodPropertyCalculateList)
            {
                if (!IsFoodPropertyMatch(foodPropertyCalculate, cardList))
                {
                    isMatch = false;
                    break;
                }
            }
            foreach (ProcessCardData processCardData in processData.CardDataList)
            {
                if (!IsCardMatch(processCardData, cardList))
                {
                    isMatch = false;
                    break;
                }
            }
            if (isMatch)
            {
                result.Add(processData);
            }
        }
        return result;
    }
    //根据传入卡牌与各温度回合数判断可加工配方
    public static List<ProcessData> FindProcessByCardsAndTemperture(List<Card> cardIdList, List<TempertureData> tempertureDataList)
    {
        List<ProcessData> result = new List<ProcessData>();
        result = FindProcessByCards(cardIdList);
        List<ProcessData> filteredResult = new List<ProcessData>();
        foreach (ProcessData processData in result)
        {
            if (IsTempertureMatch(processData, tempertureDataList))
            {
                filteredResult.Add(processData);
            }
        }
        result = filteredResult;
        return result;
    }
    //根据配方列表的优先度判断结算的加工
    public static ProcessData FindProcessByPriority(List<ProcessData> processDataList)
    {
        processDataList.Sort((x, y) => y.Priority.CompareTo(x.Priority));
        int MaxPriority=processDataList[0].Priority;
        List<ProcessData> resList = new List<ProcessData>();
        foreach (var data in processDataList)
        {
            if (data.Priority == MaxPriority)
            {
                resList.Add(data);
            }
            else
            {
                break;
            }
        }

        return resList[Random.Range(0,resList.Count)];

    }
    //判断食物属性是否符合条件
    public static bool IsFoodPropertyMatch(FoodPropertyCalculate foodPropertyCalculate, List<Card> cardList)
    {
        int foodPropertyValue = 0;
        foreach (Card card in cardList)
        {
            card.TryGetComponent<FoodPropertyComponent>(out FoodPropertyComponent foodPropertyComponent);
            foodPropertyComponent.foodPropertyDict.TryGetValue(foodPropertyCalculate.FoodProperty, out int value);
            foodPropertyValue += value;
        }
        return PassCalculate(foodPropertyCalculate, foodPropertyValue);
    }
    //判断卡牌是否符合条件
    public static bool IsCardMatch(ProcessCardData CardDataList, List<Card> cardList)
    {
        int cardCount = 0;
        foreach (string cardId in CardDataList.CardIdList)
        {
            cardCount += cardList.Count(card => card.CardId == cardId);
        }
        return cardCount >= CardDataList.CardCount;
    }
    //判断是否通过食物属性判断
    public static bool PassCalculate(FoodPropertyCalculate foodPropertyCalculate, int value)
    {
        switch (foodPropertyCalculate.CalculateType)
        {
            case CalculateType.Greater:
                return value >= foodPropertyCalculate.Value;
            case CalculateType.Less:
                return value <= foodPropertyCalculate.Value;
            case CalculateType.Equal:
                return value == foodPropertyCalculate.Value;
            default:
                Debug.LogError($"无效的计算类型: {foodPropertyCalculate.CalculateType}");
                return default;
        }
    }
    //判断是否通过温度回合数判断
    public static bool IsTempertureMatch(ProcessData processData, List<TempertureData> tempertureDataList)
    {
        int round = 0;
        foreach (TempertureData tempertureData in tempertureDataList)
        {
            if (processData.TempertureTypeList.Contains(tempertureData.TempertureType))
            {
                round += tempertureData.round;
            }
        }
        switch (processData.Round.CalculateType)
        {
            case CalculateType.Greater:
                return round >= processData.Round.value;
            case CalculateType.Less:
                return round <= processData.Round.value;
            case CalculateType.Equal:
                return round == processData.Round.value;
            default:
                Debug.LogError($"无效的计算类型: {processData.Round.CalculateType}");
                return false;
        }
    }
}
