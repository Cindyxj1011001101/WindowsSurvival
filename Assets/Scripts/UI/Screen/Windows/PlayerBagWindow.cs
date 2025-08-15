public class PlayerBagWindow : BagWindow
{
    protected override void Init()
    {
        DisplayBag(GameDataManager.Instance.PlayerBagData);
    }
}