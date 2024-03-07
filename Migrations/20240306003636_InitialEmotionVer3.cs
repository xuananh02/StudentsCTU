using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaWisLam.Migrations
{
    /// <inheritdoc />
    public partial class InitialEmotionVer3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "emoji",
                table: "Emotion",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "emoji",
                table: "Emotion");
        }
    }
}
