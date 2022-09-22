using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistributedCache.Data.Migrations
{
    public partial class AddedCountry_Field_Project : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Projects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Projects");
        }
    }
}
