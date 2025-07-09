using UnityEngine;

public class WindowGroup : MonoBehaviour
{
    private Transform closed;
    private Transform minimized;
    private Transform opened;

    public Transform Closed => closed;
    public Transform Minimized => minimized;
    public Transform Opened => opened;

    private void Awake()
    {
        closed = transform.Find("Closed");
        minimized = transform.Find("Minimized");
        opened = transform.Find("Opened");
    }

    public void FocusWindow(WindowBase window)
    {
        if (window == null) return;
        window.transform.SetParent(opened);
        window.transform.SetAsLastSibling();
    }

    public void CloseWindow(WindowBase window)
    {
        if (window == null) return;
        window.transform.SetParent(closed);
    }

    public void MinimizeWindow(WindowBase window)
    {
        if (window == null) return;
        window.transform.SetParent(minimized);
    }

    public WindowBase GetTheFrontWindow()
    {
        if (opened.childCount == 0)
            return null;
        return opened.GetChild(opened.childCount - 1).GetComponent<WindowBase>();
    }

    public bool TeyGetWindowInClosedGroup(string appName, out WindowBase window) 
    {
        for (int i = 0; i < closed.childCount; i++)
        {
            window = closed.GetChild(i).GetComponent<WindowBase>();
            if (window.AppName == appName) return true;
        }
        window = null;
        return false;
    }
}