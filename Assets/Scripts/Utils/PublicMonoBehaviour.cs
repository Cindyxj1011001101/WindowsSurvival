using UnityEngine;

public class PublicMonobBehaviour : MonoBehaviour
{
    private static PublicMonobBehaviour instance;
    public static PublicMonobBehaviour Instance => instance;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}