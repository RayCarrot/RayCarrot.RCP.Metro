using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

// TODO: Refactor this by combining BindableOperation and Operation. Rather than creating a disposable we can pass in an Action. This
//       will allow us to have an option where it will return and not run the action if it's already running rather than just waiting
//       for the lock to release. We should also avoid all of these actions for setting text, progress etc.

public class BindableOperation : BaseViewModel
{
    public BindableOperation()
    {
        _operation = new Operation(
            startAction: x =>
            {
                LoadingMessage = x;
                IsLoading = true;
            },
            disposeAction: () =>
            {
                HasProgress = false;
                LoadingMessage = null;
                IsLoading = false;
            },
            textUpdatedAction: x =>
            {
                IsLoading = false;
                LoadingMessage = x;
                IsLoading = true;
            },
            progressUpdatedAction: x =>
            {
                HasProgress = true;
                MinProgress = x.Min;
                MaxProgress = x.Max;
                CurrentProgress = x.Current;
            });
    }

    private readonly Operation _operation;

    public string? LoadingMessage { get; set; }
    public bool IsLoading { get; set; }

    public double CurrentProgress { get; set; }
    public double MinProgress { get; set; }
    public double MaxProgress { get; set; }
    public bool HasProgress { get; set; }

    public Task<DisposableOperation> RunAsync(string? displayStatus = null) => _operation.RunAsync(displayStatus);
}