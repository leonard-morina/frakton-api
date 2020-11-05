using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Frakton.Migrations
{
    public partial class AddedFavoritCryptoCoinModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavoriteCryptoCoins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CryptoCoinId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteCryptoCoins", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteCryptoCoins");
        }
    }
}
