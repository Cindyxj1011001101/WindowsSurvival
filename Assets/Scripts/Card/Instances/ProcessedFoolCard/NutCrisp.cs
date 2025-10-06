public class NutCrisp: Card
{
    private NutCrisp()
    {
        Events = new()
        {
            new Event("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 30,
            () => new()
            {
                { PlayerStateEnum.Fullness, 54 },
                { PlayerStateEnum.Health, 5 },
                { PlayerStateEnum.San, 3 },
            })
        };
    }
}   