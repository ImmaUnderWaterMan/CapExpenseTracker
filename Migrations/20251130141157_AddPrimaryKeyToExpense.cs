using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CapExpenseTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimaryKeyToExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TypeExpense = table.Column<string>(type: "character varying", maxLength: 100, nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    CurrentRun = table.Column<int>(type: "integer", nullable: true),
                    Comment = table.Column<string>(type: "character varying", maxLength: 1000, nullable: true),
                    CheckFile = table.Column<string>(type: "character varying", maxLength: 500, nullable: true),
                    Periodicity = table.Column<string>(type: "character varying", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expenses");
        }
    }
}
