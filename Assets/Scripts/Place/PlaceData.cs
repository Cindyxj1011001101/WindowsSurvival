using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaceData", menuName = "ScriptableObject/PlaceData")]
public class PlaceData : ScriptableObject
{
    public string placeName;
    public string placeDesc;
    public PlaceEnum placeType;
    public bool isIndoor;
    public bool isInWater;
    public bool isInSpacecraft;
    public bool isInCave;
    public Sprite placeImage;
    public int exploreTime;
    [HideInInspector] public float minCoord = 0;
    public float maxCoord;
    public PlaceEnum connectedOutdoorPlace;

    public InitialBagStateConfig initialBagStateConfig;

    private void OnValidate()
    {
        placeName = name;
    }
}

[Serializable]
public class InitialBagStateConfig
{
    public bool hasCable;
    public List<string> containedCards;
    public PressureLevel pressureLevel;
}