using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 段落触发条件：第一天11点时修复未完成时触发
/// 检测事件类型："Day1Hour11"，值："FixUnConplished"
/// </summary>
public class Day1Hour11FixUnConplished:ParagraphCondition
{
    public Day1Hour11FixUnConplished(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if (type == "Day1Hour11" && value=="FixUnConplished")
        {
            return true;
        }
        return false;
    }
}