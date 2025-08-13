using System.Collections.Generic;

public class GeneratedChatData
{
    public bool init;
    public Dictionary<string, ParagraphCondition> DetectedParagraphConditions = new Dictionary<string, ParagraphCondition>();
    public List<ChatData> GeneratedChatDataList = new();
    //需要触发的段落列表
    public List<ParagraphData> ParagraphToTriggeer = new List<ParagraphData>();
    //当前段落数据
    public ParagraphData CurrentParagraphData;
    //当前选项数据
    public ChatData ChoosedChatData;
    //是否在段落中
    public bool inParagraph = false;
    //打断的段落数据
    public ParagraphData InterruptParagraphData = null;
    //当前是否在选择中
    public bool Choosing = false;
    
}