//#nullable disable
namespace RayCarrot.RCP.Metro;

public class ConstLocString : LocalizedString
{
    public ConstLocString(string stringValue) : base(refreshOnCultureChanged: false)
    {
        StringValue = stringValue;

        // Refresh the value
        RefreshValue();
    }

    private string StringValue { get; }

    protected override string GetValue()
    {
        return StringValue;
    }
}