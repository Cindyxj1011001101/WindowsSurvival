using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话触发条件的基类
/// 所有对话相关的触发条件都应继承此类
/// 子类需要实现 Detect() 或 OnCardChanges() 方法来定义具体的触发逻辑
/// </summary>
[Serializable]
public class ChatCondition
{
    public string name;
    public bool startedDetect;
    public bool isUnlocked;
    public bool Repeat;
    public Action<List<ChatData>> onUnlocked;
    public List<ChatData> ChatDatas=new List<ChatData>();

    public ChatCondition(string name, bool startedDetect, bool isUnlocked,Action<List<ChatData>> onUnlocked,ChatData chatData)
    {
        this.name = name;
        this.startedDetect = startedDetect;
        this.isUnlocked = isUnlocked;
        Repeat = false;
        AddData(chatData);
        this.onUnlocked = onUnlocked;
    }
    public void UpdateProgress(string type,string value)
    {
        if (!startedDetect) return;
        if (isUnlocked) return;
        if(Detect(type,value))
        {
            Unlock();
        }
    }
    public void UpdateProgress(Card card,int add)
    {
        if (!startedDetect) return;
        if (isUnlocked) return;
        if(OnCardChanges(card,add))
        {
            Unlock();
        }
    }
    public virtual bool Detect(string type,string value)
    {
        return false;
    }

    public virtual bool OnCardChanges(Card card,int add)
    {
        return false;
    }

    public virtual void Unlock()
    {
        if(!startedDetect) return;
        isUnlocked = true;
        onUnlocked?.Invoke(ChatDatas);
        if(!Repeat) ChatConditionManager.Instance.DetectedChatConditions.Remove(name);
    }

    public virtual void AddData(ChatData chatData)
    {
        ChatDatas.Add(chatData);
    }
}