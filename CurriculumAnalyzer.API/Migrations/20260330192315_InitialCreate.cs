using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CurriculumAnalyzer.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Curriculums",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FirebaseUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RawText = table.Column<string>(type: "TEXT", nullable: false),
                    ExperienceLevel = table.Column<string>(type: "TEXT", nullable: false),
                    Specialization = table.Column<string>(type: "TEXT", nullable: false),
                    MarketObjective = table.Column<string>(type: "TEXT", nullable: false),
                    TargetSalary = table.Column<decimal>(type: "TEXT", nullable: true),
                    CurrentLocation = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curriculums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CurriculumId = table.Column<string>(type: "TEXT", nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OverallScore = table.Column<int>(type: "INTEGER", nullable: false),
                    ScoreExplanation = table.Column<string>(type: "TEXT", nullable: false),
                    SectionsJson = table.Column<string>(type: "TEXT", nullable: false),
                    StrengthsJson = table.Column<string>(type: "TEXT", nullable: false),
                    WeaknessesJson = table.Column<string>(type: "TEXT", nullable: false),
                    OpportunitiesJson = table.Column<string>(type: "TEXT", nullable: false),
                    ActionPlanJson = table.Column<string>(type: "TEXT", nullable: false),
                    JobRecommendationsJson = table.Column<string>(type: "TEXT", nullable: false),
                    EstimatedMinSalary = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstimatedMaxSalary = table.Column<decimal>(type: "TEXT", nullable: false),
                    RawGrokResponse = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analyses_Curriculums_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "Curriculums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_AnalysisDate",
                table: "Analyses",
                column: "AnalysisDate");

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_CurriculumId",
                table: "Analyses",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_Curriculums_UploadedAt",
                table: "Curriculums",
                column: "UploadedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Analyses");

            migrationBuilder.DropTable(
                name: "Curriculums");
        }
    }
}
