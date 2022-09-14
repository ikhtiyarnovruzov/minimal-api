using Mini.WebApi.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TranslationService>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Mini"));

app.MapGet("/translate/{from}/{to}/{key}", async (string from, string to, string key, TranslationService facade) =>
    await facade.TranslateAsync(from, to, key) is string translatedKey
        ? Results.Ok(translatedKey)
        : Results.StatusCode(StatusCodes.Status500InternalServerError));

app.MapGet("/random/{source}/{capacity}", async (string source, int capacity, TranslationService facade) =>
    await facade.RandomAsync(source, capacity) is string translatedKey
        ? Results.Ok(translatedKey)
        : Results.StatusCode(StatusCodes.Status500InternalServerError));

app.Run();