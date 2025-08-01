using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTipController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject hoverTipPrefab;
    private HoverTip hoverTip;

    public Vector2 offset;

    public void Awake()
    {
        hoverTipPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/HoverTip/HoverTip");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTip();
    }

    public void SetEvent(Event e, bool interactable)
    {
        hoverTip = Instantiate(hoverTipPrefab, WindowsManager.Instance.transform).GetComponent<HoverTip>();
        hoverTip.SetEvent(e, interactable);
    }

    private void ShowTip()
    {
        // 设置位置
        var rect = transform as RectTransform;
        hoverTip.transform.position = transform.position +
            new Vector3(rect.sizeDelta.x / 2 + offset.x, rect.sizeDelta.y / 2 + offset.y);

        hoverTip.Show();
    }

    private void HideTip()
    {
        hoverTip.Hide();
    }

    public void OnDestroy()
    {
        if (hoverTip != null)
            hoverTip.SelfDestroy();
    }
}
