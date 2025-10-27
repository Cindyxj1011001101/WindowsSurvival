using System;

public class LoadData
{
    public Load[] loads = new Load[4];
}

public class Load
{
    public DateTime gameTime;
    public bool skipGuide = true;
    // public Sprite NPCSprite;
    // public DateTime LastPlayTime;
    public Load()
    {
        gameTime = DateTime.MinValue;
        skipGuide = true;
    }
    public Load(DateTime time, bool skipGuide)
    {
        gameTime = time;
        this.skipGuide = skipGuide;
    }
}