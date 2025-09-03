using System;
using UnityEngine;

public class GlobalDataManager:MonoBehaviour
{
        private static GlobalDataManager instance;
        public static GlobalDataManager Instance => instance;
        public GlobalData globalData=new GlobalData();
        public GlobalData saveData=new GlobalData();
        private void Awake()
        {
                if (instance == null)
                {
                        instance = this;
                        DontDestroyOnLoad(gameObject);
                }
                else
                {
                        Destroy(gameObject);
                }
                EventManager.Instance.AddListener(EventType.AnotherDay,OnAnotherDay);
        }

        private void OnDestroy()
        {
                EventManager.Instance.RemoveListener(EventType.AnotherDay,OnAnotherDay);
        }

        private void OnAnotherDay()
        {
                SolveReduce();

        }

        private void SolveReduce()
        {
                foreach (var reduce in saveData.ReduceActionDict)
                {
                        Debug.Log("1");
                        reduce.Value.curReduceCount=0;       
                }
        }

}