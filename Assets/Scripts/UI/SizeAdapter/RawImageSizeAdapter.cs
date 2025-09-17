using UnityEngine;
using UnityEngine.UI;

public class RawImageSizeAdapter : MonoBehaviour, IAdaptiveSize
{
    private RawImage rawImage;
    private RectTransform rectTransform;
    private RectTransform parentTransform;

    private float initialWidth;
    private float initialHeight;

    private void Awake()
    {
        // rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();
        parentTransform = transform.parent.GetComponent<RectTransform>();

        initialWidth = rectTransform.rect.width;
        initialHeight = rectTransform.rect.height;
    }

    private void Start()
    {
        UpdateSize();
    }

    public void UpdateSize()
    {
        float currentWidth = parentTransform.rect.width;
        float currentHeight = parentTransform.rect.height;
        
        float min = Mathf.Min(currentWidth, currentHeight);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, min);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, min);

        // var uvRect = new Rect();
        //
        // uvRect.width = currentWidth / initialWidth;
        // uvRect.height = currentHeight / initialHeight;
        // uvRect.x = -(currentWidth / 2 - initialWidth / 2) / initialWidth;
        // uvRect.y = -(currentHeight / 2 - initialHeight / 2) / initialHeight;
        //
        // rawImage.uvRect = uvRect;
    }
}