namespace TriviaApp.API.Models;

public class Support
{
    public int SupportId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public bool IsOpen { get; set; } = true;
    public DateTime? ClosedOn { get; set; }
}
