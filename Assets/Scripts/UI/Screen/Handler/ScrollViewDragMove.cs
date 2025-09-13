using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ScrollViewMoveDirection
{
    Left,
    Right,
    Up,
    Down
}

public class ScrollViewDragMove : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool isDragging = false;
    private bool isPointerIn = false;

    public ScrollViewMoveDirection direction;
    private float moveSpeed = 1f;

    private ScrollRect scrollRect;

    private void Awake()
    {
        scrollRect = GetComponentInParent<ScrollRect>();

        if (scrollRect.vertical && (direction == ScrollViewMoveDirection.Left || direction == ScrollViewMoveDirection.Right))
        {
            gameObject.SetActive(false);
            return;
        }

        if (scrollRect.horizontal && (direction == ScrollViewMoveDirection.Up || direction == ScrollViewMoveDirection.Down))
        {
            gameObject.SetActive(false);
            return;
        }

        EventManager.Instance.AddListener<Card>(EventType.PickUpCard, OnPickUp);
        EventManager.Instance.AddListener(EventType.PutDownCard, OnPutDown);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<Card>(EventType.PickUpCard, OnPickUp);
        EventManager.Instance.RemoveListener(EventType.PutDownCard, OnPutDown);
    }

    private void OnPickUp(Card card)
    {
        isDragging = true;
    }

    private void OnPutDown()
    {
        isDragging = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerIn = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerIn = false;
    }

    private void Update()
    {
        if (!isDragging || !isPointerIn) return;

        switch (direction)
        {
            case ScrollViewMoveDirection.Left:
                scrollRect.horizontalNormalizedPosition -= Time.deltaTime * moveSpeed;
                break;
            case ScrollViewMoveDirection.Right:
                scrollRect.horizontalNormalizedPosition += Time.deltaTime * moveSpeed;
                break;
            case ScrollViewMoveDirection.Up:
                scrollRect.verticalNormalizedPosition += Time.deltaTime * moveSpeed;
                break;
            case ScrollViewMoveDirection.Down:
                scrollRect.verticalNormalizedPosition -= Time.deltaTime * moveSpeed;
                break;
        }
    }
}