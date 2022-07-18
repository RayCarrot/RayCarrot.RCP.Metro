using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro.Patcher;

public abstract class PatchViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    protected PatchViewModel(PatcherViewModel patcherViewModel)
    {
        PatcherViewModel = patcherViewModel;
    }

    #endregion

    #region Public Properties

    public PatcherViewModel PatcherViewModel { get; }
    public abstract string ID { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }
    public ImageSource? Thumbnail { get; set; }

    #endregion

    #region Public Methods

    public virtual void Dispose() { }

    #endregion
}