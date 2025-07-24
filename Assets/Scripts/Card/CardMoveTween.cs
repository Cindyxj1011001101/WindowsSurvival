using UnityEngine;
using DG.Tweening;

public class CardMoveTween
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
            Resources.Load<GameObject>("Prefabs/UI/Controls/CardSlot"),
            Canvas.transform);

        // 获取RectTransform并设置位置
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.anchoredPosition = screenPosition;
        slotRect.localRotation = Quaternion.identity;
        slotRect.localScale = Vector3.one;

        // 设置CardSlot组件
        CardSlot slot = slotObj.GetComponent<CardSlot>();
        slot.GetComponent<CanvasGroup>().blocksRaycasts = false;

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
        float duration,
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
            });
    }
}