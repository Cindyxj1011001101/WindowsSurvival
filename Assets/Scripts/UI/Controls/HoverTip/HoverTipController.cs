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
    private Dictionary<PlayerStateEnum, float> playerStateChanges;
    private Dictionary<EnvironmentStateEnum, float> envStateChanges;

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
        Dictionary<PlayerStateEnum, float> playerStateChanges,
        Dictionary<EnvironmentStateEnum, float> envStateChanges)
    {
        this.textTip = textTip;
        this.textColor = textColor;
        this.time = time;
        this.playerStateChanges = playerStateChanges;
        this.envStateChanges = envStateChanges;
    }

    public void SetTip(
        string textTip,
        int time,
        Dictionary<PlayerStateEnum, float> playerStateChanges,
        Dictionary<EnvironmentStateEnum, float> envStateChanges)
    {
        SetTip(textTip, ColorManager.White, time, playerStateChanges, envStateChanges);
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
        if (string.IsNullOrEmpty(textTip) && time == 0 && playerStateChanges.IsNullOrEmpty() && envStateChanges.IsNullOrEmpty())
        {
            HideTip();
            return;
        }

        hoverTip = ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/Tips", "HoverTip", WindowsManager.Instance.HoverTipLayer).GetComponent<HoverTip>();

        hoverTip.SetTip(textTip, textColor, time, playerStateChanges, envStateChanges);
        hoverTip.transform.position = CalcTipPos();
        hoverTip.Show();
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
        var rect = transform as RectTransform;
        var worldPos = rect.position + new Vector3(rect.sizeDelta.x / 2 + offset.x, rect.sizeDelta.y / 2 + offset.y);

        // 获取桌面大小
        (float left, float top, float right, float bottom) = MonoUtility.GetFourBorders(WindowsManager.Instance.Desktop);

        if (worldPos.x + tipSize.x > right)
        {
            pivot.x = 1;
            tipRect.pivot = pivot;
            worldPos.x = rect.position.x - rect.sizeDelta.x /2 - offset.x;
        }
        if (worldPos.y - tipSize.y < bottom)
        {
            pivot.y = 0;
            tipRect.pivot = pivot;
            worldPos.y = rect.position.y - rect.sizeDelta.y / 2 - offset.y;
        }
        if (worldPos.y > top)
        {
            Debug.Log("超过上边界");
        }
        if (worldPos.x < left)
        {
            Debug.Log("超过左边界");
        }

        return worldPos;
    }
}
