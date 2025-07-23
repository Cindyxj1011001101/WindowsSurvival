using System;

public class OpenCameraWindow : Condition
{
    public OpenCameraWindow(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked) { }

    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="Camera")
        {
            return true;
        }
        return false;
    }
}