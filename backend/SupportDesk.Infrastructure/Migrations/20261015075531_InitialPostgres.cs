using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SupportDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnowledgeArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Keywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAgentId = table.Column<int>(type: "integer", nullable: true),
                    EscalatedFromBot = table.Column<bool>(type: "boolean", nullable: false),
                    BotActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_AssignedAgentId",
                        column: x => x.AssignedAgentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SenderType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_TicketId",
                table: "Messages",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedAgentId",
                table: "Tickets",
                column: "AssignedAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId",
                table: "Tickets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.Sql(@"SELECT setval(pg_get_serial_sequence('""KnowledgeArticles""', 'Id'), (SELECT MAX(""Id"") FROM ""KnowledgeArticles""));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnowledgeArticles");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
