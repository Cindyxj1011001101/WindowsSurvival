using System.Collections.Generic;
public enum MessageSenderEnum
{
    NPC,//NPC
    Player,//玩家
    Aside//旁白
}

public class ChatData
{
    public string MessageID;//消息ID
    public string ParagraphID;//段落ID
    public MessageSenderEnum MessageSender;//消息发送者
    public string MessageType;//消息类型（对话/选项）
    public string Message;//消息内容文本
    public int WaitTime;//播放本句后的等待时间
    public string TriggerMessageEffect;//消息触发时效果
    public ChatData(string[] line)
    {
        MessageID = line[0];
        ParagraphID = line[1];
        MessageSender = (MessageSenderEnum)int.Parse(line[2]);
        MessageType = line[3];
    }
}



