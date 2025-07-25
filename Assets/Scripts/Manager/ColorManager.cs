using UnityEngine;

public class ColorManager : MonoBehaviour
{
    private static ColorManager instance;

    public static ColorManager Instance => instance;

    public Color black;
    public Color darkGrey;
    public Color lightGrey;
    public Color white;
    public Color blue;
    public Color skyBlue;
    public Color cyan;
    public Color green;
    public Color yellow;
    public Color orange;
    public Color red;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}