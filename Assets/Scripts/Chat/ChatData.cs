using System;
public enum MessageSenderEnum
{
    NPC, //NPC
    Player, //玩家
    Aside, //旁白
    Alert // 警示
}

[Serializable]
public class ChatData
{
    public MessageSenderEnum MessageSender; //消息发送者
    public string MessageCondition; //消息条件
    public string Message; //消息内容文本
    public float preWaitTime; //播放本句前的等待时间
    public float lateWaitTime; //播放本句后的等待时间
    public string TriggerMessageEffect; //消息触发时效果

    public ChatData(MessageSenderEnum messageSender, string message, string messageCondition,float preWaitTime,float lateWaitTime,string triggerMessageEffect)
    {
        MessageSender=messageSender;
        Message=message;
        MessageCondition=messageCondition;
        this.preWaitTime=preWaitTime;
        this.lateWaitTime=lateWaitTime;
        TriggerMessageEffect=triggerMessageEffect;
    }
}