using UnityEngine;

public class EquipmentWindow : WindowBase
{
    private GameObject bodyEquipment;
    private GameObject headEquipment;
    private GameObject legEquipment;
    private GameObject backEquipment;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Init()
    {
        //获取装备位置卡槽
        bodyEquipment = transform.Find("EquipmentSpace/BodyEquipment").gameObject;
        headEquipment = transform.Find("EquipmentSpace/HeadEquipment").gameObject;
        legEquipment = transform.Find("EquipmentSpace/LegEquipment").gameObject;
        backEquipment = transform.Find("EquipmentSpace/BackEquipment").gameObject;

        return;
    }
}