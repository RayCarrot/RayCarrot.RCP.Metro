namespace RayCarrot.RCP.Metro;

public class BindableAsyncLazy<T> : BaseViewModel
    where T : class
{
    public BindableAsyncLazy(Func<Task<T?>> valueFactory)
    {
        _valueFactory = valueFactory;
    }

    private Func<Task<T?>>? _valueFactory;
    private T? _value;
    private Task<T?>? _runningValueTask;

    public T? Value
    {
        get
        {
            // Return the value if we have one
            if (_value != null)
                return _value;

            // Return null if the task is running
            if (_runningValueTask != null)
                return null;

            // If we've already run the task and it returned null
            // we want to return null here as well
            if (_valueFactory == null)
                return null;

            // Retrieve the value
            RetrieveValue();

            // Return null for now
            return null;
        }
    }

    private async void RetrieveValue()
    {
        _runningValueTask = _valueFactory!();
        _value = await _runningValueTask;
        _runningValueTask = null;
        _valueFactory = null; // Avoid running multiple times if it returns null

        OnPropertyChanged(nameof(Value));
    }
}