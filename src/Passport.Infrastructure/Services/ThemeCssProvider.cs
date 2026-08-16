using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Passport.Infrastructure.Services;

internal sealed partial class ThemeCssProvider
{
    private readonly Dictionary<string, string> _cssCache = [];
    private readonly Dictionary<string, string> _urlCache = [];
    private static readonly string[] ThemeKeys = ["obsidian", "pearl", "matrix", "vapor"];

    [GeneratedRegex(@"^(obsidian|pearl|matrix|vapor)-[a-zA-Z0-9]+\.css$")]
    private static partial Regex ThemeFilePattern();

    public ThemeCssProvider(string wwwrootPath)
    {
        var assetsDir = Path.Combine(wwwrootPath, "assets");
        if (!Directory.Exists(assetsDir))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(assetsDir, "*.css"))
        {
            var fileName = Path.GetFileName(file);
            var match = ThemeFilePattern().Match(fileName);
            if (!match.Success)
            {
                continue;
            }

            var key = match.Groups[1].Value;
            _cssCache[key] = File.ReadAllText(file);
            _urlCache[key] = $"/assets/{fileName}";
        }
    }

    public bool TryGetCss(string themeKey, [NotNullWhen(true)] out string? css)
    {
        return _cssCache.TryGetValue(themeKey, out css);
    }

    public string GetThemeUrlsScript()
    {
        var json = JsonSerializer.Serialize(_urlCache);
        return $"<script>window.__gestaltThemeUrls = {json};</script>";
    }

    public string ResolveThemeKey(string? cookieValue)
    {
        if (!string.IsNullOrEmpty(cookieValue) && _cssCache.ContainsKey(cookieValue))
        {
            return cookieValue;
        }

        return "obsidian";
    }

    public int ThemeCount => _cssCache.Count;
}