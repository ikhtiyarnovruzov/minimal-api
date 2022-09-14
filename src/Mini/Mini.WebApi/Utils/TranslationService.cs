using Mini.WebApi.Models;

using Newtonsoft.Json;

using System.Text.RegularExpressions;

namespace Mini.WebApi.Utils;

public sealed class TranslationService
{
    private readonly string _pattern = @"source\.[a-z]{2}\.json";
    private SourceModel[] _sources;

    public TranslationService()
    {
        _sources = GetSources().GetAwaiter().GetResult();
    }

    private async Task<SourceModel[]> GetSources()
    {
        try
        {
            var files = Directory.GetFiles(Environment.CurrentDirectory);
            if (files?.Length > 0)
            {
                foreach (var file in files)
                {
                    if (Regex.IsMatch(Path.GetFileName(file), _pattern))
                    {
                        var sourcesString = await File.ReadAllTextAsync(file);
                        var sourceModels = JsonConvert.DeserializeObject<SourceModel[]>(sourcesString);
                        return sourceModels ?? Array.Empty<SourceModel>();
                    }
                }
            }
        }
        catch { }

        return Array.Empty<SourceModel>();
    }

    public async Task<string> TranslateAsync(string from, string to, string key)
    {
        return await Task.Run(() =>
        {
            return "Translated " + key;
        });
    }
}
