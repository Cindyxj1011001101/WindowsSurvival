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

    public static CardSlot CreateSlot(Vector2 screenPosition)
    {
        // 获取Canvas和它的RectTransform
        RectTransform canvasRect = Canvas.GetComponent<RectTransform>();

        // 获取事件相机（对于Screen Space - Camera模式很重要）
        Camera eventCamera = Canvas.worldCamera;
        // 或者使用 EventSystem.current.currentInputModule.eventCamera;

        // 将屏幕坐标转换为Canvas局部坐标
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            eventCamera,
            out Vector2 localPosition))
        {
            // 实例化预制体
            GameObject slotObj = Object.Instantiate(
                Resources.Load<GameObject>("Prefabs/UI/Controls/CardSlot"),
                Canvas.transform);

            // 获取RectTransform并设置位置
            RectTransform slotRect = slotObj.GetComponent<RectTransform>();
            slotRect.anchoredPosition = localPosition;
            slotRect.localRotation = Quaternion.identity;
            slotRect.localScale = Vector3.one;

            // 设置CardSlot组件
            CardSlot slot = slotObj.GetComponent<CardSlot>();
            slot.GetComponent<CanvasGroup>().blocksRaycasts = false;

            return slot;
        }

        Debug.LogError("无法在位置 " + screenPosition + " 创建CardSlot");
        return null;
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