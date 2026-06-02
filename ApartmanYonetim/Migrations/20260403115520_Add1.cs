using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApartmanYonetim.Migrations
{
    /// <inheritdoc />
    public partial class Add1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Kullanicilar_OlusturanId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "OluşturanId",
                table: "Announcements");

            migrationBuilder.AlterColumn<string>(
                name: "OlusturanId",
                table: "Announcements",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Kullanicilar_OlusturanId",
                table: "Announcements",
                column: "OlusturanId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Kullanicilar_OlusturanId",
                table: "Announcements");

            migrationBuilder.AlterColumn<string>(
                name: "OlusturanId",
                table: "Announcements",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "OluşturanId",
                table: "Announcements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Kullanicilar_OlusturanId",
                table: "Announcements",
                column: "OlusturanId",
                principalTable: "Kullanicilar",
                principalColumn: "Id");
        }
    }
}
