using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadData:VersionMigrator
{
    public Load[] loads=new Load[4];
    public LoadData()
    {
        loads=new Load[4];
    }
    public override IVersionMigrator ReadJSON(string FilePath,string FileName)
    {
        return JsonManager.LoadData<LoadData>(FilePath, FileName);
    }
}
public class Load:VersionMigrator
{
    public DateTime GameTime;
    public bool SkipGuide=true;
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