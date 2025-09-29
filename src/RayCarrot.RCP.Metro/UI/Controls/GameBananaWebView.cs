using System.Windows;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace RayCarrot.RCP.Metro;

public class GameBananaWebView : WebView2CompositionControl
{
    #region Constructor

    public GameBananaWebView()
    {
        DefaultBackgroundColor = System.Drawing.Color.Transparent;

        // When using the composition control it results in a blurry. This is a bit hacky, but makes it clearer. 
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

        Loaded += GameBananaWebView_OnLoaded;
        CoreWebView2InitializationCompleted += GameBananaWebView_OnCoreWebView2InitializationCompleted;
        NavigationStarting += GameBananaWebView_OnNavigationStarting;
    }

    #endregion

    #region Dependency Properties

    public string Html
    {
        get => (string)GetValue(HtmlProperty);
        set => SetValue(HtmlProperty, value);
    }

    public static readonly DependencyProperty HtmlProperty = DependencyProperty.Register(
        name: nameof(Html), 
        propertyType: typeof(string), 
        ownerType: typeof(GameBananaWebView),
        typeMetadata: new FrameworkPropertyMetadata(GameBananaWebView_OnHtmlChanged));

    public Color ForegroundColor
    {
        get => (Color)GetValue(ForegroundColorProperty);
        set => SetValue(ForegroundColorProperty, value);
    }

    public static readonly DependencyProperty ForegroundColorProperty = DependencyProperty.Register(
        name: nameof(ForegroundColor), 
        propertyType: typeof(Color), 
        ownerType: typeof(GameBananaWebView),
        typeMetadata: new FrameworkPropertyMetadata(GameBananaWebView_OnColorChanged));

    public Color HighlightColor
    {
        get => (Color)GetValue(HighlightColorProperty);
        set => SetValue(HighlightColorProperty, value);
    }

    public static readonly DependencyProperty HighlightColorProperty = DependencyProperty.Register(
        name: nameof(HighlightColor), 
        propertyType: typeof(Color), 
        ownerType: typeof(GameBananaWebView),
        typeMetadata: new FrameworkPropertyMetadata(GameBananaWebView_OnColorChanged));

    public Color GreenColor
    {
        get => (Color)GetValue(GreenColorProperty);
        set => SetValue(GreenColorProperty, value);
    }

    public static readonly DependencyProperty GreenColorProperty = DependencyProperty.Register(
        name: nameof(GreenColor), 
        propertyType: typeof(Color), 
        ownerType: typeof(GameBananaWebView),
        typeMetadata: new FrameworkPropertyMetadata(GameBananaWebView_OnColorChanged));

    public Color RedColor
    {
        get => (Color)GetValue(RedColorProperty);
        set => SetValue(RedColorProperty, value);
    }

    public static readonly DependencyProperty RedColorProperty = DependencyProperty.Register(
        name: nameof(RedColor), 
        propertyType: typeof(Color), 
        ownerType: typeof(GameBananaWebView),
        typeMetadata: new FrameworkPropertyMetadata(GameBananaWebView_OnColorChanged));

    #endregion

    #region Event Handlers

    private async void GameBananaWebView_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Since we're not setting the Source property we have to manually initialize the core
        await EnsureCoreWebView2Async(null);
    }

    private void GameBananaWebView_OnCoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        CoreWebView2.Settings.IsZoomControlEnabled = false;
        CoreWebView2.Settings.IsPinchZoomEnabled = false;
        CoreWebView2.Settings.IsStatusBarEnabled = false;
    }

    private void GameBananaWebView_OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
        // Open externally, unless it's local data (like when we navigate to a string)
        if (!e.Uri.StartsWith("data"))
        {
            e.Cancel = true;
            Services.App.OpenUrl(e.Uri);
        }
    }

    private static async void GameBananaWebView_OnHtmlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is GameBananaWebView webView && e.NewValue is string html)
        {
            // Initialize the core
            await webView.EnsureCoreWebView2Async(null);

            // Update the color scheme
            webView.UpdateColorScheme();

            // Navigate to the string
            webView.NavigateToString(webView.StyleHtml(html));
        }
    }

    private static async void GameBananaWebView_OnColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is GameBananaWebView { Html: { } html } webView)
        {
            // Initialize the core
            await webView.EnsureCoreWebView2Async(null);

            // Update the color scheme
            webView.UpdateColorScheme();

            // Navigate to the string
            webView.NavigateToString(webView.StyleHtml(html));
        }
    }

    #endregion

    #region Private Methods

    private static string ColorToHtml(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    private void UpdateColorScheme()
    {
        CoreWebView2.Profile.PreferredColorScheme = Services.Data.Theme_DarkMode
            ? CoreWebView2PreferredColorScheme.Dark
            : CoreWebView2PreferredColorScheme.Light;
    }

    private string StyleHtml(string html)
    {
        // Get colors
        string backgroundHighlightColor = ColorToHtml(HighlightColor);
        string foregroundColor = ColorToHtml(ForegroundColor);
        string greenColor = ColorToHtml(GreenColor);
        string redColor = ColorToHtml(RedColor);

        // Create the css to style it
        string css = $$"""
                       body {
                           font: 12px segoe ui, sans-serif;
                           overflow-wrap: break-word;
                           user-select: none;
                           color: {{foregroundColor}};
                           a { color: {{foregroundColor}} }
                           ul,ol { padding: 0 0 0 20 };
                           hr { height: 1px; border-width: 0; background-color: {{foregroundColor}} };
                           pre { border: 2px solid {{backgroundHighlightColor}}; padding: 5px; border-radius: 5px }
                       }

                       .GreenColor { color: {{greenColor}}; }
                       .RedColor { color: {{redColor}}; }

                       .Spoiler { background: {{backgroundHighlightColor}}; color: transparent; border-radius: 5px; transition: color 0.2s ease, background 0.2s ease; }
                       .Spoiler:hover { background: none; color: {{foregroundColor}}; }

                       img { max-width: 100% }
                       iframe { max-width: 100% }
                       """;

        // Return full html page
        return $"""
                <style>
                {css}
                </style>
                {html}
                """;
    }

    #endregion
}