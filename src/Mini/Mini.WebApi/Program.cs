using Microsoft.AspNetCore.Mvc;

using Mini.WebApi.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TranslationService>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Mini"));

app.MapGet("/translate/{from}/{to}/{key}", async (string from, string to, string key, TranslationService facade) =>
    await facade.TranslateAsync(from, to, key) is string translatedKey
        ? Results.Ok(translatedKey)
        : Results.StatusCode(StatusCodes.Status500InternalServerError));

app.MapGet("/random/{source:int}/{capacity}", async (string source, int? capacity, TranslationService facade) =>
    await facade.RandomAsync(source, capacity ?? 10) is string translatedKey
        ? Results.Ok(translatedKey)
        : Results.StatusCode(StatusCodes.Status500InternalServerError));

app.MapPost("/translate/{from}/{to}", async (string from, string to, [FromBody] string text, TranslationService facade) =>
    await facade.TranslateTextAsync(from, to, text) is string translatedKey
        ? Results.Ok(translatedKey)
        : Results.StatusCode(StatusCodes.Status500InternalServerError));

app.Run();