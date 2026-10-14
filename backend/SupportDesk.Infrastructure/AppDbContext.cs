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
        public DbSet<KnowledgeArticle> KnowledgeArticles => Set<KnowledgeArticle>();

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
            b.Entity<KnowledgeArticle>(e =>
            {
                e.Property(a => a.Title).HasMaxLength(200).IsRequired();
                e.Property(a => a.Keywords).HasMaxLength(500).IsRequired();
                e.Property(a => a.Answer).HasMaxLength(4000).IsRequired();

                var seeded = new DateTime(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc);

                e.HasData(
                    new KnowledgeArticle
                    {
                        Id = 1,
                        CreatedAt = seeded,
                        Title = "Login problems",
                        Keywords = "login, log in, sign in, cannot login, cant login, unable to login, locked out, account locked, invalid credentials, access",
                        Answer = "If you can't log in, try these steps:\n1. Check that your email address is typed correctly.\n2. Use the 'Forgot password' option to reset your password.\n3. If you've had several failed attempts, wait 15 minutes and try again, as your account may be temporarily locked."
                    },
                    new KnowledgeArticle
                    {
                        Id = 2,
                        CreatedAt = seeded,
                        Title = "Reset your password",
                        Keywords = "reset password, forgot password, change password, password reset, new password, reset email, email not received, forgot",
                        Answer = "To reset your password, open the login page, choose 'Forgot password', and enter your email. You'll get a reset link within a few minutes. If it doesn't arrive, check your spam folder and make sure you used the email address you registered with."
                    },
                    new KnowledgeArticle
                    {
                        Id = 3,
                        CreatedAt = seeded,
                        Title = "Billing and invoices",
                        Keywords = "billing, invoice, payment, charged, refund, receipt, subscription, card, charge, double charged",
                        Answer = "Your invoices and receipts are available under Account > Billing. For a refund or a duplicate charge, our team will need to review it, so please tell us the date and amount."
                    },
                    new KnowledgeArticle
                    {
                        Id = 4,
                        CreatedAt = seeded,
                        Title = "Delete or deactivate your account",
                        Keywords = "delete account, close account, cancel account, remove account, deactivate account",
                        Answer = "You can deactivate your account under Account > Settings > Delete account. This can't be undone. If you'd like a person to handle the request, choose 'Talk to a human'."
                    },
                    new KnowledgeArticle
                    {
                        Id = 5,
                        CreatedAt = seeded,
                        Title = "Update your profile or email",
                        Keywords = "change email, update email, change name, update profile, profile, account details, edit profile",
                        Answer = "You can update your name and email under Account > Profile. Changing your email may require you to confirm the new address."
                    },
                    new KnowledgeArticle
                    {
                        Id = 6,
                        CreatedAt = seeded,
                        Title = "Slow or not working",
                        Keywords = "slow, outage, down, not loading, error, crash, crashing, not working, bug, broken, loading",
                        Answer = "Sorry you're running into trouble. Please try refreshing the page, clearing your browser cache, or using another browser. If it continues, tell us what you were doing and any error message you saw, and an agent will take a look."
                    });
            });
        }
    }
}