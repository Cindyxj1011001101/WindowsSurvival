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

    public static Tween Shake(this Transform target,
                           float duration = .6f,
                           float pStrengthX = 2.3f, float pStrengthY = 1.2f, float pStrengthZ = 0, int pVibrato = 15,
                           float rStrengthX = 0, float rStrengthY = 0, float rStrengthZ = 0.7f, int rVibrato = 12)
    {
        target.GetPositionAndRotation(out var originalPos, out var originalRotation);

        var seq = DOTween.Sequence();
        seq.Join(target.DOShakePosition(duration, new Vector3(pStrengthX, pStrengthY, pStrengthZ), vibrato: pVibrato, fadeOut: false).OnComplete(() => { target.position = originalPos; })); // 位置抖动
        seq.Join(target.DOShakeRotation(duration, new Vector3(rStrengthX, rStrengthY, rStrengthZ), vibrato: rVibrato, fadeOut: false).OnComplete(() => { target.rotation = originalRotation; })); // 旋转抖动

        return seq;
    }

    public static Tween ShakeAndBounce(this Transform target,
                                    TweenCallback onShakeComplete = null,
                                    TweenCallback onBounceComplete = null,
                                    float bounceMaxScale = 1.09f, float bounceDuration = 0.15f,
                                    float shakeDuration = .6f,
                                    float pStrengthX = 2.3f, float pStrengthY = 1.2f, float pStrengthZ = 0, int pVibrato = 15,
                                    float rStrengthX = 0, float rStrengthY = 0, float rStrengthZ = 0.7f, int rVibrato = 12)
    {
        var seq = DOTween.Sequence();

        seq.Append(target.Shake(shakeDuration, pStrengthX, pStrengthY, pStrengthZ, pVibrato, rStrengthX, rStrengthY, rStrengthZ, rVibrato).OnComplete(onShakeComplete));
        seq.Append(target.Bounce(bounceMaxScale, bounceDuration).OnComplete(onBounceComplete));

        return seq;
    }

    public static void ShowTip(this Transform target, string tip, float vecticalOffsetScale = .4f)
    {
        MFXUtility.ShowTip(tip, target.position + (target as RectTransform).sizeDelta.y * vecticalOffsetScale * Vector3.up);
    }
}