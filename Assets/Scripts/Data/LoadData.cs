using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadData
{
    public Load[] loads=new Load[4];
}
public class Load
{
    public DateTime GameTime;
    // public Sprite NPCSprite;
    // public DateTime LastPlayTime;
    public Load(DateTime time)
    {
        GameTime=time;
    }
}