using System;
using System.Collections.Generic;
[Serializable]
public class ParagraphData
{
    public string ParagraphName;
    public float ParagraphPriority;//段落优先级
    public string ParagraphCondition;//触发段落方法

    public ParagraphData(string paragraphName, float paragraphPriority,string paragraphCondition)
    {
        ParagraphName=paragraphName;
        ParagraphPriority=paragraphPriority;
        ParagraphCondition=paragraphCondition;
    }
}   