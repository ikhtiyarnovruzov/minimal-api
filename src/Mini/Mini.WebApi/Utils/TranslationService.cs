namespace Mini.WebApi.Utils
{
    public class TranslationService
    {
        public async Task<string> TranslateAsync(string key)
        {
            return await Task.FromResult("Translated " + key);
        }
    }
}
