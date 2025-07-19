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

    private static CardSlot CreateSlot(Vector3 position)
    {
        var slot = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/CardSlot"),
            position, Quaternion.identity, Canvas.transform).GetComponent<CardSlot>();
        slot.GetComponent<CanvasGroup>().blocksRaycasts = false;
        return slot;
    }

    /// <summary>
    /// 移动卡牌并执行回调
    /// </summary>
    /// <param name="card">要移动的卡牌</param>
    /// <param name="sourcePosition">起始位置</param>
    /// <param name="targetPosition">目标位置</param>
    /// <param name="duration">动画时长</param>
    /// <param name="onComplete">动画完成回调</param>
    /// <param name="ease">缓动类型(默认OutQuad)</param>
    public static void MoveCard(
        Card card,
        Vector3 sourcePosition,
        Vector3 targetPosition,
        float duration,
        System.Action onComplete = null,
        Ease ease = Ease.OutQuad)
    {
        var slot = CreateSlot(sourcePosition);
        slot.DisplayCard(card, 1);

        slot.transform.DOMove(targetPosition, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                Object.Destroy(slot.gameObject);
            });
    }

    /// <summary>
    /// 移动卡牌并带缩放效果
    /// </summary>
    /// <param name="card">要移动的卡牌</param>
    /// <param name="sourcePosition">起始位置</param>
    /// <param name="targetPosition">目标位置</param>
    /// <param name="duration">动画时长</param>
    /// <param name="scaleMultiplier">缩放倍数</param>
    /// <param name="onComplete">动画完成回调</param>
    /// <param name="moveEase">移动缓动类型(默认OutBack)</param>
    /// <param name="scaleEase">缩放缓动类型(默认OutQuad)</param>
    public static void MoveCardWithScale(
        Card card,
        Vector3 sourcePosition,
        Vector3 targetPosition,
        float duration,
        float scaleMultiplier,
        System.Action onComplete = null,
        Ease moveEase = Ease.OutBack,
        Ease scaleEase = Ease.OutQuad)
    {
        var slot = CreateSlot(sourcePosition);
        slot.DisplayCard(card, 1);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(slot.transform.DOMove(targetPosition, duration).SetEase(moveEase));
        sequence.Join(slot.transform.DOScale(slot.transform.localScale * scaleMultiplier, duration / 2)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(scaleEase));

        sequence.OnComplete(() =>
        {
            onComplete?.Invoke();
            Object.Destroy(slot.gameObject);
        });
    }

    /// <summary>
    /// 移动卡牌并返回Tween对象以便进一步控制
    /// </summary>
    public static Tween MoveCardWithCallback(
        Card card,
        Vector3 sourcePosition,
        Vector3 targetPosition,
        float duration,
        System.Action<CardSlot> onComplete = null,
        Ease ease = Ease.OutQuad)
    {
        var slot = CreateSlot(sourcePosition);
        slot.DisplayCard(card, 1);

        return slot.transform.DOMove(targetPosition, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                onComplete?.Invoke(slot);
                Object.Destroy(slot.gameObject);
            });
    }
}