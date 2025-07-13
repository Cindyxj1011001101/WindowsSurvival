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
        EventManager.Instance.AddListener<EquipCardArgs>(EventType.EquipCard, EquipCard);
        EventManager.Instance.AddListener<EquipmentType>(EventType.UnequipCard, UnequipCard);
        Init();
    }
    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<EquipCardArgs>(EventType.EquipCard, EquipCard);
        EventManager.Instance.RemoveListener<EquipmentType>(EventType.UnequipCard, UnequipCard);
    }
    public void Init()
    {
        HeadEquipment = null;
        BodyEquipment = null;
        BackEquipment = null;
        LegEquipment = null;
    }
    #endregion

    #region 装备卡牌
    /// <summary>
    /// 装备卡牌
    /// </summary>
    /// <param name="type">装备的位置</param>
    /// <param name="card">装备的卡牌</param>
    /// 是否需要判断能否装备对应卡牌到对应为止？-卡牌标签中增加对应可装备位置的标签
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