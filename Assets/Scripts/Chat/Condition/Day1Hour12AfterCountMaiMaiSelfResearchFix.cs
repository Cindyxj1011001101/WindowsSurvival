using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 段落触发条件：第一天12点后，且"麦麦自己研究修理"计数等于1时触发
/// 检测事件类型："Day1Hour12After"，值："CheckCount"
/// 同时监听计数变化事件
/// </summary>
public class Day1Hour12AfterCountMaiMaiSelfResearchFix : ParagraphCondition
{
    private const string COUNT_NAME = "麦麦自己研究修理";
    private const int TARGET_COUNT_VALUE = 1;
    private bool timeConditionMet = false; // 时间条件是否满足

    public Day1Hour12AfterCountMaiMaiSelfResearchFix(string name, bool startedDetect, bool isUnlocked, 
        Action<List<ParagraphData>> onUnlocked, ParagraphData paragraphData) 
        : base(name, startedDetect, isUnlocked, onUnlocked)
    {
        AddData(paragraphData);
        
        // 验证计数是否已定义
        if (!CountDefinition.IsCountDefined(COUNT_NAME))
        {
            UnityEngine.Debug.LogError($"[段落条件错误] 计数 \"{COUNT_NAME}\" 未在 CountDefinition.cs 中定义！条件：\"{name}\"。请在 CountDefinition.DefinedCounts 中添加该计数。");
        }
        
        // 初始检查条件
        CheckCondition();
    }

    public override bool Detect(string type, string value)
    {
        // 检查时间条件：第一天12点后
        if (type == "Day1Hour12After" && value == "CheckCount")
        {
            timeConditionMet = true;
            CheckCondition();
            return false; // 不直接触发，需要同时满足计数条件
        }
        
        // 检查计数变化
        if (type == "CountChanged" && value == COUNT_NAME)
        {
            CheckCondition();
            return false; // 不直接触发，需要同时满足时间条件
        }
        
        return false;
    }

    private void CheckCondition()
    {
        if (!startedDetect || isUnlocked) return;

        // 检查时间条件：第一天且12点后
        TimeSpan difference = TimeManager.Instance.CurTime - TimeManager.Instance.StartDateTime;
        if (difference.Days == 0 && TimeManager.Instance.CurTime.Hour >= 12)
        {
            timeConditionMet = true;
        }
        else
        {
            timeConditionMet = false;
        }

        // 检查计数条件
        int currentCount = CountManager.Instance.GetCount(COUNT_NAME);
        bool countConditionMet = (currentCount == TARGET_COUNT_VALUE);

        // 两个条件都满足时才触发
        if (timeConditionMet && countConditionMet)
        {
            Unlock();
        }
    }
}
