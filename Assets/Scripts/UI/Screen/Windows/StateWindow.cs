using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StateWindow : WindowBase
{
    //订阅状态变化监听
    public Slider[] Sliders;
    public TMP_Text[] StateNumTexts;
    protected override void Awake()
    {
        base.Awake();
        GameObject Container = GetComponentInChildren<GridLayoutGroup>().gameObject;
        Sliders = new Slider[Container.transform.childCount];
        StateNumTexts = new TMP_Text[Container.transform.childCount];
        for (int i = 0; i < Container.transform.childCount; i++)
        {
            Sliders[i] = Container.transform.GetChild(i).gameObject.GetComponentInChildren<Slider>();
            StateNumTexts[i] = Container.transform.GetChild(i).gameObject.transform.Find("StateNum").GetComponent<TMP_Text>();
        }
    }

    protected override void Start()
    {
        base.Start();
        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, RefreshState);
    }

    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, RefreshState);
    }

    //更新显示数据
    public void RefreshState(PlayerStateEnum stateEnum)
    {
        PlayerState state = StateManager.Instance.PlayerStateDict[stateEnum];
        Sliders[(int)stateEnum].value = state.curValue / state.MaxValue;
        //StateNumTexts[(int)stateEnum].text = state.curValue.ToString() + "/" + state.MaxValue.ToString();
        // 对当前值进行向上取整到小数点后一位
        float roundedCurValue = Mathf.Ceil(state.curValue * 10) / 10f;
    
        // 使用F1格式确保显示一位小数
        StateNumTexts[(int)stateEnum].text = $"{roundedCurValue:F1}/{state.MaxValue}";
    }

    //初始化显示数据
    protected override void Init()
    {
        RefreshState(PlayerStateEnum.Fullness);
        RefreshState(PlayerStateEnum.Health);
        RefreshState(PlayerStateEnum.Thirst);
        RefreshState(PlayerStateEnum.San);
        RefreshState(PlayerStateEnum.Oxygen);
        RefreshState(PlayerStateEnum.Tired);
    }
}