using UnityEngine;

public enum EquipmentType
{
    Head,
    Body,
    Back,
    Leg,
}

public class EquipmentManager : MonoBehaviour
{
    private static EquipmentManager instance;
    public static EquipmentManager Instance => instance;

    [Header("装备")]
    //头部
    public CardInstance HeadEquipment;
    //身体
    public CardInstance BodyEquipment;
    //背部
    public CardInstance BackEquipment;
    //腿部
    public CardInstance LegEquipment;

    #region 初始化
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        Init();
    }
    public void Init()
    {
        HeadEquipment = null;
        BodyEquipment = null;
        BackEquipment = null;
        LegEquipment = null;
    }
    #endregion

    #region 判断是否可以装备
    /// <summary>
    /// 判断是否可以装备
    /// 卡牌tag中需包含对应装备位置的tag
    /// </summary>
    /// <param name="args">装备卡牌参数(卡牌，装备位置)</param>
    /// <returns>是否可以装备</returns>
    public bool CanEquipCard(EquipCardArgs args)
    {
        if (args.card.CardData.cardType != CardType.Equipment)
        {
            return false;
        }
        EquipmentCardData equipmentCardData = args.card.CardData as EquipmentCardData;
        if (equipmentCardData.equipmentType == args.type)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region 装备卡牌
    /// <summary>
    /// 装备卡牌
    /// </summary>
    /// <param name="type">装备的位置</param>
    /// <param name="card">装备的卡牌</param>
    public void EquipCard(EquipCardArgs args)
    {
        switch (args.type)  
        {
            case EquipmentType.Head:
                HeadEquipment = args.card;
                break;
            case EquipmentType.Body:
                BodyEquipment = args.card;
                break;
            case EquipmentType.Back:
                BackEquipment = args.card;
                break;
            case EquipmentType.Leg:
                LegEquipment = args.card;
                break;
        }
    }
    #endregion

    #region 卸下卡牌
    /// <summary>
    /// 卸下卡牌
    /// </summary>
    /// <param name="type">卸下的位置</param>
    public void UnequipCard(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Head:
                HeadEquipment = null;
                break;
            case EquipmentType.Body:
                BodyEquipment = null;
                break;
            case EquipmentType.Back:
                BackEquipment = null;
                break;
            case EquipmentType.Leg:
                LegEquipment = null;
                break;
        }
    }
    #endregion
}