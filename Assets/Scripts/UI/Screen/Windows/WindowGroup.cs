using UnityEngine;

public class WindowGroup : MonoBehaviour
{
    private Transform closed;
    private Transform minimized;
    private Transform opened;
    [SerializeField] private Transform modal;


    private void Awake()
    {
        closed = transform.Find("Closed");
        minimized = transform.Find("Minimized");
        opened = transform.Find("Opened");

        modal.gameObject.SetActive(false);
    }

    public void SetFocused(WindowBase window)
    {
        if (window == null) return;
        if (window.IsModal)
        {
            window.transform.SetParent(modal);
            modal.gameObject.SetActive(true);
        }
        else
        {
            window.transform.SetParent(opened);
        }
        window.transform.SetAsLastSibling();
    }

    public void SetClosed(WindowBase window)
    {
        if (window == null) return;
        if (window.IsModal)
        {
            modal.gameObject.SetActive(false);
        }
        window.transform.SetParent(closed);
    }

    public void SetMinimized(WindowBase window)
    {
        if (window == null) return;
        if (window.IsModal)
        {
            modal.gameObject.SetActive(false);
        }
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