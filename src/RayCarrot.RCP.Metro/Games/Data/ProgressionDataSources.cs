namespace RayCarrot.RCP.Metro.Games.Data;

public class ProgressionDataSources
{
    public ProgressionDataSources() : this(null) { }
    public ProgressionDataSources(Dictionary<string, ProgramDataSource>? dataSources)
    {
        DataSources = dataSources ?? new Dictionary<string, ProgramDataSource>();
    }

    public Dictionary<string, ProgramDataSource> DataSources { get; }
}