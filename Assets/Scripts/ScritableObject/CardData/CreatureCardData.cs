using UnityEngine;

/// <summary>
/// 生物卡
/// 新鲜度
/// 生长度
/// 产物进度
/// 最大产物数量
/// </summary>
[CreateAssetMenu(fileName = "CreatureCardData", menuName = "ScritableObject/CreatureCardData")]
public class CreatureCardData : CardData
{
    public int MaxFresh;
    public float MaxProductProcedure;
    public int MaxProductAmount;
}