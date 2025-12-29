using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 段落触发条件：完成研究修复时触发
/// 检测事件类型："FinishResearch"，值："修理"
/// </summary>
public class FinishResearchFix : ParagraphCondition
{
    public FinishResearchFix(string name, bool startedDetect, bool isUnlocked,  Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        AddData(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if(type=="FinishResearch"&&value=="修理")
        {

            return true;
        }
        return false;
    }
}   