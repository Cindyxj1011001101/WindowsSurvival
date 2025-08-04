using System;

[Serializable]
public class Condition
{
    public string name;
    public bool startedDetect;
    public bool isUnlocked;
    public Action onUnlocked;

    public Condition(string name, bool startedDetect, bool isUnlocked,Action onUnlocked)
    {
        this.name = name;
        this.startedDetect = startedDetect;
        this.isUnlocked = isUnlocked;
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

    private void Unlock()
    {
        if(!startedDetect) return;
        isUnlocked = true;
        onUnlocked?.Invoke();
        ChatConditionManager.Instance.DetectedConditions.Remove(name);
    }
}