using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

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
        GameObject slotObj = ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/CardSlot", "TempCardSlot", WindowsManager.Instance.TempCardSlotLayer);

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
        float duration = 0.3f,
        UnityAction onStart = null,
        UnityAction onComplete = null,
        Ease ease = Ease.OutQuad,
        bool freezeTime = false)
    {
        var slot = CreateSlot(sourcePosition);

        slot.DisplayCard(card, count);

        // 打开卡牌目标背包所属的窗口
        var targetWindow = card.Bag.Window;
        if (targetWindow != null && !WindowsManager.Instance.IsWindowOpen(targetWindow.AppName))
            WindowsManager.Instance.OpenWindow(targetWindow.AppName);

        var seq = DOTween.Sequence();

        seq.Join(slot.transform.DOMove(card.Slot.transform.position, duration).SetEase(ease));

        seq.Join(slot.transform.DOScale(1f, duration));

        seq.OnStart(() =>
        {
            onStart?.Invoke();
        });
        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
            ObjectBufferPool.Instance.Restore(slot.gameObject);
            SoundManager.Instance.PlaySound("放置卡牌", true);
        });

        if (freezeTime)
            TimeManager.Instance.FreezeTimePass(seq.Duration());

        return seq;
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
        Card[] cards,
        Vector3 sourcePosition,
        float duration = 0.3f,
        float interval = 0.1f,
        UnityAction onStart = null,
        UnityAction<Card> onComplete = null,
        Ease ease = Ease.OutQuad,
        bool freezeTime = false
        )
    {
        var seq = DOTween.Sequence();
        
        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];
            seq.Join(MoveCard(
                card,
                1,
                sourcePosition,
                duration,
                onStart,
                () =>
                {
                    onComplete?.Invoke(card);
                },
                ease,
                freezeTime
                ).SetDelay(i * interval));
        }

        return seq;
    }

    /// <summary>
    /// 从一张卡牌变成另一张卡牌
    /// </summary>
    /// <param name="sourceCard"></param>
    /// <param name="targetCard"></param>
    /// <param name="onStart"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public static Tween TurnTo(
        Card sourceCard,
        Card targetCard,
        UnityAction onStart = null,
        UnityAction onComplete = null,
        bool freezeTime = false)
    {
        var slot = CreateSlot(sourceCard.Transform.position);
        slot.DisplayCard(sourceCard, 1, false);

        // 打开卡牌目标背包所属的窗口
        var targetWindow = targetCard.Bag.Window;
        if (targetWindow != null && !WindowsManager.Instance.IsWindowOpen(targetWindow.AppName))
            WindowsManager.Instance.OpenWindow(targetWindow.AppName);

        var mainSeq = DOTween.Sequence();

        var transform = slot.transform as RectTransform;

        var scaleSeq = DOTween.Sequence();
        scaleSeq.Append(transform.DOScale(1.15f, .2f));
        scaleSeq.AppendInterval(.4f);
        scaleSeq.Append(transform.DOScale(1f, .2f));

        mainSeq.Join(scaleSeq);

        var moveSeq = DOTween.Sequence();
        moveSeq.Join(transform.DOAnchorPos(targetCard.Transform.position, .4f).SetDelay(.4f));

        mainSeq.Join(moveSeq);

        var rotateSeq = DOTween.Sequence();
        rotateSeq.Append(transform.DOScaleX(0, .25f).OnComplete(() =>
        {
            slot.Clear();
            slot.DisplayCard(targetCard, 1, false);
        }));
        rotateSeq.Append(transform.DOScaleX(1.15f, .25f));

        mainSeq.Join(rotateSeq); // 总时长0.9s

        mainSeq.OnStart(() =>
        {
            onStart?.Invoke();
        });

        mainSeq.OnComplete(() =>
        {
            onComplete?.Invoke();
            ObjectBufferPool.Instance.Restore(slot.gameObject);
            SoundManager.Instance.PlaySound("放置卡牌", true);
        });

        if (freezeTime)
            TimeManager.Instance.FreezeTimePass(mainSeq.Duration());

        return mainSeq;
    }

    public static void ShowTip(string tip, Vector3 position, float duration = 2f)
    {
        ShowTip(tip, position, ColorManager.White, duration);
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