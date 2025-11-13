using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class UpdateManager : IManager
{
    public static UpdateManager Instance { get; } = new();

    public UnityEvent GameEventUpdate { get; private set; } = new();
    public UnityEvent PlayerUpdate { get; private set; } = new();
    public UnityEvent EnvironmentUpdate { get; private set; } = new();
    public UnityEvent PopulationUpdate { get; private set; } = new();
    public UnityEvent TechnologyUpdate { get; private set; } = new();
    public UnityEvent SunlightUpdate { get; private set; } = new();

    private SortedList<int, UnityAction> sortedCardUpdates = new();
    private SortedList<int, UnityAction> sortedCardFineUpdates = new();
    private int currentOrder = 0;


    public void Init()
    {
        EventManager.Instance.AddListener(EventType.Update, OnUpdate);
        EventManager.Instance.AddListener(EventType.FineUpdate, OnFineUpdate);
    }

    public void Reset()
    {
        Clear();
        EventManager.Instance.RemoveListener(EventType.Update, OnUpdate);
        EventManager.Instance.AddListener(EventType.FineUpdate, OnFineUpdate);
    }

    public void AddCardUpdateListener(ref int order, UnityAction update, UnityAction fineUpdate)
    {
        if (order <= 0)
            order = currentOrder + 1;
        
        sortedCardUpdates.Add(order, update);
        sortedCardFineUpdates.Add(order, fineUpdate);
        currentOrder = Mathf.Max(currentOrder, order);
    }

    public void RemoveCardUpdateListener(int order)
    {
        sortedCardUpdates.Remove(order);
        sortedCardFineUpdates.Remove(order);
    }

    private void CardUpdate()
    {
        foreach (var update in sortedCardUpdates.Values.ToList())
        {
            update();
        }
    }

    private void CardFineUpdate()
    {
        foreach (var fineUpdate in sortedCardFineUpdates.Values.ToList())
        {
            fineUpdate();
        }
    }

    private void OnUpdate()
    {
        EventManager.Instance.TriggerEvent(EventType.UpdateBegin);
        // 顺序很重要
        TechnologyUpdate.Invoke();
        //CardUpdate.Invoke();
        CardUpdate();
        EnvironmentUpdate.Invoke();
        PlayerUpdate.Invoke();
        PopulationUpdate.Invoke();
        GameEventUpdate.Invoke();
        SunlightUpdate.Invoke();
    }

    private void OnFineUpdate()
    {
        CardFineUpdate();
    }

    private void Clear()
    {
        sortedCardUpdates.Clear();
        sortedCardFineUpdates.Clear();
        GameEventUpdate.RemoveAllListeners();
        PlayerUpdate.RemoveAllListeners();
        EnvironmentUpdate.RemoveAllListeners();
        //CardUpdate.RemoveAllListeners();
        PopulationUpdate.RemoveAllListeners();
        TechnologyUpdate.RemoveAllListeners();
        SunlightUpdate.RemoveAllListeners();
    }
}