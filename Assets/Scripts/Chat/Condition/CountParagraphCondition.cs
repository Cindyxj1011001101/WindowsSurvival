using System;
using System.Collections.Generic;

/// <summary>
/// 基于计数的段落条件
/// 格式：计数_计数名_==值、计数_计数名_>=值 等
/// </summary>
public class CountParagraphCondition : ParagraphCondition
{
    private string countName;
    private string op;
    private int targetValue;

    public CountParagraphCondition(string name, bool startedDetect, bool isUnlocked, 
        Action<List<ParagraphData>> onUnlocked, ParagraphData paragraphData) 
        : base(name, startedDetect, isUnlocked, onUnlocked)
    {
        AddData(paragraphData);
        
        // 解析条件字符串，格式：计数_计数名_==值
        if (TryParseCountCondition(name, out countName, out op, out targetValue))
        {
            // 验证计数是否已定义（即使未定义也继续执行，但会报错）
            if (!CountDefinition.IsCountDefined(countName))
            {
                UnityEngine.Debug.LogError($"[段落条件错误] 计数 \"{countName}\" 未在 CountDefinition.cs 中定义！条件：\"{name}\"。请在 CountDefinition.DefinedCounts 中添加该计数。");
            }
            // 无论是否定义都检查条件（未定义时使用默认值0）
            CheckCondition();
        }
    }

    private bool TryParseCountCondition(string condition, out string countName, out string op, out int value)
    {
        countName = null;
        op = null;
        value = 0;

        // 检查是否以 "计数_" 开头
        if (!condition.StartsWith("计数_"))
        {
            return false;
        }

        // 找到第二个下划线的位置
        int firstUnderscore = 2; // "计数_" 的长度是3，索引2是下划线
        int secondUnderscore = condition.IndexOf('_', firstUnderscore + 1);
        
        if (secondUnderscore == -1)
        {
            return false;
        }

        // 提取计数名（第一个下划线和第二个下划线之间的内容）
        countName = condition.Substring(firstUnderscore + 1, secondUnderscore - firstUnderscore - 1);
        
        // 从第二个下划线之后开始查找操作符
        string remaining = condition.Substring(secondUnderscore + 1);
        
        // 支持的比较操作符
        string[] operators = { ">=", "<=", "==", ">", "<" };
        
        foreach (string opStr in operators)
        {
            if (remaining.StartsWith(opStr))
            {
                op = opStr;
                string valueStr = remaining.Substring(opStr.Length);
                if (int.TryParse(valueStr, out value))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool EvaluateCountCondition(int currentCount, string op, int targetValue)
    {
        switch (op)
        {
            case "==":
                return currentCount == targetValue;
            case ">=":
                return currentCount >= targetValue;
            case "<=":
                return currentCount <= targetValue;
            case ">":
                return currentCount > targetValue;
            case "<":
                return currentCount < targetValue;
            default:
                return false;
        }
    }

    private void CheckCondition()
    {
        if (!startedDetect || isUnlocked) return;

        int currentCount = CountManager.Instance.GetCount(countName);
        if (EvaluateCountCondition(currentCount, op, targetValue))
        {
            Unlock();
        }
    }

    public override bool Detect(string type, string value)
    {
        // 当计数变化时，可以通过事件触发检查
        // 这里我们检查是否是计数相关的事件
        if (type == "CountChanged" && value == countName)
        {
            CheckCondition();
        }
        return false;
    }

    // 提供一个公共方法供外部调用，用于定期检查或计数变化时检查
    public void UpdateCountCheck()
    {
        CheckCondition();
    }
}

