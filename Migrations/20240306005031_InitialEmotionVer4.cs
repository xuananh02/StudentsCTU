using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaWisLam.Migrations
{
    /// <inheritdoc />
    public partial class InitialEmotionVer4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "emoji",
                table: "Emotion",
                newName: "Emoji");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Emoji",
                table: "Emotion",
                newName: "emoji");
        }
    }
}
