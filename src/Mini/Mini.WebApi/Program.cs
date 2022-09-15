using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Mini.WebApi.Models;
using Mini.WebApi.Utils;

using Newtonsoft.Json;

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

app.Use(async (context, next) =>
{
    var ipAddress = context.Connection.RemoteIpAddress.ToString();

    if (context.Session.GetString(ipAddress) == null)
    {
        SessionModel sessionModel = new()
        {
            IPAddress = ipAddress,
            LastAccessDate = DateTime.UtcNow,
        };
        context.Session.SetString(ipAddress, JsonConvert.SerializeObject(sessionModel));
    }
    else
    {
        var sessionModel = JsonConvert.DeserializeObject<SessionModel>(context.Session.GetString(ipAddress));
        var waiting = DateTime.UtcNow - sessionModel.LastAccessDate;

        if (waiting < new TimeSpan(0, 0, 5))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            var diff = 5 - waiting.Seconds;
            var message = $"Wait for {(diff <= 1 ? "1 second" : $"{diff} seconds")}";
            await context.Response.WriteAsync(message);
            return;
        }
        else
        {
            sessionModel.LastAccessDate = DateTime.UtcNow;
            context.Session.Remove(ipAddress);
            context.Session.SetString(ipAddress, JsonConvert.SerializeObject(sessionModel));
        }
    }

    await next.Invoke();
});

app.MapGet("/", () => Results.Ok("Mini"));

app.MapGet("/sources", (TranslationService facade) => facade.GetSources() is string[] sources
        ? Results.Ok(sources)
        : Results.NotFound());

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