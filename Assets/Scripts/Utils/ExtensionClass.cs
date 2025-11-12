using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
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

    public static float DistanceTo(this IEntity target, IEntity other) => target.Coordinate.DistanceTo(other.Coordinate);
    public static bool IsInSameLocation(this IEntity target, IEntity other) => target.Coordinate.IsInSameLocation(other.Coordinate);
    
    public static T GetRandomly<T>(this List<T> target, bool repeatable = true)
    {
        var idx = Random.Range(0, target.Count);
        var result = target[idx];
        if (!repeatable)
            target.RemoveAt(idx);
        return result;
    }
}