using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace twetty.Migrations
{
    /// <inheritdoc />
    public partial class okwawaaa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Users_FollowingUsername",
                table: "Follows");

            migrationBuilder.RenameColumn(
                name: "FollowingUsername",
                table: "Follows",
                newName: "TargetUsername");

            migrationBuilder.RenameIndex(
                name: "IX_Follows_FollowingUsername",
                table: "Follows",
                newName: "IX_Follows_TargetUsername");

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Users_TargetUsername",
                table: "Follows",
                column: "TargetUsername",
                principalTable: "Users",
                principalColumn: "Username",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Users_TargetUsername",
                table: "Follows");

            migrationBuilder.RenameColumn(
                name: "TargetUsername",
                table: "Follows",
                newName: "FollowingUsername");

            migrationBuilder.RenameIndex(
                name: "IX_Follows_TargetUsername",
                table: "Follows",
                newName: "IX_Follows_FollowingUsername");

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Users_FollowingUsername",
                table: "Follows",
                column: "FollowingUsername",
                principalTable: "Users",
                principalColumn: "Username",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
