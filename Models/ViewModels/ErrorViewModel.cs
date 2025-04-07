namespace BlogApp.Models.ViewModels
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; } = string.Empty;
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ErrorMessage { get; set; } = string.Empty;
        public int? StatusCode { get; set; }
    }
} 