using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Mini.WebApi.Middlewares;
using Mini.WebApi.Utils;

using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddSingleton<TranslationService>();

var app = builder.Build();

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        context.Response.ContentType = Text.Plain;
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var handlerFeature = context.Features.Get<IExceptionHandlerPathFeature>();

        await context.Response.WriteAsync(handlerFeature.Error.Message);
    });
});

app.UseSession();

app.UseLimitedSessionMiddleware();

app.MapGet("/", () => Results.Ok("Mini"));

app.MapGet("/sources", (TranslationService facade) => facade.GetSources() is string[] sources
        ? Results.Ok(sources)
        : Results.NotFound());

app.MapGet("/translate/{from}/{to}/{key}", async (string from, string to, string key, TranslationService facade) =>
    await facade.TranslateAsync(from, to, key) is string translatedKey
        ? Results.Ok(translatedKey)
        : Results.StatusCode(StatusCodes.Status500InternalServerError));

app.MapGet("/random/{source}/{capacity}", async (string source, int? capacity, TranslationService facade) =>
    await facade.RandomAsync(source, capacity ?? 10) is string translatedKey
        ? Results.Ok(translatedKey)
        : Results.StatusCode(StatusCodes.Status500InternalServerError));

app.MapPost("/translate/{from}/{to}", async (string from, string to, [FromBody] string text, TranslationService facade) =>
    await facade.TranslateTextAsync(from, to, text) is string translatedKey
        ? Results.Ok(translatedKey)
        : Results.StatusCode(StatusCodes.Status500InternalServerError));

app.Run();