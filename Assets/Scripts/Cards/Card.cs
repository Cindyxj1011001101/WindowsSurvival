using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//卡牌基类
public abstract class Card
{
    public string cardName;//显示名称
    public string cardDesc;//描述
    public Sprite cardImage;//根据名称在文件夹中找图片
    public CardType cardType;
    public int maxStackNum;//最大堆叠数
    public bool moveable;
    public float weight;
    public List<CardTag> tags;
    public List<Event> events;

    public abstract void Use();
    public abstract void Fresh();
    public abstract void Grow();
}

//事件类
public class Event
{
    public string name;
    public string description;
    public UnityAction action;
    public UnityAction condition;

    public Event(string name, string description, UnityAction action, UnityAction condition)
    {
        this.name = name;
        this.description = description;
        this.action = action;
        this.condition = condition;
    }

    public void Inovke()
    {
        action?.Invoke();
    }

    public bool Judge()
    {
        return false;
    }
}
