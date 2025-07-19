using System.Collections.Generic;
public class ParagraphData
{
    public int ParagraphID;//段落ID
    public List<ChatData> ChatDataList;//段落内消息列表
    public string TriggerParagraphEffect;//触发段落方法
    public ParagraphData(int paragraphID, List<ChatData> chatDataList,string triggerParagraphEffect)
    {
        ParagraphID = paragraphID;
        ChatDataList = chatDataList;
        TriggerParagraphEffect = triggerParagraphEffect;
    }
}   