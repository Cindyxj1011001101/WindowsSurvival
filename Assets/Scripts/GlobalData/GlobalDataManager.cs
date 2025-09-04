using UnityEngine;

public class GlobalDataManager : MonoBehaviour
{
    private static GlobalDataManager instance;
    public static GlobalDataManager Instance => instance;

    public GlobalData globalData;
    public GlobalData saveData;

    private void Awake()
    {
        instance = this;

        globalData = GameDataManager.Instance.GlobalData;
        saveData = GameDataManager.Instance.SaveData;

        EventManager.Instance.AddListener(EventType.AnotherDay, OnAnotherDay);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.AnotherDay, OnAnotherDay);
    }

    private void OnAnotherDay()
    {
        SolveReduce();
    }

    private void SolveReduce()
    {
        foreach (var reduce in saveData.reduceActionDict.Values)
        {
            reduce.curReduceCount = 0;
        }
    }
}