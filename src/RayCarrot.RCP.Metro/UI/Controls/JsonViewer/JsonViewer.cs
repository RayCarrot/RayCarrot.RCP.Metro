using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

public class JsonViewer : Control
{
    public JToken Source
    {
        get => (JToken)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(JToken), typeof(JsonViewer));
}