using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAPIv3.Migrations
{
    /// <inheritdoc />
    public partial class AddSalePriceFixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "selling_price",
                table: "product",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "selling_price",
                table: "product");
        }

    }
}
