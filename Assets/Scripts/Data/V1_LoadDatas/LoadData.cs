using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadData
{
    public Load[] loads=new Load[4];
    public LoadData()
    {
        loads=new Load[4];
    }
}
public class Load
{
    public DateTime GameTime;
    public bool SkipGuide=true;
    // public Sprite NPCSprite;
    // public DateTime LastPlayTime;
    public Load()
    {
        GameTime=DateTime.MinValue;
        SkipGuide=true;
    }
    public Load(DateTime time,bool SkipGuide)
    {
        GameTime=time;
        this.SkipGuide=SkipGuide;
    }
}