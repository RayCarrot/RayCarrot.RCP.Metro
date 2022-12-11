using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class Utility_Decoders_TypeViewModel : BaseRCPViewModel, IDisposable
{
    public Utility_Decoders_TypeViewModel(
        LocalizedString name, 
        IStreamEncoder encoder, 
        Func<string> getFileFilter, 
        params GameSearch.Predicate[] gameSearchPredicates)
    {
        Name = name;
        Encoder = encoder;
        GetFileFilter = getFileFilter;
        GameSearchPredicates = gameSearchPredicates;
    }

    public LocalizedString Name { get; }
    public IStreamEncoder Encoder { get; }
    public Func<string> GetFileFilter { get; }
    public GameSearch.Predicate[] GameSearchPredicates { get; }

    public void Dispose()
    {
        Name.Dispose();
    }
}