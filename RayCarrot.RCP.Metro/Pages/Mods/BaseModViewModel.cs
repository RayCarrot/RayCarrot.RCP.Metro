using System.Threading.Tasks;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro
{
    public abstract class BaseModViewModel : BaseRCPViewModel
    {
        public abstract PackIconMaterialKind Icon { get; }
        public abstract LocalizedString Header { get; }
        public abstract object UIContent { get; }

        public abstract Task InitializeAsync();
    }
}