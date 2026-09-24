namespace BlazorApp.Models;

public enum TicketStatus
{
    Waiting,
    NowServing,
    Completed,
    Skipped
}

public class QueueTicket
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Teller { get; set; } = string.Empty;
    public bool IsPriority { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public TicketStatus Status { get; set; } = TicketStatus.Waiting;
}