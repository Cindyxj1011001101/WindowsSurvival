using System;

/// <summary>
/// 用于标记卡牌类对应的卡牌ID，CardFactory会自动扫描并建立映射
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CardIdAttribute : Attribute
{
    public string CardId { get; }

    public CardIdAttribute(string cardId)
    {
        CardId = cardId;
    }
}
