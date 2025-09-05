using System;
using System.Collections.Generic;
using UnityEngine;

public static class ColorManager
{
    public static Color32 Black { get; private set; } = new(17, 17, 17, 255);
    public static Color32 DarkGrey { get; private set; } = new(90, 90, 90, 255);
    public static Color32 LightGrey { get; private set; } = new(118, 118, 118, 255);
    public static Color32 White { get; private set; } = new(255, 255, 255, 255);
    public static Color32 Blue { get; private set; } = new(77, 154, 255, 255);
    public static Color32 DarkBlue { get; private set; } = new(0, 128, 255, 255);
    public static Color32 SkyBlue { get; private set; } = new(1, 255, 249, 255);
    public static Color32 Cyan { get; private set; } = new(10, 229, 176, 255);
    public static Color32 Green { get; private set; } = new(0, 209, 63, 255);
    public static Color32 Yellow { get; private set; } = new(255, 232, 13, 255);
    public static Color32 Orange { get; private set; } = new(255, 128, 11, 255);
    public static Color32 BurntOrange { get; private set; } = new(255, 82, 13, 255);
    public static Color32 Red { get; private set; } = new(255, 9, 9, 255);
    public static Color32 FreshWater { get; private set; } = new(147, 219, 247, 255);
    public static Color32 SalineWater { get; private set; } = new(59, 124, 246, 255);

    public static Dictionary<Type, Color32> CardComponentColors = new()
    {
        { typeof(DurabilityComponent), White },
        { typeof(FreshnessComponent), Orange },
        { typeof(ProgressComponent), Green },
        { typeof(GrowthComponent), White },
        { typeof(PlantGrowthComponent), Green },
        { typeof(FuelStorageComponent), BurntOrange },
        { typeof(OxygenStorageComponent), SkyBlue },
        { typeof(FreshWaterStorageComponent), FreshWater },
        { typeof(SalineWaterStorageComponent), SalineWater },
    };

    public static Dictionary<int, Color32> LoadColors = new()
    {
        { 0,  Green },
        { 1,  Yellow },
        { 2,  Orange },
        { 3,  Red },
    };

    public static Dictionary<int, Color32> TemperatureColors = new()
    {
        { 0, DarkBlue },
        { 1, SkyBlue },
        { 2, Green },
        { 3, Yellow },
        { 4, Red },
    };
}