using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportDesk.Domain
{
   public enum UserRole { Customer = 0, Agent = 1, Admin = 2 };
   public enum TicketStatus { Open = 0, InProgress=1, Resolved = 2 ,Closed = 3 };
    public enum TicketPriority { Low = 0, Medium = 1, High = 2, Urgent = 3 };
    public enum SenderType { Customer = 0, Agent = 1, Bot = 2 };
}
