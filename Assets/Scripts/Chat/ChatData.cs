using System.Collections.Generic;
using UnityEngine;
public enum MessageSenderEnum
{
    NPC,//NPC
    Player,//玩家
    Aside//旁白
}

public class ChatData
{
    public int MessageID;//消息ID
    public int ParagraphID;//段落ID
    public MessageSenderEnum MessageSender;//消息发送者
    public int MessageType;//消息类型（对话/选项）
    public string Message;//消息内容文本
    public int NextMessageID;//下一条消息ID
    public int WaitTime;//播放本句后的等待时间
    public string TriggerMessageEffect;//消息触发时效果
    public ChatData(string[] line)
    {
        MessageID = int.Parse(line[0]);
        ParagraphID = int.Parse(line[1]);
        MessageSender = (MessageSenderEnum)(int.Parse(line[2])-1);
        MessageType = int.Parse(line[3]);
        Message=line[4];
        if(line[5]!="")NextMessageID=int.Parse(line[5]);
        else NextMessageID=-1;
        WaitTime=int.Parse(line[7]);
        TriggerMessageEffect=line[8];
    }
}



