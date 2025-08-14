using System.Collections;

/// <summary>
/// 扩展方法类
/// </summary>
public static class ExtensionClass
{
    public static bool IsNullOrEmpty(this ICollection target)
    {
        return target == null || target.Count == 0;
    }
}