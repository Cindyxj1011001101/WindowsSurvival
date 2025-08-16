using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 电动排水机
/// </summary>
public class Trap : Card
{
    public bool isWorking; // 是否已打开
    private Trap()
    {
        isWorking = false;
        Events = new()
        {
            new Event("布置", "布置", Event_Arrange, Judge_Arrange),
        };

        // 仅在室内、非水域地点建造
        AddComponent(new ConstructionComponent()
        {
            onlyInDoor = true,
            onlyOutWater = true,
            needCable = true,
        });
    }
    
    public void Event_Arrange(out string tip)
    {
        tip = string.Empty;
        TimeManager.Instance.AddTime(15);
        isWorking = true;

    }

    public bool Judge_Arrange(out string hint)
    {
        hint = string.Empty;
        return !isWorking;
    }

    protected override System.Action OnUpdate => () =>
    {
        int Probability = 48;
        if (TryGetComponent<InnerContentsComponent>(out InnerContentsComponent constructionComponent))
        {
            Probability = 3;
        }

        if (Random.Range(0, Probability) == 0)
        {
            //实现抽取
        }
    };
}