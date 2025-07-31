using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowDetail : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Event e;
    public bool canShowDetail = false;
    public Sprite[] LoadSprite;
    public Color[] LoadColor;
    public Sprite[] TempretureSprite;
    public Color[] TempretureColor;
    private GameObject DetailInfoPrefab;
    private GameObject DetailInfo;
    public Vector3 offset;
    public void Awake()
    {
        DetailInfoPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/DetailInfo");
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!canShowDetail && e.hint != null)
        {
            ShowHint();
            return;
        }
        ShowDetailInfo();

    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(DetailInfo);
    }
    private void ShowDetailInfo()
    {
        //创建详情区域
        DetailInfo = Instantiate(DetailInfoPrefab, FindObjectOfType<WindowsManager>().transform);
        DetailInfo.transform.position = transform.position + offset;
        //时间变化显示
        if (e.Time != 0)
        {
            GameObject Time = DetailInfo.transform.Find("Time").gameObject;
            Time.transform.Find("Text").GetComponent<Text>().text = e.Time.ToString() + "分钟";
            Time.SetActive(true);
        }
        //玩家状态变化显示
        foreach (var item in e.PlayerStateDict)
        {
            GameObject State = DetailInfo.transform.Find(item.Key.ToString()).gameObject;
            string r = item.Value.ToString();
            if (item.Value > 0)
            {
                r = "+" + r;
                float curValue = StateManager.Instance.PlayerStateDict[item.Key].CurValue;
                float maxValue = StateManager.Instance.PlayerStateDict[item.Key].MaxValue;
                State.transform.Find("Slider/Max").GetComponent<Slider>().value = (curValue + item.Value) / maxValue;
                State.transform.Find("Slider/Max/Fill").GetComponent<Image>().color = ColorManager.green;
                State.transform.Find("Slider/Min").GetComponent<Slider>().value = curValue / maxValue;
                State.transform.Find("Slider/Min/Fill").GetComponent<Image>().color = ColorManager.white;
            }
            else
            {
                float curValue = StateManager.Instance.PlayerStateDict[item.Key].CurValue;
                float maxValue = StateManager.Instance.PlayerStateDict[item.Key].MaxValue;
                State.transform.Find("Slider/Max").GetComponent<Slider>().value = curValue / maxValue;
                State.transform.Find("Slider/Max/Fill").GetComponent<Image>().color = ColorManager.white;
                State.transform.Find("Slider/Min").GetComponent<Slider>().value = (curValue + item.Value) / maxValue;
                State.transform.Find("Slider/Min/Fill").GetComponent<Image>().color = ColorManager.red;
            }
            State.transform.Find("Text").GetComponent<Text>().text = r;
            if (item.Key == PlayerStateEnum.Load)
            {
                int level = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel;
                State.transform.Find("ICON").GetComponent<Image>().sprite = LoadSprite[level];
                State.transform.Find("Name").GetComponent<Text>().color = LoadColor[level];
            }
            else if (item.Key == PlayerStateEnum.BodyTemperature)
            {
                int level = StateManager.Instance.PlayerStateDict[PlayerStateEnum.BodyTemperature].StateLevel;
                State.transform.Find("ICON").GetComponent<Image>().sprite = TempretureSprite[level];
                State.transform.Find("Name").GetComponent<Text>().color = TempretureColor[level];
            }
            State.SetActive(true);
        }
        //环境状态变化显示
        foreach (var item in e.EnvironmentStateDict)
        {
            float curValue;
            float maxValue;
            if (item.Key == EnvironmentStateEnum.Electricity)
            {
                curValue = StateManager.Instance.Electricity.CurValue;
                maxValue = StateManager.Instance.Electricity.MaxValue;
            }
            else if (item.Key == EnvironmentStateEnum.WaterLevel)
            {
                curValue = StateManager.Instance.WaterLevel.CurValue;
                maxValue = StateManager.Instance.WaterLevel.MaxValue;
            }
            else
            {
                curValue = GameManager.Instance.CurEnvironmentBag.StateDict[item.Key].CurValue;
                maxValue = GameManager.Instance.CurEnvironmentBag.StateDict[item.Key].MaxValue;
            }
            GameObject State = DetailInfo.transform.Find(item.Key.ToString()).gameObject;
            string r = item.Value.ToString();
            if (item.Value > 0)
            {
                r = "+" + r;
                State.transform.Find("Slider/Max").GetComponent<Slider>().value = (curValue + item.Value) / maxValue;
                State.transform.Find("Slider/Max/Fill").GetComponent<Image>().color = ColorManager.green;
                State.transform.Find("Slider/Min").GetComponent<Slider>().value = curValue / maxValue;
                State.transform.Find("Slider/Min/Fill").GetComponent<Image>().color = ColorManager.white;
            }
            else
            {
                State.transform.Find("Slider/Max").GetComponent<Slider>().value = curValue / maxValue;
                State.transform.Find("Slider/Max/Fill").GetComponent<Image>().color = ColorManager.white;
                State.transform.Find("Slider/Min").GetComponent<Slider>().value = (curValue + item.Value) / maxValue;
                State.transform.Find("Slider/Min/Fill").GetComponent<Image>().color = ColorManager.red;
            }
            State.transform.Find("Text").GetComponent<Text>().text = r;
            State.SetActive(true);
        }
        if (e.description != null && e.description != "")
        {
            GameObject Description = DetailInfo.transform.Find("Desc").gameObject;
            Description.transform.Find("Text").GetComponent<Text>().text = e.description;
            Description.SetActive(true);
        }
    }
    public void ShowHint()
    {
        //创建详情区域
        DetailInfo = Instantiate(DetailInfoPrefab, FindObjectOfType<WindowsManager>().transform);
        DetailInfo.transform.position = transform.position + offset;
        GameObject Hint = DetailInfo.transform.Find("Hint").gameObject;
        Hint.transform.Find("Text").GetComponent<Text>().text = e.hint.ToString();
        Hint.SetActive(true);
    }
}
