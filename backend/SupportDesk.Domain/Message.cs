using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportDesk.Domain
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public SenderType SenderType { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public int? SenderId { get; set; }   // null when the sender is the bot
        public User? Sender { get; set; }
    }
}
