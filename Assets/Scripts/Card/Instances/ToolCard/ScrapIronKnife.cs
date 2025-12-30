/// <summary>
/// 废铁刀
/// </summary>
[CardId("废铁刀")]
public class ScrapIronKnife : Card
{
    protected override void OnLateConstructor()
    {
        base.OnLateConstructor();
        if (TryGetComponent<WeaponComponent>(out var weapon))
        {
            weapon.attackSound = "金属刀攻击声";
        }
    }
}