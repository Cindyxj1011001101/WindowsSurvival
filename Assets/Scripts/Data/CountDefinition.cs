using System;
using System.Collections.Generic;

/// <summary>
/// 计数定义类，用于定义游戏中所有可用的计数
/// 在剧情编辑器中使用计数前，必须在此文件中先定义
/// </summary>
public static class CountDefinition
{
    /// <summary>
    /// 所有已定义的计数名称集合，默认初始化计数为0
    /// </summary>
    public static readonly HashSet<string> DefinedCounts = new HashSet<string>
    {
        // 在此处添加所有计数定义
        // 示例：
        // "噩梦次数",
        // "对话次数",
        // "探索次数",
        "麦麦自己研究修理",
    };

    /// <summary>
    /// 检查计数名是否已定义
    /// </summary>
    /// <param name="countName">计数名</param>
    /// <returns>如果已定义返回true，否则返回false</returns>
    public static bool IsCountDefined(string countName)
    {
        return DefinedCounts.Contains(countName);
    }

    /// <summary>
    /// 获取所有已定义的计数名称列表（用于调试和查看）
    /// </summary>
    /// <returns>所有已定义的计数名称列表</returns>
    public static List<string> GetAllDefinedCounts()
    {
        return new List<string>(DefinedCounts);
    }
}

