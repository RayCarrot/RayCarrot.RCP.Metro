namespace RayCarrot.RCP.Metro;

public class EditorFieldGroupViewModel : BaseViewModel
{
    public EditorFieldGroupViewModel(LocalizedString header, IEnumerable<EditorFieldViewModel> editorFields)
    {
        Header = header;
        EditorFields = new ObservableCollection<EditorFieldViewModel>(editorFields);
    }

    public LocalizedString Header { get; }
    public ObservableCollection<EditorFieldViewModel> EditorFields { get; }

    public void Refresh()
    {
        foreach (EditorFieldViewModel field in EditorFields)
            field.Refresh();
    }
}