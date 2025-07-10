using UnityEngine.UI;

public class EnvironmentBagWindow : BagWindow
{
    private Text discoveryDegreeText; // ̽������ʾ
    private Text placeNameText; // �ص�����
    private Text placeDetailsText; // �ص�����
    private Button discoverButton; // ̽����ť

    //private PlaceEnum currentPlace;

    //private Dictionary<PlaceEnum, BagBase> environmentBags = new();

    protected override void Awake()
    {
        base.Awake();

        discoveryDegreeText = transform.Find("LeftBar/DiscoveryDegree").GetComponent<Text>();
        placeNameText = transform.Find("TopBar/Name").GetComponent<Text>();
        placeDetailsText = transform.Find("LeftBar/Details/Text").GetComponent<Text>();
        discoverButton = transform.Find("LeftBar/DiscoverButton").GetComponent<Button>();

        discoverButton.onClick.AddListener(() =>
        {
            EffectResolve.Instance.ResolveExplore();
        });

        // ע��̽���ȱ仯�¼�
        EventManager.Instance.AddListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDicoveryDegreeChanged);
        // ע��ص��ƶ��¼�
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
    }

    private void OnEnable()
    {
        // ���ڼ���ʱ����ʾ��ǰ�ص�
        EffectResolve.Instance.Move(EffectResolve.Instance.CurEnvironmentBag.PlaceData.placeType);
    }

    protected override void Init()
    {
    }

    /// <summary>
    /// �ƶ���ָ���ص�
    /// </summary>
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // ���µص���Ϣ
        discoveryDegreeText.text = $"{curEnvironmentBag.DiscoveryDegree} %";
        placeNameText.text = $"{curEnvironmentBag.PlaceData.placeName}";
        placeDetailsText.text = $"{curEnvironmentBag.PlaceData.placeDesc}";
    }

    private void OnDicoveryDegreeChanged(ChangeDiscoveryDegreeArgs args)
    {
        if (args.place == EffectResolve.Instance.CurEnvironmentBag.PlaceData.placeType)
            discoveryDegreeText.text = $"{args.discoveryDegree} %";
    }

    private void OnDestroy()
    {
        // �Ƴ�ע���¼�
        EventManager.Instance.RemoveListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDicoveryDegreeChanged);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
    }
}
