using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        EffectResolve.Instance.CurEnvironmentBag.Init();
        EffectResolve.Instance.ResolveExplore();
    }
}
