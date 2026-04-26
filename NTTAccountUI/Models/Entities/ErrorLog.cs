namespace NTTAccountUI.Models.Entities;

public class ErrorLog
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? Source { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}