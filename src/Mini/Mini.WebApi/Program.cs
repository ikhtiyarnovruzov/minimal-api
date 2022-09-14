using Mini.WebApi.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TranslationService>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Mini"));

app.MapGet("/translate/{key}", async (string key, TranslationService facade) =>
    await facade.TranslateAsync(key) is string translatedKey
        ? Results.Ok(translatedKey)
        : Results.StatusCode(StatusCodes.Status500InternalServerError));

app.Run();