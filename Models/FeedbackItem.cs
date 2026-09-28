namespace BlazorApp.Models;

public class FeedbackItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string StudentName { get; set; } = "Anonymous Wildcat";
    public string Department { get; set; } = "Registrar Office";
    public string? TicketNumber { get; set; }
    public int Rating { get; set; } = 5;
    public string Comment { get; set; } = string.Empty;
    public List<string> SelectedTags { get; set; } = new();
    public DateTime SubmittedAt { get; set; } = DateTime.Now;
}