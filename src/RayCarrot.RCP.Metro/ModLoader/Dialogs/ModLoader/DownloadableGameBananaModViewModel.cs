using System.Text.RegularExpressions;
using System.Windows.Input;
using RayCarrot.RCP.Metro.GameBanana;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableGameBananaModViewModel : BaseViewModel
{
    public DownloadableGameBananaModViewModel(GameBananaMod mod)
    {
        Mod = mod;

        Name = mod.Name;
        UploaderUserName = mod.Submitter?.Name ?? String.Empty;
        UploadDate = mod.DateAdded;
        Description = mod.Description ?? String.Empty;
        Text = RemoveHthmlFromString(mod.Text);

        if (mod.PreviewMedia?.Images is { Length: > 0 } images)
        {
            GameBananaImage img = images[0];

            // Use the 220px version if it exists
            if (img.File220 != null)
            {
                ImageUrl = img.BaseUrl + "/" + img.File220;
                ImageWidth = img.File220Width;
                ImageHeight = img.File220Height;
            }
            else
            {
                ImageUrl = img.BaseUrl + "/" + img.File;
                ImageWidth = 220;
                ImageHeight = Double.NaN;
            }
        }

        LikesCount = mod.LikeCount;
        DownloadsCount = mod.DownloadCount;
        ViewsCount = mod.ViewCount;

        OpenInGameBananaCommand = new RelayCommand(OpenInGameBanana);
    }

    public ICommand OpenInGameBananaCommand { get; }

    public GameBananaMod Mod { get; }

    public string Name { get; }
    public string UploaderUserName { get; }
    public DateTime UploadDate { get; }
    public string Description { get; }
    public string Text { get; }
    
    public string? ImageUrl { get; }
    public double ImageWidth { get; }
    public double ImageHeight { get; }

    public int LikesCount { get; }
    public int DownloadsCount { get; }
    public int ViewsCount { get; }

    private static string RemoveHthmlFromString(string? html)
    {
        if (html == null)
            return String.Empty;

        // Linebreaks
        html = html.Replace("<br>", Environment.NewLine);
        html = html.Replace(@"</li>", Environment.NewLine);
        html = html.Replace(@"</h3>", Environment.NewLine);
        html = html.Replace(@"</h2>", Environment.NewLine);
        html = html.Replace(@"</h1>", Environment.NewLine);
        html = html.Replace("<ul>", Environment.NewLine);
        
        // Bullet point
        html = html.Replace("<li>", "• ");
        
        // Unique spaces
        html = html.Replace("&nbsp;", " ");
        html = html.Replace(@"\u00a0", " ");

        // Unique characters
        html = html.Replace("&amp;", "&");
        html = html.Replace("&gt;", ">");
        
        // Remove tabs
        html = html.Replace("\t", String.Empty);
        
        // Remove all remaining html tags
        html = Regex.Replace(html, "<.*?>", String.Empty);
        
        // Convert newlines of 3 or more to 2 newlines
        html = Regex.Replace(html, "[\\r\\n]{3,}", "\n\n", RegexOptions.Multiline);
        
        // Trim extra whitespace at start and end
        return html.Trim();
    }

    public void OpenInGameBanana()
    {
        Services.App.OpenUrl($"https://gamebanana.com/mods/{Mod.Id}");
    }
}