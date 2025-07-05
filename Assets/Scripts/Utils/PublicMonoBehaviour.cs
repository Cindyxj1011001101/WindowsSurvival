using UnityEngine;

public class PublicMonoBehaviour : MonoBehaviour
{
    private static PublicMonoBehaviour instance;
    public static PublicMonoBehaviour Instance => instance;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}