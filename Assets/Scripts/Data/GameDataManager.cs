public class GameDataManager
{
    private static GameDataManager instance = new();
    public static GameDataManager Instance => instance;

    public PlayerBagRuntimeData PlayerBagRuntimeData { get; private set; }

    public GameDataManager()
    {
        PlayerBagRuntimeData = new();
        EnvironmentBagRuntimeData = new();
    }

    public void RecordPlayerBagRuntimeData(PlayerBagRuntimeData runtimeData)
    {
        this.PlayerBagRuntimeData = runtimeData;
    }


    public EnvironmentBagRuntimeData EnvironmentBagRuntimeData { get; private set; }

    public void RecordEnvironmentBagRuntimeData(EnvironmentBagRuntimeData runtimeData)
    {
        this.EnvironmentBagRuntimeData = runtimeData;
    }
}