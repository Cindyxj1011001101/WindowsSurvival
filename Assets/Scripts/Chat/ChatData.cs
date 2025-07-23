using System.Collections.Generic;
using System.Data;
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
    public string MessageType;//消息类型（对话/选项/分支对话/提示）
    public string MessageCondition;//消息条件
    public string Message;//消息内容文本
    public int NextMessageID;//下一条消息ID
    public int WaitTime;//播放本句后的等待时间
    public string TriggerMessageEffect;//消息触发时效果

    public ChatData(int paragraphID,DataRow row)
    {
        MessageID = int.Parse(row[0].ToString());
        ParagraphID = paragraphID;
        switch(row[2].ToString())
        {
            case "麦麦":
                MessageSender = MessageSenderEnum.NPC;
                break;
            case "玩家":
                MessageSender = MessageSenderEnum.Player;
                break;
            case "求生系统":
                MessageSender = MessageSenderEnum.Aside;
                break;
        }
        MessageType = row[3].ToString();
        MessageCondition = row[4].ToString();
        Message=row[5].ToString();
        if(row[6].ToString()!="")NextMessageID=int.Parse(row[6].ToString());
        else NextMessageID=-1;
        if(row[7].ToString()!="")WaitTime=int.Parse(row[7].ToString());
        else WaitTime=0;
        TriggerMessageEffect=row[8].ToString();
    }
}



