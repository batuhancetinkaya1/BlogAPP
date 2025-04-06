using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models;

public class ErrorViewModel
{
    public int? StatusCode { get; set; }
    public string? Message { get; set; }
    public string? DetailedMessage { get; set; }
    public string? RequestId { get; set; }
    public bool ShowRequestId { get; set; }
} 