using System;
using System.Collections.Generic;

/// <summary>
/// 段落触发条件：玩家生命值为0时触发
/// 检测事件类型："PlayerHealth"，值："0"
/// </summary>
public class HealthZero : ParagraphCondition
{
    public HealthZero(string name, bool startedDetect, bool isUnlocked,  Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked)
    {
        AddData(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if(type=="PlayerHealth"&&value=="0")
        {
            return true;
        }
        return false;
    }
}