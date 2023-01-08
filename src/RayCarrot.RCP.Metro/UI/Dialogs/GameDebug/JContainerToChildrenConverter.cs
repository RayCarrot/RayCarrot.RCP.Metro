using System.Globalization;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

public class JContainerToChildrenConverter : BaseValueConverter<JContainerToChildrenConverter, JContainer, IEnumerable>
{
    public override IEnumerable ConvertValue(JContainer value, Type targetType, object? parameter, CultureInfo culture)
    {
        List<JToken> children = value.Children().ToList();

        // If there is just one child we might want to show that directly to make the view nicer
        if (children.Count == 1)
        {
            JToken? onlyChild = children.First();

            if (onlyChild is JObject or JArray)
                return onlyChild.Children();
        }

        return children;
    }
}