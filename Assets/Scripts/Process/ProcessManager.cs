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
    EatableDegree,//可食用度
    UneatableDegree,//不可食用度
    Meatiness,//肉度
    Fishiness,//鱼度
    Shellfishiness,//贝度
    Wateriness,//水度
    Vegetableness,//菜度
    Fruitiness,//果度
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
    public int CardCount;//卡牌数量
    public ProcessCardData(List<string> cardIdList, int cardCount)
    {
        CardIdList = cardIdList;
        CardCount = cardCount;
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
    public List<TempertureType> TempertureTypeList;//温度类型列表
    public int Round;//回合数
    public List<FoodPropertyCalculate> FoodPropertyCalculateList;//食物属性计算列表
    public List<ProcessCardData> CardDataList;//卡牌数据列表
    public ProcessData(DataRow row)
    {
        //产出卡牌ID
        OutcomeID = row[0].ToString();
        //优先级
        Priority = int.Parse(row[1].ToString());
        //温度类型列表
        foreach (string tempertureType in row[2].ToString().Split(','))
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
        //回合数
        Round = int.Parse(row[3].ToString());
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
        for (int i = 12; i < row.ItemArray.Length; i++)
        {
            string data = row[i].ToString();
            try
            {
                // 分离卡牌列表和数量
                string[] parts = data.Split('*');
                if (parts.Length != 2)
                {
                    Debug.LogError("数据格式错误: " + data);
                    return;
                }

                // 解析卡牌ID列表
                string cardListPart = parts[0].Trim('(', ')');
                string[] cardIds = cardListPart.Split(',');

                // 解析数量
                if (!int.TryParse(parts[1], out int count))
                {
                    Debug.LogError("数量解析失败: " + parts[1]);
                    return;
                }

                // 赋值
                CardDataList.Add(new ProcessCardData(new List<string>(cardIds), count));
            }
            catch (Exception e)
            {
                Debug.LogError($"解析数据失败: {data}, 错误: {e.Message}");
            }
        }
    }
}
public static class ProcessManager
{
    public static List<ProcessData> ProcessDataList;

    static ProcessManager()
    {
        ProcessDataList = ExcelReader.ReadProcess("Process");
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
        foreach (ProcessData processData in result)
        {
            if (!IsTempertureMatch(processData, tempertureDataList))
            {
                result.Remove(processData);
            }
        }
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
                return value > foodPropertyCalculate.Value;
            case CalculateType.Less:
                return value < foodPropertyCalculate.Value;
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
        return round >= processData.Round;
    }
}
