using UnityEngine;
using UnityEngine.Events;

public class PublicMono : MonoBehaviour
{
    private static PublicMono instance;

    public static PublicMono Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    private event UnityAction update;
    private event UnityAction fixedUpdate;
    private event UnityAction lateUpdate;

    public void AddUpdateListener(UnityAction action) => update += action;

    public void AddFixedUpdateListener(UnityAction action) => fixedUpdate += action;

    public void AddLateUpdateListener(UnityAction action) => lateUpdate += action;

    public void RemoveUpdateListener(UnityAction action) => update -= action;

    public void RemoveFixedUpdateListener(UnityAction action) => fixedUpdate -= action;

    public void RemoveLateUpdateListener(UnityAction action) => lateUpdate -= action;

    private void Update()
    {
        update?.Invoke();
    }

    private void FixedUpdate()
    {
        fixedUpdate?.Invoke();
    }

    private void LateUpdate()
    {
        lateUpdate?.Invoke();
    }
}
