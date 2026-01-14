using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAPIv3.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnFieldsForInventoryAndDebt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "client_id",
                table: "return_tbl",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "supplier_id",
                table: "return_tbl",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "unit_id",
                table: "return_product",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "third_with_lastname",
                table: "person",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "person",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "person",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_return_tbl_client_id",
                table: "return_tbl",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_return_tbl_supplier_id",
                table: "return_tbl",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_return_product_unit_id",
                table: "return_product",
                column: "unit_id");

            migrationBuilder.AddForeignKey(
                name: "FK_return_product_unit_unit_id",
                table: "return_product",
                column: "unit_id",
                principalTable: "unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_return_tbl_person_client_id",
                table: "return_tbl",
                column: "client_id",
                principalTable: "person",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_return_tbl_person_supplier_id",
                table: "return_tbl",
                column: "supplier_id",
                principalTable: "person",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_return_product_unit_unit_id",
                table: "return_product");

            migrationBuilder.DropForeignKey(
                name: "FK_return_tbl_person_client_id",
                table: "return_tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_return_tbl_person_supplier_id",
                table: "return_tbl");

            migrationBuilder.DropIndex(
                name: "IX_return_tbl_client_id",
                table: "return_tbl");

            migrationBuilder.DropIndex(
                name: "IX_return_tbl_supplier_id",
                table: "return_tbl");

            migrationBuilder.DropIndex(
                name: "IX_return_product_unit_id",
                table: "return_product");

            migrationBuilder.DropColumn(
                name: "client_id",
                table: "return_tbl");

            migrationBuilder.DropColumn(
                name: "supplier_id",
                table: "return_tbl");

            migrationBuilder.DropColumn(
                name: "unit_id",
                table: "return_product");

            migrationBuilder.AlterColumn<string>(
                name: "third_with_lastname",
                table: "person",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "person",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "person",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
