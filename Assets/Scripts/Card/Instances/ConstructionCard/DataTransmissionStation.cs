/// <summary>
/// 数据传输台
/// </summary>
public class DataTransmissionStation : Card
{
    public int MaxTimes;
    public int curTimes;

    public bool isWorking;

    private DataTransmissionStation()
    {
        MaxTimes = 2;
        curTimes = 0;
        Events = new()
        {
            new Event("数据传输", "数据传输", Event_Tech, Judge_Tech),
            new Event("完整拆卸", "完整拆卸", Event_CompleteTearDown, Judge_CompleteTearDown),
            new Event("暴力拆毁", "暴力拆毁", Event_ViolentTearDown, Judge_ViolentTearDown)
        };

        AddComponent(new ConstructionComponent()
        {
            needCable = true,
            onlyInDoor = true,
            onlyOutWater = true
        });
    }

    protected override void LateInit()
    {
        base.LateInit();
        EventManager.Instance.AddListener(EventType.StudyStarted, StartWorking);
        EventManager.Instance.AddListener(EventType.StudyStoped, StopWorking);
        // 当前有科技在研究
        if (TechnologyManager.Instance.CurStudiedTechNode != null && !isWorking)
        {
            StartWorking();
        }
    }

    public override void DestroyThis()
    {
        base.DestroyThis();

        // 当前有科技在研究
        if (isWorking)
        {
            StopWorking();
        }
        EventManager.Instance.RemoveListener(EventType.StudyStarted, StartWorking);
        EventManager.Instance.RemoveListener(EventType.StudyStoped, StopWorking);
    }

    private void StartWorking()
    {
        isWorking = true;

        StateManager.Instance.ChangeElectricityChangeRate(-0.5f);
    }

    private void StopWorking()
    {
        isWorking = false;

        StateManager.Instance.ChangeElectricityChangeRate(+0.5f);
    }

    public void Event_Tech(out string tip)
    {
        tip = string.Empty;
        curTimes++;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -10);
        TechnologyManager.Instance.AddStudyProcess(28);
        TimeManager.Instance.AddTime(60);
    }

    public bool Judge_Tech(out string hint)
    {
        hint = string.Empty;
        if (curTimes >= MaxTimes)
        {
            return false;
        }

        if (TechnologyManager.Instance.CurStudiedTechNode == null)
        {
            return false;
        }
        return true;
    }

    public void Event_CompleteTearDown(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfName("精密扳手").Use();
        DestroyThis();
        AddCard("建筑工程包(数据传输台)", true);
        TimeManager.Instance.AddTime(60);
    }

    public bool Judge_CompleteTearDown(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("精密扳手") != null)
        {
            return true;
        }
        return false;
    }
    public void Event_ViolentTearDown(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfName("钢锤").Use();
        DestroyThis();
        AddCards("珊瑚", 2, true);
        AddCard("韧性胶管", true);
        AddCard("废金属", true);
        TimeManager.Instance.AddTime(15);

    }

    public bool Judge_ViolentTearDown(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("钢锤") != null)
        {
            return true;
        }
        return false;
    }

    protected override System.Action OnUpdate => () =>
    {
        if (TimeManager.Instance.AnotherDay()) curTimes = 0;
    };
}