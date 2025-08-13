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
    public bool SkipGuide;
    // public Sprite NPCSprite;
    // public DateTime LastPlayTime;
    public Load()
    {
        GameTime=DateTime.MinValue;
        SkipGuide=false;
    }
    public Load(DateTime time,bool SkipGuide)
    {
        GameTime=time;
        this.SkipGuide=SkipGuide;
    }
}