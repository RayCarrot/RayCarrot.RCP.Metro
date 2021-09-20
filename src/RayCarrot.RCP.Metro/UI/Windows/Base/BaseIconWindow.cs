using System.Windows;

namespace RayCarrot.RCP.Metro
{
    public class BaseIconWindow : BaseWindow
    {
        public GenericIconKind GenericIcon
        {
            get => (GenericIconKind)GetValue(GenericIconProperty);
            set => SetValue(GenericIconProperty, value);
        }

        public static readonly DependencyProperty GenericIconProperty = DependencyProperty.Register(
            nameof(GenericIcon), typeof(GenericIconKind), typeof(BaseWindow), new PropertyMetadata(default(GenericIconKind)));
    }
}