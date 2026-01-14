using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAPIv3.Migrations
{
    /// <inheritdoc />
    public partial class FixPersonTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "return_type_enum",
                table: "return_tbl",
                newName: "return_type");

            migrationBuilder.AlterColumn<string>(
                name: "return_type",
                table: "return_tbl",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "person_type_enum",
                table: "person",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "invoice_type",
                table: "invoice",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "return_type",
                table: "return_tbl",
                newName: "return_type_enum");

            migrationBuilder.AlterColumn<int>(
                name: "return_type_enum",
                table: "return_tbl",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "person_type_enum",
                table: "person",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "invoice_type",
                table: "invoice",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
