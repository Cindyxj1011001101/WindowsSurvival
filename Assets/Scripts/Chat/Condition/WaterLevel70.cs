using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 段落触发条件：水位从低于70上升到70或以上时触发（可重复触发）
/// 检测事件类型："WaterLevel"，值为水位数值
/// 只有当水位从低于70跨越到70或以上时才会触发
/// 设置为可重复触发（Repeat = true）
/// </summary>
public class WaterLevel70:ParagraphCondition
{
    public float LastWaterLevel=0;
    public WaterLevel70(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
        Repeat = true;
    }

    public override bool Detect(string type, string value)
    {
        if (type == "WaterLevel"&& float.TryParse(value, out float waterLevel))
        {
            if (LastWaterLevel < 70 && waterLevel >= 70)
            {
                LastWaterLevel=waterLevel;
                return true;
            }
            LastWaterLevel=waterLevel;
        }
        return false;
    }
}