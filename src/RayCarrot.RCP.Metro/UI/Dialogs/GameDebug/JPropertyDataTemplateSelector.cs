using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

public sealed class JPropertyDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? PrimitivePropertyTemplate { get; set; }
    public DataTemplate? ArrayPropertyTemplate { get; set; }
    public DataTemplate? ObjectPropertyTemplate { get; set; }
    public DataTemplate? ValueTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item switch
        {
            null => null,
            JProperty jProperty => jProperty.Value.Type switch
            {
                JTokenType.Object => ObjectPropertyTemplate,
                JTokenType.Array => ArrayPropertyTemplate,
                _ => PrimitivePropertyTemplate
            },
            JValue => ValueTemplate,
            _ => null
        };
    }
}