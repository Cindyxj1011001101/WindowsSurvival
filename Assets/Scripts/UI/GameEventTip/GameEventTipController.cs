using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameEventTipController : MonoBehaviour
{
    public GameObject gameEventTipPrefab;

    public List<Sprite> icons = new();

    private Dictionary<string, Sprite> eventIconDict = new();

    private List<GameEventTip> activeTips = new();

    private int maxActiveTips = 5;

    private void Awake()
    {
        foreach (var sprite in icons)
        {
            eventIconDict.Add(sprite.name, sprite);
        }
        icons.Clear();
    }

    private void Start()
    {
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventTrigger, OnGameEventTrigger);
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventEnd, OnGameEventEnd);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventTrigger, OnGameEventTrigger);
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventEnd, OnGameEventEnd);
    }

    private void OnGameEventTrigger(GameEvent gameEvent)
    {
        CreateTip(gameEvent, ParseColor(gameEvent.ThreatLevel));
    }

    private void OnGameEventEnd(GameEvent gameEvent)
    {
        CreateTip(gameEvent, ColorManager.DarkGrey);
    }

    public void CreateTip(GameEvent gameEvent, Color color)
    {
        // 如果提示数量已达上限，移除最早的提示
        if (activeTips.Count == maxActiveTips)
            RemoveTip(activeTips[0]);

        var icon = eventIconDict["Icons_" + gameEvent.GetType().Name];
        var eventDetails = GetDetails(gameEvent.GetDetails());
        var eventName = gameEvent.EventName;

        var tip = ObjectBufferPool.Instance.Get(gameEventTipPrefab, transform).GetComponent<GameEventTip>();
        activeTips.Add(tip);

        tip.SetGameEvent(icon, color, eventName);
        tip.onClick.AddListener(() =>
        {
            // 隐藏提示
            RemoveTip(tip);
            // 显示详情
            var window = WindowsManager.Instance.OpenWindow("EventTip", true) as EventTipWindow;
            window.SetContent(eventDetails);
            window.SetTitle(icon, eventName, color);
        });
        tip.Show();
    }

    public void RemoveTip(GameEventTip tip)
    {
        activeTips.Remove(tip);
        tip.Hide();
    }

    private string GetDetails(string eventDetails)
    {
        var sb = new StringBuilder();
        sb.AppendLine(eventDetails);
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine(ColorManager.Colorize($" -  Day {TimeManager.Instance.Days}   {TimeManager.Instance.CurTime:HH : mm}", ColorManager.LightGrey));
        return sb.ToString();
    }

    private Color ParseColor(int threatLevel)
    {
        return threatLevel switch
        {
            -4 or -3 or -2 => ColorManager.Cyan,
            -1 or 0 or 1 => ColorManager.White,
            2 or 3 => ColorManager.Yellow,
            4 => ColorManager.Red,
            _ => ColorManager.White,
        };
    }
}