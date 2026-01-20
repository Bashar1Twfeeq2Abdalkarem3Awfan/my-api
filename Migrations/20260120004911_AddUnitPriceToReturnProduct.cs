using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAPIv3.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitPriceToReturnProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "unit_price",
                table: "return_product",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unit_price",
                table: "return_product");
        }
    }
}
