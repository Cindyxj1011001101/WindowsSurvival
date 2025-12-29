using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 段落触发条件：玩家清醒度小于30（进入疲劳状态）时触发（可重复触发）
/// 检测事件类型："PlayerSobriety"，值："还不困-疲劳"
/// 设置为可重复触发（Repeat = true）
/// </summary>
public class SobrietyLessThan30:ParagraphCondition
{
    public SobrietyLessThan30(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
        Repeat = true;
    }

    public override bool Detect(string type, string value)
    {
        if (type == "PlayerSobriety" && value=="还不困-疲劳")
        {
            return true;
        }
        return false;
    }
}