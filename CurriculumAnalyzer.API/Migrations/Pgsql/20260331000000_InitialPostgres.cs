using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CurriculumAnalyzer.API.Migrations.Pgsql
{
    public partial class InitialPostgres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Curriculums",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirebaseUrl = table.Column<string>(type: "text", nullable: false),
                    RawText = table.Column<string>(type: "text", nullable: false),
                    ExperienceLevel = table.Column<string>(type: "text", nullable: false),
                    Specialization = table.Column<string>(type: "text", nullable: false),
                    MarketObjective = table.Column<string>(type: "text", nullable: false),
                    TargetSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    CurrentLocation = table.Column<string>(type: "text", nullable: false),
                    ContentHash = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curriculums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Curriculums_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CurriculumId = table.Column<string>(type: "text", nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false),
                    ScoreExplanation = table.Column<string>(type: "text", nullable: false),
                    SectionsJson = table.Column<string>(type: "text", nullable: false),
                    StrengthsJson = table.Column<string>(type: "text", nullable: false),
                    WeaknessesJson = table.Column<string>(type: "text", nullable: false),
                    OpportunitiesJson = table.Column<string>(type: "text", nullable: false),
                    ActionPlanJson = table.Column<string>(type: "text", nullable: false),
                    JobRecommendationsJson = table.Column<string>(type: "text", nullable: false),
                    EstimatedMinSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    EstimatedMaxSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    RawGrokResponse = table.Column<string>(type: "text", nullable: false)
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
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Curriculums_UploadedAt",
                table: "Curriculums",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Curriculums_UserId",
                table: "Curriculums",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_AnalysisDate",
                table: "Analyses",
                column: "AnalysisDate");

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_CurriculumId",
                table: "Analyses",
                column: "CurriculumId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Analyses");
            migrationBuilder.DropTable(name: "Curriculums");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
