using UnityEngine;

public class KettleFlower : Card
{
    public bool hasWound = false;
    public int woundCount = 0;
    public int woundMaxCount = 10;
    private KettleFlower()
    {
        Events = new()
        {
            new Event("划一个口", "", Event_Hurt, Judge_Hurt),
            new Event("铲起", "", Event_DigUp, Judge_DigUp),
            new Event("饮用汁液", "", Event_Drink, Judge_Drink),
        };
    }
    private void Event_Hurt(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut).Use();
        TryGetComponent<PlantGrowthComponent>(out var growthComponent);
        growthComponent.growth -= 10;
        growthComponent.StopGrow=true;
        hasWound = true;
        int rad= Random.Range(0,100);
        if (rad <= 5)
        {
            AddCard("水壶兰种子", true);
        }
        //TODO：切换图片为有伤口的水壶兰
        TimeManager.Instance.AddTime(15);
    }

    private bool Judge_Hurt(out string hint)
    {
        hint = string.Empty;
        TryGetComponent<PlantGrowthComponent>(out var growthComponent);
        if (hasWound)
        {
            hint = "此时已有伤口，无需划口";
            return false;
        }
        if (growthComponent.growth < 30)
        {
            hint = "需要生长度大于等于30";
            return false;
        }
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }
    private void Event_DigUp(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig).Use();
        DestroyThis();
        AddCard("水壶兰种子", true);
        TimeManager.Instance.AddTime(15);
    }
    private bool Judge_DigUp(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }
    private void Event_Drink(out string tip)
    {
        tip = string.Empty;
        // 播放喝水的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("喝_01", true);
        TryGetComponent<PlantGrowthComponent>(out var growthComponent);
        growthComponent.growth -= 20;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 14);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -3);
        TimeManager.Instance.AddTime(15);
    }
    private bool Judge_Drink(out string hint)
    {
        hint = string.Empty;
        if (!hasWound)
        {
            hint = "此时没有伤口";
            return false;
        }
        TryGetComponent<PlantGrowthComponent>(out var growthComponent);
        if (growthComponent.growth < 20)
        {
            hint = "此时生长度不足20，无法饮用";
            return false;
        }
        return true;
    }
    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (hasWound)
        {
            woundCount++;
            if (woundCount == woundMaxCount)
            {
                hasWound = false;
                woundCount = 0;
                TryGetComponent<PlantGrowthComponent>(out var growthComponent);
                growthComponent.StopGrow=false;
                //TODO：切换图片为正常状态
            }
        }
        
    }
    
}