using UnityEngine;

/// <summary>
/// 卡牌工厂，用于创建卡牌的实例
/// </summary>
public static class CardFactory
{
    public static T CreateCardInstance<T>(string cardName) where T : CardInstance, new()
    {
        string dataPath = "ScriptableObject/Card/" + cardName;

        // 1. 加载ScriptableObject默认数据
        CardData defaultData = Resources.Load<CardData>(dataPath);
        if (defaultData == null)
        {
            Debug.LogError($"Failed to load ObjectAData at path: {dataPath}");
            return null;
        }

        // 2. 创建运行时数据实例
        T instance = new T { dataPath = dataPath };
        instance.InitFromCardData(defaultData);

        return instance;
    }

    public static CardInstance CreateCardIntance(CardData cardData)
    {
        string cardName = cardData.cardName;
        switch (cardData)
        {
            case FoodCardData:
                return CreateCardInstance<FoodCardInstance>(cardName);
            case PlaceCardData:
                return CreateCardInstance<PlaceCardInstance>(cardName);
            case ResourceCardData:
                return CreateCardInstance<ResourceCardInstance>(cardName);
            case ResourcePointCardData:
                return CreateCardInstance<ResourcePointCardInstance>(cardName);
            case ToolCardData:
                return CreateCardInstance<ToolCardInstance>(cardName);
            default:
                break;
        }
        return null;
    }
}