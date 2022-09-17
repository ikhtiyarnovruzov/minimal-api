using Mini.WebApi.Models;

using Newtonsoft.Json;

namespace Mini.WebApi.Middlewares
{
    public class LimitedSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public LimitedSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
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

            await _next.Invoke(context);
        }
    }
    public static class LimitedSessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseLimitedSessionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LimitedSessionMiddleware>();
        }
    }
}
