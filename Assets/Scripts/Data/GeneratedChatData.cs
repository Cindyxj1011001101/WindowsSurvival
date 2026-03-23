using System.Collections.Generic;

public class GeneratedChatData
{
    public bool init;
    public List<ParagraphData> ParagraphConditionsToTrigger = new List<ParagraphData>();
    public List<ChatData> GeneratedChatDataList = new List<ChatData>();
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

    // 已进行过的剧情段落（仅段落名，不含对话文本）
    public List<string> StoryProgressParagraphs = new List<string>();

    // LLM 每轮返回的前文概括（用于下轮请求）
    public string LLMPreviousSummary = "无";

    // 自动唤起LLM的计时器状态（游戏内分钟）
    public int AutoLLMElapsedMinutes = 0;
    public int AutoLLMTargetMinutes = -1;
}
