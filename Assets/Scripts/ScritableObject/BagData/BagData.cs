using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BagData", menuName ="ScritableObject/BagData")]
public class BagData : ScriptableObject
{
    public List<CardData> cardList;
}