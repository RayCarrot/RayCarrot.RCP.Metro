using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    public abstract class BaseModViewModel : BaseRCPViewModel
    {
        public abstract GenericIconKind Icon { get; }
        public abstract LocalizedString Header { get; }
        public abstract object UIContent { get; }

        public abstract Task InitializeAsync();
    }
}