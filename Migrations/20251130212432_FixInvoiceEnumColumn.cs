using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAPIv3.Migrations
{
    /// <inheritdoc />
    public partial class FixInvoiceEnumColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // إعادة تسمية الأعمدة لتتوافق مع DbContext
            migrationBuilder.RenameColumn(
            name: "return_type",
            table: "return_tbl",
            newName: "return_type_enum");

        migrationBuilder.RenameColumn(
            name: "person_type",
            table: "person",
            newName: "person_type_enum");

            // تعديل الأعمدة لتخزين Enum كنصوص بدلاً من integers
            migrationBuilder.AlterColumn<string>(
                name: "return_type_enum",
                table: "return_tbl",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "person_type_enum",
                table: "person",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "invoice_type",
                table: "invoice",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // التراجع عن التغييرات
            migrationBuilder.RenameColumn(
                name: "return_type_enum",
                table: "return_tbl",
                newName: "return_type");

            migrationBuilder.RenameColumn(
                name: "person_type_enum",
                table: "person",
                newName: "person_type");

            migrationBuilder.AlterColumn<string>(
                name: "return_type",
                table: "return_tbl",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "person_type",
                table: "person",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "invoice_type",
                table: "invoice",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }

}
