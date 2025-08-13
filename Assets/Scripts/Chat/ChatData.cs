using System;
using System.Collections.Generic;
using UnityEngine;
public enum MessageSenderEnum
{
    NPC,//NPC
    Player,//玩家
    Aside//旁白
}
[Serializable]
public class ChatData
{
    public int MessageID; //消息ID
    public MessageSenderEnum MessageSender; //消息发送者
    public string MessageCondition; //消息条件
    public string Message; //消息内容文本
    public int WaitTime; //播放本句后的等待时间
    public string TriggerMessageEffect; //消息触发时效果
    public ChatData()
    {
        
    }
}