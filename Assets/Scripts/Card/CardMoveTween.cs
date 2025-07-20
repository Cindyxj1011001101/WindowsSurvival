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

    public static CardSlot CreateSlot(Vector3 position)
    {
        var slot = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/CardSlot"),
            position, Quaternion.identity, Canvas.transform).GetComponent<CardSlot>();
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