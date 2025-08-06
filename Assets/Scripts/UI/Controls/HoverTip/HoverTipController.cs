using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoverTipController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject hoverTipPrefab;
    private HoverTip hoverTip;

    public Vector2 offset = new Vector2(10, 0);

    public UnityEvent onPointerEnter = new();

    public void Awake()
    {
        hoverTipPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Tips/HoverTip");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter?.Invoke();
        ShowTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTip();
    }

    public void SetTip(Event e, bool interactable)
    {
        SetTip(interactable ? e.description : e.hint, e.time, e.playerEffects, e.envEffects);
    }

    public void SetTip(
        string textTip,
        int time,
        Dictionary<PlayerStateEnum, float> playerEffects,
        Dictionary<EnvironmentStateEnum, float> envEffects)
    {
        if (hoverTipPrefab == null)
            hoverTipPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Tips/HoverTip");

        if (hoverTip == null)
            hoverTip = Instantiate(hoverTipPrefab, WindowsManager.Instance.HoverTipLayer).GetComponent<HoverTip>();
        hoverTip.SetTip(textTip, time, playerEffects, envEffects);
    }

    public void SetTip(string textTip)
    {
        SetTip(textTip, 0, null, null);
    }

    public void ShowTip()
    {
        // 设置位置
        hoverTip.transform.position = CalcTipPos();

        hoverTip.Show();
    }

    public void HideTip()
    {
        hoverTip.Hide();
    }

    public void OnDestroy()
    {
        if (hoverTip != null)
        {
            hoverTip.SelfDestroy();
            hoverTip = null;
        }
    }

    private Vector3 CalcTipPos()
    {
        // 获取tip的尺寸
        var tipRect = hoverTip.transform as RectTransform;
        Vector2 tipSize = tipRect.sizeDelta * tipRect.lossyScale;

        // 默认锚点是左上角
        var pivot = tipRect.pivot = new Vector2(0, 1);

        // 获取屏幕坐标
        var canvas = WindowsManager.Instance.GetComponentInParent<Canvas>();
        var rect = transform as RectTransform;
        var worldPos = rect.position + new Vector3(rect.sizeDelta.x / 2 + offset.x, rect.sizeDelta.y / 2 + offset.y);
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos);

        if (screenPos.x + tipSize.x > Screen.width)
        {
            pivot.x = 1;
            tipRect.pivot = pivot;
            worldPos.x = rect.position.x - rect.sizeDelta.x /2 - offset.x;
        }
        if (screenPos.y - tipSize.y < 0)
        {
            pivot.y = 0;
            tipRect.pivot = pivot;
            worldPos.y = rect.position.y - rect.sizeDelta.y / 2 - offset.y;
        }
        if (screenPos.y > Screen.height)
        {
            Debug.Log("超过上边界");
        }
        if (screenPos.x < 0)
        {
            Debug.Log("超过左边界");
        }

        return worldPos;
    }
}
