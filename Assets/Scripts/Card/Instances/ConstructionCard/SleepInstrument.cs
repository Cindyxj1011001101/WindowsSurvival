/// <summary>
/// 睡眠脉冲仪
/// </summary>
public class SleepInstrument : Card
{
    public bool isWorking; // 是否已打开
    private SleepInstrument()
    {
        isWorking = false;
        Events = new()
        {
            new Event("接电", "", Event_ConnectElectricity, Judge_ConnectElectricity),
            new Event("断电", "", Event_DisconnectElectricity, Judge_DisconnectElectricity),
            new Event("完整拆卸", "", Event_CompleteTearDown, Judge_CompleteTearDown),
            new Event("暴力拆毁", "", Event_ViolentTearDown, Judge_ViolentTearDown),
        };

        // 仅在室内、非水域地点建造
        AddComponent(new ConstructionComponent()
        {
            onlyInDoor = true,
            onlyOutWater = true,
            needCable = true,
        });
    }

    protected override void LateInit()
    {
        base.LateInit();
        EventManager.Instance.AddListener(EventType.StartSleeping, OnStartSleeping);
    }

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener(EventType.StartSleeping, OnStopSleeping);
    }

    private void OnStartSleeping()
    {
        if (GameManager.Instance.CurEnvironmentBag != Bag || !isWorking) return;

        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, +1.2f);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, +1f);
        StateManager.Instance.ChangeElectricityChangeRate(-.6f);
    }

    private void OnStopSleeping()
    {
        if (GameManager.Instance.CurEnvironmentBag != Bag || !isWorking) return;

        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, -1.2f);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, -1f);
        StateManager.Instance.ChangeElectricityChangeRate(.6f);
    }


    private void Event_ConnectElectricity(out string tip)
    {
        tip = string.Empty;
        isWorking = true;
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
    }

    private bool Judge_ConnectElectricity(out string hint)
    {
        hint = string.Empty;

        if (StateManager.Instance.Electricity.CurValue <= 0)
        {
            hint = "电力不足";
            return false;
        }

        return !isWorking;
    }
    private void Event_DisconnectElectricity(out string tip)
    {
        tip = string.Empty;
        isWorking = false;
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
    }
    private bool Judge_DisconnectElectricity(out string hint)
    {
        hint = string.Empty;
        return isWorking;
    }
    private void Event_CompleteTearDown(out string tip)
    {
        tip = string.Empty;
        isWorking = false;
        GameManager.Instance.PlayerBag.FindCardOfName("精密扳手").Use();
        DestroyThis();
        AddCard("建筑工程包(睡眠脉冲仪)", true);
        TimeManager.Instance.AddTime(45);
    }

    private bool Judge_CompleteTearDown(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("精密扳手") != null)
        {
            return true;
        }
        return false;
    }
    private void Event_ViolentTearDown(out string tip)
    {
        tip = string.Empty;
        isWorking = false;
        GameManager.Instance.PlayerBag.FindCardOfName("钢锤").Use();
        DestroyThis();
        AddCards("韧性胶管", 2, true);
        AddCards("玻璃沙", 3, true);
        AddCards("废金属", 2, true);
        TimeManager.Instance.AddTime(15);

    }

    private bool Judge_ViolentTearDown(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("钢锤") != null)
        {
            return true;
        }
        return false;
    }
}