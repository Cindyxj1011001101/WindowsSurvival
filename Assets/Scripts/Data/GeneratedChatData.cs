using System.Collections.Generic;

public class GeneratedChatData
{
    public bool init;
    public List<ParagraphData> ParagraphConditionsToTrigger = new List<ParagraphData>();
    public List<ChatData> GeneratedChatDataList = new();
    //需要触发的段落列表
    public List<string> ParagraphToTriggeer = new List<string>();
    //当前选项数据
    public string ChoosedChatData;
    //是否在段落中
    public bool inParagraph = false;
    //打断的段落数据
    public ParagraphData InterruptParagraphData = null;
    //当前是否在选择中
    public bool Choosing = false;
    //当前节点数据
    public GraphData.SerializedNode CurrentNodeData;
    //当前段落图数据
    public  GraphData CurrentGraphData;
}