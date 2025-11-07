using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Complaints.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "complaints",
                columns: table => new
                {
                    complaint_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transaction_id = table.Column<int>(type: "integer", nullable: false),
                    complaintant_id = table.Column<int>(type: "integer", nullable: false),
                    against_user_id = table.Column<int>(type: "integer", nullable: false),
                    reason_complaint = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    evidence_url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    complaint_status = table.Column<int>(type: "integer", nullable: false),
                    resolution = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_by = table.Column<int>(type: "integer", nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delete_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_complaints", x => x.complaint_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "complaints");
        }
    }
}
