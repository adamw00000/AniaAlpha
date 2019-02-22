using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AniaAlpha.Migrations
{
    public partial class ChangedAliasTablePK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Aliases");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Aliases",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Aliases",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Aliases",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases",
                column: "Id");
        }
    }
}
