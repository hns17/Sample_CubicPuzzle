public class StageListData
{
    public string StageName { get; }
    public string StagePath { get; }

    public StageListData(string stageName, string stagePath)
    {
        StageName = stageName;
        StagePath = stagePath;
    }
}
