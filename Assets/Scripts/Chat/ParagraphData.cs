using System.Collections.Generic;
public class ParagraphData
{
    public int ParagraphID;//段落ID
    public int ParagraphPriority;//段落优先级
    public List<ChatData> ChatDataList;//段落内消息列表
    public string TriggerParagraphCondition;//触发段落方法
    public ParagraphData(int paragraphID, int paragraphPriority, List<ChatData> chatDataList,string triggerParagraphCondition)
    {
        ParagraphID = paragraphID;
        ParagraphPriority = paragraphPriority; 
        ChatDataList = chatDataList;
        TriggerParagraphCondition = triggerParagraphCondition;
    }
}   