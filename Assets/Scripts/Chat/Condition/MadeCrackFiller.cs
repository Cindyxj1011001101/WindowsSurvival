using System;
using System.Collections.Generic;

/// <summary>
/// 段落触发条件：制作裂缝填充物时触发
/// 检测事件类型："Craft"，值："裂缝填充物"
/// </summary>
public class MadeCrackFiller:ParagraphCondition
{
    public MadeCrackFiller(string name, bool startedDetect, bool isUnlocked,  Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        AddData(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if (type == "Craft" && value == "裂缝填充物")
        {
            return true;
        }
        return false;
    }
}