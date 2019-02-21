using Microsoft.EntityFrameworkCore.Migrations;

namespace AniaAlpha.Migrations
{
    public partial class RequiredFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AnimeLists",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AnimeLists",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
