//V1
public class AudioData:VersionMigrator
{
    public float masterVolume;
    public float bgmVolume;
    public float sfxVolume;
    public override IVersionMigrator ReadJSON(string FilePath,string FileName)
    {
        return JsonManager.LoadData<AudioData>(FilePath,FileName);
    }
}