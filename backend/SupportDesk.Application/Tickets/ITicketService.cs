using SupportDesk.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportDesk.Application.Tickets
{
    public interface ITicketService
    {
        Task<TicketDetailDto> CreateAsync(int userId, CreateTicketRequest request);
        Task<List<TicketSummaryDto>> GetListAsync(int userId, UserRole role);
        Task<TicketDetailDto?> GetByIdAsync(int ticketId, int userId, UserRole role);
        Task<TicketDetailDto?> UpdateAsync(int ticketId, UpdateTicketRequest request);
        Task<TicketDetailDto?> AssignToMeAsync(int ticketId, int agentId);
        Task<MessageDto?> AddMessageAsync(int ticketId, int userId, UserRole role, AddMessageRequest request);
    }
}
