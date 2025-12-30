/// <summary>
/// 废铁铲
/// </summary>
[CardId("废铁铲")]
public class ScrapShovel : Card
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