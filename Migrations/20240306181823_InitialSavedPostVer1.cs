using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaWisLam.Migrations
{
    /// <inheritdoc />
    public partial class InitialSavedPostVer1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedPost",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserIdNotOwner = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedPost", x => new { x.PostId, x.UserIdNotOwner });
                    table.ForeignKey(
                        name: "FK_SavedPost_AspNetUsers_UserIdNotOwner",
                        column: x => x.UserIdNotOwner,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedPost_Post_PostId",
                        column: x => x.PostId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedPost_UserIdNotOwner",
                table: "SavedPost",
                column: "UserIdNotOwner");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedPost");
        }
    }
}
