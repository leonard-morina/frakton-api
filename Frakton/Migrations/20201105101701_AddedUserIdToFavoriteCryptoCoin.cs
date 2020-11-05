using Microsoft.EntityFrameworkCore.Migrations;

namespace Frakton.Migrations
{
    public partial class AddedUserIdToFavoriteCryptoCoin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "FavoriteCryptoCoins",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FavoriteCryptoCoins");
        }
    }
}
