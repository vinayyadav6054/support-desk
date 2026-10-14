using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportDesk.Domain
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int CustomerId { get; set; }
        public User Customer { get; set; } = null!;

        public int? AssignedAgentId { get; set; }
        public User? AssignedAgent { get; set; }

        public bool EscalatedFromBot { get; set; }

        public List<Message> Messages { get; set; } = new();
    }
}
