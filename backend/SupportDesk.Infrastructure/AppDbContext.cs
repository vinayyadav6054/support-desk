using Microsoft.EntityFrameworkCore;
using SupportDesk.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportDesk.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Message> Messages => Set<Message>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.FullName).HasMaxLength(150).IsRequired();
                e.Property(u => u.Email).HasMaxLength(256).IsRequired();
                e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            });

            b.Entity<Ticket>(e =>
            {
                e.Property(t => t.Subject).HasMaxLength(200).IsRequired();
                e.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
                e.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);

                e.HasOne(t => t.Customer)
                 .WithMany(u => u.CreatedTickets)
                 .HasForeignKey(t => t.CustomerId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.AssignedAgent)
                 .WithMany(u => u.AssignedTickets)
                 .HasForeignKey(t => t.AssignedAgentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<Message>(e =>
            {
                e.Property(m => m.SenderType).HasConversion<string>().HasMaxLength(20);

                e.HasOne(m => m.Ticket)
                 .WithMany(t => t.Messages)
                 .HasForeignKey(m => m.TicketId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(m => m.Sender)
                 .WithMany()
                 .HasForeignKey(m => m.SenderId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}