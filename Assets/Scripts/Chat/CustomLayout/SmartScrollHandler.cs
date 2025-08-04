using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SmartScrollHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float clickThreshold = 10f;
    [SerializeField] private float clickTimeThreshold = 0.3f;
    private Vector2 startPosition;
    private float startTime;
    
    
    public void OnPointerDown(PointerEventData eventData)
    {
        startPosition = eventData.position;
        startTime = Time.time;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        float distance = Vector2.Distance(startPosition, eventData.position);
        float timeElapsed = Time.time - startTime;
        
        if (distance < clickThreshold && timeElapsed < clickTimeThreshold)
        {
            HandleClick(eventData);
        }
    }
    
    private void HandleClick(PointerEventData eventData)
    {
        transform.parent.Find("MessageSpace").gameObject.SetActive(false);
        transform.parent.GetComponent<CustomMessageLayout>().Refresh();
        transform.parent.Find("ScrollView").GetComponentInChildren<Scrollbar>().value = 0;
    }
}