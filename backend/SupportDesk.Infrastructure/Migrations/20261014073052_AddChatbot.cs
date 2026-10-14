using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SupportDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatbot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BotActive",
                table: "Tickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "KnowledgeArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeArticles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "KnowledgeArticles",
                columns: new[] { "Id", "Answer", "CreatedAt", "IsActive", "Keywords", "Title" },
                values: new object[,]
                {
                    { 1, "If you can't log in, try these steps:\n1. Check that your email address is typed correctly.\n2. Use the 'Forgot password' option to reset your password.\n3. If you've had several failed attempts, wait 15 minutes and try again, as your account may be temporarily locked.", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "login, log in, sign in, cannot login, cant login, unable to login, locked out, account locked, invalid credentials, access", "Login problems" },
                    { 2, "To reset your password, open the login page, choose 'Forgot password', and enter your email. You'll get a reset link within a few minutes. If it doesn't arrive, check your spam folder and make sure you used the email address you registered with.", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "reset password, forgot password, change password, password reset, new password, reset email, email not received, forgot", "Reset your password" },
                    { 3, "Your invoices and receipts are available under Account > Billing. For a refund or a duplicate charge, our team will need to review it, so please tell us the date and amount.", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "billing, invoice, payment, charged, refund, receipt, subscription, card, charge, double charged", "Billing and invoices" },
                    { 4, "You can deactivate your account under Account > Settings > Delete account. This can't be undone. If you'd like a person to handle the request, choose 'Talk to a human'.", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "delete account, close account, cancel account, remove account, deactivate account", "Delete or deactivate your account" },
                    { 5, "You can update your name and email under Account > Profile. Changing your email may require you to confirm the new address.", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "change email, update email, change name, update profile, profile, account details, edit profile", "Update your profile or email" },
                    { 6, "Sorry you're running into trouble. Please try refreshing the page, clearing your browser cache, or using another browser. If it continues, tell us what you were doing and any error message you saw, and an agent will take a look.", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "slow, outage, down, not loading, error, crash, crashing, not working, bug, broken, loading", "Slow or not working" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnowledgeArticles");

            migrationBuilder.DropColumn(
                name: "BotActive",
                table: "Tickets");
        }
    }
}
