namespace Mini.WebApi.Models;

public sealed class SessionModel
{
    public string IPAddress { get; set; }
    public DateTime LastAccessDate { get; set; }
}
