using UnityEngine;

public enum EquipmentType
{
   Head = 0,
   Body = 1,
   Back = 2,
   Leg = 3,
}

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
   public bool CanEquipCard(CardInstance card)
   {
       if (card is not EquipmentCardInstance) return false;
       EquipmentCardData data = card.CardData as EquipmentCardData;
       return data.equipmentType switch
       {
           EquipmentType.Head => HeadEquipment == null,
           EquipmentType.Body => BodyEquipment == null,
           EquipmentType.Back => BackEquipment == null,
           EquipmentType.Leg => LegEquipment == null,
           _ => false,
       };
   }
   #endregion

   #region 装备卡牌
   /// <summary>
   /// 装备卡牌
   /// </summary>
   public void EquipCard(Card equipment)
   {
        if(equipment.cardType!=CardType.Equipment)
        {
            return;
        }
       if(equipment.tags.Contains(CardTag.Head))
       {
            HeadEquipment = equipment;
       }
       else if(equipment.tags.Contains(CardTag.Body))
       {
            BodyEquipment = equipment;
       }
       else if(equipment.tags.Contains(CardTag.Back))
       {
            BackEquipment = equipment;
       }
       else if(equipment.tags.Contains(CardTag.Leg))
       {
            LegEquipment = equipment;
       }
   }
   #endregion

   #region 卸下卡牌
   /// <summary>
   /// 卸下卡牌
   /// </summary>
   /// <param name="type">卸下的位置</param>
   public void UnequipCard(Card equipment)
   {
       if(equipment.cardType!=CardType.Equipment)
        {
            return;
        }
       if(equipment.tags.Contains(CardTag.Head))
       {
            HeadEquipment = null;
       }
       else if(equipment.tags.Contains(CardTag.Body))
       {
            BodyEquipment = null;
       }
       else if(equipment.tags.Contains(CardTag.Back))
       {
            BackEquipment = null;
       }
       else if(equipment.tags.Contains(CardTag.Leg))
       {
            LegEquipment = null;
       }
   }
   #endregion
}