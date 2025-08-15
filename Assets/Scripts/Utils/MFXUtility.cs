using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

public class MFXUtility
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
        GameObject slotObj = ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/CardSlot", "CardSlot", WindowsManager.Instance.TempCardSlotLayer);

        // 获取RectTransform并设置位置
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.anchoredPosition = screenPosition;
        slotRect.localRotation = Quaternion.identity;
        slotRect.localScale = Vector3.one;

        // 设置CardSlot组件
        CardSlot slot = slotObj.GetComponent<CardSlot>();
        slot.GetComponent<CanvasGroup>().blocksRaycasts = false;

        slot.GetComponentInChildren<ChangeMouse>().changeMouseType = ChangeMouseType.Drag;

        return slot;
    }

    /// <summary>
    /// 移动卡牌
    /// </summary>
    public static Tween MoveCard(
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

        return slot.transform.DOMove(targetPosition, duration)
             .SetEase(ease)
             .OnStart(() => onStart?.Invoke())
             .OnComplete(() =>
             {
                 onComplete?.Invoke();
                 ObjectBufferPool.Instance.Restore(slot.gameObject);
                 SoundManager.Instance.PlaySound("放置卡牌", true);
             });
    }

    /// <summary>
    /// 一次移动多张卡牌
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="sourcePosition"></param>
    /// <param name="duration"></param>
    /// <param name="interval"></param>
    /// <param name="onStart"></param>
    /// <param name="onComplete"></param>
    /// <param name="ease"></param>
    /// <returns></returns>
    public static Tween MoveCards(
        List<Card> cards,
        Vector3 sourcePosition,
        float duration = 0.3f,
        float interval = 0.1f,
        System.Action onStart = null,
        System.Action<Card> onComplete = null,
        Ease ease = Ease.OutQuad
        )
    {
        var seq = DOTween.Sequence();

        Card card;
        for (int i = 0; i < cards.Count; i++)
        {
            card = cards[i];
            seq.Join(MoveCard(
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
                ).SetDelay(i * interval));
        }

        return seq;
    }

    public static void ShowTip(string tip, Vector3 position, float duration = 1f)
    {
        ShowTip(tip, position, ColorManager.Yellow, duration);
    }

    public static void ShowTip(string tip, Vector3 position, Color textColor, float duration = 1f)
    {
        if (string.IsNullOrEmpty(tip)) return;

        var floatingTip = ObjectBufferPool.Instance
            .Get("Prefabs/UI/Controls/Tips", "FloatingTip", WindowsManager.Instance.FloatingTipLayer)
            .GetComponent<FloatingTip>();
        floatingTip.ShowTip(tip, position, textColor, duration);
    }

    public static void ShowArrows(RectTransform rectTransform, bool up, int level, Color color, int arrowCount = 6)
    {
        if (!rectTransform.gameObject.activeInHierarchy) return;

        float xMin, xMax, yMin, yMax;
        xMin = rectTransform.position.x + rectTransform.rect.xMin;
        xMax = rectTransform.position.x + rectTransform.rect.xMax;
        yMin = rectTransform.position.y + rectTransform.rect.yMin;
        yMax = rectTransform.position.y + rectTransform.rect.yMax;
        if (up)
            yMax -= rectTransform.rect.height / 2;
        else
            yMin += rectTransform.rect.height / 2;

        Vector3 randomPos;
        GameObject obj;
        Sequence seq = DOTween.Sequence();
        float duration = 0.66f;
        for (int i = 0; i < arrowCount; i++)
        {
            randomPos = new(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
            obj = ObjectBufferPool.Instance.Get($"Prefabs/UI/Controls/Arrow", $"{(up ? "Up" : "Down")}_Lv{level}", rectTransform);
            obj.transform.position = randomPos;

            obj.GetComponentInChildren<Image>().color = color;

            seq.Join(obj.transform.DOMove(new Vector3(obj.transform.position.x, obj.transform.position.y + (up ? 70f : -70f), obj.transform.position.z), duration)
                                    .SetEase(Ease.OutQuad));

            seq.Join(obj.transform.GetComponent<CanvasGroup>().DOFade(1, duration / 2)
                                                    .From(0)
                                                    .SetLoops(2, LoopType.Yoyo));

            seq.OnComplete(() => ObjectBufferPool.Instance.Restore(obj));
        }
    }

    public static void ShowArrows(RectTransform rectTransform, List<(bool up, int level, Color color)> groups, int arrowCount = 6)
    {
        foreach (var (up, level, color) in groups)
        {
            ShowArrows(rectTransform, up, level, color, arrowCount / groups.Count);
        }
    }
}