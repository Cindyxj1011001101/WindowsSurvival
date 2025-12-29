using System;
using System.Collections.Generic;

/// <summary>
/// 段落触发条件：首次进入珊瑚岛时触发（仅触发一次）
/// 检测事件类型："EnterEnvironment"，值："CoralCoast"
/// 使用静态变量确保只触发一次
/// </summary>
public class FirstEnterCoralIsland:ParagraphCondition
{
    public static bool triggered=false;
    public FirstEnterCoralIsland(string name, bool startedDetect, bool isUnlocked,  Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        AddData(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if (triggered) return false;
        if (type == "EnterEnvironment" && value == "CoralCoast")
        {
            triggered = true;
            return true ;
        }
        return false;

    }
}