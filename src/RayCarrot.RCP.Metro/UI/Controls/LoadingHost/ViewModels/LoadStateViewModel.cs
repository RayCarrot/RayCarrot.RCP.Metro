namespace RayCarrot.RCP.Metro;

public class LoadStateViewModel : BaseViewModel
{
    public string? Status { get; set; }

    public bool HasProgress { get; set; }
    public double CurrentProgress { get; set; }
    public double MinProgress { get; set; }
    public double MaxProgress { get; set; }

    public void SetProgress(Progress progress)
    {
        HasProgress = true;
        CurrentProgress = progress.Current;
        MinProgress = progress.Min;
        MaxProgress = progress.Max;
    }
}