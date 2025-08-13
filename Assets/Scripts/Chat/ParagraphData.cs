using System.Collections.Generic;
public class ParagraphData
{
    public int ParagraphPriority;//段落优先级
    public string TriggerParagraphCondition;//触发段落方法
    public ParagraphData()
    {
        
    }
    public ParagraphData(int paragraphPriority,string triggerParagraphCondition)
    {
        ParagraphPriority = paragraphPriority; 
        TriggerParagraphCondition = triggerParagraphCondition;
    }
}   