using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSample.Migrations
{
  /// <inheritdoc />
  public partial class AddSummaries : Migration
  {
    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "summaries");
    }

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "summaries",
        columns: table => new
        {
          id = table.Column<Guid>(
            type: "uniqueidentifier",
            nullable: false,
            defaultValueSql: "NEWSEQUENTIALID()"
          ),
          value = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("pk_summaries", x => x.id);
        }
      );

      migrationBuilder.CreateIndex(
        name: "ix_summaries_value",
        table: "summaries",
        column: "value",
        unique: true
      );
    }
  }
}
