using UnityEngine;
using UnityEngine.EventSystems;

public class ShowDetail : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject DetailInfoPrefab;
    private GameObject TimePrefab;
    private GameObject StatePrefab;
    private GameObject DetailPrefab;
    private GameObject DetailInfo;
    public Vector3 offset;
    public void Awake()
    {
        DetailInfoPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Detail/DetailInfo");
        TimePrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Detail/Time");
        StatePrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Detail/State");
        DetailPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Detail/Detail");
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //创建详情
        Debug.Log(DetailInfoPrefab);
        DetailInfo = Instantiate(DetailInfoPrefab, transform);
        DetailInfo.transform.position = transform.position + offset;
        //创建时间
        GameObject Time = Instantiate(TimePrefab, DetailInfo.transform);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(DetailInfo);
    }
}
