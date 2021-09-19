using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A dialog base manager for managing dialogs
    /// </summary>
    public interface IDialogBaseManager
    {
        Task<Result> ShowDialogWindowAsync<UserInput, Result>(IDialogWindowControl<UserInput, Result> windowContent) 
            where UserInput : UserInputViewModel
            where Result : UserInputResult;

        Task ShowWindowAsync(IWindowControl windowContent, ShowWindowFlags flags = ShowWindowFlags.None, params string[] groupNames);
    }
}