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
            var files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Sources"));
            if (files?.Length > 0)
            {
                var sourceModels = new List<SourceModel>();
                foreach (var file in files)
                {
                    if (Regex.IsMatch(Path.GetFileName(file), _pattern))
                    {
                        var sourcesString = await File.ReadAllTextAsync(file);
                        var sourceModel = JsonConvert.DeserializeObject<SourceModel>(sourcesString);

                        if (sourceModel != null)
                        {
                            sourceModels.Add(sourceModel);
                        }
                    }
                }
                return sourceModels.ToArray();
            }
        }
        catch { }

        return Array.Empty<SourceModel>();
    }

    public async Task<string> TranslateAsync(string from, string to, string key)
    {
        return await Task.Run(() =>
        {
            var fromSource = _sources.FirstOrDefault(x => x.Code.Equals(from, StringComparison.OrdinalIgnoreCase));
            var toSource = _sources.FirstOrDefault(x => x.Code.Equals(to, StringComparison.OrdinalIgnoreCase));

            if (fromSource != null && toSource != null)
            {
                for (int i = 0; i < fromSource.Keys.Length; i++)
                {
                    if (fromSource.Keys[i].Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        return toSource.Keys[i].Key;
                    }
                }
            }

            return string.Empty;
        });
    }

    public async Task<string> TranslateTextAsync(string from, string to, string text)
    {
        if (string.IsNullOrEmpty(from))
        {
            throw new ArgumentNullException(nameof(from));
        }

        if (string.IsNullOrEmpty(to))
        {
            throw new ArgumentNullException(nameof(from));
        }

        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentNullException(nameof(from));
        }

        if (text.Length > 250)
        {
            throw new ArgumentOutOfRangeException(nameof(from));
        }

        return await Task.Run(() =>
        {
            var fromSource = _sources.FirstOrDefault(x => x.Code.Equals(from, StringComparison.OrdinalIgnoreCase));
            var toSource = _sources.FirstOrDefault(x => x.Code.Equals(to, StringComparison.OrdinalIgnoreCase));

            if (fromSource != null && toSource != null)
            {
                var keys = text.Split(new char[] { ' ', '_' }).ToArray();
                var translatedKeys = new List<string>();

                for (int k = 0; k < keys.Length; k++)
                {
                    var key = keys[k];

                    for (int i = 0; i < fromSource.Keys.Length; i++)
                    {
                        if (fromSource.Keys[i].Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            translatedKeys.Add(toSource.Keys[i].Key);
                            break;
                        }
                    }
                }

                return string.Join(" ", translatedKeys);
            }

            return string.Empty;
        });
    }

    public async Task<string> RandomAsync(string source, int capacity)
    {
        return await Task.Run(() =>
        {
            var theSource = _sources.FirstOrDefault(x => x.Code.Equals(source, StringComparison.OrdinalIgnoreCase));

            if (theSource == null)
            {
                return string.Empty;
            }

            var result = new List<string>();
            var random = new Random();

            for (int i = 0; i < capacity; i++)
            {
                var index = random.Next(0, 1000);
                result.Add(theSource.Keys[index].Key);
            }

            return string.Join(" ", result);
        });
    }
}
