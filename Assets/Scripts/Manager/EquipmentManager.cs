using UnityEngine;


public class EquipmentManager : MonoBehaviour
{
   private static EquipmentManager instance;
   public static EquipmentManager Instance => instance;

   [Header("装备")]
   //头部
   public Card HeadEquipment;
   //身体
   public Card BodyEquipment;
   //背部
   public Card BackEquipment;
   //腿部
   public Card LegEquipment;

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
   /// <returns>是否可以装备</returns>
   public bool CanEquipCard(Card card)
   {
       if(card.TryGetComponent<EquipmentComponent>(out var equipmentComponent))
       {
            return equipmentComponent.equipmentType == EquipmentType.Head ? HeadEquipment == null :
                   equipmentComponent.equipmentType == EquipmentType.Body ? BodyEquipment == null :
                   equipmentComponent.equipmentType == EquipmentType.Back ? BackEquipment == null :
                   equipmentComponent.equipmentType == EquipmentType.Leg ? LegEquipment == null : false;
       }
       return false;
   }
   #endregion

   #region 装备卡牌
   /// <summary>
   /// 装备卡牌
   /// </summary>
   public void EquipCard(Card equipment)
   {
        if(CanEquipCard(equipment))
        {
            equipment.TryGetComponent<EquipmentComponent>(out var equipmentComponent);
            switch(equipmentComponent.equipmentType)
            {
                case EquipmentType.Head:
                    HeadEquipment = equipment;
                    break;
                case EquipmentType.Body:
                    BodyEquipment = equipment;
                    break;
                case EquipmentType.Back:
                    BackEquipment = equipment;
                    break;
                case EquipmentType.Leg:
                    LegEquipment = equipment;
                    break;
            }
        }
   }
   #endregion

   #region 卸下卡牌
   /// <summary>
   /// 卸下卡牌
   /// </summary>
   /// <param name="equipment">卸下的卡牌</param>
   public void UnequipCard(Card equipment)
   {
        if(equipment.TryGetComponent<EquipmentComponent>(out var equipmentComponent))
        {
            switch(equipmentComponent.equipmentType)
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
    }
   #endregion
}
