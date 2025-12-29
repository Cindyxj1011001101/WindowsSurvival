using System;
using System.Collections.Generic;

/// <summary>
/// 段落触发条件：首次打开气密舱门时触发（仅触发一次）
/// 检测事件类型："Detail"，值："气密舱门"
/// 使用静态变量确保只触发一次
/// </summary>
public class FirstOpenAirtightDoor:ParagraphCondition
{    
    public static bool triggered=false;
    public FirstOpenAirtightDoor(string name, bool startedDetect, bool isUnlocked,  Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        AddData(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if (triggered) return false;
        if (type == "Detail" && value == "气密舱门")
        {
            triggered = true;
            return true ;
        }
        return false;
    }
}