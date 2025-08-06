using System;
using System.Collections.Generic;
using UnityEngine;

public class OpenCameraWindow : ChatCondition
{
    public OpenCameraWindow(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
    }

    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="Camera")
        {
            return true;
        }
        return false;
    }
}