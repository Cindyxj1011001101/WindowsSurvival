using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using System.Collections.Generic;

public class DynamicEffectUtility
{
    private static Canvas canvas;

    public static Canvas Canvas
    {
        get
        {
            if (canvas == null)
            {
                canvas = Object.FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("No Canvas found in the scene. Please ensure there is a Canvas for card movement.");
                }
            }
            return canvas;
        }
    }

    public static Vector2 ScreenPointToLocalPointInRectangle(Vector2 screenPosition)
    {
        // 获取Canvas和它的RectTransform
        RectTransform canvasRect = Canvas.GetComponent<RectTransform>();
        // 获取事件相机（对于Screen Space - Camera模式很重要）
        Camera eventCamera = Canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            eventCamera,
            out Vector2 localPosition))
            return localPosition;

        Debug.LogError($"无法将位置{screenPosition}转换为屏幕坐标");
        return Vector2.zero;
    }

    public static CardSlot CreateSlot(Vector2 screenPosition)
    {
        // 实例化预制体
        GameObject slotObj = Object.Instantiate(
            Resources.Load<GameObject>("Prefabs/UI/Controls/CardSlot/CardSlot"),
            WindowsManager.Instance.TempCardSlotLayer);

        // 获取RectTransform并设置位置
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.anchoredPosition = screenPosition;
        slotRect.localRotation = Quaternion.identity;
        slotRect.localScale = Vector3.one;

        // 设置CardSlot组件
        CardSlot slot = slotObj.GetComponent<CardSlot>();
        slot.GetComponent<CanvasGroup>().blocksRaycasts = false;
        slot.dontRefresh = true;

        slot.GetComponentInChildren<ChangeMouse>().changeMouseType = ChangeMouseType.Drag;

        return slot;
    }

    /// <summary>
    /// 移动卡牌并执行回调
    /// </summary>
    /// <param name="onStart">动画开始回调（可选）</param>
    public static void MoveCard(
        Card card,
        int count,
        Vector3 sourcePosition,
        Vector3 targetPosition,
        float duration = 0.3f,
        System.Action onStart = null,
        System.Action onComplete = null,
        Ease ease = Ease.OutQuad)
    {
        var slot = CreateSlot(sourcePosition);
        slot.DisplayCard(card, count);

        slot.transform.DOMove(targetPosition, duration)
            .SetEase(ease)
            .OnStart(() => onStart?.Invoke())
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                Object.Destroy(slot.gameObject);
                SoundManager.Instance.PlaySound("放置卡牌", true);
            });
    }

    public static async void MoveCardsWithDelay(
        List<Card> cards,
        Vector3 sourcePosition,
        float duration = 0.3f,
        int millisecondsDelay = 100,
        System.Action onStart = null,
        System.Action<Card> onComplete = null,
        Ease ease = Ease.OutQuad
        )
    {
        foreach (var card in cards)
        {
            MoveCard(
               card,
               1,
               sourcePosition,
               card.Slot.transform.position,
               duration,
               onStart,
               () =>
               {
                   onComplete?.Invoke(card);
               },
               ease
               );

            await Task.Delay(millisecondsDelay);
        }
    }

    public static void ShowTip(string tip, Vector3 position, Color textColor, float duration = 1f)
    {
        if (string.IsNullOrEmpty(tip)) return;

        var obj = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/Tips/FloatingTip"), WindowsManager.Instance.FloatingTipLayer);
        var floatingTip = obj.GetComponent<FloatingTip>();
        floatingTip.ShowTip(tip, position, textColor, duration);
    }
}