using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// 扩展方法类
/// </summary>
public static class ExtensionClass
{
    public static bool IsNullOrEmpty(this ICollection target)
    {
        return target == null || target.Count == 0;
    }

    public static Tween Bounce(this Transform target, float maxScale = 1.09f, float duration = 0.15f)
    {
        var originalScale = target.localScale;
        return target.DOScale(maxScale, duration).SetLoops(2, LoopType.Yoyo).OnComplete(() => target.localScale = originalScale);
    }

    public static Tween Punch(this Transform target, float duration,
                           float pStrengthX, float pStrengthY, float pStrengthZ, int pVibrato,
                           float rStrengthX, float rStrengthY, float rStrengthZ, int rVibrato)
    {
        target.GetPositionAndRotation(out var originalPos, out var originalRotation);

        var seq = DOTween.Sequence();
        seq.Join(target.DOPunchPosition(new Vector3(pStrengthX, pStrengthY, pStrengthZ), duration, vibrato: pVibrato).OnComplete(() => { target.position = originalPos; })); // 位置抖动
        seq.Join(target.DOPunchRotation(new Vector3(rStrengthX, rStrengthY, rStrengthZ), duration, vibrato: rVibrato).OnComplete(() => { target.rotation = originalRotation; })); // 旋转抖动

        return seq;
    }

    public static Tween PunchAndBounce(this Transform target,
                                    TweenCallback onPunchComplete = null,
                                    TweenCallback onBounceComplete = null,
                                    float bounceMaxScale = 1.09f, float bounceDuration = 0.15f,
                                    float punchDuration = .6f,
                                    float pStrengthX = 4f, float pStrengthY = 2.5f, float pStrengthZ = 0, int pVibrato = 18,
                                    float rStrengthX = 0, float rStrengthY = 0, float rStrengthZ = 1.5f, int rVibrato = 15)
    {
        var seq = DOTween.Sequence();

        seq.Join(target.Punch(punchDuration, pStrengthX, pStrengthY, pStrengthZ, pVibrato, rStrengthX, rStrengthY, rStrengthZ, rVibrato).OnComplete(onPunchComplete));
        seq.Join(target.Bounce(bounceMaxScale, bounceDuration).SetDelay(punchDuration * .82f).OnComplete(onBounceComplete));

        return seq;
    }

    public static void ShowTip(this Transform target, string tip, float vecticalOffsetScale = .4f)
    {
        MFXUtility.ShowTip(tip, target.position + (target as RectTransform).sizeDelta.y * vecticalOffsetScale * Vector3.up);
    }

    public static float DistanceTo(this IEntity target, IEntity other) => target.Coordinate.DistanceTo(other.Coordinate);
    public static void Move(this IEntity target, float dist) => target.Coordinate.Move(dist);
    public static void MoveTowards(this IEntity target, IEntity other, float dist) => target.Coordinate.MoveTowards(other.Coordinate, dist);
    public static void MoveAwayFrom(this IEntity target, IEntity other, float dist) => target.Coordinate.MoveAwayFrom(other.Coordinate, dist);
}