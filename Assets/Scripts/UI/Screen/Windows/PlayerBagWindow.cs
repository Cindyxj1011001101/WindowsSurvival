public class PlayerBagWindow : BagWindow
{
    protected override void Init()
    {
        DisplayBag(GameManager.Instance.PlayerBag);
    }
}