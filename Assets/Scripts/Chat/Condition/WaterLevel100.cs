using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 段落触发条件：水位达到100时触发
/// 检测事件类型："WaterLevel"，值："100"
/// </summary>
public class WaterLevel100:ParagraphCondition
{
    public WaterLevel100(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if (type == "WaterLevel" && value=="100")
        {
            return true;
        }
        return false;
    }
}