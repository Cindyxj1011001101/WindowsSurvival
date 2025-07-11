using System.Collections.Generic;

public class EnvironmentBagRuntimeData : BagRuntimeData
{
    //public PlaceEnum place;
    public float discoveryDegree; // 探索度
    public List<Drop> disposableDropList = new(); // 一次性掉落列表
}