//using System;
//using UnityEngine;
//using DG.Tweening;
//using System.Collections.Generic;
//using UnityEngine.UI;
//using UnityEngine.Events;

///// <summary>
///// [已废弃] 动效工具类
///// 请使用 AnimationManager.Instance 调用动效API
///// </summary>
//[Obsolete("MFXUtility已废弃，请使用AnimationManager.Instance调用动效API")]
//public static class MFXUtility
//{
//    [Obsolete("使用 AnimationManager.Instance.Canvas")]
//    public static Canvas Canvas => AnimationManager.Instance.Canvas;

//    [Obsolete("使用 AnimationManager.Instance.ScreenToCanvasPosition")]
//    public static Vector2 ScreenPointToLocalPointInRectangle(Vector2 screenPosition)
//        => AnimationManager.Instance.ScreenToCanvasPosition(screenPosition);

//    [Obsolete("使用 AnimationManager.Instance.CreateTempSlot")]
//    public static CardSlot CreateSlot(Vector2 screenPosition)
//        => AnimationManager.Instance.CreateTempSlot(screenPosition);

//    [Obsolete("使用 AnimationManager.Instance.PlayCardMove")]
//    public static Tween MoveCard(Card card, int count, Vector3 sourcePosition,
//        float duration = 0.3f, UnityAction onStart = null, UnityAction onComplete = null, Ease ease = Ease.OutQuad)
//        => AnimationManager.Instance.PlayCardMove(card, count, sourcePosition, duration, onStart, onComplete, ease);

//    [Obsolete("使用 AnimationManager.Instance.PlayCardMoveMultiple")]
//    public static Tween MoveCards(Card[] cards, Vector3 sourcePosition,
//        float duration = 0.3f, float interval = 0.1f, UnityAction onStart = null, UnityAction<Card> onComplete = null, Ease ease = Ease.OutQuad)
//        => AnimationManager.Instance.PlayCardMoveMultiple(cards, sourcePosition, duration, interval, onStart, onComplete, ease);

//    [Obsolete("使用 AnimationManager.Instance.PlayCardTransform")]
//    public static Tween TurnTo(Card sourceCard, Card targetCard, UnityAction onStart = null, UnityAction onComplete = null)
//        => AnimationManager.Instance.PlayCardTransform(sourceCard, targetCard, onStart, onComplete);

//    [Obsolete("使用 sourcePosition.MoveCardAndFreezeTime(card) 扩展方法或 AnimationManager.Instance.PlayCardMoveAndFreezeTime")]
//    public static Tween MoveCardAndFreezeTime(this Vector3 sourcePosition, Card card)
//        => AnimationManager.Instance.PlayCardMoveAndFreezeTime(card, sourcePosition);

//    [Obsolete("使用 sourcePosition.MoveCardsAndFreezeTime(cards) 扩展方法或 AnimationManager.Instance.PlayCardMoveMultipleAndFreezeTime")]
//    public static Tween MoveCardsAndFreezeTime(this Vector3 sourcePosition, Card[] cards)
//        => AnimationManager.Instance.PlayCardMoveMultipleAndFreezeTime(cards, sourcePosition);

//    [Obsolete("使用 sourceCard.TurnToAndFreezeTime(targetCard) 扩展方法或 AnimationManager.Instance.PlayCardTransformAndFreezeTime")]
//    public static Tween TurnToAndFreezeTime(this Card sourceCard, Card targetCard)
//        => AnimationManager.Instance.PlayCardTransformAndFreezeTime(sourceCard, targetCard);

//    [Obsolete("使用 AnimationManager.Instance.ShowFloatingTip")]
//    public static void ShowTip(string tip, Vector3 position, float duration = 2f)
//        => AnimationManager.Instance.ShowFloatingTip(tip, position, duration);

//    [Obsolete("使用 AnimationManager.Instance.ShowFloatingTip")]
//    public static void ShowTip(string tip, Vector3 position, Color textColor, float duration = 1f)
//        => AnimationManager.Instance.ShowFloatingTip(tip, position, textColor, duration);

//    [Obsolete("使用 AnimationManager.Instance.ShowArrows")]
//    public static void ShowArrows(RectTransform rectTransform, bool up, int level, Color color, int arrowCount = 6)
//        => AnimationManager.Instance.ShowArrows(rectTransform, up, level, color, arrowCount);

//    [Obsolete("使用 AnimationManager.Instance.ShowArrows")]
//    public static void ShowArrows(RectTransform rectTransform, List<(bool up, int level, Color color)> groups, int arrowCount = 6)
//        => AnimationManager.Instance.ShowArrows(rectTransform, groups, arrowCount);

//    [Obsolete("使用 AnimationManager.Instance.PlayBounce 或 transform.Bounce() 扩展方法")]
//    public static Tween Bounce(this Transform target, float maxScale = 1.09f, float duration = 0.15f)
//        => AnimationManager.Instance.PlayBounce(target, maxScale, duration);

//    [Obsolete("使用 AnimationManager.Instance.PlayPunch 或 transform.Punch() 扩展方法")]
//    public static Tween Punch(this Transform target, float duration,
//        float pStrengthX, float pStrengthY, float pStrengthZ, int pVibrato,
//        float rStrengthX, float rStrengthY, float rStrengthZ, int rVibrato)
//        => AnimationManager.Instance.PlayPunch(target, duration, pStrengthX, pStrengthY, pStrengthZ, pVibrato, rStrengthX, rStrengthY, rStrengthZ, rVibrato);

//    [Obsolete("使用 AnimationManager.Instance.PlayPunchAndBounce 或 transform.PunchAndBounce() 扩展方法")]
//    public static Tween PunchAndBounce(this Transform target,
//        TweenCallback onPunchComplete = null, TweenCallback onBounceComplete = null,
//        float bounceMaxScale = 1.09f, float bounceDuration = 0.15f, float punchDuration = .6f,
//        float pStrengthX = 4f, float pStrengthY = 2.5f, float pStrengthZ = 0, int pVibrato = 18,
//        float rStrengthX = 0, float rStrengthY = 0, float rStrengthZ = 1.5f, int rVibrato = 15)
//        => AnimationManager.Instance.PlayPunchAndBounce(target, onPunchComplete, onBounceComplete, bounceMaxScale, bounceDuration, punchDuration);

//    [Obsolete("使用 AnimationManager.Instance.ShowFloatingTipAbove 或 transform.ShowTip() 扩展方法")]
//    public static void ShowTip(this Transform target, string tip, float vecticalOffsetScale = .4f)
//        => AnimationManager.Instance.ShowFloatingTipAbove(target, tip, vecticalOffsetScale);
//}