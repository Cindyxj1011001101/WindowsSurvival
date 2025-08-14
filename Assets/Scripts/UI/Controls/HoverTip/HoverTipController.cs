using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoverTipController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private HoverTip hoverTip;

    public Vector2 offset = new Vector2(10, 0);

    public UnityEvent onPointerEnter = new();

    private string textTip;
    private Color textColor;
    private int time;
    private Dictionary<PlayerStateEnum, float> playerEffects;
    private Dictionary<EnvironmentStateEnum, float> envEffects;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter?.Invoke();
        ShowTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTip();
    }

    public void SetTip(
        string textTip,
        Color textColor,
        int time,
        Dictionary<PlayerStateEnum, float> playerEffects,
        Dictionary<EnvironmentStateEnum, float> envEffects)
    {
        this.textTip = textTip;
        this.textColor = textColor;
        this.time = time;
        this.playerEffects = playerEffects;
        this.envEffects = envEffects;
    }

    public void SetTip(
        string textTip,
        int time,
        Dictionary<PlayerStateEnum, float> playerEffects,
        Dictionary<EnvironmentStateEnum, float> envEffects)
    {
        SetTip(textTip, ColorManager.White, time, playerEffects, envEffects);
    }

    public void SetTip(string textTip)
    {
        SetTip(textTip, 0, null, null);
    }

    public void SetTip(string textTip, Color textColor)
    {
        SetTip(textTip, textColor, 0, null, null);
    }

    public void ShowTip()
    {
        ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/Tips", "HoverTip", WindowsManager.Instance.HoverTipLayer, (asset) =>
        {
            hoverTip = asset.GetComponent<HoverTip>();
            hoverTip.SetTip(textTip, textColor, time, playerEffects, envEffects);
            hoverTip.transform.position = CalcTipPos();
            hoverTip.Show();
        });
    }

    public void HideTip()
    {
        if (hoverTip != null)
            hoverTip.Hide();
        hoverTip = null;
    }

    public void OnDisable()
    {
        HideTip();
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
