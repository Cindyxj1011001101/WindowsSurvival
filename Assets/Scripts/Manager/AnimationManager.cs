using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// 动效配置参数
/// </summary>
public static class AnimationConfig
{
    // 卡牌移动动效
    public const float CARD_MOVE_DURATION = 0.3f;
    public const float CARD_MOVE_INTERVAL = 0.1f;
    public static readonly Ease CARD_MOVE_EASE = Ease.OutQuad;

    // 卡牌转变动效
    public const float CARD_TURN_SCALE_UP = 1.15f;
    public const float CARD_TURN_DURATION = 0.8f;

    // 悬停动效
    public const float HOVER_SCALE = 1.05f;
    public const float HOVER_TRANSITION = 0.1f;

    // 按下动效
    public const float POINTER_DOWN_SCALE = 1.0f;
    public const float POINTER_DOWN_TRANSITION = 0.15f;

    // 弹跳动效
    public const float BOUNCE_MAX_SCALE = 1.09f;
    public const float BOUNCE_DURATION = 0.15f;

    // 抖动动效
    public const float PUNCH_DURATION = 0.6f;
    public const float PUNCH_POS_STRENGTH_X = 4f;
    public const float PUNCH_POS_STRENGTH_Y = 2.5f;
    public const float PUNCH_POS_STRENGTH_Z = 0f;
    public const int PUNCH_POS_VIBRATO = 18;
    public const float PUNCH_ROT_STRENGTH_X = 0f;
    public const float PUNCH_ROT_STRENGTH_Y = 0f;
    public const float PUNCH_ROT_STRENGTH_Z = 1.5f;
    public const int PUNCH_ROT_VIBRATO = 15;

    // 浮动提示动效
    public const float TIP_FADE_IN_DURATION = 0.2f;
    public const float TIP_FADE_OUT_DURATION = 0.2f;
    public const float TIP_DEFAULT_SHOW_DURATION = 2f;
    public const float TIP_VERTICAL_OFFSET = 100f;

    // 箭头粒子动效
    public const float ARROW_DURATION = 0.66f;
    public const float ARROW_MOVE_DISTANCE = 70f;
    public const int ARROW_DEFAULT_COUNT = 6;

    // 窗口动效
    public const float WINDOW_ANIM_DURATION = 0.2f;
    public const float WINDOW_OPEN_DURATION = 0.16f;
    public const float WINDOW_CLOSE_DURATION = 0.12f;
    public const float WINDOW_OPEN_START_SCALE = 0.96f;
    public const float WINDOW_CLOSE_END_SCALE = 0.97f;
    public const float WINDOW_OPEN_OFFSET_Y = -12f;
    public const float WINDOW_CLOSE_OFFSET_Y = -8f;
    public static readonly Ease WINDOW_OPEN_EASE = Ease.OutCubic;
    public static readonly Ease WINDOW_CLOSE_EASE = Ease.InCubic;
    public static readonly Ease WINDOW_MAXIMIZE_RESTORE_EASE = Ease.OutCubic;
    public static readonly Ease WINDOW_MINIMIZE_EASE = Ease.InCubic;

    // 状态图标动效
    public const float STATE_ICON_SCALE_START = 0.8f;
    public const float STATE_ICON_SCALE_MAX = 1.2f;
    public const float STATE_ICON_SCALE_DURATION = 0.3f;
    public const float STATE_ICON_MOVE_DURATION = 0.6f;
    public const float STATE_ICON_BOUNCE_SCALE = 1.3f;
    public const float STATE_ICON_BOUNCE_DURATION = 0.15f;

    // 玩家受击屏幕闪红动效
    public const float PLAYER_DAMAGED_FLASH_MAX_ALPHA = 0.15f;
    public const float PLAYER_DAMAGED_FLASH_FADE_IN = 0.05f;
    public const float PLAYER_DAMAGED_FLASH_FADE_OUT = 0.25f;

    // 移动意图动效（卡牌在原地上下浮动，可带缩放）
    public const float MOVE_INTENTION_BOB_OFFSET_Y = 10f;
    public const float MOVE_INTENTION_BOB_HALF_DURATION = 0.12f;
    public const int MOVE_INTENTION_BOB_CYCLES = 3;
    public const float MOVE_INTENTION_SCALE = 1.03f;
    public const float MOVE_INTENTION_SWAY_OFFSET_X = 6f;
    public const float MOVE_INTENTION_ROT_Z = 4f;
}

/// <summary>
/// 统一动效管理器
/// 负责管理游戏中所有UI动效，提供一致的外部调用接口
/// </summary>
public class AnimationManager
{
    public static AnimationManager Instance { get; } = new();

    private Canvas _canvas;

    private CanvasGroup _screenFlashCanvasGroup;
    private Image _screenFlashImage;
    private Tween _screenFlashTween;

    public Canvas Canvas
    {
        get
        {
            if (_canvas == null)
            {
                _canvas = Object.FindObjectOfType<Canvas>();
                if (_canvas == null)
                {
                    Debug.LogError("No Canvas found in the scene.");
                }
            }
            return _canvas;
        }
    }

    private AnimationManager() { }

    private void EnsureScreenFlash()
    {
        if (_screenFlashCanvasGroup != null && _screenFlashImage != null) return;
        if (Canvas == null) return;

        var go = new GameObject("ScreenFlash", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(Canvas.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.SetAsLastSibling();

        _screenFlashImage = go.GetComponent<Image>();
        _screenFlashImage.raycastTarget = false;
        _screenFlashImage.color = Color.red;

        _screenFlashCanvasGroup = go.GetComponent<CanvasGroup>();
        _screenFlashCanvasGroup.alpha = 0f;
        _screenFlashCanvasGroup.interactable = false;
        _screenFlashCanvasGroup.blocksRaycasts = false;

        go.SetActive(false);
    }

    public Tween PlayPlayerDamagedScreenFlash(float maxAlpha = -1f, float fadeIn = -1f, float fadeOut = -1f)
    {
        if (maxAlpha < 0) maxAlpha = AnimationConfig.PLAYER_DAMAGED_FLASH_MAX_ALPHA;
        if (fadeIn < 0) fadeIn = AnimationConfig.PLAYER_DAMAGED_FLASH_FADE_IN;
        if (fadeOut < 0) fadeOut = AnimationConfig.PLAYER_DAMAGED_FLASH_FADE_OUT;

        if (WindowsManager.Instance == null) return null;

        EnsureScreenFlash();
        if (_screenFlashCanvasGroup == null || _screenFlashImage == null) return null;

        _screenFlashTween?.Kill();

        var go = _screenFlashCanvasGroup.gameObject;
        go.SetActive(true);
        _screenFlashCanvasGroup.alpha = 0f;
        _screenFlashImage.color = ColorManager.Red;
        _screenFlashImage.enabled = true;

        var seq = DOTween.Sequence();
        seq.Append(_screenFlashCanvasGroup.DOFade(maxAlpha, fadeIn));
        seq.Append(_screenFlashCanvasGroup.DOFade(0f, fadeOut));
        seq.OnComplete(() =>
        {
            if (_screenFlashCanvasGroup != null)
                _screenFlashCanvasGroup.gameObject.SetActive(false);
        });

        _screenFlashTween = seq;
        return seq;
    }

    public Tween PlayMoveIntentionEffect(
        Card target,
        CardSlot tempSlot,
        UnityAction onComplete,
        float offsetY = float.NaN,
        float halfDuration = -1f,
        int cycles = -1,
        float maxScale = -1f)
    {
        if (float.IsNaN(offsetY)) offsetY = AnimationConfig.MOVE_INTENTION_BOB_OFFSET_Y;
        if (halfDuration < 0) halfDuration = AnimationConfig.MOVE_INTENTION_BOB_HALF_DURATION;
        if (cycles < 0) cycles = AnimationConfig.MOVE_INTENTION_BOB_CYCLES;
        if (maxScale < 0) maxScale = AnimationConfig.MOVE_INTENTION_SCALE;

        // 刷新临时卡槽的显示
        tempSlot.DisplayCard(target, 1, false);

        var transform = tempSlot.transform;

        var originalLocalPos = transform.localPosition;
        var originalLocalScale = transform.localScale;
        var originalLocalEulerAngles = transform.localEulerAngles;

        var seq = DOTween.Sequence();

        var swayX = AnimationConfig.MOVE_INTENTION_SWAY_OFFSET_X;
        var rotZ = AnimationConfig.MOVE_INTENTION_ROT_Z;

        for (int i = 0; i < cycles; i++)
        {
            var dir = (i % 2 == 0) ? 1f : -1f;

            seq.Append(transform.DOLocalMoveY(originalLocalPos.y + offsetY, halfDuration).SetEase(Ease.OutBack));
            seq.Join(transform.DOLocalMoveX(originalLocalPos.x + swayX * dir, halfDuration).SetEase(Ease.OutSine));
            seq.Join(transform.DOScale(originalLocalScale * maxScale, halfDuration).SetEase(Ease.OutBack));
            seq.Join(transform.DOLocalRotate(new Vector3(0f, 0f, originalLocalEulerAngles.z + rotZ * dir), halfDuration)
                .SetEase(Ease.OutSine));

            seq.Append(transform.DOLocalMoveY(originalLocalPos.y, halfDuration).SetEase(Ease.InOutSine));
            seq.Join(transform.DOLocalMoveX(originalLocalPos.x - swayX * dir * 0.6f, halfDuration).SetEase(Ease.InOutSine));
            seq.Join(transform.DOScale(originalLocalScale, halfDuration).SetEase(Ease.InOutSine));
            seq.Join(transform.DOLocalRotate(new Vector3(0f, 0f, originalLocalEulerAngles.z - rotZ * dir * 0.6f), halfDuration)
                .SetEase(Ease.InOutSine));
        }

        seq.Append(transform.DOLocalMove(originalLocalPos, 0.05f).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(originalLocalScale, 0.05f).SetEase(Ease.OutQuad));
        seq.Join(transform.DOLocalRotate(originalLocalEulerAngles, 0.05f).SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
            ObjectBufferPool.Instance.Restore(tempSlot.gameObject);
        });

        return seq;
    }

    #region 坐标转换工具
    /// <summary>
    /// 将屏幕坐标转换为Canvas本地坐标
    /// </summary>
    public Vector2 ScreenToCanvasPosition(Vector2 screenPosition)
    {
        RectTransform canvasRect = Canvas.GetComponent<RectTransform>();
        Camera eventCamera = Canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPosition, eventCamera, out Vector2 localPosition))
            return localPosition;

        Debug.LogError($"无法将位置{screenPosition}转换为屏幕坐标");
        return Vector2.zero;
    }
    #endregion

    #region 卡槽创建
    /// <summary>
    /// 在指定位置创建临时卡槽
    /// </summary>
    public CardSlot CreateTempSlot(Vector2 screenPosition)
    {
        GameObject slotObj = ObjectBufferPool.Instance.Get(
            "Prefabs/UI/Controls/CardSlot",
            "TempCardSlot",
            WindowsManager.Instance.TempCardSlotLayer);

        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.anchoredPosition = screenPosition;
        slotRect.localRotation = Quaternion.identity;
        slotRect.localScale = Vector3.one;

        CardSlot slot = slotObj.GetComponent<CardSlot>();
        slot.GetComponent<CanvasGroup>().blocksRaycasts = false;
        slot.GetComponentInChildren<ChangeMouse>().changeMouseType = ChangeMouseType.Drag;

        return slot;
    }
    #endregion

    #region 卡牌移动动效
    /// <summary>
    /// 播放卡牌移动动效
    /// </summary>
    /// <param name="card">要移动的卡牌</param>
    /// <param name="count">显示数量</param>
    /// <param name="sourcePosition">起始位置</param>
    /// <param name="duration">动画时长</param>
    /// <param name="onStart">开始回调</param>
    /// <param name="onComplete">完成回调</param>
    /// <param name="ease">缓动类型</param>
    public Tween PlayCardMove(
        Card card,
        int count,
        Vector3 sourcePosition,
        float duration = -1,
        CardSlot tempSlot = null,
        UnityAction onStart = null,
        UnityAction onComplete = null,
        Ease ease = Ease.Unset)
    {
        if (duration < 0) duration = AnimationConfig.CARD_MOVE_DURATION;
        if (ease == Ease.Unset) ease = AnimationConfig.CARD_MOVE_EASE;

        var slot = tempSlot == null ? CreateTempSlot(sourcePosition) : tempSlot;
        slot.DisplayCard(card, count);

        // 打开卡牌目标背包所属的窗口
        var targetWindow = card.Bag?.Window;
        if (targetWindow != null && !WindowsManager.Instance.IsWindowOpen(targetWindow.AppName))
            WindowsManager.Instance.OpenWindow(targetWindow.AppName);

        var seq = DOTween.Sequence();
        seq.Join(slot.transform.DOMove(card.SlotTransform.position, duration).SetEase(ease));
        seq.Join(slot.transform.DOScale(1f, duration));

        seq.OnStart(() => onStart?.Invoke());
        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
            ObjectBufferPool.Instance.Restore(slot.gameObject);
            PlayCardPlaceSound(card.TextureType);
        });

        return seq;
    }

    /// <summary>
    /// 批量播放卡牌移动动效
    /// </summary>
    public Tween PlayCardMoveMultiple(
        Card[] cards,
        Vector3 sourcePosition,
        float duration = -1,
        float interval = -1,
        UnityAction onStart = null,
        UnityAction<Card> onComplete = null,
        Ease ease = Ease.Unset)
    {
        if (duration < 0) duration = AnimationConfig.CARD_MOVE_DURATION;
        if (interval < 0) interval = AnimationConfig.CARD_MOVE_INTERVAL;
        if (ease == Ease.Unset) ease = AnimationConfig.CARD_MOVE_EASE;

        var seq = DOTween.Sequence();

        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];
            seq.Join(PlayCardMove(
                card, 1, sourcePosition, duration, null,
                onStart,
                () => onComplete?.Invoke(card),
                ease
            ).SetDelay(i * interval));
        }

        return seq;
    }

    /// <summary>
    /// 添加单个卡牌动效
    /// </summary>
    public Tween PlayAddCard(Card card, Vector3 sourcePosition)
    {
        var tween = PlayCardMove(card, 1, sourcePosition, 0.4f, onComplete: () => card.RefreshSlot());
        TimeManager.Instance.FreezeTimePass(tween.Duration());
        return tween;
    }

    /// <summary>
    /// 批量添加卡牌动效
    /// </summary>
    public Tween PlayAddCards(Card[] cards, Vector3 sourcePosition)
    {
        var tween = PlayCardMoveMultiple(cards, sourcePosition, 0.4f, onComplete: card => card.RefreshSlot());
        TimeManager.Instance.FreezeTimePass(tween.Duration());
        return tween;
    }
    #endregion

    #region 卡牌转变动效
    /// <summary>
    /// 播放卡牌转变动效（一张卡变成另一张卡）
    /// </summary>
    public Tween PlayCardTransform(
        Card sourceCard,
        Card targetCard,
        UnityAction onStart = null,
        UnityAction onComplete = null)
    {
        var slot = CreateTempSlot(sourceCard.SlotTransform.position);
        slot.DisplayCard(sourceCard, 1, false);

        // 打开卡牌目标背包所属的窗口
        var targetWindow = targetCard.Bag?.Window;
        if (targetWindow != null && !WindowsManager.Instance.IsWindowOpen(targetWindow.AppName))
            WindowsManager.Instance.OpenWindow(targetWindow.AppName);

        var mainSeq = DOTween.Sequence();
        var transform = slot.transform as RectTransform;

        // 缩放动画
        var scaleSeq = DOTween.Sequence();
        scaleSeq.Append(transform.DOScale(AnimationConfig.CARD_TURN_SCALE_UP, 0.2f));
        scaleSeq.AppendInterval(0.4f);
        scaleSeq.Append(transform.DOScale(1f, 0.2f));
        mainSeq.Join(scaleSeq);

        // 移动动画
        var moveSeq = DOTween.Sequence();
        moveSeq.Join(transform.DOAnchorPos(targetCard.Transform.position, 0.4f).SetDelay(0.4f));
        mainSeq.Join(moveSeq);

        // 翻转动画（卡面切换）
        var rotateSeq = DOTween.Sequence();
        rotateSeq.Append(transform.DOScaleX(0, 0.25f).OnComplete(() =>
        {
            slot.Clear();
            slot.DisplayCard(targetCard, 1, false);
        }));
        rotateSeq.Append(transform.DOScaleX(AnimationConfig.CARD_TURN_SCALE_UP, 0.25f));
        mainSeq.Join(rotateSeq);

        mainSeq.OnStart(() => onStart?.Invoke());
        mainSeq.OnComplete(() =>
        {
            onComplete?.Invoke();
            ObjectBufferPool.Instance.Restore(slot.gameObject);
            PlayCardPlaceSound(sourceCard.TextureType);
        });

        return mainSeq;
    }

    /// <summary>
    /// 播放卡牌转变动效并冻结时间
    /// </summary>
    public Tween PlayCardTransformAndFreezeTime(Card sourceCard, Card targetCard)
    {
        var tween = PlayCardTransform(sourceCard, targetCard, onComplete: () => targetCard.RefreshSlot());
        TimeManager.Instance.FreezeTimePass(tween.Duration());
        return tween;
    }
    #endregion

    #region 缩放动效
    /// <summary>
    /// 播放弹跳动效
    /// </summary>
    public Tween PlayBounce(Transform target, float maxScale = -1, float duration = -1)
    {
        if (maxScale < 0) maxScale = AnimationConfig.BOUNCE_MAX_SCALE;
        if (duration < 0) duration = AnimationConfig.BOUNCE_DURATION;

        var originalScale = target.localScale;
        return target.DOScale(maxScale, duration)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() => target.localScale = originalScale);
    }

    /// <summary>
    /// 播放悬停放大动效
    /// </summary>
    public Tween PlayHoverEnter(Transform target, float scale = -1, float duration = -1)
    {
        if (scale < 0) scale = AnimationConfig.HOVER_SCALE;
        if (duration < 0) duration = AnimationConfig.HOVER_TRANSITION;

        return target.DOScale(scale, duration)
            .OnComplete(() => target.localScale = Vector3.one * scale);
    }

    /// <summary>
    /// 播放悬停离开动效
    /// </summary>
    public Tween PlayHoverExit(Transform target, float duration = -1)
    {
        if (duration < 0) duration = AnimationConfig.HOVER_TRANSITION;

        target.DOKill();
        return target.DOScale(1f, duration)
            .OnComplete(() => target.localScale = Vector3.one);
    }

    /// <summary>
    /// 播放按下动效
    /// </summary>
    public Tween PlayPointerDown(Transform target, float scale = -1, float duration = -1)
    {
        if (scale < 0) scale = AnimationConfig.POINTER_DOWN_SCALE;
        if (duration < 0) duration = AnimationConfig.POINTER_DOWN_TRANSITION;

        return target.DOScale(scale, duration).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// 播放抬起动效
    /// </summary>
    public Tween PlayPointerUp(Transform target, float scale = -1, float duration = -1)
    {
        if (scale < 0) scale = AnimationConfig.HOVER_SCALE;
        if (duration < 0) duration = AnimationConfig.POINTER_DOWN_TRANSITION;

        return target.DOScale(scale, duration).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// 播放缩放动效
    /// </summary>
    public Tween PlayScale(Transform target, float targetScale, float duration, Ease ease = Ease.OutQuad)
    {
        return target.DOScale(targetScale, duration).SetEase(ease);
    }
    #endregion

    #region 抖动动效
    /// <summary>
    /// 播放位置和旋转抖动动效
    /// </summary>
    public Tween PlayPunch(
        Transform target,
        float duration = -1,
        float pStrengthX = -1, float pStrengthY = -1, float pStrengthZ = -1, int pVibrato = -1,
        float rStrengthX = -1, float rStrengthY = -1, float rStrengthZ = -1, int rVibrato = -1)
    {
        if (duration < 0) duration = AnimationConfig.PUNCH_DURATION;
        if (pStrengthX < 0) pStrengthX = AnimationConfig.PUNCH_POS_STRENGTH_X;
        if (pStrengthY < 0) pStrengthY = AnimationConfig.PUNCH_POS_STRENGTH_Y;
        if (pStrengthZ < 0) pStrengthZ = AnimationConfig.PUNCH_POS_STRENGTH_Z;
        if (pVibrato < 0) pVibrato = AnimationConfig.PUNCH_POS_VIBRATO;
        if (rStrengthX < 0) rStrengthX = AnimationConfig.PUNCH_ROT_STRENGTH_X;
        if (rStrengthY < 0) rStrengthY = AnimationConfig.PUNCH_ROT_STRENGTH_Y;
        if (rStrengthZ < 0) rStrengthZ = AnimationConfig.PUNCH_ROT_STRENGTH_Z;
        if (rVibrato < 0) rVibrato = AnimationConfig.PUNCH_ROT_VIBRATO;

        target.GetPositionAndRotation(out var originalPos, out var originalRotation);

        var seq = DOTween.Sequence();
        seq.Join(target.DOPunchPosition(new Vector3(pStrengthX, pStrengthY, pStrengthZ), duration, vibrato: pVibrato)
            .OnComplete(() => target.position = originalPos));
        seq.Join(target.DOPunchRotation(new Vector3(rStrengthX, rStrengthY, rStrengthZ), duration, vibrato: rVibrato)
            .OnComplete(() => target.rotation = originalRotation));

        return seq;
    }

    /// <summary>
    /// 播放抖动+弹跳组合动效
    /// </summary>
    public Tween PlayPunchAndBounce(
        Transform target,
        TweenCallback onPunchComplete = null,
        TweenCallback onBounceComplete = null,
        float bounceMaxScale = -1,
        float bounceDuration = -1,
        float punchDuration = -1)
    {
        if (bounceMaxScale < 0) bounceMaxScale = AnimationConfig.BOUNCE_MAX_SCALE;
        if (bounceDuration < 0) bounceDuration = AnimationConfig.BOUNCE_DURATION;
        if (punchDuration < 0) punchDuration = AnimationConfig.PUNCH_DURATION;

        var seq = DOTween.Sequence();
        seq.Join(PlayPunch(target, punchDuration).OnComplete(onPunchComplete));
        seq.Join(PlayBounce(target, bounceMaxScale, bounceDuration)
            .SetDelay(punchDuration * 0.82f)
            .OnComplete(onBounceComplete));

        return seq;
    }
    #endregion

    #region 提示动效
    /// <summary>
    /// 显示浮动提示
    /// </summary>
    public void ShowFloatingTip(string tip, Vector3 position, float duration = -1)
    {
        ShowFloatingTip(tip, position, ColorManager.White, duration);
    }

    /// <summary>
    /// 显示浮动提示（带颜色）
    /// </summary>
    public void ShowFloatingTip(string tip, Vector3 position, Color textColor, float duration = -1)
    {
        if (string.IsNullOrEmpty(tip)) return;
        if (duration < 0) duration = AnimationConfig.TIP_DEFAULT_SHOW_DURATION;

        var floatingTip = ObjectBufferPool.Instance
            .Get("Prefabs/UI/Controls/Tips", "FloatingTip", WindowsManager.Instance.FloatingTipLayer)
            .GetComponent<FloatingTip>();
        floatingTip.ShowTip(tip, position, textColor, duration);
    }

    /// <summary>
    /// 在Transform上方显示浮动提示
    /// </summary>
    public void ShowFloatingTipAbove(Transform target, string tip, float verticalOffsetScale = 0.4f)
    {
        var rectTransform = target as RectTransform;
        var offset = rectTransform != null ? rectTransform.sizeDelta.y * verticalOffsetScale : 50f;
        ShowFloatingTip(tip, target.position + offset * Vector3.up);
    }
    #endregion

    #region 箭头粒子动效
    /// <summary>
    /// 显示箭头粒子效果
    /// </summary>
    public void ShowArrows(RectTransform rectTransform, bool up, int level, Color color, int arrowCount = -1)
    {
        if (!rectTransform.gameObject.activeInHierarchy) return;
        if (arrowCount < 0) arrowCount = AnimationConfig.ARROW_DEFAULT_COUNT;

        float xMin = rectTransform.position.x + rectTransform.rect.xMin;
        float xMax = rectTransform.position.x + rectTransform.rect.xMax;
        float yMin = rectTransform.position.y + rectTransform.rect.yMin;
        float yMax = rectTransform.position.y + rectTransform.rect.yMax;

        if (up)
            yMax -= rectTransform.rect.height / 2;
        else
            yMin += rectTransform.rect.height / 2;

        Vector3 randomPos;
        GameObject obj;
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < arrowCount; i++)
        {
            randomPos = new(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
            obj = ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/Arrow", $"{(up ? "Up" : "Down")}_Lv{level}", rectTransform);
            obj.transform.position = randomPos;
            obj.GetComponentInChildren<Image>().color = color;

            float moveDistance = up ? AnimationConfig.ARROW_MOVE_DISTANCE : -AnimationConfig.ARROW_MOVE_DISTANCE;

            seq.Join(obj.transform.DOMove(
                new Vector3(obj.transform.position.x, obj.transform.position.y + moveDistance, obj.transform.position.z),
                AnimationConfig.ARROW_DURATION).SetEase(Ease.OutQuad));

            seq.Join(obj.transform.GetComponent<CanvasGroup>()
                .DOFade(1, AnimationConfig.ARROW_DURATION / 2)
                .From(0)
                .SetLoops(2, LoopType.Yoyo));

            seq.OnComplete(() => ObjectBufferPool.Instance.Restore(obj));
        }
    }

    /// <summary>
    /// 显示多组箭头粒子效果
    /// </summary>
    public void ShowArrows(RectTransform rectTransform, List<(bool up, int level, Color color)> groups, int totalArrowCount = -1)
    {
        if (totalArrowCount < 0) totalArrowCount = AnimationConfig.ARROW_DEFAULT_COUNT;

        foreach (var (up, level, color) in groups)
        {
            ShowArrows(rectTransform, up, level, color, totalArrowCount / groups.Count);
        }
    }
    #endregion

    #region 窗口动效
    /// <summary>
    /// 播放窗口打开动效（更贴近 Windows：淡入 + 轻微缩放 + 轻微位移）
    /// </summary>
    public Tween PlayWindowOpen(RectTransform window, CanvasGroup canvasGroup, Vector3 targetPosition,
        float duration = -1, float startScale = -1, float offsetY = float.NaN)
    {
        if (duration < 0) duration = AnimationConfig.WINDOW_OPEN_DURATION;
        if (startScale < 0) startScale = AnimationConfig.WINDOW_OPEN_START_SCALE;
        if (float.IsNaN(offsetY)) offsetY = AnimationConfig.WINDOW_OPEN_OFFSET_Y;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        window.position = targetPosition + Vector3.up * offsetY;
        window.localScale = Vector3.one * startScale;

        var seq = DOTween.Sequence();
        seq.Join(canvasGroup.DOFade(1f, duration));
        seq.Join(window.DOMove(targetPosition, duration).SetEase(AnimationConfig.WINDOW_OPEN_EASE));
        seq.Join(window.DOScale(Vector3.one, duration).SetEase(AnimationConfig.WINDOW_OPEN_EASE));
        seq.OnComplete(() => canvasGroup.interactable = true);

        return seq;
    }

    /// <summary>
    /// 播放窗口关闭动效（更贴近 Windows：淡出 + 轻微缩小 + 轻微位移）
    /// </summary>
    public Tween PlayWindowClose(RectTransform window, CanvasGroup canvasGroup, Vector3 targetPosition,
        float duration = -1, float endScale = -1, float offsetY = float.NaN)
    {
        if (duration < 0) duration = AnimationConfig.WINDOW_CLOSE_DURATION;
        if (endScale < 0) endScale = AnimationConfig.WINDOW_CLOSE_END_SCALE;
        if (float.IsNaN(offsetY)) offsetY = AnimationConfig.WINDOW_CLOSE_OFFSET_Y;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        var endPosition = targetPosition + Vector3.up * offsetY;

        var seq = DOTween.Sequence();
        seq.Join(canvasGroup.DOFade(0f, duration));
        seq.Join(window.DOMove(endPosition, duration).SetEase(AnimationConfig.WINDOW_CLOSE_EASE));
        seq.Join(window.DOScale(Vector3.one * endScale, duration).SetEase(AnimationConfig.WINDOW_CLOSE_EASE));
        seq.OnComplete(() =>
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            window.position = targetPosition;
            window.localScale = Vector3.one;
        });

        return seq;
    }

    /// <summary>
    /// 播放窗口最小化动效
    /// </summary>
    public Tween PlayWindowMinimize(Transform window, CanvasGroup canvasGroup, Transform shortcut, float duration = -1)
    {
        if (duration < 0) duration = AnimationConfig.WINDOW_ANIM_DURATION;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        var seq = DOTween.Sequence();
        seq.Join(window.DOScale(Vector3.one * 0.01f, duration).SetEase(AnimationConfig.WINDOW_MINIMIZE_EASE));
        seq.Join(window.DOMove(shortcut.position, duration).SetEase(AnimationConfig.WINDOW_MINIMIZE_EASE));
        seq.OnComplete(() =>
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        });

        return seq;
    }

    /// <summary>
    /// 播放窗口恢复动效
    /// </summary>
    public Tween PlayWindowRestore(RectTransform window, CanvasGroup canvasGroup, Vector3 targetPosition, Vector2 targetSize, float duration = -1)
    {
        if (duration < 0) duration = AnimationConfig.WINDOW_ANIM_DURATION;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        var seq = DOTween.Sequence();
        seq.Join(window.DOMove(targetPosition, duration).SetEase(AnimationConfig.WINDOW_MAXIMIZE_RESTORE_EASE));
        seq.Join(window.DOScale(Vector3.one, duration).SetEase(AnimationConfig.WINDOW_MAXIMIZE_RESTORE_EASE));
        seq.Join(window.DOSizeDelta(targetSize, duration).SetEase(AnimationConfig.WINDOW_MAXIMIZE_RESTORE_EASE));
        seq.OnComplete(() => canvasGroup.interactable = true);

        return seq;
    }

    /// <summary>
    /// 播放窗口最大化动效
    /// </summary>
    public Tween PlayWindowMaximize(RectTransform window, CanvasGroup canvasGroup, RectTransform targetRect, float duration = -1)
    {
        if (duration < 0) duration = AnimationConfig.WINDOW_ANIM_DURATION;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        var seq = DOTween.Sequence();
        seq.Join(window.DOMove(targetRect.position, duration).SetEase(AnimationConfig.WINDOW_MAXIMIZE_RESTORE_EASE));
        seq.Join(window.DOScale(Vector3.one, duration).SetEase(AnimationConfig.WINDOW_MAXIMIZE_RESTORE_EASE));
        seq.Join(window.DOSizeDelta(targetRect.rect.size, duration).SetEase(AnimationConfig.WINDOW_MAXIMIZE_RESTORE_EASE));
        seq.OnComplete(() => canvasGroup.interactable = true);

        return seq;
    }
    #endregion

    #region 状态变化动效
    /// <summary>
    /// 播放状态图标飞向目标的动效
    /// </summary>
    public void PlayStateIconFly(Image targetIcon, Sprite iconSprite, Vector3 center, float stateMaxValue, float delta, int baseCount = 2)
    {
        float halfLength = 85f;
        float xMax = center.x + halfLength;
        float xMin = center.x - halfLength;
        float yMax = center.y + halfLength;
        float yMin = center.y - halfLength;

        int count = baseCount + Mathf.CeilToInt(Mathf.Abs(delta) * 10 / stateMaxValue);

        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = new(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
            var iconTransform = ObjectBufferPool.Instance
                .Get("Prefabs/UI/Controls/State", "StateIcon", WindowsManager.Instance.FloatingTipLayer)
                .transform;

            iconTransform.GetComponent<Image>().sprite = iconSprite;
            iconTransform.position = randomPos;
            iconTransform.localScale = Vector3.one * AnimationConfig.STATE_ICON_SCALE_START;

            var seq = DOTween.Sequence();
            seq.Append(iconTransform.DOScale(AnimationConfig.STATE_ICON_SCALE_MAX, AnimationConfig.STATE_ICON_SCALE_DURATION));
            seq.AppendInterval(0.4f);
            seq.Join(iconTransform.DOMove(targetIcon.transform.position, AnimationConfig.STATE_ICON_MOVE_DURATION));
            seq.Join(iconTransform.DOScale(AnimationConfig.STATE_ICON_SCALE_START, AnimationConfig.STATE_ICON_MOVE_DURATION));
            seq.OnComplete(() =>
            {
                ObjectBufferPool.Instance.Restore(iconTransform.gameObject);
                targetIcon.transform.DOScale(AnimationConfig.STATE_ICON_BOUNCE_SCALE, AnimationConfig.STATE_ICON_BOUNCE_DURATION)
                    .SetLoops(2, LoopType.Yoyo);
            });
        }
    }
    #endregion

    #region 淡入淡出动效
    /// <summary>
    /// 播放淡入动效
    /// </summary>
    public Tween PlayFadeIn(CanvasGroup canvasGroup, float duration = 0.2f, UnityAction onComplete = null)
    {
        canvasGroup.alpha = 0;
        return canvasGroup.DOFade(1, duration).OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 播放淡出动效
    /// </summary>
    public Tween PlayFadeOut(CanvasGroup canvasGroup, float duration = 0.2f, UnityAction onComplete = null)
    {
        return canvasGroup.DOFade(0, duration).OnComplete(() => onComplete?.Invoke());
    }
    #endregion

    #region 移动动效
    /// <summary>
    /// 播放锚点位置移动动效
    /// </summary>
    public Tween PlayAnchorMove(RectTransform target, Vector2 targetPos, float duration = 0.2f, Ease ease = Ease.OutQuad)
    {
        target.DOKill();
        return target.DOAnchorPos(targetPos, duration).SetEase(ease);
    }

    /// <summary>
    /// 播放世界坐标移动动效
    /// </summary>
    public Tween PlayMove(Transform target, Vector3 targetPos, float duration = 0.2f, Ease ease = Ease.OutQuad)
    {
        return target.DOMove(targetPos, duration).SetEase(ease);
    }
    #endregion

    #region 意图切换动效
    /// <summary>
    /// 播放意图执行失败动效
    /// </summary>
    public Tween PlayIntentionFailed(Image intentionIcon)
    {
        var seq = DOTween.Sequence();
        seq.Join(intentionIcon.transform.DOPunchPosition(Vector3.one * 1.5f, 0.6f, 20));
        seq.Join(intentionIcon.DOColor(ColorManager.Red, 0.5f));
        seq.Join(intentionIcon.DOFade(0, 0.2f).SetDelay(0.4f));
        seq.Join(intentionIcon.rectTransform.DOAnchorPosY(-10f, 0.2f));
        seq.OnComplete(() =>
        {
            intentionIcon.transform.localPosition = Vector3.zero;
            intentionIcon.color = new Color(1, 1, 1, 1);
        });

        return seq;
    }
    #endregion

    #region 音效辅助
    private void PlayCardPlaceSound(CardTextureType textureType)
    {
        SoundManager.Instance.PlaySound(GetCardPlaceSoundName(textureType), true);
    }

    /// <summary>
    /// 获取卡牌拿起音效名称
    /// </summary>
    public string GetCardPickSoundName(CardTextureType textureType)
    {
        return textureType switch
        {
            CardTextureType.Flesh => "肉质感卡牌拿起",
            CardTextureType.Metal => "金属质感卡牌拿起",
            CardTextureType.Liquid => "液体质感卡牌拿起",
            _ => "默认质感卡牌拿起"
        };
    }

    /// <summary>
    /// 获取卡牌放置音效名称
    /// </summary>
    public string GetCardPlaceSoundName(CardTextureType textureType)
    {
        return textureType switch
        {
            CardTextureType.Flesh => "肉质感卡牌放置",
            CardTextureType.Metal => "金属质感卡牌放置",
            CardTextureType.Liquid => "液体质感卡牌放置",
            _ => "默认质感卡牌放置"
        };
    }
    #endregion
}

