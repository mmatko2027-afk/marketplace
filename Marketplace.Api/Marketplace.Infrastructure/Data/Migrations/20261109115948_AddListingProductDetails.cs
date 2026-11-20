using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddListingProductDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Listings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Defects",
                table: "Listings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Listings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackageContents",
                table: "Listings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProductConditionId",
                table: "Listings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionYear",
                table: "Listings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Warranty",
                table: "Listings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ListingImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ListingId = table.Column<int>(type: "INTEGER", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingImages_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListingSpecifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ListingId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingSpecifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingSpecifications_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductConditions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Listings_BrandId",
                table: "Listings",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_ProductConditionId",
                table: "Listings",
                column: "ProductConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                table: "Brands",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListingImages_ListingId",
                table: "ListingImages",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingSpecifications_ListingId_Name",
                table: "ListingSpecifications",
                columns: new[] { "ListingId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductConditions_Name",
                table: "ProductConditions",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_Brands_BrandId",
                table: "Listings",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_ProductConditions_ProductConditionId",
                table: "Listings",
                column: "ProductConditionId",
                principalTable: "ProductConditions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_Brands_BrandId",
                table: "Listings");

            migrationBuilder.DropForeignKey(
                name: "FK_Listings_ProductConditions_ProductConditionId",
                table: "Listings");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "ListingImages");

            migrationBuilder.DropTable(
                name: "ListingSpecifications");

            migrationBuilder.DropTable(
                name: "ProductConditions");

            migrationBuilder.DropIndex(
                name: "IX_Listings_BrandId",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_ProductConditionId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Defects",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "PackageContents",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ProductConditionId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ProductionYear",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Warranty",
                table: "Listings");
        }
    }
}
