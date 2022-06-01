using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

public class GameIcon : Control
{
    public ImageSource Source
    {
        get => (ImageSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(ImageSource), typeof(GameIcon), new PropertyMetadata(default(ImageSource)));

    public GameIconSize IconSize
    {
        get => (GameIconSize)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize), typeof(GameIconSize), typeof(GameIcon), new PropertyMetadata(GameIconSize.Default));

    public bool IsDemo
    {
        get => (bool)GetValue(IsDemoProperty);
        set => SetValue(IsDemoProperty, value);
    }

    public static readonly DependencyProperty IsDemoProperty = DependencyProperty.Register(
        nameof(IsDemo), typeof(bool), typeof(GameIcon), new PropertyMetadata(false));

    public enum GameIconSize
    {
        /// <summary>
        /// 128x128
        /// </summary>
        Default,

        /// <summary>
        /// 64x64
        /// </summary>
        Small,

        /// <summary>
        /// 32x32
        /// </summary>
        Smallest,
    }
}