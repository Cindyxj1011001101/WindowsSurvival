using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 段落触发条件：堵住渗水裂缝时触发（可重复触发）
/// 检测事件类型："渗水裂缝"，值："堵住"
/// 设置为可重复触发（Repeat = true）
/// </summary>
public class SealCracks:ParagraphCondition
{
    public SealCracks(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
        Repeat = true;
    }

    public override bool Detect(string type, string value)
    {
        if (type == "渗水裂缝" && value=="堵住")
        {
            return true;
        }
        return false;
    }
}