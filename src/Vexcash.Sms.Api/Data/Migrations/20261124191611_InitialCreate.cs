using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Vexcash.Sms.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sms");

            migrationBuilder.CreateTable(
                name: "customers",
                schema: "sms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CreditNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "message_templates",
                schema: "sms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sms_messages",
                schema: "sms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Recipient = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ProviderMessageId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ProviderStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProviderHttpStatus = table.Column<int>(type: "integer", nullable: true),
                    ProviderResponseJson = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sms_messages_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "sms",
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sms_messages_message_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "sms",
                        principalTable: "message_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "sms",
                table: "customers",
                columns: new[] { "Id", "Amount", "CreditNumber", "DueDate", "Name", "PhoneNumber" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 480.00m, "VC-10482", new DateOnly(2026, 11, 28), "Amelia Hart", "+31612345678" });

            migrationBuilder.InsertData(
                schema: "sms",
                table: "message_templates",
                columns: new[] { "Id", "Body", "Kind", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "{CreditNumber}\n\nDear {Name},\n\nPlease pay the outstanding amount of {Amount} by {DueDate}.\n\nKind regards,\nVexcash", "Reminder", "Payment reminder", new DateTimeOffset(new DateTime(2026, 11, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "{CreditNumber}\n\nDear {Name},\n\nDespite our reminder, {Amount} remains outstanding. Please make payment no later than {DueDate}.\n\nKind regards,\nVexcash", "PaymentDemand", "Payment demand", new DateTimeOffset(new DateTime(2026, 11, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "{CreditNumber}\n\nDear {Name},\n\nBecause the outstanding amount of {Amount} was not received by {DueDate}, we are terminating the credit agreement. Please contact us immediately.\n\nKind regards,\nVexcash", "ContractTermination", "Contract termination", new DateTimeOffset(new DateTime(2026, 11, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_customers_CreditNumber",
                schema: "sms",
                table: "customers",
                column: "CreditNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_message_templates_Name",
                schema: "sms",
                table: "message_templates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_CreatedAt",
                schema: "sms",
                table: "sms_messages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_CustomerId",
                schema: "sms",
                table: "sms_messages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_ProviderMessageId",
                schema: "sms",
                table: "sms_messages",
                column: "ProviderMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_sms_messages_TemplateId",
                schema: "sms",
                table: "sms_messages",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sms_messages",
                schema: "sms");

            migrationBuilder.DropTable(
                name: "customers",
                schema: "sms");

            migrationBuilder.DropTable(
                name: "message_templates",
                schema: "sms");
        }
    }
}
