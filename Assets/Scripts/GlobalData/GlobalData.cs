using System.Collections.Generic;
using UnityEngine;

public class Reduce
{
        public int maxReduceCount = 2;
        public int curReduceCount = 0;
        public float reduceRate = 0.5f;
        public float reduce => Mathf.Pow(reduceRate, curReduceCount);
}
public class GlobalData
{
        public Dictionary<string, Reduce> ReduceActionDict = new  Dictionary<string, Reduce>();

        public GlobalData()
        {
                ReduceActionDict = new Dictionary<string, Reduce>();
        }
        public void AddCardReduce(string CardId)
        {
                if (ReduceActionDict.ContainsKey(CardId))
                {
                        ReduceActionDict[CardId].curReduceCount++;
                        ReduceActionDict[CardId].curReduceCount=Mathf.Min(
                                ReduceActionDict[CardId].maxReduceCount,
                                ReduceActionDict[CardId].curReduceCount);
                }
        }
        public float GetReduce(string CardId)
        {
                if (ReduceActionDict.ContainsKey(CardId))
                {
                        return ReduceActionDict[CardId].reduce;
                }
                return 1;
        }
}