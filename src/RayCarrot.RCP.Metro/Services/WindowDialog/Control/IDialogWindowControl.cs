#nullable disable
namespace RayCarrot.RCP.Metro;

public interface IDialogWindowControl<out UserInput, out Result> : IWindowControl
    where UserInput : UserInputViewModel
    where Result : UserInputResult
{
    UserInput ViewModel { get; }
    Result GetResult();
}