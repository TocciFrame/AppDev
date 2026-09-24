using BlazorApp.Models;

namespace BlazorApp.Services;

public class QueueService
{
    public event Action? OnChange;
    public event Action<QueueTicket>? OnTicketCalled; // 👈 Audio event trigger

    private int _registrarCounter = 16;
    private int _financeCounter = 8;
    private int _etoCounter = 5;

    public List<QueueTicket> Tickets { get; private set; } = new();
    public List<QueueTicket> History { get; private set; } = new();

    public QueueService()
    {
        SeedInitialData();
    }

    public void SeedInitialData()
    {
        Tickets.Clear();
        History.Clear();

        Tickets.AddRange(new[]
        {
            new QueueTicket { TicketNumber = "REG-0013", Department = "Registrar Office", Service = "Clearance Signing", StudentName = "Jake Bajenting", StudentId = "20-1123-456", Teller = "Teller 2", CreatedAt = DateTime.Now.AddMinutes(-15), Status = TicketStatus.NowServing },
            new QueueTicket { TicketNumber = "REG-P0014", Department = "Registrar Office", Service = "Transcript of Records", StudentName = "Maria Santos", StudentId = "21-0452-119", Teller = "Teller 2", IsPriority = true, CreatedAt = DateTime.Now.AddMinutes(-10), Status = TicketStatus.Waiting },
            new QueueTicket { TicketNumber = "REG-0015", Department = "Registrar Office", Service = "Enrollment Validation", StudentName = "Juan Dela Cruz", StudentId = "22-9981-302", Teller = "Teller 2", CreatedAt = DateTime.Now.AddMinutes(-5), Status = TicketStatus.Waiting },
            new QueueTicket { TicketNumber = "FIN-0006", Department = "Finance and Accounting Office", Service = "Tuition Payment", StudentName = "Ana Reyes", StudentId = "19-4402-981", Teller = "Window 1", CreatedAt = DateTime.Now.AddMinutes(-20), Status = TicketStatus.NowServing },
            new QueueTicket { TicketNumber = "FIN-0007", Department = "Finance and Accounting Office", Service = "Refund Request", StudentName = "Carlos Tan", StudentId = "21-3329-874", Teller = "Window 1", CreatedAt = DateTime.Now.AddMinutes(-12), Status = TicketStatus.Waiting },
            new QueueTicket { TicketNumber = "ETO-0004", Department = "Enrollment Technical Office", Service = "Change of Subject", StudentName = "David Sy", StudentId = "23-0192-882", Teller = "Window 2", CreatedAt = DateTime.Now.AddMinutes(-8), Status = TicketStatus.NowServing }
        });

        NotifyStateChanged();
    }

    public QueueTicket CreateTicket(string department, string service, string studentName, string studentId, bool isPriority = false)
    {
        string prefix = department switch
        {
            "Registrar Office" => isPriority ? "REG-P" : "REG",
            "Finance and Accounting Office" => isPriority ? "FIN-P" : "FIN",
            _ => isPriority ? "ETO-P" : "ETO"
        };

        int count = department switch
        {
            "Registrar Office" => ++_registrarCounter,
            "Finance and Accounting Office" => ++_financeCounter,
            _ => ++_etoCounter
        };

        var ticket = new QueueTicket
        {
            TicketNumber = $"{prefix}-{count:D4}",
            Department = department,
            Service = service,
            StudentName = studentName,
            StudentId = studentId,
            IsPriority = isPriority,
            CreatedAt = DateTime.Now,
            Status = TicketStatus.Waiting
        };

        Tickets.Add(ticket);
        NotifyStateChanged();
        return ticket;
    }

    public void CompleteServingTicket(string ticketNumber)
    {
        var ticket = Tickets.FirstOrDefault(t => t.TicketNumber == ticketNumber);
        if (ticket != null)
        {
            ticket.Status = TicketStatus.Completed;
            History.Insert(0, ticket);
            Tickets.Remove(ticket);

            var nextTicket = Tickets
                .Where(t => t.Department == ticket.Department && t.Status == TicketStatus.Waiting)
                .OrderByDescending(t => t.IsPriority)
                .ThenBy(t => t.CreatedAt)
                .FirstOrDefault();

            if (nextTicket != null)
            {
                nextTicket.Status = TicketStatus.NowServing;
                nextTicket.Teller = ticket.Teller;
                OnTicketCalled?.Invoke(nextTicket);
            }
            NotifyStateChanged();
        }
    }

    public void SkipTicket(string ticketNumber)
    {
        var ticket = Tickets.FirstOrDefault(t => t.TicketNumber == ticketNumber);
        if (ticket != null)
        {
            ticket.Status = TicketStatus.Skipped;
            History.Insert(0, ticket);
            Tickets.Remove(ticket);

            var nextTicket = Tickets
                .Where(t => t.Department == ticket.Department && t.Status == TicketStatus.Waiting)
                .OrderByDescending(t => t.IsPriority)
                .ThenBy(t => t.CreatedAt)
                .FirstOrDefault();

            if (nextTicket != null)
            {
                nextTicket.Status = TicketStatus.NowServing;
                nextTicket.Teller = ticket.Teller;
                OnTicketCalled?.Invoke(nextTicket);
            }
            NotifyStateChanged();
        }
    }

    public void CallNext(string department, string tellerName, string? specificTicketNumber = null)
    {
        QueueTicket? nextTicket;

        if (!string.IsNullOrEmpty(specificTicketNumber))
        {
            nextTicket = Tickets.FirstOrDefault(t => t.TicketNumber == specificTicketNumber);
        }
        else
        {
            nextTicket = Tickets
                .Where(t => t.Department == department && t.Status == TicketStatus.Waiting)
                .OrderByDescending(t => t.IsPriority)
                .ThenBy(t => t.CreatedAt)
                .FirstOrDefault();
        }

        if (nextTicket != null)
        {
            nextTicket.Status = TicketStatus.NowServing;
            nextTicket.Teller = tellerName;
            OnTicketCalled?.Invoke(nextTicket);
            NotifyStateChanged();
        }
    }

    public void RecallTicket(string ticketNumber)
    {
        var ticket = Tickets.FirstOrDefault(t => t.TicketNumber == ticketNumber && t.Status == TicketStatus.NowServing);
        if (ticket != null)
        {
            OnTicketCalled?.Invoke(ticket);
        }
    }

    private void NotifyStateChanged()
    {
        if (OnChange == null) return;
        foreach (Action handler in OnChange.GetInvocationList())
        {
            try { handler.Invoke(); } catch { }
        }
    }
}