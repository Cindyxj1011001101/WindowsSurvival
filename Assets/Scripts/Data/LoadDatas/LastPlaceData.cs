public class LastPlaceData:VersionMigrator
{
    public int lastPlace=-1;
    public PlaceEnum LastPlace => (PlaceEnum)lastPlace;
    public override IVersionMigrator ReadJSON(string FilePath,string FileName)
    {
        return JsonManager.LoadData<LastPlaceData>(FilePath, FileName);
    }
}