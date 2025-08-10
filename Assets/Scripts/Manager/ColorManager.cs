using UnityEngine;

public static class ColorManager
{
    public static Color32 Black { get; private set; } = new(17, 17, 17, 255);
    public static Color32 DarkGrey { get; private set; } = new(90, 90, 90, 255);
    public static Color32 LightGrey { get; private set; } = new(118, 118, 118, 255);
    public static Color32 White { get; private set; } = new(255, 255, 255, 255);
    public static Color32 Blue { get; private set; } = new(77, 154, 255, 255);
    public static Color32 SkyBlue { get; private set; } = new(1, 255, 249, 255);
    public static Color32 Cyan { get; private set; } = new(10, 229, 176, 255);
    public static Color32 Green { get; private set; } = new(0, 209, 63, 255);
    public static Color32 Yellow { get; private set; } = new(255, 232, 13, 255);
    public static Color32 Orange { get; private set; } = new(255, 128, 11, 255);
    public static Color32 Red { get; private set; } = new(255, 9, 9, 255);
}