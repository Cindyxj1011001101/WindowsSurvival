using System.Collections.Generic;

public class CardSlotRuntimeData
{
    public List<CardInstance> cardInstanceList = new();

    public bool IsEmpty => cardInstanceList.Count == 0;
}