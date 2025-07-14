using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : Toggle
{
    private Image selectImage;

    public UnityEvent onSubmit { get; private set; } = new UnityEvent();
    public UnityEvent onSelect { get; private set; } = new UnityEvent();
    public UnityEvent onDeselect { get; private set; } = new UnityEvent();

    protected override void Awake()
    {
        base.Awake();
        selectImage = transform.Find("Selected").GetComponent<Image>();
        onValueChanged.AddListener((value) =>
        {
            //if (!value)
            //{
            //    selectImage.enabled = false;
            //}
            selectImage.enabled = value;
            if (value)
                onSelect?.Invoke();
        });
    }

    protected override void Start()
    {
        if (isOn) onSelect?.Invoke();
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
        onSubmit?.Invoke();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        EventSystem.current.SetSelectedGameObject(gameObject);
        OnSubmit(eventData);
    }

    //public override void OnSelect(BaseEventData eventData)
    //{
    //    base.OnSelect(eventData);
    //    //selectImage.enabled = true;
    //    //isOn = true;
    //    onSelect?.Invoke();
    //}
}
